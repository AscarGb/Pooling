using System;
using System.Collections.Concurrent;

namespace Pooling
{
    public sealed class Pool<T>
    {
        readonly ConcurrentBag<T> _items = new ConcurrentBag<T>();
        readonly Func<T> _itemCreator = null;
        readonly Action<T> _itemClearer = null;

        public Pool(Func<T> itemCreator)
        {
            _itemCreator = itemCreator ?? throw new ArgumentNullException(nameof(itemCreator));
        }

        public Pool(Func<T> itemCreator, Action<T> itemClearer) : this(itemCreator)
        {
            _itemClearer = itemClearer ?? throw new ArgumentNullException(nameof(itemClearer));
        }

        public Pooled<T> Rent()
        {
            T rented;

            if (_items.TryTake(out var item))
                rented = item;
            else
                rented = _itemCreator();

            return new Pooled<T>(rented, this);
        }

        public void Return(T item)
        {
            _itemClearer?.Invoke(item);

            _items.Add(item);
        }

        public int Count => _items.Count;
    }

    public class Pooled<T> : IDisposable
    {
        readonly Pool<T> _pool;
        public readonly T item;
        public Pooled(T item, Pool<T> pool)
        {
            this.item = item;
            _pool = pool;
        }

        private bool disposedValue = false;
        public void Dispose()
        {
            if (!disposedValue)
            {
                _pool.Return(item);
                disposedValue = true;
            }
        }
    }
}