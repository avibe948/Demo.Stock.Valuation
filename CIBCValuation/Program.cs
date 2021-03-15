using Autofac;
using Domain.Trade;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Autofac.Extensions.DependencyInjection;
using Cibc.Core.Infrastracture.Autofac;
using Cibc.Pricing.ValuationModels;
using Cibc.Core;
using System.IO;
using Domain.MarketData;
using System.Linq;
using Cibc.Core.TradeProviders;
using System.Collections.Generic;
using Cibc.Core.Validators.Trades;

namespace CIBC.Valuation
{
    class Program
    {
        static async Task Main(string[] args)
        {
            
            using (var host = CreateHostBuilder(args).Build())
            {
                    var services = host.Services;
                    var logger = services.GetService<ILoggerFactory>().CreateLogger<Program>();

                    logger.LogInformation("Calling CIBC Valuation console");

                try { 
                    var currentDir = Directory.GetCurrentDirectory();
                    var marketDataProvider = new CsvMarketDataProvider<MarketDataItem>(Path.Combine(currentDir, "Files", "MarketDataSources", "MarketData.csv"));
                    var loggerFactory = services.GetService<ILoggerFactory>();
                    var csvTradeFiles = "Stock,IRSwap,FxForward,FxOption".Split(',').Select(fileName => Path.Combine(currentDir, "Files", "TradeSources", $"{fileName}.csv")).ToArray(); ;
                    var csvStockLogger = loggerFactory.CreateLogger<CsvFileTradeProvider<StockTrade>>();
                    var csvIRSwapLogger = loggerFactory.CreateLogger<CsvFileTradeProvider<IRSwapTrade>>();
                    var csvFxForwardLogger = loggerFactory.CreateLogger<CsvFileTradeProvider<FxForwardTrade>>();
                    var csvFxOptionLogger = loggerFactory.CreateLogger<CsvFileTradeProvider<FxOptionTrade>>();

                    var tradeProviders = new List<IFileTradeProvider<Trade>>() { new CsvFileTradeProvider<StockTrade>(csvTradeFiles[0], csvStockLogger),
                                              new CsvFileTradeProvider<IRSwapTrade>(csvTradeFiles[1], csvIRSwapLogger),
                                              new CsvFileTradeProvider<FxForwardTrade>(csvTradeFiles[2], csvFxForwardLogger),
                                              new CsvFileTradeProvider<FxOptionTrade>(csvTradeFiles[3], csvFxOptionLogger) };

                    var resultFilePath = Path.Combine(currentDir, "Results.csv");
                    var valuationModelsCreator = Container.Resolve<Func<TradeType, IValuationModel>>();
                    var valuationsLogger = services.GetService<ILoggerFactory>().CreateLogger<ValuationOrchestrator>();
                    var tradeValidator = new PolymorphicTradeValidator(); // services.GetService<IValidator<Trade>>(); no time to solve not that important anyway given instructions; 

                    var valuations = new ValuationOrchestrator(marketDataProvider, tradeProviders, valuationModelsCreator, new StandardMktDataCache(), tradeValidator, valuationsLogger);

                    var completed = await valuations.RunValuations(resultFilePath);

                    while (!completed)
                    {
                    }

                    logger.LogInformation("All done! , click any key to close the window");
                    Console.ReadKey();
                }
                catch(Exception e)
                {
                    logger.LogError(e,"");
                }
            }
        }

        private static int IMarketDataProvider<T>()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Can improve this by scanning the Core lib with a Validators namespace. Using Autofac DI its easier by no time for plumbing to show how its done. 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static IHostBuilder CreateHostBuilder(string[] args) =>
              Host.CreateDefaultBuilder(args).ConfigureServices(_ => ConfigureServices())
                                              .ConfigureLogging(logging =>
                                              {
                                                  logging.ClearProviders();
                                                  logging.AddConsole();
                                              });

        static IContainer Container { get; set; }
        public static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            services.AddLogging();

            var builder = new ContainerBuilder();

            // Once you've registered everything in the ServiceCollection, call
            // Populate to bring those registrations into Autofac. This is
            // just like a foreach over the list of things in the collection
            // to add them to Autofac.
            builder.Populate(services);

            // Make your Autofac registrations. Order is important!
            // If you make them BEFORE you call Populate, then the
            // registrations in the ServiceCollection will override Autofac
            // registrations; if you make them AFTER Populate, the Autofac
            // registrations will override. You can make registrations
            // before or after Populate, however you choose.

            // Will scan the validators in the parent assembly of StockTradeValidator. 
            AssemblyScanner.FindValidatorsInAssemblyContaining<FluentValidatorFactory>().ForEach(x => builder.RegisterType(x.ValidatorType).As(x.InterfaceType).SingleInstance());
            builder.RegisterType<FluentValidatorFactory>().As<IValidatorFactory>().SingleInstance();
            builder.RegisterModule<ValuationAutofacModule>();
            // Creating a new AutofacServiceProvider makes the container
            // available to your app using the Microsoft IServiceProvider
            // interface so you can use those abstractions rather than
            // binding directly to Autofac.
            Container = builder.Build();

            return Container.Resolve<IServiceProvider>();
        }
    }
}
