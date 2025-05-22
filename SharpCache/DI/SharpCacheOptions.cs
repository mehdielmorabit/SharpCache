namespace SharpCache.DI
{
    public class SharpCacheOptions
    {
        /// <summary>
        /// Gets or sets whether in-memory cache is enabled
        /// </summary>
        public bool IsInMemoryEnabled { get; private set; }

        /// <summary>
        /// Gets or sets the cleanup interval for memory cache
        /// </summary>
        public TimeSpan CleanupInterval { get; private set; } = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Gets or sets the persistence provider
        /// </summary>
        public IPersistenceProviderOptions? PersistenceProviderOptions { get; private set; }

        public Type? PersistenceProviderType { get; private set; } = null;

        /// <summary>
        /// Configures the cache to use in-memory storage
        /// </summary>
        /// <param name="cleanupInterval">Optional cleanup interval</param>
        /// <returns>The options instance for chaining</returns>
        public SharpCacheOptions UseInMemory(TimeSpan? cleanupInterval = null)
        {
            IsInMemoryEnabled = true;

            if (cleanupInterval.HasValue)
            {
                CleanupInterval = cleanupInterval.Value;
            }

            return this;
        }

        /// <summary>
        /// Configures persistence using the specified provider
        /// </summary>
        /// <typeparam name="TProvider">The persistence provider type</typeparam>
        /// <param name="configureProvider">Action to configure the provider</param>
        /// <returns>The options instance for chaining</returns>
        public SharpCacheOptions WithPersistence<TProvider>(Action<TProvider> configureProvider)
            where TProvider : IPersistenceProviderOptions, new()
        {
            var provider = new TProvider();
            configureProvider(provider);
            PersistenceProviderOptions = provider;

            return this;
        }


    }
}
