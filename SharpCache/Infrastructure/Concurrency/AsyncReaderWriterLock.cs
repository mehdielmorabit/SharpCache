namespace SharpCache.Infrastructure.Concurrency
{
    /// <summary>  
    /// A simple implementation of a reader-writer lock that supports both synchronous and asynchronous operations  
    /// </summary>  
    public class AsyncReaderWriterLock : IDisposable
    {
        private readonly SemaphoreSlim _readSemaphore = new(1, 1);
        private readonly SemaphoreSlim _writeSemaphore = new(1, 1);
        private int _readersCount = 0;

        public IDisposable ReadLock()
        {
            _readSemaphore.Wait();
            try
            {
                Interlocked.Increment(ref _readersCount);
                if (_readersCount == 1)
                {
                    _writeSemaphore.Wait();
                }
            }
            finally
            {
                _readSemaphore.Release();
            }

            return new DisposableAction(() =>
            {
                _readSemaphore.Wait();
                try
                {
                    Interlocked.Decrement(ref _readersCount);
                    if (_readersCount == 0)
                    {
                        _writeSemaphore.Release();
                    }
                }
                finally
                {
                    _readSemaphore.Release();
                }
            });
        }

        public async Task<IDisposable> ReadLockAsync()
        {
            await _readSemaphore.WaitAsync();
            try
            {
                Interlocked.Increment(ref _readersCount);
                if (_readersCount == 1)
                {
                    await _writeSemaphore.WaitAsync();
                }
            }
            finally
            {
                _readSemaphore.Release();
            }

            return new DisposableAction(() =>
            {
                _readSemaphore.Wait();
                try
                {
                    Interlocked.Decrement(ref _readersCount);
                    if (_readersCount == 0)
                    {
                        _writeSemaphore.Release();
                    }
                }
                finally
                {
                    _readSemaphore.Release();
                }
            });
        }

        public IDisposable WriteLock()
        {
            _writeSemaphore.Wait();
            return new DisposableAction(() => _writeSemaphore.Release());
        }

        public async Task<IDisposable> WriteLockAsync()
        {
            await _writeSemaphore.WaitAsync();
            return new DisposableAction(() => _writeSemaphore.Release());
        }

        public void Dispose()
        {
            _readSemaphore.Dispose();
            _writeSemaphore.Dispose();
            GC.SuppressFinalize(this);
        }

        private class DisposableAction : IDisposable
        {
            private readonly Action _action;

            public DisposableAction(Action action)
            {
                _action = action;
            }

            public void Dispose()
            {
                _action();
            }
        }
    }
}
