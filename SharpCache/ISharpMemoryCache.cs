using SharpCache;

namespace SharpCache
{
    public interface ISharpMemoryCache
    {
        public void Add(string key, object value, TimeSpan? slidingExpiration = null, DateTime? absoluteExpiration = null);
        public object? Get(string key);
        public bool TryGet(string key, out object? value);
        public void Remove(string key);
        public bool TryRemove(string key);
        public void CleanupExpiredItems();
        public void Clear();
        public bool IsEmpty();
        public CacheItem? GetCacheItem(string key);

    }
}
