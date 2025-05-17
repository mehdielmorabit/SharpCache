using System.Text;
using System.Security.Cryptography;

namespace SharpCache.Infrastructure.Keys
{
    public class CacheKeyBuilder
    {
        private readonly StringBuilder _builder = new();
        private bool _useHashing = false;
        private string? _salt = Environment.GetEnvironmentVariable("SHARPCACHE_CACHE_KEY_SALT");
        private static bool _saltWarningShown = false;

        public static CacheKeyBuilder Create() => new();

        public CacheKeyBuilder()
        {
            CheckSalt();
        }

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


        /// <summary>
        /// Appends the string representation of the provided value to the cache key.
        /// Warning: Only use this for primitive types or objects with consistent ToString() implementations.
        /// For complex objects, use: WithSegment(YourCustomSerializeMethod(obj))
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public CacheKeyBuilder WithValue(object? value)
        {
            if (value != null)
            {
                _builder.Append(":").Append(value.ToString());
            }
            return this;
        }

        public CacheKeyBuilder WithHashing(bool useHashing = true)
        {
            _useHashing = useHashing;
            return this;
        }

        public string Build()
        {
            var rawKey = _builder.ToString();
            if (!_useHashing) return rawKey;

            var keyToHash = _salt is null ? rawKey : _salt + rawKey;
            return ComputeSHA256(keyToHash);
        }

        private static string ComputeSHA256(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hash);
        }

        private void CheckSalt()
        {
            if (_saltWarningShown || !string.IsNullOrEmpty(_salt))
                return;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[SharpCache] Warning: SHARPCACHE_CACHE_KEY_SALT environment variable is not set. Consider setting it for better security.");
            Console.ResetColor();
            _saltWarningShown = true;
        }
    }
}

