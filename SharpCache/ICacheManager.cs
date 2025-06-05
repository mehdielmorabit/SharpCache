namespace SharpCache
{
    public interface ICacheManager
    {
        CacheItem Add(string key, object value, TimeSpan? slidingExpiration = null, DateTime? absoluteExpiration = null);
        CacheItem Add<T>(T item, TimeSpan? slidingExpiration = null, DateTime? absoluteExpiration = null);
        object? Get(string key);
        T? Get<T>(string key);
        T? GetBySegments<T>(params object[] segments);
        bool TryGetBySegments<T>(out T? value, params object[] segments);
        bool TryGet(string key, out object? value);
        bool TryGet<T>(string key, out T? value);
        void Remove(string key);
        bool TryRemove(string key);
        void CleanupExpiredItems();
        void Clear();
        bool IsEmpty();
        CacheItem? GetCacheItem(string key);

        Task<CacheItem> AddAsync(string key, object value, TimeSpan? slidingExpiration = null, DateTime? absoluteExpiration = null);
        Task<CacheItem> AddAsync<T>(T item, TimeSpan? slidingExpiration = null, DateTime? absoluteExpiration = null);
        Task<object?> GetAsync(string key);
        Task<T?> GetAsync<T>(string key);
        Task<T?> GetBySegmentsAsync<T>(params object[] segments);
        Task<(bool Success, T? Value)> TryGetBySegmentsAsync<T>(params object[] segments);
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
