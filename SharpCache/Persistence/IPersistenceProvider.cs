namespace SharpCache.Persistence
{
    public interface IPersistenceProvider : IDisposable, IAsyncDisposable
    {
        public void Save(string key, CacheItem item);
        public Task SaveAsync(string key, CacheItem item);
        public CacheItem? Load(string key);
        public bool TryLoad(string key, out CacheItem? item);
        public Task<CacheItem?> LoadAsync(string key);
        public Task<(bool Success, CacheItem? Item)> TryLoadAsync(string key);
        public void Remove(string key);
        public Task RemoveAsync(string key);
        public IEnumerable<(string Key, CacheItem Item)> LoadAll();
        public Task<IEnumerable<(string Key, CacheItem Item)>> LoadAllAsync();
    }
}
