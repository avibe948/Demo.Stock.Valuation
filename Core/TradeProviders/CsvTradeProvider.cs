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
                   using (var csvReader = new CsvReader(stream, CultureInfo.InvariantCulture))
                   {
                       await foreach (var record in csvReader.GetRecordsAsync<T>(cancellationToken))
                       {
                           obs.OnNext(record);
                       }
                   }
                   return Disposable.Create(() => _logger.LogInformation($"Disposing read stream , completed reading file {FilePath}") );
               });
        }
    }

}
