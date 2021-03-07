using Cibc.Pricing.ValuationModels;
using Core.TradeProviders;
using Domain.MarketData;
using Domain.TradeTypes;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class ValuationOrchestrator : IValuationOrchestrator
    {
        private readonly IMarketDataProvider<MarketDataItem> _marketDataProvider;
        private readonly IEnumerable<ITradeProvider<Trade>> _tradeProviders;
        private readonly IValuationModelFactory _valuationModelFactory;
        private readonly StandardMktDataCache _marketDataCache;
        private readonly ILogger<ValuationOrchestrator> _logger;
       
        public ValuationOrchestrator(IMarketDataProvider<MarketDataItem> marketDataProvider, 
                                     IEnumerable<ITradeProvider<Trade>> tradeProviders,
                                     IValuationModelFactory valuationModelFactory,
                                     StandardMktDataCache marketDataCache, 
                                     ILogger<ValuationOrchestrator> logger)
        {
            _marketDataProvider = marketDataProvider ?? throw new ArgumentNullException("marketDataProvider");
            _tradeProviders = tradeProviders ?? throw new ArgumentNullException("tradeProviders");
            _valuationModelFactory = valuationModelFactory;
            _marketDataCache = marketDataCache;
            _logger = logger;


             

        }
        public async Task<bool> LoadMarketData()
        {
            Logger.LogInformation("Loading MarketData....");

            var mktDataCollection = _marketDataProvider.LoadMarketDataAsync();

            await foreach (var mktData in mktDataCollection)
            {
                _marketDataCache.TryAdd(mktData.Key, mktData.Price);
            }

            return await Task.FromResult(true);
        }


        public ILogger<ValuationOrchestrator> Logger => _logger;

        public async IAsyncEnumerable<ValuationResult> Evaluate<TTrade>(ITradeProvider<TTrade> tradeProvider) where TTrade: Trade
        {
            _logger.LogInformation("Evaluate trades..");

            await foreach (var trade in LoadTradesAsync(tradeProvider).ConfigureAwait(false))
            {                
                if (trade is TTrade)
                {
                    var valuationModel = _valuationModelFactory.Create<TTrade>(); /// TODO Reuse this from pool instead of creating a new object every time. 
                    decimal? underlyingPrice;
                    if (_marketDataCache.TryGetValue(trade.Underlying, out underlyingPrice))
                    {
                        yield return await valuationModel.CalcAsync(new PricingInputs<TTrade>(trade, underlyingPrice));
                    }
                    else
                    {
                        yield return new ValuationResult(trade.TradeId, null, new List<string>() { $"Market data for underlying {trade.Underlying} is missing" });
                    }
                }
            }      
        }
  
        public IAsyncEnumerable<TTrade> LoadTradesAsync<TTrade>(ITradeProvider<TTrade> tradeProvider) where TTrade:Trade
        {
            Logger.LogInformation("Loading Trades async....");

            return tradeProvider.LoadTradesAsync();
        }
    }

    public class ValuationResults
    {
    }

    public interface IValuationOrchestrator
    {
    }


}
