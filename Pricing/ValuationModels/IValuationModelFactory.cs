using Cibc.Pricing.ValuationModels;
using Domain.TradeTypes;

namespace Cibc.Pricing.ValuationModels
{
    public interface IValuationModelFactory
    {
        IValuationModel Create(TradeType type);
        
    }
}