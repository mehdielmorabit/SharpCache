namespace SharpCache.Infrastructure.Keys
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class CacheKeyAttribute : Attribute
    {
        public int? Order { get; set; }
    }
}
