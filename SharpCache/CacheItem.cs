namespace SharpCache
{
    public class CacheItem
    {
        public object? Value { get; init; }
        public DateTime Expiration { get; set; }
        public DateTime CreatedAt { get; init; }

        public DateTime LastAccessedAt { get; set; }

        public DateTime? AbsoluteExpiration { get; set; }

        public TimeSpan? SlidingExpiration { get; set; }

        public bool IsExpired => HasExpired();

        public bool HasExpired()
        {
            if (AbsoluteExpiration.HasValue && DateTime.UtcNow >= AbsoluteExpiration.Value)
                return true;

            if (SlidingExpiration.HasValue && DateTime.UtcNow - LastAccessedAt > SlidingExpiration.Value)
                return true;

            return false;
        }

        public void UpdateLastAccess()
        {
            LastAccessedAt = DateTime.UtcNow;
        }
    }
}
