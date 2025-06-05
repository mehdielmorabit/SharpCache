using System.Collections.Concurrent;
using System.Reflection;

namespace SharpCache.Infrastructure.Keys
{
    public static class CacheKey
    {
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _cacheKeyProps =
            new ConcurrentDictionary<Type, PropertyInfo[]>();

        public static string From<T>(T instance, bool hash = true)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            var type = typeof(T);
            var props = _cacheKeyProps.GetOrAdd(type, t => GetCacheKeyProperties(t));

            object[] segments;

            if (props.Length > 0)
            {
                segments = props
                   .Select(p => p.GetValue(instance))
                   .Where(v => v != null)
                   .Cast<object>()
                   .ToArray();
            }
            else
            {
                // Use the "Id" column
                var idProp = type.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
                    ?? throw new InvalidOperationException($"Type {type.Name} has no [CacheKey] properties or 'Id' property.");

                var idValue = idProp.GetValue(instance);
                if (idValue == null || (idValue is string stringValue && string.IsNullOrEmpty(stringValue)))
                    throw new InvalidOperationException("Cache key segment cannot be null pr empty string.");

                segments = new[] { idValue };
            }

            var builder = new CacheKeyBuilder().WithType<T>();

            foreach (var segment in segments)
            {
                builder.WithValue(segment.ToString());
            }

            return hash ? builder.WithHashing().Build() : builder.Build();
        }
        public static string FromSegments<T>(object[] segments, bool hash = true)
        {
            if (segments == null || segments.Length == 0)
                throw new ArgumentException("You must provide at least one segment.", nameof(segments));

            var expectedProps = DescribeExpectedSegments<T>();

            if (expectedProps.Length != segments.Length)
            {
                throw new ArgumentException(
                    $"Expected {expectedProps.Length} segments for type {typeof(T).Name}, but got {segments.Length}. " +
                    $"Expected segments: {string.Join(", ", expectedProps)}",
                    nameof(segments));
            }

            var builder = new CacheKeyBuilder().WithType<T>();

            foreach (var segment in segments)
            {
                if (segment == null)
                    throw new ArgumentException("Cache key segments cannot contain null values.", nameof(segments));

                builder.WithValue(segment.ToString()!);
            }

            return hash ? builder.WithHashing().Build() : builder.Build();
        }

        private static string[] DescribeExpectedSegments<T>()
        {
            var props = typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => new
                {
                    p.Name,
                    Attribute = p.GetCustomAttribute<CacheKeyAttribute>()
                })
                .Where(x => x.Attribute != null)
                .OrderBy(x => x.Attribute!.Order)
                .ThenBy(x => x.Name)
                .Select((x, i) => $"{i}: {x.Name}")
                .ToArray();

            if(props.Length == 0)
            {
                //check if there is an "Id" property
                var idProp = typeof(T).GetProperty("Id", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (idProp != null)
                {
                    return new[] { "0: Id" };
                }
                throw new InvalidOperationException($"Type {typeof(T).Name} has no [CacheKey] properties or 'Id' property.");
            }
            return props;
        }

        private static PropertyInfo[] GetCacheKeyProperties(Type type)
        {
            var propsWithAttr = type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => new
                {
                    Property = p,
                    Attribute = p.GetCustomAttribute<CacheKeyAttribute>()
                })
                .Where(x => x.Attribute != null)
                .ToArray();

            if (propsWithAttr.Length == 0)
                return Array.Empty<PropertyInfo>();

            // If Order is set, sort by Order, otherwise, preserve declaration order
            return propsWithAttr
                .OrderBy(x => x.Attribute?.Order ?? int.MaxValue)
                .ThenBy(x => x.Property.MetadataToken) 
                .Select(x => x.Property)
                .ToArray();
        }
    }
}
