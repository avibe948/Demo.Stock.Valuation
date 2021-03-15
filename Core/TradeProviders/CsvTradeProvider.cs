using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using System.Globalization;
using System.Reactive.Linq;
using System.Threading;
using System.Reactive.Disposables;
using Microsoft.Extensions.Logging;
using CsvHelper.Configuration;
using Domain.Trade;
using CsvHelper.TypeConversion;

namespace Cibc.Core.TradeProviders
{
    public class CsvFileTradeProvider<T> : IFileTradeProvider<T> where T:Trade
    {
        private readonly ILogger<CsvFileTradeProvider<T>> _logger;

        private readonly string _filePath;

        public CsvFileTradeProvider(string filePath, ILogger<CsvFileTradeProvider<T>> logger){
            _filePath = filePath;
            _logger = logger;
        }
        public FileFormat FileFormat => FileFormat.CsvFile;

        public string FilePath => _filePath;

        public IObservable<T> GetTradeStream(CancellationToken cancellationToken= default)
        {

            return Observable.Create<T>(async obs =>
               {
                   if (FilePath == null)
                       obs.OnError(new ArgumentNullException("filePath must not be null"));

                   if (!File.Exists(FilePath))
                       obs.OnError(new FileNotFoundException($"File path doesn't exist :  {FilePath}"));

                   if (!FilePath.EndsWith(".csv"))
                   {
                       obs.OnError(new ArgumentException($"Can only handle csv files. file name : {FilePath}"));

                   }
                   using (var stream = new StreamReader(FilePath))
                   {
                       _logger.LogInformation($"reading file named {FilePath}");
                       var config = new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ","};                  

                       using (var csv = new CsvReader(stream, config))
                       {
                           RegisterClassMaps(csv.Context);

                           await foreach (var record in csv.GetRecordsAsync<T>(cancellationToken))
                           {
                               obs.OnNext(record);
                           }
                           obs.OnCompleted();
                       }
                   }
                   return Disposable.Create(() => _logger.LogInformation($"Completed reading file {FilePath} , disposing stream") );
               });
        }

        private void RegisterClassMaps(CsvContext context)
        {
            context.RegisterClassMap<StockDefinitionMap>();
            context.RegisterClassMap<FxOptionTradeDefinitionMap>();
            context.RegisterClassMap<IRSwapDefinitionMap>();
            context.RegisterClassMap<FxForwardTradeDefinitionMap>();
        }


        public class StockDefinitionMap : ClassMap<StockTrade>
        { 
             public StockDefinitionMap()
             {
                Map(m => m.Underlying).Name("Ticker");
                Map(m => m.TradeId).Name("TradeId").Index(1);
                Map(m => m.Counterparty).Name("Counterparty").Index(2);
                Map(m => m.Quantity).Name("Quantity").Index(3);
                Map(m => m.TradeType).Name("TradeType").Index(4);
            }
        }
        public class FxOptionTradeDefinitionMap : ClassMap<FxOptionTrade>
        {
            public FxOptionTradeDefinitionMap()
            {
                Map(m => m.Underlying).Name("CurrencyPair").Index(0);
                Map(m => m.Strike).Name("Strike").Index(1);
                Map(m => m.OptionType).Name("OptionType").Index(2);
                Map(m => m.ExpiryDate).Name("Quantity").Index(3);
                Map(m => m.TradeId).Name("TradeId").Index(4);
                Map(m => m.Counterparty).Name("Counterparty").Index(5);
                Map(m => m.Quantity).Name("Quantity").Index(6);
                Map(m => m.TradeType).Name("TradeType").Index(7);

            }
        }
        public class FxForwardTradeDefinitionMap : ClassMap<FxForwardTrade>
        {
            public FxForwardTradeDefinitionMap()
            {
                Map(m => m.Underlying).Name("CurrencyPair").Index(0);
                Map(m => m.Strike).Name("Strike").Index(1);
                Map(m => m.ExpiryDate).Name("ExpiryDate").TypeConverter<DateTimeConverter>().Index(2);
                Map(m => m.TradeId).Name("TradeId").Index(3);
                Map(m => m.Counterparty).Name("Counterparty").Index(4);
                Map(m => m.Quantity).Name("Quantity").Index(5);
                Map(m => m.TradeType).Name("TradeType").Index(6);
            }
        }
        public class IRSwapDefinitionMap : ClassMap<IRSwapTrade>
        {
            public IRSwapDefinitionMap()
            {
                Map(m => m.Underlying).Name("Underlying");
                Map(m => m.FixedRate).Name("FixedRate");
                Map(m => m.TradeId).Name("TradeId");
                Map(m => m.Quantity).Name("Quantity");
                Map(m => m.TradeType).Name("TradeType");
                Map(m => m.Counterparty).Name("Counterparty");
            }
        }
    
	}
}


