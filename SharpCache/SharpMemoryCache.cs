using SharpCache;
using System.Collections.Concurrent;

namespace SharpCache
{
    public class SharpMemoryCache : ISharpMemoryCache
    {
        private ConcurrentDictionary<string, CacheItem> Cache { get; init; } = new ConcurrentDictionary<string, CacheItem>();

        public void Add(string key, object value, TimeSpan? slidingExpiration = null, DateTime? absoluteExpiration = null)
        {
            var cacheItem = new CacheItem
            {
                Value = value,
                CreatedAt = DateTime.UtcNow,
                LastAccessedAt = DateTime.UtcNow,
                SlidingExpiration = slidingExpiration,
                AbsoluteExpiration = absoluteExpiration

            };

            Cache[key] = cacheItem;
        }
        public object? Get(string key)
        {
            if (Cache.TryGetValue(key, out var item))
            {
                if (!item.IsExpired)
                {
                    item.UpdateLastAccess();
                    return item.Value;
                }
            }

            return null;
        }

        public bool TryGet(string key, out object? value)
        {
            if(Cache.TryGetValue(key, out var cacheResult) && !cacheResult.IsExpired)
            {
                cacheResult.UpdateLastAccess();
                value = cacheResult.Value;
                return true;
            } 
            
            value = null;
            return false;
        }

        public void Remove(string key)
        {
            Cache.Remove(key, out _);
        }

        public bool TryRemove(string key)
        {
            return Cache.TryRemove(key, out _);
        }

        public void CleanupExpiredItems()
        {

            var expiredKeys = Cache
                .Where(kvp => kvp.Value.IsExpired)
                .Select(kvp => kvp.Key)
                .ToList();
            foreach (var key in expiredKeys)
            {
                Cache.TryRemove(key, out _);
            }

        }

        public void Clear()
        {
            Cache.Clear();
        }

        public bool IsEmpty()
        {
            return Cache.IsEmpty;
        }

        public CacheItem? GetCacheItem(string key)
        {
            Cache.TryGetValue(key, out var item);
            return item?.IsExpired == false ? item : null;
        }
    }
}
