using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.MarketData
{
    public class MarketDataItem
    {
      
        public MarketDataItem( string key, decimal price)
        {
            Key = key;
            Price = price;
        }
        public string Key { get; private set; }
        public decimal Price { get; private set; }
    }
}
