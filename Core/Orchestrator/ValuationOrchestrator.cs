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
        IAsyncEnumerable<ValuationResult> Evaluate<TTrade>(IFileTradeProvider tradeProvider, [EnumeratorCancellation] CancellationToken cancellationToken = default) where TTrade : Trade;
        Task<bool> LoadMarketData(CancellationToken cancellationToken = default);
        IAsyncEnumerable<TTrade> LoadTradesAsync<TTrade>(IFileTradeProvider tradeProvider) where TTrade : Trade;
    }

    public class ValuationOrchestrator : IValuationOrchestrator
    {
        private readonly IMarketDataProvider<MarketDataItem> _marketDataProvider;
        private readonly IFileTradeProvider _tradeProvider;
        private readonly IValuationModelFactory _valuationModelFactory;
        private readonly StandardMktDataCache _marketDataCache;
        public readonly ILogger<ValuationOrchestrator> Log;

        public ValuationOrchestrator(IValuationModelFactory valuationModelFactory,
                                     StandardMktDataCache marketDataCache, 
                                     ILogger<ValuationOrchestrator> logger)
        {
            _valuationModelFactory = valuationModelFactory;
            _marketDataCache = marketDataCache;
            Log = logger;
        }

        /// <summary>
        /// /
        /// </summary>
        /// <param name="marketDataProvider">source provider for market data</param>
        /// <param name="tradefileProviders">a collection of key value pairs where the key is the path of the </param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> RunValuations(IMarketDataProvider<MarketDataItem> marketDataProvider,
                                              IFileTradeProvider tradefileProvider,
                                              CancellationToken cancellationToken = default)
        {
            await LoadMarketData();

            var directory = System.IO.Directory.GetCurrentDirectory();
                        
            var  Evaluate<StockTrade>(marketDataProvider, tradefileProvider, System.IO.Path.Combine(directory, "Stock.csv"), cancellationToken);
            await Evaluate<IRSwapTrade>(marketDataProvider, tradefileProvider, System.IO.Path.Combine(directory, "Stock.csv"), cancellationToken);
            await Evaluate<FxForwardTrade>(marketDataProvider, tradefileProvider, System.IO.Path.Combine(directory, "Stock.csv"), cancellationToken);
            await Evaluate<FxOptionTrade>(marketDataProvider, tradefileProvider, System.IO.Path.Combine(directory, "Stock.csv"), cancellationToken);

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

        public IObservable<ValuationResult> Evaluate<TTrade>(IMarketDataProvider<MarketDataItem> marketDataProvider,
                                                             IFileTradeProvider tradeProvider, 
                                                             Func<TTrade,IValuationModel<TTrade>> valuationModel,
                                                             string filePath,
                                                             CancellationToken cancellationToken = default) where TTrade : Trade
        {
            marketDataProvider = marketDataProvider ?? throw new ArgumentNullException("marketDataProvider");
            tradeProvider = tradeProvider ?? throw new ArgumentNullException("tradeProviders");

            Log.LogInformation("Evaluate trades..");

            return tradeProvider.GetTradeStream<TTrade>(filePath, cancellationToken)
                                 .SubscribeOn(TaskPoolScheduler.Default)
                                 .SubscribeOn(TaskPoolScheduler.Default)
                                 .Select((trade) =>
                                 {
                                     decimal? underlyingPrice;
                                     if (_marketDataCache.TryGetValue(trade.Underlying, out underlyingPrice))
                                     {
                                         return valuationModel.Calc(new PricingInputs<TTrade>(trade, underlyingPrice));
                                     }
                                     else
                                     {
                                         return new ValuationResult(trade.TradeId, null, new List<string>() { $"Market data for underlying {trade.Underlying} is missing" });
                                     }
                                 });
            
        }
  
        public IObservable<TTrade> GetTradeStream<TTrade>(IFileTradeProvider tradeProvider, string filePath) where TTrade:Trade
        {
            Log.LogInformation("Loading Trades async....");

            return tradeProvider.GetTradeStream<TTrade>(filePath);
        }
    }




}
