using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Domain.MarketData
{
    public class StandardMktDataCache: MarketDataCache<string, decimal?> {}


    public abstract class MarketDataCache<TKey,TValue>
    {
        Dictionary<TKey, TValue> _cache;
        public MarketDataCache()
        {
            _cache = new Dictionary<TKey, TValue>();
        }

        public bool TryAdd(TKey key , TValue value)
        {
            return _cache.TryAdd(key, value);
        }
        public bool TryGetValue(TKey key, out TValue value)
        {
            return _cache.TryGetValue(key, out value);
        }

        public bool ContainsKey(TKey key)
        {
            return _cache.ContainsKey(key);
        }
    }


}
