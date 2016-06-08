using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

namespace Fr8.Infrastructure.Utilities
{
    /// <summary>
    /// A threadsafe, typed memory cache
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MemoryCache<T> : IDisposable
    {
        private readonly MemoryCache _innerCache = new MemoryCache(Guid.NewGuid().ToString());
        private readonly double _intExpireInSeconds;

        public MemoryCache(double intExpireInSeconds)
        {
            _intExpireInSeconds = intExpireInSeconds;
        }

        public MemoryCache(TimeSpan expireAfter)
            : this(expireAfter.TotalSeconds)
        {
        }

        public void Append(T value)
        {
            var key = Guid.NewGuid().ToString();
            _innerCache.Set(key, value, new CacheItemPolicy { AbsoluteExpiration = DateTime.UtcNow.AddSeconds(_intExpireInSeconds) });
        }

        public IEnumerable<T> GetAndRemove(Func<T, bool> predicate)
        {
            var itemsToReturnAndRemove = _innerCache.Where(kvp => predicate((T)kvp.Value)).ToList();
            foreach (var itemToReturnAndRemove in itemsToReturnAndRemove)
            {
                _innerCache.Remove(itemToReturnAndRemove.Key);
            }
            return itemsToReturnAndRemove.Select(val => (T)val.Value);
        }

        public void Dispose()
        {
            _innerCache.Dispose();
        }
    }
}
