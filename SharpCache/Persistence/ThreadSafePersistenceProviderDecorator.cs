using SharpCache.Infrastructure.Concurrency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpCache.Persistence
{
    /// <summary>
    /// A thread-safe decorator for persistence providers.
    /// This class wraps any IPersistenceProvider implementation and ensures thread safety
    /// by using an AsyncReaderWriterLock for concurrent operations.
    /// </summary>
    public class ThreadSafePersistenceProviderDecorator : IPersistenceProvider
    {
        private readonly IPersistenceProvider _innerProvider;
        private readonly AsyncReaderWriterLock _lock = new AsyncReaderWriterLock();
        private bool _disposed = false;

        public ThreadSafePersistenceProviderDecorator(IPersistenceProvider innerProvider)
        {
            _innerProvider = innerProvider ?? throw new ArgumentNullException(nameof(innerProvider));
        }

        public void Save(string key, CacheItem item)
        {
            using (_lock.WriteLock())
            {
                ThrowIfDisposed();
                _innerProvider.Save(key, item);
            }
        }

        public async Task SaveAsync(string key, CacheItem item)
        {
            using (await _lock.WriteLockAsync())
            {
                ThrowIfDisposed();
                await _innerProvider.SaveAsync(key, item);
            }
        }

        public CacheItem? Load(string key)
        {
            using (_lock.ReadLock())
            {
                ThrowIfDisposed();
                return _innerProvider.Load(key);
            }
        }

        public bool TryLoad(string key, out CacheItem? item)
        {
            using (_lock.ReadLock())
            {
                ThrowIfDisposed();
                return _innerProvider.TryLoad(key, out item);
            }
        }

        public async Task<CacheItem?> LoadAsync(string key)
        {
            using (await _lock.ReadLockAsync())
            {
                ThrowIfDisposed();
                return await _innerProvider.LoadAsync(key);
            }
        }

        public async Task<(bool Success, CacheItem? Item)> TryLoadAsync(string key)
        {
            using (await _lock.ReadLockAsync())
            {
                ThrowIfDisposed();
                var item = await _innerProvider.LoadAsync(key);
                return (item != null, item);
            }
        }

        public void Remove(string key)
        {
            using (_lock.WriteLock())
            {
                ThrowIfDisposed();
                _innerProvider.Remove(key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            using (await _lock.WriteLockAsync())
            {
                ThrowIfDisposed();
                await _innerProvider.RemoveAsync(key);
            }
        }

        public IEnumerable<(string Key, CacheItem Item)> LoadAll()
        {
            using (_lock.ReadLock())
            {
                ThrowIfDisposed();
                return _innerProvider.LoadAll();
            }
        }

        public async Task<IEnumerable<(string Key, CacheItem Item)>> LoadAllAsync()
        {
            using (await _lock.ReadLockAsync())
            {
                ThrowIfDisposed();
                return await _innerProvider.LoadAllAsync();
            }
        }

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
                    _innerProvider.Dispose();
                    _lock.Dispose();
                }
                _disposed = true;
            }
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore();
            Dispose(false);
            GC.SuppressFinalize(this);
        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            if (!_disposed)
            {
                await _innerProvider.DisposeAsync();
                _lock.Dispose();
                _disposed = true;
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ThreadSafePersistenceProviderDecorator));
            }
        }
    }
}
