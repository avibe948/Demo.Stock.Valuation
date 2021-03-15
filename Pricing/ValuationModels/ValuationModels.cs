using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.MarketData;
using Domain.Trade;
using Domain.Trade;
using FluentValidation;

namespace Cibc.Pricing.ValuationModels
{


    public class PricingInputs
    {
        public PricingInputs(Trade trade, decimal? underlyingMarketPrice)
        {
            Trade = trade;
            UnderlyingMarketPrice = underlyingMarketPrice;
        }
        public Trade Trade { get; private set; }
        public decimal? UnderlyingMarketPrice { get; private set;}
    }

    public class ValuationMeasure
    {
        public ValuationMeasure(decimal? pv)
        {
            PV = pv;
        }
        public decimal? PV { get; private set; }

        public string Errors { get; set; }
    }
    public class ValuationResult
    {
        public ValuationResult(long tradeId, ValuationMeasure valuationMeasure, IList<string> Errors=null)
        {
            TradeId = tradeId;
            ValuationMeasure = valuationMeasure;          
            this.Errors = Errors;
        }
        public long TradeId { get; private set; }
        public ValuationMeasure ValuationMeasure { get; private set; }
        public IList<string> Errors { get; private set; }
    }

    public abstract class ValuationModelBase : IValuationModel
    {
        public ValuationModelBase() { }

        protected Func<PricingInputs, ValuationMeasure> PriceFunc { get; set; }
     

        public ValuationResult Calc(PricingInputs input)
        {
            List<string> errors = input.UnderlyingMarketPrice.HasValue ? null : new List<string>() { "Price must be greater than 0" };

            var price = PriceFunc(input);

            return new ValuationResult(input.Trade.TradeId, price, errors);
        }
    }
    public class StockValuationModel : ValuationModelBase
    {       
        public StockValuationModel()
        {
            PriceFunc = (PricingInputs pricingInputs) => new ValuationMeasure(pricingInputs.Trade.Quantity * pricingInputs.UnderlyingMarketPrice);
        }
    }
   
    public class FxOptionValuationModel : ValuationModelBase
    {
        public FxOptionValuationModel()
        {
            PriceFunc = (PricingInputs pricingInputs) =>
            {
                var trade = pricingInputs.Trade as FxOptionTrade ?? throw new ArgumentNullException(nameof(pricingInputs));
                
                decimal payoff = trade.OptionType == OptionType.Call ?
                                 trade.Quantity * Math.Min(pricingInputs.UnderlyingMarketPrice.Value, trade.Strike) :
                                 trade.Quantity * Math.Max(pricingInputs.UnderlyingMarketPrice.Value, trade.Strike);

                return new ValuationMeasure(payoff);
            };
        }
    }

    public class FxForwardValuationModel : ValuationModelBase
    {
        public FxForwardValuationModel(){

            PriceFunc = (PricingInputs pricingInputs) => new ValuationMeasure(pricingInputs.Trade.Quantity * pricingInputs.UnderlyingMarketPrice);

        }

    }

    public class IRSwapValuationModel : ValuationModelBase
    {
  
        public IRSwapValuationModel()
        {
          
            PriceFunc = (PricingInputs pricingInputs) =>
            {
                if (Math.Abs((double)pricingInputs.UnderlyingMarketPrice.Value) < 10E-16)
                    return new ValuationMeasure(null) {Errors = "IR Swap market price must not be 0"};

                var trade = pricingInputs.Trade as IRSwapTrade ?? throw new ArgumentNullException(nameof(pricingInputs));

                return new ValuationMeasure(trade.Quantity * trade.FixedRate / pricingInputs.UnderlyingMarketPrice);
                
            };
        }
    }



}
