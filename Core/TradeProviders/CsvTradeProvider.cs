using Domain.TradeTypes;
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

namespace Cibc.Core.TradeProviders
{
    public class CsvFileTradeProvider : IFileTradeProvider
    {
        private readonly ILogger<CsvFileTradeProvider> _logger;

        public CsvFileTradeProvider(ILogger<CsvFileTradeProvider> logger){
            _logger = logger;
        }
        public FileFormat FileFormat => FileFormat.CsvFile;

        public IObservable<T> GetTradeStream<T>(string filePath, CancellationToken cancellationToken= default) where T : Trade
        {

            return Observable.Create<T>(async obs =>
               {
                   if (filePath == null)
                       obs.OnError(new ArgumentNullException("filePath must not be null"));

                   if (!File.Exists(filePath))
                       obs.OnError(new FileNotFoundException($"File path doesn't exist :  {filePath}"));

                   if (!filePath.EndsWith(".csv"))
                   {
                       obs.OnError(new ArgumentException($"Can only handle csv files. file name : {filePath}"));

                   }
                   using (var stream = new StreamReader(filePath))
                   using (var csvReader = new CsvReader(stream, CultureInfo.InvariantCulture))
                   {
                       await foreach (var record in csvReader.GetRecordsAsync<T>(cancellationToken))
                       {
                           obs.OnNext(record);
                       }
                   }
                   return Disposable.Create(() => _logger.LogInformation($"Disposing treade stream , completed reading file {filePath}") );
               });
        }
    }

    public interface IFileTradeProvider
    {
        // loading sync will be faster, but the idea  to use AsyncEnumerable to show that I understand the concept. In real scenarios (Network rather than local machine disk) using async may be more efficient if the network is slow.   
        // This is a pull model and in real applications normally a push model is much better, 
        IObservable<TTrade> GetTradeStream<TTrade>(string filePath, CancellationToken cancellationToken = default) where TTrade : Trade;

        FileFormat FileFormat { get;  }

    }

}
