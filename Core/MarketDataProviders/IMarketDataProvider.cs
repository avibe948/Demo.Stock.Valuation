using System.Collections.Generic;
using Domain.MarketData;

namespace Cibc.Core
{
    public interface IMarketDataProvider<T> where T : MarketDataItem
    {
        IAsyncEnumerable<T> LoadMarketDataAsync();
        FileFormat MarketDataSource { get;  }
    }
}
