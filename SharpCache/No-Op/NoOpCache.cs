namespace SharpCache.No_Op
{
    public class NoOpCache : ISharpCache
    {
        public CacheItem Add(string key, object value, TimeSpan? slidingExpiration = null, DateTime? absoluteExpiration = null)
            => new() { Value = value, CreatedAt = DateTime.UtcNow };

        public object? Get(string key) => null;

        public bool TryGet(string key, out object? value)
        {
            value = null;
            return false;
        }

        public void Remove(string key) { }

        public bool TryRemove(string key) => false;

        public void CleanupExpiredItems() { }

        public void Clear() { }

        public bool IsEmpty() => true;

        public CacheItem? GetCacheItem(string key) => null;

        public Task<CacheItem> AddAsync(string key, object value, TimeSpan? slidingExpiration = null, DateTime? absoluteExpiration = null)
            => Task.FromResult(Add(key, value, slidingExpiration, absoluteExpiration));

        public Task<object?> GetAsync(string key) => Task.FromResult<object?>(null);

        public Task<(bool Success, object? Value)> TryGetAsync(string key)
            => Task.FromResult((false, (object?)null));

        public Task RemoveAsync(string key) => Task.CompletedTask;

        public Task<bool> TryRemoveAsync(string key) => Task.FromResult(false);

        public Task CleanupExpiredItemsAsync() => Task.CompletedTask;

        public Task ClearAsync() => Task.CompletedTask;

        public Task<bool> IsEmptyAsync() => Task.FromResult(true);

        public Task<CacheItem?> GetCacheItemAsync(string key)
            => Task.FromResult<CacheItem?>(null);
    }

}
