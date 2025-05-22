using SharpCache.Persistence;

namespace SharpCache.No_Op
{
    public class NoOpPersistenceProvider : IPersistenceProvider
    {
        public void Save(string key, CacheItem item) { }

        public Task SaveAsync(string key, CacheItem item) => Task.CompletedTask;

        public CacheItem? Load(string key) => null;

        public bool TryLoad(string key, out CacheItem? item)
        {
            item = null;
            return false;
        }

        public Task<CacheItem?> LoadAsync(string key) => Task.FromResult<CacheItem?>(null);

        public Task<(bool Success, CacheItem? Item)> TryLoadAsync(string key)
            => Task.FromResult((false, (CacheItem?)null));

        public void Remove(string key) { }

        public Task RemoveAsync(string key) => Task.CompletedTask;

        public IEnumerable<(string Key, CacheItem Item)> LoadAll() => [];

        public Task<IEnumerable<(string Key, CacheItem Item)>> LoadAllAsync()
            => Task.FromResult(Enumerable.Empty<(string, CacheItem)>());

        public void Dispose() { }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

}
