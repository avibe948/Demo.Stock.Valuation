using Cibc.Pricing.ValuationModels;
using Domain.TradeTypes;

namespace Cibc.Pricing
{
    public interface IValuationModelFactory
    {
        IValuationModel<TTrade> Create<TTrade>(TradeType tradeType) where TTrade : Trade;
        
    }
}