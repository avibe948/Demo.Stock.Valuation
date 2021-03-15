using Cibc.Pricing.ValuationModels;
using Cibc.Core.TradeProviders;
using Domain.MarketData;
using Domain.Trade;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.CompilerServices;
using CsvHelper;
using System.IO;
using System.Globalization;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using Core.Validators.Trades;
using FluentValidation;
using FluentValidation.Results;

namespace Cibc.Core
{
    public interface IValuationOrchestrator
    {
        Task<bool> LoadMarketData(CancellationToken cancellationToken = default);
        Task<bool> RunValuations(string resultsFilePath, CancellationToken cancellationToken = default);
        IObservable<Trade> GetAllTrades(CancellationToken cancellationToken = default);
        void ReportResults(IObservable<ValuationResult> results, string resultPath, CancellationToken cancellationToken = default);
    }

    public class ValuationOrchestrator : IValuationOrchestrator
    {
        private readonly IMarketDataProvider<MarketDataItem> _marketDataProvider;
        private readonly IEnumerable<IFileTradeProvider<Trade>> _tradeProviders;
        private readonly Func<TradeType, IValuationModel> _valuationModelCreator;
        private readonly StandardMktDataCache _marketDataCache;
        private readonly IValidator<Trade> _tradeValidator;
        public readonly  ILogger<ValuationOrchestrator> Log;

        public ValuationOrchestrator(IMarketDataProvider<MarketDataItem> marketDataProvider,
                                     IEnumerable<IFileTradeProvider<Trade>> tradeProviders,
                                     Func<TradeType, IValuationModel> valuationModelCreator,
                                     StandardMktDataCache marketDataCache,
                                     IValidator<Trade> tradeValidator,
                                     ILogger<ValuationOrchestrator> logger)
        {
            _marketDataProvider = marketDataProvider ?? throw new ArgumentNullException(nameof(marketDataProvider));
            _tradeProviders = tradeProviders ?? throw new ArgumentNullException(nameof(tradeProviders));
            _valuationModelCreator = valuationModelCreator ?? throw new ArgumentNullException(nameof(_valuationModelCreator));
            _marketDataCache = marketDataCache ?? throw new ArgumentNullException(nameof(marketDataCache));
            _tradeValidator = tradeValidator;
            Log = logger;
        }

        public async Task<bool> RunValuations(string resultsFilePath, CancellationToken cancellationToken = default)
        {
            await LoadMarketData();

            var resultsObservable = GetValuationResults();

            ReportResults(resultsObservable, resultsFilePath, cancellationToken);

            return true;
        }
        public async Task<bool> LoadMarketData(CancellationToken cancellationToken = default)
        {
            Log.LogInformation("Loading MarketData....");

            var mktDataCollection = _marketDataProvider.LoadMarketDataAsync();

            await foreach (var mktData in mktDataCollection.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                _marketDataCache.TryAdd(mktData.Key, mktData.Price);
            }

            return true;
        }
        public void ReportResults(IObservable<ValuationResult> valuationResultsStream, string resultPath, CancellationToken cancellation)
        {
            if (valuationResultsStream == null)
                throw new ArgumentNullException(nameof(valuationResultsStream));

            if (string.IsNullOrEmpty(resultPath))
                throw new ArgumentNullException(nameof(valuationResultsStream));

            var csv = new System.Text.StringBuilder();
            csv.AppendLine("TradeId, Pv, ErrorMessage");
            
            valuationResultsStream
            .ObserveOn(TaskPoolScheduler.Default)
            .Subscribe(result =>
            { 
                    var tradeId = result.TradeId.ToString();
                    var pv = result?.ValuationMeasure?.PV.ToString() ?? string.Empty;
                    var errorMessage = (result.Errors != null && result.Errors.Any())?  string.Join(';', result?.Errors) :string.Empty;
                    var line = $"{tradeId},{pv},{errorMessage}";
                    csv.AppendLine(line);
                    
            },
                onError: (error) => Log.LogError($"Error during result reporting, exception: {error}"),
                onCompleted: () => {
                    Log.LogError("Completed writing results to file");
                    File.WriteAllTextAsync(resultPath, csv.ToString());
                }, cancellation);
        }

        public IObservable<Trade> GetAllTrades(CancellationToken cancellationToken = default)
        {
            var tradeStreams = _tradeProviders.Select(t => t.GetTradeStream(cancellationToken)).ToArray();
            return Observable.Concat<Trade>(tradeStreams);
        }
        public IObservable<ValuationResult> GetValuationResults(CancellationToken cancellationToken = default)
        {

            return GetAllTrades(cancellationToken)
                             .ValidateTrades(_tradeValidator)
                             .Do(x => Log.LogInformation("Evaluate trades async .."))
                             .Select(tradeValidation =>
                            {
                                 var trade = tradeValidation.Trade;

                                 if (!tradeValidation.ValidationResults.IsValid)
                                 {
                                     return new ValuationResult(tradeValidation.Trade.TradeId, null, tradeValidation.ValidationResults.Errors.Select(v => v.ErrorMessage).ToList());
                                 }
                                 decimal? underlyingPrice;
                                 if (_marketDataCache.TryGetValue(trade.Underlying, out underlyingPrice))
                                 {
                                    
                                     return _valuationModelCreator(trade.TradeType).Calc(new PricingInputs(trade, underlyingPrice));
                                 }
                                 else
                                 {
                                     return new ValuationResult(trade.TradeId, null, new List<string>() { $"Market data errror: Price for underlying {trade.Underlying} is missing" });
                                 }
                             });
        }

    }
}
