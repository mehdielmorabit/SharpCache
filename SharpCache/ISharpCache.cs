namespace SharpCache
{
    public interface ISharpCache
    {
        CacheItem Add(string key, object value, TimeSpan? slidingExpiration = null, DateTime? absoluteExpiration = null);
        object? Get(string key);
        bool TryGet(string key, out object? value);
        void Remove(string key);
        bool TryRemove(string key);
        void CleanupExpiredItems();
        void Clear();
        bool IsEmpty();
        CacheItem? GetCacheItem(string key);

        Task<CacheItem> AddAsync(string key, object value, TimeSpan? slidingExpiration = null, DateTime? absoluteExpiration = null);
        Task<object?> GetAsync(string key);
        Task<(bool Success, object? Value)> TryGetAsync(string key);
        Task RemoveAsync(string key);
        Task<bool> TryRemoveAsync(string key);
        Task CleanupExpiredItemsAsync();
        Task ClearAsync();
        Task<bool> IsEmptyAsync();
        Task<CacheItem?> GetCacheItemAsync(string key);

    }
}
