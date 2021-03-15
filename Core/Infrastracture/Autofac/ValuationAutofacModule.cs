using System;
using System.Collections.Generic;
using System.Text;
using Autofac;
using Cibc.Pricing.ValuationModels;
using Domain.MarketData;
using Cibc.Core;
using Cibc.Core.TradeProviders;
using Domain.Trade;
using System.IO;
using Microsoft.Extensions.Logging;
using FluentValidation;
using Cibc.Core.Validators.Trades;

namespace Cibc.Core.Infrastracture.Autofac
{
    public class ValuationAutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(typeof(IFileTradeProvider<>).Assembly).AsClosedTypesOf(typeof(IFileTradeProvider<>));
            builder.RegisterType<CsvMarketDataProvider<MarketDataItem>>().As<IMarketDataProvider<MarketDataItem>>();

            builder.RegisterAssemblyTypes(typeof(IValuationModel).Assembly).AssignableTo<IValuationModel>().AsSelf().SingleInstance();

            builder.RegisterType<PolymorphicTradeValidator>().As<IValidator<Trade>>();
            
            builder.Register(c =>
            {
                var cc = c.Resolve<IComponentContext>();

                return new Func<TradeType, IValuationModel>((tradeType) =>
                {
                   
                    return tradeType switch
                    {
                        TradeType.Stock => cc.Resolve<StockValuationModel>(),
                        TradeType.FXForward => cc.Resolve<FxForwardValuationModel>(),
                        TradeType.FxOption => cc.Resolve<FxOptionValuationModel>(),
                        TradeType.IRSwap => cc.Resolve<IRSwapValuationModel>(),
                        _=> throw new ArgumentException("Unknown trade type , No valiation model mappped to this type.")
                    };
                });
            });

            builder.RegisterType<ValuationOrchestrator>().AsImplementedInterfaces();
            builder.RegisterType<StandardMktDataCache>().SingleInstance();

        }
    }
}
