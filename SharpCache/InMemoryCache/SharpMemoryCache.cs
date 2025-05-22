using System.Collections.Concurrent;

namespace SharpCache.InMemoryCache
{

    public class SharpMemoryCache : ISharpCache, IDisposable
    {
        private readonly ConcurrentDictionary<string, CacheItem> _cache;
        private readonly SemaphoreSlim _cleanupLock = new(1, 1);
        private readonly Timer _cleanupTimer;
        private readonly TimeSpan _cleanupInterval;

        public SharpMemoryCache()
            : this(TimeSpan.FromMinutes(5))
        {
        }

        public SharpMemoryCache(TimeSpan cleanupInterval)
        {
            _cache = new ConcurrentDictionary<string, CacheItem>();
            _cleanupInterval = cleanupInterval;
            _cleanupTimer = new Timer(CleanupCallback, null, _cleanupInterval, _cleanupInterval);
        }

        // Synchronous methods
        public CacheItem Add(string key, object value, TimeSpan? slidingExpiration = null, DateTime? absoluteExpiration = null)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(value);

            var now = DateTime.UtcNow;
            var cacheItem = new CacheItem
            {
                Value = value,
                CreatedAt = now,
                SlidingExpiration = slidingExpiration,
                AbsoluteExpiration = absoluteExpiration,
            };

            _cache[key] = cacheItem;

            return cacheItem;
        }

        public object? Get(string key)
        {
            ArgumentNullException.ThrowIfNull(key);

            if (TryGet(key, out var value))
            {
                return value;
            }

            return null;
        }

        public bool TryGet(string key, out object? value)
        {
            ArgumentNullException.ThrowIfNull(key);

            value = null;

            if (_cache.TryGetValue(key, out var cacheItem))
            {
                if (cacheItem.IsExpired)
                {
                    TryRemove(key);
                    return false;
                }

                cacheItem.UpdateLastAccess();
                value = cacheItem.Value;
                return true;
            }

            return false;
        }

        public void Remove(string key)
        {
            ArgumentNullException.ThrowIfNull(key);

            _cache.TryRemove(key, out _);
        }

        public bool TryRemove(string key)
        {
            ArgumentNullException.ThrowIfNull(key);

            return _cache.TryRemove(key, out _);
        }

        public void CleanupExpiredItems()
        {
            foreach (var key in _cache.Keys)
            {
                if (_cache.TryGetValue(key, out var cacheItem) && cacheItem.IsExpired)
                {
                    TryRemove(key);
                }
            }
        }

        public void Clear()
        {
            _cache.Clear();
        }

        public bool IsEmpty()
        {
            return _cache.IsEmpty;
        }

        public CacheItem? GetCacheItem(string key)
        {
            ArgumentNullException.ThrowIfNull(key);

            if (_cache.TryGetValue(key, out var cacheItem))
            {
                if (cacheItem.IsExpired)
                {
                    TryRemove(key);
                    return null;
                }

                cacheItem.UpdateLastAccess();
                return cacheItem;
            }

            return null;
        }

        // Asynchronous methods
        public Task<CacheItem> AddAsync(string key, object value, TimeSpan? slidingExpiration = null, DateTime? absoluteExpiration = null)
        {
            return Task.FromResult(Add(key, value, slidingExpiration, absoluteExpiration));
        }

        public Task<object?> GetAsync(string key)
        {
            return Task.FromResult(Get(key));
        }

        public Task<(bool Success, object? Value)> TryGetAsync(string key)
        {
            bool success = TryGet(key, out var value);
            return Task.FromResult((success, value));
        }

        public Task RemoveAsync(string key)
        {
            Remove(key);
            return Task.CompletedTask;
        }

        public Task<bool> TryRemoveAsync(string key)
        {
            return Task.FromResult(TryRemove(key));
        }

        public async Task CleanupExpiredItemsAsync()
        {
            await _cleanupLock.WaitAsync();
            try
            {
                CleanupExpiredItems();
            }
            finally
            {
                _cleanupLock.Release();
            }
        }

        public Task ClearAsync()
        {
            Clear();
            return Task.CompletedTask;
        }

        public Task<bool> IsEmptyAsync()
        {
            return Task.FromResult(IsEmpty());
        }

        public Task<CacheItem?> GetCacheItemAsync(string key)
        {
            return Task.FromResult(GetCacheItem(key));
        }

        private void CleanupCallback(object? state)
        {
            // Fire and forget
            _ = CleanupExpiredItemsAsync();
        }

        // Dispose pattern to clean up timer resources
        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _cleanupTimer?.Dispose();
                    _cleanupLock?.Dispose();
                }

                _disposed = true;
            }
        }

        ~SharpMemoryCache()
        {
            Dispose(false);
        }
    }
}
