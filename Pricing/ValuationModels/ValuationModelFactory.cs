﻿using Cibc.Pricing.ValuationModels;
using Domain.TradeTypes;
using Cibc.Pricing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Cibc.Pricing.ValuationModels
{
    public class ValuationModelFactory: IValuationModelFactory
    {
        
        public  IValuationModel Create(TradeType tradeType)
        {
            return tradeType  switch
            {
                TradeType.Stock => new StockValuationModel(),
                TradeType.FxForward => new FxForwardValuationModel(),
                TradeType.FxOption=> new FxOptionValuationModel(),
                TradeType.IRSwap => new IRSwapValuationModel(),
                _ => null,
            };
        }
    }
}
