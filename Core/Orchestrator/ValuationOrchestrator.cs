using Cibc.Pricing.ValuationModels;
using Cibc.Core.TradeProviders;
using Domain.MarketData;
using Domain.TradeTypes;
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


namespace Cibc.Core
{
    public interface IValuationOrchestrator
    {
        Task<bool> LoadMarketData(CancellationToken cancellationToken = default);
    }

    public class ValuationOrchestrator : IValuationOrchestrator
    {
        private readonly IMarketDataProvider<MarketDataItem> _marketDataProvider;
        private readonly IEnumerable<IFileTradeProvider<Trade>> _tradeProviders;
        private readonly Func<TradeType, IValuationModel> _valuationModelCreator;
        private readonly StandardMktDataCache _marketDataCache;
        public  readonly ILogger<ValuationOrchestrator> Log;

        public ValuationOrchestrator(IMarketDataProvider<MarketDataItem> marketDataProvider,
                                     IEnumerable<IFileTradeProvider<Trade>> tradeProviders,
                                     Func<TradeType,IValuationModel> valuationModelCreator,
                                     StandardMktDataCache marketDataCache, 
                                     ILogger<ValuationOrchestrator> logger)
        {
            marketDataProvider = marketDataProvider ?? throw new ArgumentNullException(nameof(marketDataProvider));
            _tradeProviders = tradeProviders ?? throw new ArgumentNullException(nameof(tradeProviders));
            _valuationModelCreator = valuationModelCreator ?? throw new ArgumentNullException(nameof(_valuationModelCreator));
            _marketDataCache = marketDataCache ?? throw new ArgumentNullException(nameof(marketDataCache));
            Log = logger;
        }

        /// <summary>
        /// /
        /// </summary>
        /// <param name="marketDataProvider">source provider for market data</param>
        /// <param name="tradefileProviders">a collection of key value pairs where the key is the path of the </param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
        /// <returns></returns>
        public async Task<bool> RunValuations(string resultsFilePath, CancellationToken cancellationToken = default)
        {            
            await LoadMarketData();

            var resultsAsAsyncEnumerable = GetValuationResults().ToAsyncEnumerable();

            await ReportResults(resultsAsAsyncEnumerable, resultsFilePath);            
            
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
        public async Task ReportResults(IAsyncEnumerable<ValuationResult> results, string resultPath)
        {
            using (var stream = new StreamWriter(resultPath))
            using (var csvWriter = new CsvWriter(stream, CultureInfo.InvariantCulture))
            {
                await foreach (var record in results)
                {
                    csvWriter.WriteRecord(record);
                }
            }
        }

        public IObservable<Trade> GetAllTrades(CancellationToken cancellationToken=default)
        {
            var tradeStreams = _tradeProviders.Select(t => t.GetTradeStream(cancellationToken)).ToArray();
            return Observable.Concat<Trade>(tradeStreams); 
        }
        public IObservable<ValuationResult> GetValuationResults(CancellationToken cancellationToken = default)
        {
            
            Log.LogInformation("Evaluate trades async ..");

                return           GetAllTrades(cancellationToken)
                                 .SubscribeOn(TaskPoolScheduler.Default)
                                 .Select((trade) =>
                                 {
                                     decimal? underlyingPrice;
                                     if (_marketDataCache.TryGetValue(trade.Underlying, out underlyingPrice))
                                     {
                                         return _valuationModelCreator(trade.TradeType).Calc(new PricingInputs(trade, underlyingPrice));
                                     }
                                     else
                                     {
                                         return new ValuationResult(trade.TradeId, null, new List<string>() { $"Market data for underlying {trade.Underlying} is missing" });
                                     }
                                 });
            
        }
  
    }




}
