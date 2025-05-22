namespace SharpCache
{
    public class CacheItem
    {
        public CacheItem()
        {
            var utcNow = DateTime.UtcNow;
            LastAccessedAt = utcNow;
            CreatedAt = utcNow;
        }

        public CacheItem(object value, DateTime? absoluteExpiration = null, TimeSpan? slidingExpiration = null)
        {
            Value = value;
            AbsoluteExpiration = absoluteExpiration;
            SlidingExpiration = slidingExpiration;

            var utcNow = DateTime.UtcNow;
            LastAccessedAt = utcNow;
            CreatedAt = utcNow;

        }

        private DateTime _lastAccessedAt;
        private readonly Lock _lock = new();

        public object Value { get; init; } = null!;
        public DateTime CreatedAt { get; init; }

        public DateTime LastAccessedAt
        {
            get { lock (_lock) return _lastAccessedAt; }
            private set { lock (_lock) _lastAccessedAt = value; }
        }
        public DateTime? AbsoluteExpiration { get; set; }

        public TimeSpan? SlidingExpiration { get; set; }

        public bool IsExpired
        {
            get
            {
                var now = DateTime.UtcNow;
                lock (_lock)
                {
                    if (AbsoluteExpiration.HasValue && now >= AbsoluteExpiration.Value)
                        return true;

                    if (SlidingExpiration.HasValue && now - _lastAccessedAt >= SlidingExpiration.Value)
                        return true;

                    return false;
                }
            }
        }

        public void UpdateLastAccess()
        {
            lock (_lock)
            {
                _lastAccessedAt = DateTime.UtcNow;
            }
        }
    }
}
