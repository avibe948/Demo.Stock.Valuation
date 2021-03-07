using Autofac;
using Domain.TradeTypes;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Cibc.PricingValidators.Trades;
using System;
using System.Threading.Tasks;
using Autofac.Extensions.DependencyInjection;
using Core.Infrastracture.Autofac;
using Cibc.Pricing.ValuationModels;

namespace CIBC.Valuation
{ 
    class Program
    {
        static void Main(string[] args)
        {
            using (var host = CreateHostBuilder(args).Build())
            {

                var services = host.Services;
                var logger = services.GetService<ILoggerFactory>().CreateLogger<Program>();
                var valuationService = services.GetService<IValuationModel<StockTrade>>();
                
                logger.LogInformation("Starting CIBC Valuation console");                
                logger.LogInformation("All done!");
            }


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
            AssemblyScanner.FindValidatorsInAssemblyContaining<AutofacValidatorFactory>().ForEach(x => builder.RegisterType(x.ValidatorType).As(x.InterfaceType).SingleInstance());

            builder.RegisterType<AutofacValidatorFactory>().As<IValidatorFactory>().SingleInstance();

            builder.RegisterModule<ValuationAutofacModule>();
            // Creating a new AutofacServiceProvider makes the container
            // available to your app using the Microsoft IServiceProvider
            // interface so you can use those abstractions rather than
            // binding directly to Autofac.
            var container = builder.Build();
            
            var serviceProvider = new AutofacServiceProvider(container);

            return serviceProvider;
        }
    }
}
