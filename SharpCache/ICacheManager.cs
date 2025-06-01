namespace SharpCache
{
    public interface ICacheManager
    {
        CacheItem Add(string key, object value, TimeSpan? slidingExpiration = null, DateTime? absoluteExpiration = null);
        object? Get(string key);
        T? Get<T>(string key);
        bool TryGet(string key, out object? value);
        bool TryGet<T>(string key, out T? value);
        void Remove(string key);
        bool TryRemove(string key);
        void CleanupExpiredItems();
        void Clear();
        bool IsEmpty();
        CacheItem? GetCacheItem(string key);

        Task<CacheItem> AddAsync(string key, object value, TimeSpan? slidingExpiration = null, DateTime? absoluteExpiration = null);
        Task<object?> GetAsync(string key);
        Task<T?> GetAsync<T>(string key);
        Task<(bool Success, T? Value)> TryGetAsync<T>(string key);
        Task<(bool Success, object? Value)> TryGetAsync(string key);
        Task RemoveAsync(string key);
        Task<bool> TryRemoveAsync(string key);
        Task CleanupExpiredItemsAsync();
        Task ClearAsync();
        Task<bool> IsEmptyAsync();
        Task<CacheItem?> GetCacheItemAsync(string key);
    }

}
