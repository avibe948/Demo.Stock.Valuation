using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.MarketData;
using Domain.TradeTypes;
using FluentValidation;

namespace Cibc.Pricing.ValuationModels
{

    public interface IValuationModel<TTrade> where TTrade : Trade 
    {
        Task<ValuationResult> CalcAsync(PricingInputs<TTrade> input);
    }

    public class PricingInputs<TTrade>
    {
        public PricingInputs(TTrade trade, decimal? underlyingMarketPrice)
        {
            Trade = trade;
            UnderlyingMarketPrice = underlyingMarketPrice;
        }
        public TTrade Trade { get; private set; }
        public decimal? UnderlyingMarketPrice { get; private set;}
    }

    public class ValuationMeasure
    {
        public ValuationMeasure(decimal? pv)
        {
            PV = pv;
        }
        public decimal? PV { get; private set; }
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

    public abstract class ValuationModelBase<TTrade> : IValuationModel<TTrade> where TTrade:Trade
    {
        protected Func<PricingInputs<TTrade>, ValuationMeasure> PayoutFunc { get; set; }
        public ValuationModelBase(){}

        public async Task<ValuationResult> CalcAsync(PricingInputs<TTrade> input)
        {
            List<string> errors = input.UnderlyingMarketPrice.HasValue ? null: new List<string>() { "Price must be greater than 0" };               
            
            var price = PayoutFunc(input); 

            return new ValuationResult(input.Trade.TradeId, price, errors);
        }
    }
    public class StockValuationModel : ValuationModelBase<StockTrade>
    {       
        public StockValuationModel()
        {
            PayoutFunc = (PricingInputs<StockTrade> pricingInputs) => new ValuationMeasure(pricingInputs.Trade.Quantity * pricingInputs.UnderlyingMarketPrice);
        }
    }
   
    public class FxOptionValuationModel : ValuationModelBase<FxOptionTrade>
    {
        public FxOptionValuationModel()
        {
            PayoutFunc = (PricingInputs<FxOptionTrade> pricingInputs) =>
            {
                decimal payoff = pricingInputs.Trade.OptionType == OptionType.Call ?
                                 pricingInputs.Trade.Quantity * Math.Min(pricingInputs.UnderlyingMarketPrice.Value, pricingInputs.Trade.Strike) :
                                 pricingInputs.Trade.Quantity * Math.Max(pricingInputs.UnderlyingMarketPrice.Value, pricingInputs.Trade.Strike);

                return new ValuationMeasure(payoff);
            };
        }
    }

    public class FxForwardValuationModel : ValuationModelBase<FxForwardTrade>
    {
        public FxForwardValuationModel(){

            PayoutFunc = (PricingInputs<FxForwardTrade> pricingInputs) => new ValuationMeasure(pricingInputs.Trade.Quantity * pricingInputs.UnderlyingMarketPrice);

        }

    }

    public class IRSwapValuationModel : ValuationModelBase<IRSwapTrade>
    {
        public IRSwapValuationModel()
        {
            PayoutFunc = (PricingInputs<IRSwapTrade> pricingInputs) => new ValuationMeasure(pricingInputs.Trade.Quantity * pricingInputs.Trade.FixedRate / pricingInputs.UnderlyingMarketPrice);
        }
    }



}
