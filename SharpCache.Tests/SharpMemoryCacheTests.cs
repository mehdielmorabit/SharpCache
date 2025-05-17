using System;
using SharpCache;
using SharpCache.Infrastructure.Keys;
using Xunit;

namespace SharpCache.Tests
{
    public class SharpMemoryCacheTests
    {
        [Fact]
        public void Add_And_Get_Item_Using_CacheKeyBuilder()
        {
            // Arrange
            var cache = new SharpMemoryCache();
            var key = CacheKeyBuilder.Create()
                .WithSegment("User")
                .WithType<int>()
                .WithValue(42)
                .Build();

            var value = "TestUser";

            // Act
            cache.Add(key, value);
            var result = cache.Get(key);

            // Assert
            Assert.Equal(value, result);
        }

        [Fact]
        public void TryGet_Returns_True_And_Value_When_Key_Exists()
        {
            // Arrange
            var cache = new SharpMemoryCache();
            var key = CacheKeyBuilder.Create()
                .WithSegment("Order")
                .WithType<Guid>()
                .WithValue(Guid.NewGuid())
                .Build();

            var value = 12345;
            cache.Add(key, value);

            // Act
            var found = cache.TryGet(key, out var result);

            // Assert
            Assert.True(found);
            Assert.Equal(value, result);
        }

        [Fact]
        public void Remove_Item_Using_CacheKeyBuilder_Key()
        {
            // Arrange
            var cache = new SharpMemoryCache();
            var key = CacheKeyBuilder.Create()
                .WithSegment("Session")
                .WithType<string>()
                .WithValue("abc123")
                .Build();

            cache.Add(key, "SessionData");

            // Act
            cache.Remove(key);
            var result = cache.Get(key);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetCacheItem_Returns_Null_For_Expired_Item()
        {
            // Arrange
            var cache = new SharpMemoryCache();
            var key = CacheKeyBuilder.Create()
                .WithSegment("Token")
                .WithType<string>()
                .WithValue("expired")
                .Build();

            cache.Add(key, "TokenValue", absoluteExpiration: DateTime.UtcNow.AddSeconds(-1));

            // Act
            var item = cache.GetCacheItem(key);

            // Assert
            Assert.Null(item);
        }
    }
}
