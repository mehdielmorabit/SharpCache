using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteCache.Persistence
{
    public interface IPersistenceProvider<T> : IDisposable, IAsyncDisposable where T : class
    {
        void Save(T item);
        Task SaveAsync(T item);
        T? Load(string key);
        Task<T?> LoadAsync(string key);
        void Remove(string key);
        Task RemoveAsync(string key);
        IEnumerable<T> LoadAll();
        Task<IEnumerable<T>> LoadAllAsync();
    }
}
