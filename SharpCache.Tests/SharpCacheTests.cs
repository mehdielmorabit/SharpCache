using SharpCache.InMemoryCache;

namespace SharpCache.Tests
{
    public class SharpCacheTests
    {
        [Fact]
        public void Test_Add()
        {
            // Arrange
            var cacheObject = new List<int> { 1, 2, 3 };
            var cache = new SharpMemoryCache();
            var isEmptyInitially = cache.IsEmpty();

            // Act
            cache.Add("testKey", cacheObject);

            // Assert
            Assert.True(isEmptyInitially);
            Assert.False(cache.IsEmpty());
        }

        [Fact]
        public void Test_Get()
        {
            // Arrange
            var cacheObject = new List<int> { 1, 2, 3 };
            var cache = new SharpMemoryCache();
            cache.Add("testKey", cacheObject);
            // Act
            var result = cache.Get("testKey");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(cacheObject, result);
        }

        [Fact]
        public void Test_Remove()
        {
            // Arrange
            var cacheObject = new List<int> { 1, 2, 3 };
            var cache = new SharpMemoryCache();
            cache.Add("testKey", cacheObject);

            // Act
            cache.Remove("testKey");

            // Assert
            Assert.False(cache.TryGet("testKey", out var result));
            Assert.Null(result);
        }

        [Fact]
        public void Test_CleanupExpiredItems()
        {
            // Arrange
            var cacheObject = new List<int> { 1, 2, 3 };
            var cache = new SharpMemoryCache();
            var absoluteExpiration = DateTime.UtcNow.AddSeconds(5);
            cache.Add("testKey", cacheObject, absoluteExpiration: absoluteExpiration);
            cache.TryGet("testKey", out var beforeCleanup);

            //wait 5 second
            Thread.Sleep(5000);

            // Act
            cache.CleanupExpiredItems();     

            // Assert
            Assert.False(cache.TryGet("testKey", out var result));
            Assert.Null(result);
            Assert.NotNull(beforeCleanup);
        }

        [Fact]
        public void Test_TryGet()
        {
            // Arrange
            var cacheObject = new List<int> { 1, 2, 3 };
            var cache = new SharpMemoryCache();
            cache.Add("testKey", cacheObject);
            // Act
            var result = cache.TryGet("testKey", out var value);
            // Assert
            Assert.True(result);
            Assert.NotNull(value);
            Assert.Equal(cacheObject, value);
        }

        [Fact]
        public void Test_TryRemove()
        {
            // Arrange
            var cacheObject = new List<int> { 1, 2, 3 };
            var cache = new SharpMemoryCache();
            cache.Add("testKey", cacheObject);
            // Act
            var result = cache.TryRemove("testKey");
            // Assert
            Assert.True(result);
            Assert.False(cache.TryGet("testKey", out var value));
            Assert.Null(value);
        }


    }
}
