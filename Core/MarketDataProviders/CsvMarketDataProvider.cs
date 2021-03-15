using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using System.Globalization;
using Domain.MarketData;
using CsvHelper.Configuration;

namespace Cibc.Core
{
    public class CsvMarketDataProvider<T> : IMarketDataProvider<T> where T: MarketDataItem
    {
        public CsvMarketDataProvider(string filePath)
        {
            FilePath = filePath ?? throw new ArgumentNullException("filePath must not be null");
            if (!File.Exists(FilePath))
                throw new FileNotFoundException($"File path doesn't exist{FilePath}");
            if (!FilePath.EndsWith(".csv"))
            {
                throw new ArgumentException($"Can only handle csv files{FilePath}");
            }
        }
        public FileFormat MarketDataSource => FileFormat.CsvFile;

        public string FilePath { get; private set; }
        public async IAsyncEnumerable<T> LoadMarketDataAsync()
        {
            using(var reader = new StreamReader(FilePath))
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = false,
                    Delimiter = ","
                   
                };

                using (var csv = new CsvReader(reader, config))
                {
                    
                    await foreach (var record in csv.GetRecordsAsync<T>())
                    {
                        yield return record;
                    }
                }
            }
        }
    }
}
