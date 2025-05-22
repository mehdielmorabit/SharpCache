using SharpCache.InMemoryCache;
using System.Collections.Concurrent;

namespace SharpCache.Tests
{
    public class SharpMemoryCacheTests
    {
        [Fact]
        public void Add_Get_ShouldStoreAndRetrieveValue()
        {
            // Arrange
            var cache = new SharpMemoryCache();
            var key = "testKey";
            var expectedValue = "testValue";

            // Act
            cache.Add(key, expectedValue);
            var actualValue = cache.Get(key);

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void Get_WithNonExistentKey_ShouldReturnNull()
        {
            // Arrange
            var cache = new SharpMemoryCache();

            // Act
            var result = cache.Get("nonExistentKey");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void TryGet_WithExistingKey_ShouldReturnTrueAndValue()
        {
            // Arrange
            var cache = new SharpMemoryCache();
            var key = "testKey";
            var expectedValue = "testValue";
            cache.Add(key, expectedValue);

            // Act
            bool success = cache.TryGet(key, out var actualValue);

            // Assert
            Assert.True(success);
            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void TryGet_WithNonExistentKey_ShouldReturnFalseAndNull()
        {
            // Arrange
            var cache = new SharpMemoryCache();

            // Act
            bool success = cache.TryGet("nonExistentKey", out var value);

            // Assert
            Assert.False(success);
            Assert.Null(value);
        }

        [Fact]
        public void Remove_ShouldRemoveItemFromCache()
        {
            // Arrange
            var cache = new SharpMemoryCache();
            var key = "testKey";
            cache.Add(key, "testValue");

            // Act
            cache.Remove(key);
            var result = cache.Get(key);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void TryRemove_WithExistingKey_ShouldReturnTrueAndRemoveItem()
        {
            // Arrange
            var cache = new SharpMemoryCache();
            var key = "testKey";
            cache.Add(key, "testValue");

            // Act
            bool result = cache.TryRemove(key);
            var value = cache.Get(key);

            // Assert
            Assert.True(result);
            Assert.Null(value);
        }

        [Fact]
        public void TryRemove_WithNonExistentKey_ShouldReturnFalse()
        {
            // Arrange
            var cache = new SharpMemoryCache();

            // Act
            bool result = cache.TryRemove("nonExistentKey");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Clear_ShouldRemoveAllItems()
        {
            // Arrange
            var cache = new SharpMemoryCache();
            cache.Add("key1", "value1");
            cache.Add("key2", "value2");

            // Act
            cache.Clear();

            // Assert
            Assert.True(cache.IsEmpty());
            Assert.Null(cache.Get("key1"));
            Assert.Null(cache.Get("key2"));
        }

        [Fact]
        public void IsEmpty_WithItems_ShouldReturnFalse()
        {
            // Arrange
            var cache = new SharpMemoryCache();
            cache.Add("testKey", "testValue");

            // Act & Assert
            Assert.False(cache.IsEmpty());
        }

        [Fact]
        public void IsEmpty_WithNoItems_ShouldReturnTrue()
        {
            // Arrange
            var cache = new SharpMemoryCache();

            // Act & Assert
            Assert.True(cache.IsEmpty());
        }

        [Fact]
        public void GetCacheItem_ShouldReturnCacheItem()
        {
            // Arrange
            var cache = new SharpMemoryCache();
            var key = "testKey";
            var value = "testValue";
            cache.Add(key, value);

            // Act
            var cacheItem = cache.GetCacheItem(key);

            // Assert
            Assert.NotNull(cacheItem);
            Assert.Equal(value, cacheItem.Value);
        }

        [Fact]
        public void Get_WithExpiredAbsoluteExpiration_ShouldReturnNull()
        {
            // Arrange
            var cache = new SharpMemoryCache();
            var key = "testKey";
            cache.Add(key, "testValue", absoluteExpiration: DateTime.UtcNow.AddMilliseconds(-1));

            // Add a small delay to ensure expiration takes effect
            Thread.Sleep(10);

            // Act
            var result = cache.Get(key);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Get_WithExpiredSlidingExpiration_ShouldReturnNull()
        {
            // Arrange
            var cache = new SharpMemoryCache();
            var key = "testKey";
            cache.Add(key, "testValue", slidingExpiration: TimeSpan.FromMilliseconds(50));

            // Wait for sliding expiration to occur
            Thread.Sleep(100);

            // Act
            var result = cache.Get(key);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Get_WithSlidingExpiration_ShouldExtendExpiration()
        {
            // Arrange
            var cache = new SharpMemoryCache();
            var key = "testKey";
            var value = "testValue";
            cache.Add(key, value, slidingExpiration: TimeSpan.FromMilliseconds(100));

            // Access before expiration (which should extend the expiration)
            Thread.Sleep(50);
            cache.Get(key);

            // Wait for original expiration time, but item should still be valid
            Thread.Sleep(50);

            // Act
            var result = cache.Get(key);

            // Assert
            Assert.Equal(value, result);
        }

        [Fact]
        public void CleanupExpiredItems_ShouldRemoveExpiredItems()
        {
            // Arrange
            var cache = new SharpMemoryCache();
            cache.Add("key1", "value1", absoluteExpiration: DateTime.UtcNow.AddMilliseconds(-1));
            cache.Add("key2", "value2", absoluteExpiration: DateTime.UtcNow.AddHours(1));

            // Wait for expiration to take effect
            Thread.Sleep(10);

            // Act
            cache.CleanupExpiredItems();

            // Assert
            Assert.Null(cache.Get("key1"));
            Assert.Equal("value2", cache.Get("key2"));
        }

        // Async tests

        [Fact]
        public async Task AddAsync_GetAsync_ShouldStoreAndRetrieveValue()
        {
            // Arrange
            var cache = new SharpMemoryCache();
            var key = "testKey";
            var expectedValue = "testValue";

            // Act
            await cache.AddAsync(key, expectedValue);
            var actualValue = await cache.GetAsync(key);

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public async Task TryGetAsync_WithExistingKey_ShouldReturnTrueAndValue()
        {
            // Arrange
            var cache = new SharpMemoryCache();
            var key = "testKey";
            var expectedValue = "testValue";
            await cache.AddAsync(key, expectedValue);

            // Act
            var (Success, Value) = await cache.TryGetAsync(key);

            // Assert
            Assert.True(Success);
            Assert.Equal(expectedValue, Value);
        }

        [Fact]
        public async Task TryGetAsync_WithNonExistentKey_ShouldReturnFalseAndNull()
        {
            // Arrange
            var cache = new SharpMemoryCache();

            // Act
            var (Success, Value) = await cache.TryGetAsync("nonExistentKey");

            // Assert
            Assert.False(Success);
            Assert.Null(Value);
        }

        [Fact]
        public async Task RemoveAsync_ShouldRemoveItemFromCache()
        {
            // Arrange
            var cache = new SharpMemoryCache();
            var key = "testKey";
            await cache.AddAsync(key, "testValue");

            // Act
            await cache.RemoveAsync(key);
            var result = await cache.GetAsync(key);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CleanupExpiredItemsAsync_ShouldRemoveExpiredItems()
        {
            // Arrange
            var cache = new SharpMemoryCache();
            await cache.AddAsync("key1", "value1", absoluteExpiration: DateTime.UtcNow.AddMilliseconds(-1));
            await cache.AddAsync("key2", "value2", absoluteExpiration: DateTime.UtcNow.AddHours(1));

            // Wait for expiration to take effect
            await Task.Delay(10);

            // Act
            await cache.CleanupExpiredItemsAsync();

            // Assert
            Assert.Null(await cache.GetAsync("key1"));
            Assert.Equal("value2", await cache.GetAsync("key2"));
        }

        [Fact]
        public async Task IsEmptyAsync_WithItems_ShouldReturnFalse()
        {
            // Arrange
            var cache = new SharpMemoryCache();
            await cache.AddAsync("testKey", "testValue");

            // Act & Assert
            Assert.False(await cache.IsEmptyAsync());
        }

        [Fact]
        public async Task ClearAsync_ShouldRemoveAllItems()
        {
            // Arrange
            var cache = new SharpMemoryCache();
            await cache.AddAsync("key1", "value1");
            await cache.AddAsync("key2", "value2");

            // Act
            await cache.ClearAsync();

            // Assert
            Assert.True(await cache.IsEmptyAsync());
            Assert.Null(await cache.GetAsync("key1"));
        }

        // Thread safety tests

        [Fact]
        public void ThreadSafety_MultipleThreadsAddingAndRetrieving_ShouldWorkCorrectly()
        {
            // Arrange
            var cache = new SharpMemoryCache();
            var itemCount = 1000;
            var threadCount = 8;
            var countdownEvent = new CountdownEvent(threadCount);
            var exceptions = new ConcurrentQueue<Exception>();

            // Act
            for (int t = 0; t < threadCount; t++)
            {
                int threadId = t;
                new Thread(() =>
                {
                    try
                    {
                        // Each thread adds and retrieves its own set of items
                        for (int i = 0; i < itemCount; i++)
                        {
                            var key = $"key-{threadId}-{i}";
                            var value = $"value-{threadId}-{i}";

                            cache.Add(key, value);

                            var retrievedValue = cache.Get(key);
                            if (!value.Equals(retrievedValue))
                            {
                                throw new Exception($"Thread {threadId}: Expected {value} but got {retrievedValue} for key {key}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        exceptions.Enqueue(ex);
                    }
                    finally
                    {
                        countdownEvent.Signal();
                    }
                }).Start();
            }

            // Wait for all threads to complete
            countdownEvent.Wait();

            // Assert
            Assert.Empty(exceptions);
            // Verify the expected number of items is in the cache
            int totalItems = itemCount * threadCount;
            int foundItems = 0;

            for (int t = 0; t < threadCount; t++)
            {
                for (int i = 0; i < itemCount; i++)
                {
                    if (cache.Get($"key-{t}-{i}") != null)
                    {
                        foundItems++;
                    }
                }
            }

            Assert.Equal(totalItems, foundItems);
        }

        [Fact]
        public void ThreadSafety_ConcurrentModifications_ShouldMaintainConsistency()
        {
            // Arrange
            var cache = new SharpMemoryCache();
            const string key = "sharedKey";
            var addCount = 1000;
            var removeCount = 500;
            var expectedFinalOperations = addCount - removeCount;
            var operations = new ConcurrentBag<string>();

            // Add initial value
            cache.Add(key, "initial");

            // Set up threads for concurrent adds
            var addThread = new Thread(() =>
            {
                for (int i = 0; i < addCount; i++)
                {
                    cache.Add(key, $"value-{i}");
                    operations.Add($"add-{i}");

                    // Small random delay to increase chances of thread interleaving
                    if (i % 10 == 0) Thread.Sleep(1);
                }
            });

            // Set up thread for concurrent removes
            var removeThread = new Thread(() =>
            {
                for (int i = 0; i < removeCount; i++)
                {
                    cache.Remove(key);
                    operations.Add($"remove-{i}");

                    // Re-add to ensure we're testing contention
                    cache.Add(key, $"restored-after-remove-{i}");
                    operations.Add($"restore-{i}");

                    // Small random delay to increase chances of thread interleaving
                    if (i % 5 == 0) Thread.Sleep(1);
                }
            });

            // Act
            addThread.Start();
            removeThread.Start();

            // Wait for threads to complete
            addThread.Join();
            removeThread.Join();
             
            // Assert
            Assert.NotNull(cache.Get(key)); // Some value should exist
            Assert.Equal(addCount + removeCount * 2, operations.Count); // Verify operation count
        }

        [Fact]
        public async Task AsyncThreadSafety_ParallelOperations_ShouldWorkCorrectly()
        {
            // Arrange
            var cache = new SharpMemoryCache();
            var tasks = new List<Task>();
            var random = new Random();
            var itemCount = 100;

            // Act - Run various operations in parallel
            for (int i = 0; i < itemCount; i++)
            {
                int index = i;
                // Mix of different operations
                switch (index % 4)
                {
                    case 0: // Add
                        tasks.Add(Task.Run(async () =>
                        {
                            await cache.AddAsync($"key-{index}", $"value-{index}");
                            await Task.Delay(random.Next(1, 5)); // Small random delay
                        }));
                        break;

                    case 1: // Get
                        tasks.Add(Task.Run(async () =>
                        {
                            await cache.GetAsync($"key-{index / 2}"); // May or may not exist
                            await Task.Delay(random.Next(1, 5));
                        }));
                        break;

                    case 2: // Remove
                        tasks.Add(Task.Run(async () =>
                        {
                            await cache.RemoveAsync($"key-{index / 2}"); // May or may not exist
                            await Task.Delay(random.Next(1, 5));
                        }));
                        break;

                    case 3: // Cleanup
                        tasks.Add(Task.Run(async () =>
                        {
                            await cache.CleanupExpiredItemsAsync();
                            await Task.Delay(random.Next(1, 5));
                        }));
                        break;
                }
            }

            // Add a few items with expiration to test cleanup
            for (int i = 0; i < 10; i++)
            {
                await cache.AddAsync($"expiring-key-{i}", $"value-{i}",
                    absoluteExpiration: DateTime.UtcNow.AddMilliseconds(random.Next(5, 20)));
            }

            // Wait for all tasks to complete
            await Task.WhenAll(tasks);

            // Let expiration happen
            await Task.Delay(30);
            await cache.CleanupExpiredItemsAsync();

            // Assert - no exceptions thrown means the test passes
            // We can also verify some expected state:
            int valuesFound = 0;
            for (int i = 0; i < itemCount; i++)
            {
                if (await cache.GetAsync($"key-{i}") != null)
                {
                    valuesFound++;
                }
            }

            // Some values should exist, but not all due to the removals
            Assert.True(valuesFound > 0);
            Assert.True(valuesFound < itemCount);

            // All expired items should be gone
            for (int i = 0; i < 10; i++)
            {
                Assert.Null(await cache.GetAsync($"expiring-key-{i}"));
            }
        }

        [Fact]
        public async Task AsyncThreadSafety_ConcurrentCleanup_ShouldNotCauseExceptions()
        {
            // Arrange
            var cache = new SharpMemoryCache();
            var cleanupTasks = new List<Task>();
            var operationTasks = new List<Task>();

            // Add some items with immediate expiration
            for (int i = 0; i < 100; i++)
            {
                await cache.AddAsync($"key-{i}", $"value-{i}",
                    absoluteExpiration: DateTime.UtcNow.AddMilliseconds(5));
            }

            // Add some items that won't expire
            for (int i = 100; i < 200; i++)
            {
                await cache.AddAsync($"key-{i}", $"value-{i}",
                    absoluteExpiration: DateTime.UtcNow.AddMinutes(5));
            }

            // Wait for items to expire
            await Task.Delay(10);

            // Act - Run multiple cleanup operations concurrently
            for (int i = 0; i < 5; i++)
            {
                cleanupTasks.Add(cache.CleanupExpiredItemsAsync());
            }

            // Also perform other operations during cleanup
            for (int i = 0; i < 50; i++)
            {
                int index = i;
                operationTasks.Add(Task.Run(async () =>
                {
                    await cache.AddAsync($"new-key-{index}", $"new-value-{index}");
                    await cache.GetAsync($"key-{index + 100}"); // Should be non-expired items
                    await cache.RemoveAsync($"key-{index + 150}");
                }));
            }

            // Wait for all operations to complete
            await Task.WhenAll(cleanupTasks.Concat(operationTasks));

            // Assert - All expired items should be gone
            int expiredItemsFound = 0;
            for (int i = 0; i < 100; i++)
            {
                if (await cache.GetAsync($"key-{i}") != null)
                {
                    expiredItemsFound++;
                }
            }

            Assert.Equal(0, expiredItemsFound);

            // Some non-expired items should remain, but some were removed in the operations
            int nonExpiredItemsFound = 0;
            for (int i = 100; i < 150; i++)
            {
                if (await cache.GetAsync($"key-{i}") != null)
                {
                    nonExpiredItemsFound++;
                }
            }

            Assert.True(nonExpiredItemsFound > 0);

            // New items should exist
            int newItemsFound = 0;
            for (int i = 0; i < 50; i++)
            {
                if (await cache.GetAsync($"new-key-{i}") != null)
                {
                    newItemsFound++;
                }
            }

            Assert.Equal(50, newItemsFound);
        }
    }
}
