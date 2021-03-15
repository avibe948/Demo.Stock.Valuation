using Cibc.Pricing.ValuationModels;
using Domain.Trade;

namespace Cibc.Pricing.ValuationModels
{
    public interface IValuationModelFactory
    {
        IValuationModel Create(TradeType type);
        
    }
}