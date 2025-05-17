using System.Text;

namespace SharpCache.Infrastructure.Keys
{
    public class CacheKeyBuilder
    {
        private readonly StringBuilder _builder = new();

        public static CacheKeyBuilder Create() => new();

        public CacheKeyBuilder WithSegment(string prefix)
        {
            _builder.Append(prefix);
            return this;
        }

        public CacheKeyBuilder WithType<T>()
        {
            _builder.Append(typeof(T).Name);
            return this;
        }

        public CacheKeyBuilder WithValue(object? value)
        {
            if (value != null)
            {
                _builder.Append(":").Append(value.ToString());
            }
            return this;
        }

        public string Build() => _builder.ToString();
    }
}
