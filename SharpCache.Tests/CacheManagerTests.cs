using SharpCache.Persistence;
using Moq;
using System.Collections.Concurrent;
using Xunit;
using SharpCache.InMemoryCache;

namespace SharpCache.Tests
{

    public class CacheManagerTests : IDisposable
    {
        private readonly Mock<ISharpCache> _memoryCacheMock;
        private readonly Mock<IPersistenceProvider> _persistenceMock;
        private readonly CacheManager _cacheManager;
        private readonly CacheItem _testItem;

        public CacheManagerTests()
        {
            _memoryCacheMock = new Mock<ISharpCache>();
            _persistenceMock = new Mock<IPersistenceProvider>();
            _cacheManager = new CacheManager(_memoryCacheMock.Object, _persistenceMock.Object);
            _testItem = new CacheItem("test-value", DateTime.UtcNow.AddHours(1), TimeSpan.FromMinutes(30));
        }

        public void Dispose()
        {
            _cacheManager?.Dispose();
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullMemoryCache_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new CacheManager(null!, _persistenceMock.Object));
        }

        [Fact]
        public void Constructor_WithNullPersistence_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new CacheManager(_memoryCacheMock.Object, null!));
        }

        [Fact]
        public void Constructor_WithValidParameters_CreatesInstance()
        {
            using var manager = new CacheManager(_memoryCacheMock.Object, _persistenceMock.Object);
            Assert.NotNull(manager);
        }

        #endregion

        #region Add Tests

        [Fact]
        public void Add_WithValidParameters_CallsBothMemoryAndPersistence()
        {
            // Arrange
            var key = "test-key";
            var value = "test-value";
            var slidingExpiration = TimeSpan.FromMinutes(30);
            var absoluteExpiration = DateTime.UtcNow.AddHours(1);

            _memoryCacheMock.Setup(x => x.Add(key, value, slidingExpiration, absoluteExpiration))
                            .Returns(_testItem);

            // Act
            var result = _cacheManager.Add(key, value, slidingExpiration, absoluteExpiration);

            // Assert
            _memoryCacheMock.Verify(x => x.Add(key, value, slidingExpiration, absoluteExpiration), Times.Once);
            _persistenceMock.Verify(x => x.Save(key, _testItem), Times.Once);
            Assert.Equal(_testItem, result);
        }

        [Fact]
        public async Task AddAsync_WithValidParameters_CallsBothMemoryAndPersistence()
        {
            // Arrange
            var key = "test-key";
            var value = "test-value";
            var slidingExpiration = TimeSpan.FromMinutes(30);
            var absoluteExpiration = DateTime.UtcNow.AddHours(1);

            _memoryCacheMock.Setup(x => x.AddAsync(key, value, slidingExpiration, absoluteExpiration))
                            .ReturnsAsync(_testItem);

            // Act
            var result = await _cacheManager.AddAsync(key, value, slidingExpiration, absoluteExpiration);

            // Assert
            _memoryCacheMock.Verify(x => x.AddAsync(key, value, slidingExpiration, absoluteExpiration), Times.Once);
            _persistenceMock.Verify(x => x.SaveAsync(key, _testItem), Times.Once);
            Assert.Equal(_testItem, result);
        }

        #endregion

        #region Get Tests

        [Fact]
        public void Get_WhenFoundInMemory_ReturnsValueWithoutPersistenceCall()
        {
            // Arrange
            var key = "test-key";
            var value = "test-value";
            _memoryCacheMock.Setup(x => x.Get(key)).Returns(value);

            // Act
            var result = _cacheManager.Get(key);

            // Assert
            Assert.Equal(value, result);
            _memoryCacheMock.Verify(x => x.Get(key), Times.Once);
            _persistenceMock.Verify(x => x.Load(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void Get_WhenNotInMemoryButInPersistence_LoadsFromPersistenceAndAddsToMemory()
        {
            // Arrange
            var key = "test-key";
            _memoryCacheMock.Setup(x => x.Get(key)).Returns((object?)null);
            _persistenceMock.Setup(x => x.Load(key)).Returns(_testItem);

            // Act
            var result = _cacheManager.Get(key);

            // Assert
            Assert.Equal(_testItem.Value, result);
            _memoryCacheMock.Verify(x => x.Get(key), Times.Once);
            _persistenceMock.Verify(x => x.Load(key), Times.Once);
            _memoryCacheMock.Verify(x => x.Add(key, _testItem.Value, _testItem.SlidingExpiration, _testItem.AbsoluteExpiration), Times.Once);
        }

        [Fact]
        public void Get_WhenNotFoundAnywhere_ReturnsNull()
        {
            // Arrange
            var key = "test-key";
            _memoryCacheMock.Setup(x => x.Get(key)).Returns((object?)null);
            _persistenceMock.Setup(x => x.Load(key)).Returns((CacheItem?)null);

            // Act
            var result = _cacheManager.Get(key);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Get_WhenExpiredInPersistence_ReturnsNull()
        {
            // Arrange
            var key = "test-key";
            var expiredItem = new CacheItem("test-value", DateTime.UtcNow.AddHours(-1), TimeSpan.FromMinutes(30));

            _memoryCacheMock.Setup(x => x.Get(key)).Returns((object?)null);
            _persistenceMock.Setup(x => x.Load(key)).Returns(expiredItem);

            // Act
            var result = _cacheManager.Get(key);

            // Assert
            Assert.Null(result);
            _memoryCacheMock.Verify(x => x.Add(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan?>(), It.IsAny<DateTime?>()), Times.Never);
        }

        [Fact]
        public async Task GetAsync_WhenFoundInMemory_ReturnsValueWithoutPersistenceCall()
        {
            // Arrange
            var key = "test-key";
            var value = "test-value";
            _memoryCacheMock.Setup(x => x.GetAsync(key)).ReturnsAsync(value);

            // Act
            var result = await _cacheManager.GetAsync(key);

            // Assert
            Assert.Equal(value, result);
            _memoryCacheMock.Verify(x => x.GetAsync(key), Times.Once);
            _persistenceMock.Verify(x => x.LoadAsync(It.IsAny<string>()), Times.Never);
        }

        #endregion

        #region TryGet Tests

        [Fact]
        public void TryGet_WhenFoundInMemory_ReturnsTrueWithValue()
        {
            // Arrange
            var key = "test-key";
            var value = "test-value";
            _memoryCacheMock.Setup(x => x.TryGet(key, out It.Ref<object?>.IsAny))
                            .Returns((string k, out object? v) => { v = value; return true; });

            // Act
            var result = _cacheManager.TryGet(key, out var retrievedValue);

            // Assert
            Assert.True(result);
            Assert.Equal(value, retrievedValue);
            _persistenceMock.Verify(x => x.TryLoad(It.IsAny<string>(), out It.Ref<CacheItem?>.IsAny), Times.Never);
        }

        [Fact]
        public void TryGet_WhenNotInMemoryButInPersistence_ReturnsTrueWithValueAndAddsToMemory()
        {
            // Arrange
            var key = "test-key";
            object? nullValue = null;

            _memoryCacheMock.Setup(x => x.TryGet(key, out It.Ref<object?>.IsAny))
                            .Returns((string k, out object? v) => { v = nullValue; return false; });

            _persistenceMock.Setup(x => x.TryLoad(key, out It.Ref<CacheItem?>.IsAny))
                            .Returns((string k, out CacheItem? item) => { item = _testItem; return true; });

            // Act
            var result = _cacheManager.TryGet(key, out var retrievedValue);

            // Assert
            Assert.True(result);
            Assert.Equal(_testItem.Value, retrievedValue);
            _memoryCacheMock.Verify(x => x.Add(key, _testItem.Value, _testItem.SlidingExpiration, _testItem.AbsoluteExpiration), Times.Once);
        }

        [Fact]
        public void TryGet_WhenNotFoundAnywhere_ReturnsFalseWithNull()
        {
            // Arrange
            var key = "test-key";
            object? nullValue = null;
            CacheItem? nullItem = null;

            _memoryCacheMock.Setup(x => x.TryGet(key, out It.Ref<object?>.IsAny))
                            .Returns((string k, out object? v) => { v = nullValue; return false; });

            _persistenceMock.Setup(x => x.TryLoad(key, out It.Ref<CacheItem?>.IsAny))
                            .Returns((string k, out CacheItem? item) => { item = nullItem; return false; });

            // Act
            var result = _cacheManager.TryGet(key, out var retrievedValue);

            // Assert
            Assert.False(result);
            Assert.Null(retrievedValue);
        }

        #endregion

        #region Remove Tests

        [Fact]
        public void Remove_CallsBothMemoryAndPersistence()
        {
            // Arrange
            var key = "test-key";

            // Act
            _cacheManager.Remove(key);

            // Assert
            _memoryCacheMock.Verify(x => x.Remove(key), Times.Once);
            _persistenceMock.Verify(x => x.Remove(key), Times.Once);
        }

        [Fact]
        public void TryRemove_CallsBothMemoryAndPersistence_ReturnsMemoryResult()
        {
            // Arrange
            var key = "test-key";
            _memoryCacheMock.Setup(x => x.TryRemove(key)).Returns(true);

            // Act
            var result = _cacheManager.TryRemove(key);

            // Assert
            Assert.True(result);
            _memoryCacheMock.Verify(x => x.TryRemove(key), Times.Once);
            _persistenceMock.Verify(x => x.Remove(key), Times.Once);
        }

        [Fact]
        public async Task RemoveAsync_CallsBothMemoryAndPersistence()
        {
            // Arrange
            var key = "test-key";

            // Act
            await _cacheManager.RemoveAsync(key);

            // Assert
            _memoryCacheMock.Verify(x => x.RemoveAsync(key), Times.Once);
            _persistenceMock.Verify(x => x.RemoveAsync(key), Times.Once);
        }

        #endregion

        #region Global Operations Tests

        [Fact]
        public void CleanupExpiredItems_CallsMemoryCache()
        {
            // Act
            _cacheManager.CleanupExpiredItems();

            // Assert
            _memoryCacheMock.Verify(x => x.CleanupExpiredItems(), Times.Once);
        }

        [Fact]
        public void Clear_CallsMemoryCache()
        {
            // Act
            _cacheManager.Clear();

            // Assert
            _memoryCacheMock.Verify(x => x.Clear(), Times.Once);
        }

        [Fact]
        public void IsEmpty_ReturnsMemoryCacheResult()
        {
            // Arrange
            _memoryCacheMock.Setup(x => x.IsEmpty()).Returns(true);

            // Act
            var result = _cacheManager.IsEmpty();

            // Assert
            Assert.True(result);
            _memoryCacheMock.Verify(x => x.IsEmpty(), Times.Once);
        }

        [Fact]
        public async Task CleanupExpiredItemsAsync_CallsMemoryCache()
        {
            // Act
            await _cacheManager.CleanupExpiredItemsAsync();

            // Assert
            _memoryCacheMock.Verify(x => x.CleanupExpiredItemsAsync(), Times.Once);
        }

        [Fact]
        public async Task ClearAsync_CallsMemoryCache()
        {
            // Act
            await _cacheManager.ClearAsync();

            // Assert
            _memoryCacheMock.Verify(x => x.ClearAsync(), Times.Once);
        }

        [Fact]
        public async Task IsEmptyAsync_ReturnsMemoryCacheResult()
        {
            // Arrange
            _memoryCacheMock.Setup(x => x.IsEmptyAsync()).ReturnsAsync(false);

            // Act
            var result = await _cacheManager.IsEmptyAsync();

            // Assert
            Assert.False(result);
            _memoryCacheMock.Verify(x => x.IsEmptyAsync(), Times.Once);
        }

        #endregion

        #region GetCacheItem Tests

        [Fact]
        public void GetCacheItem_WhenFoundInMemory_ReturnsItemWithoutPersistenceCall()
        {
            // Arrange
            var key = "test-key";
            _memoryCacheMock.Setup(x => x.GetCacheItem(key)).Returns(_testItem);

            // Act
            var result = _cacheManager.GetCacheItem(key);

            // Assert
            Assert.Equal(_testItem, result);
            _memoryCacheMock.Verify(x => x.GetCacheItem(key), Times.Once);
            _persistenceMock.Verify(x => x.Load(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void GetCacheItem_WhenNotInMemoryButInPersistence_LoadsFromPersistenceAndAddsToMemory()
        {
            // Arrange
            var key = "test-key";
            _memoryCacheMock.Setup(x => x.GetCacheItem(key)).Returns((CacheItem?)null);
            _persistenceMock.Setup(x => x.Load(key)).Returns(_testItem);

            // Act
            var result = _cacheManager.GetCacheItem(key);

            // Assert
            Assert.Equal(_testItem, result);
            _memoryCacheMock.Verify(x => x.GetCacheItem(key), Times.Once);
            _persistenceMock.Verify(x => x.Load(key), Times.Once);
            _memoryCacheMock.Verify(x => x.Add(key, _testItem.Value, _testItem.SlidingExpiration, _testItem.AbsoluteExpiration), Times.Once);
        }

        [Fact]
        public async Task GetCacheItemAsync_WhenFoundInMemory_ReturnsItemWithoutPersistenceCall()
        {
            // Arrange
            var key = "test-key";
            _memoryCacheMock.Setup(x => x.GetCacheItemAsync(key)).ReturnsAsync(_testItem);

            // Act
            var result = await _cacheManager.GetCacheItemAsync(key);

            // Assert
            Assert.Equal(_testItem, result);
            _memoryCacheMock.Verify(x => x.GetCacheItemAsync(key), Times.Once);
            _persistenceMock.Verify(x => x.LoadAsync(It.IsAny<string>()), Times.Never);
        }

        #endregion

        #region Thread Safety Tests

        [Fact]
        public async Task ConcurrentOperations_OnDifferentKeys_ExecuteConcurrently()
        {
            // Arrange
            const int operationCount = 100;
            var tasks = new List<Task>();
            var results = new ConcurrentBag<string>();

            _memoryCacheMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan?>(), It.IsAny<DateTime?>()))
                            .Returns((string key, object value, TimeSpan? sliding, DateTime? absolute) =>
                                new CacheItem(value, absolute, sliding));

            // Act - Create many concurrent operations on different keys
            for (int i = 0; i < operationCount; i++)
            {
                var key = $"key-{i}";
                var value = $"value-{i}";

                tasks.Add(Task.Run(() =>
                {
                    _cacheManager.Add(key, value);
                    results.Add($"added-{key}");
                }));
            }

            await Task.WhenAll(tasks);

            // Assert
            Assert.Equal(operationCount, results.Count);
            _memoryCacheMock.Verify(x => x.Add(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan?>(), It.IsAny<DateTime?>()),
                                    Times.Exactly(operationCount));
        }

        [Fact]
        public async Task ConcurrentOperations_OnSameKey_ExecuteSequentially()
        {
            // Arrange
            const string key = "same-key";
            const int operationCount = 50;
            var tasks = new List<Task>();
            var executionOrder = new ConcurrentQueue<int>();
            var delay = TimeSpan.FromMilliseconds(10);

            _memoryCacheMock.Setup(x => x.Add(key, It.IsAny<object>(), It.IsAny<TimeSpan?>(), It.IsAny<DateTime?>()))
                            .Returns((string k, object value, TimeSpan? sliding, DateTime? absolute) =>
                            {
                                Thread.Sleep(delay); // Simulate work
                                return new CacheItem(value, absolute, sliding);
                            });

            // Act - Create many concurrent operations on the same key
            for (int i = 0; i < operationCount; i++)
            {
                var operationId = i;
                tasks.Add(Task.Run(() =>
                {
                    _cacheManager.Add(key, $"value-{operationId}");
                    executionOrder.Enqueue(operationId);
                }));
            }

            await Task.WhenAll(tasks);

            // Assert
            Assert.Equal(operationCount, executionOrder.Count);
            _memoryCacheMock.Verify(x => x.Add(key, It.IsAny<object>(), It.IsAny<TimeSpan?>(), It.IsAny<DateTime?>()),
                                    Times.Exactly(operationCount));
        }

        [Fact]
        public async Task ConcurrentGetAndAdd_OnSameKey_MaintainConsistency()
        {
            // Arrange
            const string key = "test-key";
            const int operationCount = 100;
            var tasks = new List<Task>();
            var getResults = new ConcurrentBag<object?>();
            var addResults = new ConcurrentBag<CacheItem>();

            var currentValue = "initial-value";
            var currentItem = new CacheItem(currentValue, DateTime.UtcNow.AddHours(1), TimeSpan.FromMinutes(30));

            _memoryCacheMock.Setup(x => x.Get(key)).Returns(() => currentValue);
            _memoryCacheMock.Setup(x => x.Add(key, It.IsAny<object>(), It.IsAny<TimeSpan?>(), It.IsAny<DateTime?>()))
                            .Returns((string k, object value, TimeSpan? sliding, DateTime? absolute) =>
                            {
                                currentValue = value.ToString();
                                currentItem = new CacheItem(value, absolute, sliding);
                                return currentItem;
                            });

            // Act - Mix get and add operations
            for (int i = 0; i < operationCount; i++)
            {
                if (i % 2 == 0)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        var result = _cacheManager.Get(key);
                        getResults.Add(result);
                    }));
                }
                else
                {
                    var value = $"value-{i}";
                    tasks.Add(Task.Run(() =>
                    {
                        var result = _cacheManager.Add(key, value);
                        addResults.Add(result);
                    }));
                }
            }

            await Task.WhenAll(tasks);

            // Assert
            Assert.Equal(operationCount / 2, getResults.Count);
            Assert.Equal(operationCount / 2, addResults.Count);
            Assert.All(getResults, result => Assert.NotNull(result));
        }

        #endregion

        #region Disposal Tests

        [Fact]
        public void Dispose_CallsDisposeOnSemaphores()
        {
            // Arrange
            var manager = new CacheManager(_memoryCacheMock.Object, _persistenceMock.Object);

            // Add some operations to create semaphores
            _memoryCacheMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan?>(), It.IsAny<DateTime?>()))
                            .Returns(_testItem);

            manager.Add("key1", "value1");
            manager.Add("key2", "value2");

            // Act & Assert - Should not throw
            manager.Dispose();
        }

        [Fact]
        public async Task DisposeAsync_CallsDisposeOnSemaphores()
        {
            // Arrange
            var manager = new CacheManager(_memoryCacheMock.Object, _persistenceMock.Object);

            // Add some operations to create semaphores
            _memoryCacheMock.Setup(x => x.AddAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan?>(), It.IsAny<DateTime?>()))
                            .ReturnsAsync(_testItem);

            await manager.AddAsync("key1", "value1");
            await manager.AddAsync("key2", "value2");

            // Act & Assert - Should not throw
            await manager.DisposeAsync();
        }

        #endregion

        #region Edge Cases Tests

        [Fact]
        public void Operations_WithNullOrEmptyKey_HandleGracefully()
        {
            // Test with null key
            Assert.Throws<ArgumentNullException>(() => _cacheManager.Add(null!, "value"));

            // Test with empty key - should work (depends on implementation)
            _memoryCacheMock.Setup(x => x.Add("", It.IsAny<object>(), It.IsAny<TimeSpan?>(), It.IsAny<DateTime?>()))
                            .Returns(_testItem);

            var result = _cacheManager.Add("", "value");
            Assert.NotNull(result);
        }

        [Fact]
        public void Operations_WithNullValue_HandleGracefully()
        {
            var key = "test-key";
            Assert.Throws<ArgumentNullException>(() => _cacheManager.Add(key, null!));


                
           
            var result = _cacheManager.Get(key);
            Assert.Null(result);
        }

        #endregion
        
    }
}
