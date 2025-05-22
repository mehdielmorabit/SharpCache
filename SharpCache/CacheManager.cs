using SharpCache.Persistence;
using System.Collections.Concurrent;

namespace SharpCache
{
    public class CacheManager : ICacheManager, IDisposable, IAsyncDisposable
    {
        private readonly ISharpCache _memoryCache;
        private readonly IPersistenceProvider _persistence;
        private readonly SemaphoreSlim _semaphore;
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _keySemaphores;

        public CacheManager(ISharpCache memoryCache, IPersistenceProvider persistence)
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _persistence = persistence ?? throw new ArgumentNullException(nameof(persistence));
            _semaphore = new SemaphoreSlim(1, 1);
            _keySemaphores = new ConcurrentDictionary<string, SemaphoreSlim>();
        }

        private SemaphoreSlim GetKeySemaphore(string key)
        {
            return _keySemaphores.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        }

        public CacheItem Add(string key, object value, TimeSpan? slidingExpiration = null, DateTime? absoluteExpiration = null)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(value);

            var keySemaphore = GetKeySemaphore(key);
            keySemaphore.Wait();
            try
            {
                var item = _memoryCache.Add(key, value, slidingExpiration, absoluteExpiration);
                _persistence.Save(key, item);
                return item;
            }
            finally
            {
                keySemaphore.Release();
            }
        }

        public object? Get(string key)
        {
            var keySemaphore = GetKeySemaphore(key);
            keySemaphore.Wait();
            try
            {
                var value = _memoryCache.Get(key);
                if (value is not null)
                    return value;

                var item = _persistence.Load(key);
                if (item is not null && !item.IsExpired)
                {
                    _memoryCache.Add(key, item.Value, item.SlidingExpiration, item.AbsoluteExpiration);
                    return item.Value;
                }

                return null;
            }
            finally
            {
                keySemaphore.Release();
            }
        }

        public bool TryGet(string key, out object? value)
        {
            var keySemaphore = GetKeySemaphore(key);
            keySemaphore.Wait();
            try
            {
                if (_memoryCache.TryGet(key, out value))
                    return true;

                if (_persistence.TryLoad(key, out var item) && item is not null && !item.IsExpired)
                {
                    value = item.Value;
                    _memoryCache.Add(key, item.Value, item.SlidingExpiration, item.AbsoluteExpiration);
                    return true;
                }

                value = null;
                return false;
            }
            finally
            {
                keySemaphore.Release();
            }
        }

        public void Remove(string key)
        {
            var keySemaphore = GetKeySemaphore(key);
            keySemaphore.Wait();
            try
            {
                _memoryCache.Remove(key);
                _persistence.Remove(key);
            }
            finally
            {
                keySemaphore.Release();
                
                if (_keySemaphores.TryRemove(key, out var removedSemaphore))
                {
                    removedSemaphore.Dispose();
                }
            }
        }

        public bool TryRemove(string key)
        {
            var keySemaphore = GetKeySemaphore(key);
            keySemaphore.Wait();
            try
            {
                var removedMemory = _memoryCache.TryRemove(key);
                _persistence.Remove(key);
                return removedMemory;
            }
            finally
            {
                keySemaphore.Release();

                if (_keySemaphores.TryRemove(key, out var removedSemaphore))
                {
                    removedSemaphore.Dispose();
                }
            }
        }

        public void CleanupExpiredItems()
        {
            _semaphore.Wait();
            try
            {
                _memoryCache.CleanupExpiredItems();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void Clear()
        {
            _semaphore.Wait();
            try
            {
                _memoryCache.Clear();
                foreach (var kvp in _keySemaphores)
                {
                    kvp.Value.Dispose();
                }
                _keySemaphores.Clear();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public bool IsEmpty()
        {
            return _memoryCache.IsEmpty();
        }

        public CacheItem? GetCacheItem(string key)
        {
            var keySemaphore = GetKeySemaphore(key);
            keySemaphore.Wait();
            try
            {
                var item = _memoryCache.GetCacheItem(key);
                if (item is not null && !item.IsExpired)
                    return item;

                var persistedItem = _persistence.Load(key);
                if (persistedItem is not null && !persistedItem.IsExpired)
                {
                    _memoryCache.Add(key, persistedItem.Value, persistedItem.SlidingExpiration, persistedItem.AbsoluteExpiration);
                    return persistedItem;
                }

                return null;
            }
            finally
            {
                keySemaphore.Release();
            }
        }

        public async Task<CacheItem> AddAsync(string key, object value, TimeSpan? slidingExpiration = null, DateTime? absoluteExpiration = null)
        {
            var keySemaphore = GetKeySemaphore(key);
            await keySemaphore.WaitAsync();
            try
            {
                var item = await _memoryCache.AddAsync(key, value, slidingExpiration, absoluteExpiration);
                await _persistence.SaveAsync(key, item);
                return item;
            }
            finally
            {
                keySemaphore.Release();
            }
        }

        public async Task<object?> GetAsync(string key)
        {
            var keySemaphore = GetKeySemaphore(key);
            await keySemaphore.WaitAsync();
            try
            {
                var value = await _memoryCache.GetAsync(key);
                if (value is not null)
                    return value;

                var item = await _persistence.LoadAsync(key);
                if (item is not null && !item.IsExpired)
                {
                    await _memoryCache.AddAsync(key, item.Value, item.SlidingExpiration, item.AbsoluteExpiration);
                    return item.Value;
                }

                return null;
            }
            finally
            {
                keySemaphore.Release();
            }
        }

        public async Task<(bool Success, object? Value)> TryGetAsync(string key)
        {
            var keySemaphore = GetKeySemaphore(key);
            await keySemaphore.WaitAsync();
            try
            {
                var (success, value) = await _memoryCache.TryGetAsync(key);
                if (success)
                    return (true, value);

                var result = await _persistence.TryLoadAsync(key);
                if (result.Success && result.Item is not null && !result.Item.IsExpired)
                {
                    await _memoryCache.AddAsync(key, result.Item.Value, result.Item.SlidingExpiration, result.Item.AbsoluteExpiration);
                    return (true, result.Item.Value);
                }

                return (false, null);
            }
            finally
            {
                keySemaphore.Release();
            }
        }

        public async Task RemoveAsync(string key)
        {
            var keySemaphore = GetKeySemaphore(key);
            await keySemaphore.WaitAsync();
            try
            {
                await _memoryCache.RemoveAsync(key);
                await _persistence.RemoveAsync(key);
            }
            finally
            {
                keySemaphore.Release();
                if (_keySemaphores.TryRemove(key, out var removedSemaphore))
                {
                    removedSemaphore.Dispose();
                }
            }
        }

        public async Task<bool> TryRemoveAsync(string key)
        {
            var keySemaphore = GetKeySemaphore(key);
            await keySemaphore.WaitAsync();
            try
            {
                var removed = await _memoryCache.TryRemoveAsync(key);
                await _persistence.RemoveAsync(key);
                return removed;
            }
            finally
            {
                keySemaphore.Release();
                if (_keySemaphores.TryRemove(key, out var removedSemaphore))
                {
                    removedSemaphore.Dispose();
                }
            }
        }

        public async Task CleanupExpiredItemsAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                await _memoryCache.CleanupExpiredItemsAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task ClearAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                await _memoryCache.ClearAsync();
                foreach (var kvp in _keySemaphores)
                {
                    kvp.Value.Dispose();
                }
                _keySemaphores.Clear();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<bool> IsEmptyAsync()
        {
            return await _memoryCache.IsEmptyAsync();
        }

        public async Task<CacheItem?> GetCacheItemAsync(string key)
        {
            var keySemaphore = GetKeySemaphore(key);
            await keySemaphore.WaitAsync();
            try
            {
                var item = await _memoryCache.GetCacheItemAsync(key);
                if (item is not null && !item.IsExpired)
                    return item;

                var persisted = await _persistence.LoadAsync(key);
                if (persisted is not null && !persisted.IsExpired)
                {
                    await _memoryCache.AddAsync(key, persisted.Value, persisted.SlidingExpiration, persisted.AbsoluteExpiration);
                    return persisted;
                }

                return null;
            }
            finally
            {
                keySemaphore.Release();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore();
            Dispose(false);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _semaphore?.Dispose();
                foreach (var kvp in _keySemaphores)
                {
                    kvp.Value.Dispose();
                }
                _keySemaphores.Clear();
            }
        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            if (_semaphore is not null)
            {
                await _semaphore.WaitAsync();
                _semaphore.Dispose();
            }

            var disposeTasks = new List<ValueTask>();
            foreach (var kvp in _keySemaphores)
            {
                await kvp.Value.WaitAsync();
                kvp.Value.Dispose();
            }
            _keySemaphores.Clear();
        }
    }
}