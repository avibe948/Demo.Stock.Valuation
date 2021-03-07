using Cibc.Pricing.ValuationModels;
using Domain.TradeTypes;

namespace Cibc.Pricing.ValuationModels
{
    public interface IValuationModelFactory
    {
        IValuationModel<TTrade> Create<TTrade>() where TTrade : Trade;
        
    }
}