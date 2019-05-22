using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Pooling
{
    public sealed class Pool<T>
    {
        readonly List<T> _items = new List<T>();
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

        public Pool(Func<T> itemCreator, Action<T> itemClearer, int maxItems)
            : this(itemCreator, maxItems)
        {
            _itemClearer = itemClearer
                ?? throw new ArgumentNullException(nameof(itemClearer));
        }

        public Pooled Rent()
        {
            T rented;
            bool lockTaken = false;
            try
            {
                _lock.Enter(ref lockTaken);

                if (_items.Any())
                {
                    rented = _items[0];
                    _items.RemoveAt(0);
                }
                else
                    rented = _itemCreator();
            }
            finally
            {
                if (lockTaken)
                    _lock.Exit();
            }

            return new Pooled(rented, this);
        }

        public int Count => _items.Count;

        public class Pooled : IDisposable
        {
            readonly Pool<T> _pool;
            public readonly T item;
            public Pooled(T item, Pool<T> pool)
            {
                this.item = item;
                _pool = pool;
            }

            bool disposedValue = false;
            public void Dispose()
            {
                bool lockTaken = false;
                try
                {
                    _pool._lock.Enter(ref lockTaken);

                    var dValue = Volatile.Read(ref disposedValue);
                    if (!dValue)
                    {
                        Volatile.Write(ref disposedValue, true);

                        if (_pool.Count < _pool._maxItems)
                        {
                            _pool._itemClearer?.Invoke(item);
                            _pool._items.Add(item);
                        }
                    }
                }
                finally
                {
                    if (lockTaken)
                        _pool._lock.Exit();
                }
            }
        }
    }
}