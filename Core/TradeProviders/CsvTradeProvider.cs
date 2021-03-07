using Domain.TradeTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using System.Globalization;

namespace Core.TradeProviders
{
    public class CsvFileTradeProvider<T> : ITradeProvider<T> where T:Trade
    {
        public CsvFileTradeProvider(string filePath)
        {
            FilePath = filePath ?? throw new ArgumentNullException("filePath must not be null");
            if (!File.Exists(FilePath))
                throw new FileNotFoundException($"File path doesn't exist{FilePath}");
            if (!FilePath.EndsWith(".csv"))
            {
                throw new ArgumentException($"Can only handle csv files{FilePath}");

            }

        }
        public FileSource TradeSource => FileSource.CsvFile;

        public string FilePath { get; private set; }
        public async IAsyncEnumerable<T> LoadTradesAsync()
        {
            using(var stream = new StreamReader(FilePath))
            using (var csvReader = new CsvReader(stream, CultureInfo.InvariantCulture))
            {
                await foreach (var record in csvReader.GetRecordsAsync<T>())
                {
                    yield return record;
                }
            }
        }
    }

    public interface ITradeProvider<T> where T : Trade
    {
        // loading sync will be faster, but the idea  to use AsyncEnumerable to show that I understand the concept. In real scenarios (Network rather than local machine disk) using async may be more efficient if the network is slow.   
        // This is a pull model and in real applications normally a push model is much better, 
        IAsyncEnumerable<T> LoadTradesAsync(); 

        FileSource TradeSource { get;  }
    }
}
