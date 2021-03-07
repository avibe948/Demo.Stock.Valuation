using Cibc.Pricing.ValuationModels;
using Domain.TradeTypes;
using Cibc.Pricing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Cibc.Pricing.ValuationModels
{
    public interface IValuationModelFactory
    {
        IValuationModel<TTrade> Create<TTrade>() where TTrade : Trade;
    }
    public class ValuationModelFactory: IValuationModelFactory
    {
        
        public  IValuationModel<TTrade> Create<TTrade>()  where TTrade:Trade
        {
            return nameof(TTrade) switch
            {
                nameof(StockTrade) => new StockValuationModel() as IValuationModel<TTrade>,
                nameof(FxForwardTrade) => new FxForwardValuationModel() as IValuationModel<TTrade>,
                nameof(FxOptionTrade) => new FxOptionValuationModel() as IValuationModel<TTrade>,
                nameof(IRSwapTrade) => new IRSwapValuationModel() as IValuationModel<TTrade>,
                _ => null,
            };
        }
    }
}
