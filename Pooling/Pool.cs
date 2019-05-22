using System;
using System.Collections.Concurrent;
using System.Linq;
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

        public Pool(Func<T> itemCreator, Action<T> itemClearer, int maxItems)
            : this(itemCreator, maxItems)
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

        public bool TryReturn(T item)
        {
            bool isPooled = false;
            bool lockTaken = false;

            try
            {
                _lock.Enter(ref lockTaken);

                var isContains = _items.Contains(item);
                var count = Count();

                if (count < _maxItems && !isContains)
                {
                    _itemClearer?.Invoke(item);
                    _items.Add(item);

                    isPooled = true;
                }
                else if (isContains)
                    _itemClearer?.Invoke(item);
            }
            finally
            {
                if (lockTaken)
                    _lock.Exit();
            }

            return isPooled;
        }

        public int Count() => _items.Count;
    }
}