using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Pooling
{
    public sealed class Pool<T>
    {
        readonly ConcurrentBag<T> _items = new ConcurrentBag<T>();
        readonly Func<T> _itemCreator = null;
        readonly Action<T> _itemClearer = null;
        readonly int _maxItems;
        SpinLock _lock;

        public Pool(Func<T> itemCreator, int maxItems)
        {
            _itemCreator = itemCreator
                ?? throw new ArgumentNullException(nameof(itemCreator));

            _maxItems = maxItems;
        }

        public Pool(Func<T> itemCreator, Action<T> itemClearer, int maxItems) : this(itemCreator, maxItems)
        {
            _itemClearer = itemClearer
                ?? throw new ArgumentNullException(nameof(itemClearer));
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
            bool lockTaken = false;
            try
            {
                _lock.Enter(ref lockTaken);

                if (Count < _maxItems)
                {
                    _itemClearer?.Invoke(item);
                    _items.Add(item);
                }
            }
            finally
            {
                if (lockTaken)
                    _lock.Exit();
            }
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