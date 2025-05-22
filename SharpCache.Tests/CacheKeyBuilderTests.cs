using SharpCache.Infrastructure.Keys;

namespace SharpCache.Tests
{
    public class CacheKeyBuilderTests
    {
        [Fact]
        public void Create_ReturnsNewInstance()
        {
            // Act
            var builder1 = CacheKeyBuilder.Create();
            var builder2 = CacheKeyBuilder.Create();

            // Assert
            Assert.NotNull(builder1);
            Assert.NotNull(builder2);
            Assert.NotSame(builder1, builder2);
        }

        [Fact]
        public void WithSegment_AppendsPrefix()
        {
            // Arrange
            var builder = CacheKeyBuilder.Create();

            // Act
            builder.WithSegment("prefix");
            var result = builder.Build();

            // Assert
            Assert.Equal("prefix", result);
        }

        [Fact]
        public void WithType_AppendsTypeName()
        {
            // Arrange
            var builder = CacheKeyBuilder.Create();

            // Act
            builder.WithType<int>();
            var result = builder.Build();

            // Assert
            Assert.Equal("Int32", result);
        }

        [Fact]
        public void WithValue_AppendsValueWithColon_WhenValueIsNotNull()
        {
            // Arrange
            var builder = CacheKeyBuilder.Create();

            // Act
            builder.WithValue(123);
            var result = builder.Build();

            // Assert
            Assert.Equal(":123", result);
        }

        [Fact]
        public void WithValue_DoesNotAppend_WhenValueIsNull()
        {
            // Arrange
            var builder = CacheKeyBuilder.Create();

            // Act
            builder.WithValue(null);
            var result = builder.Build();

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void Build_ReturnsCombinedKey()
        {
            // Arrange
            var builder = CacheKeyBuilder.Create();

            // Act
            builder.WithSegment("user")
                    .WithType<string>()
                    .WithValue(42);
            var result = builder.Build();

            // Assert
            Assert.Equal("userString:42", result);
        }
    }
    
}
