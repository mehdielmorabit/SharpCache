using Microsoft.Extensions.DependencyInjection;
using SharpCache.InMemoryCache;
using SharpCache.No_Op;
using SharpCache.Persistence;

namespace SharpCache.DI
{
    public static class SharpCacheServiceCollectionExtensions
    {
        public static IServiceCollection AddSharpCache(
            this IServiceCollection services,
            Action<SharpCacheOptions>? configureOptions = null)
        {
            var options = new SharpCacheOptions();
            configureOptions?.Invoke(options);

            if (options.IsInMemoryEnabled)
            {
                services.AddSingleton<SharpMemoryCache>();
            }
            else
            {
                services.AddSingleton<ISharpCache, NoOpCache>();
            }

            if (options.PersistenceProviderType != null && options.PersistenceProviderOptions != null)
            {
                services.AddSingleton(typeof(IPersistenceProvider), sp =>
                {
                    return (IPersistenceProvider)ActivatorUtilities.CreateInstance(
                        sp,
                        options.PersistenceProviderType!,
                        options.PersistenceProviderOptions
                    );
                });
            }
            else
            {
                services.AddSingleton<IPersistenceProvider, NoOpPersistenceProvider>();
            }
            
            services.AddSingleton<ICacheManager>(sp =>
            {
                var memoryCache = sp.GetRequiredService<ISharpCache>();
                var provider = sp.GetRequiredService<IPersistenceProvider>();
                return new CacheManager(memoryCache, provider);
            });

            return services;
        }
    }

}