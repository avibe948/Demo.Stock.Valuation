using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.MarketData
{
    public class MarketDataItem
    {
        private MarketDataItem() { }
      
        public MarketDataItem(string key, decimal? price)
        {
            Key = key;
            Price = price;
        }

        [Name("key")]
        public string Key { get; private set; }
        [Name("price")]
        public decimal? Price { get; private set; }
    }
}
