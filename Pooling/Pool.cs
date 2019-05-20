using System;
using System.Collections.Concurrent;

namespace Pooling
{
    public sealed class Pool<T>
    {
        private ConcurrentBag<T> _items =
            new ConcurrentBag<T>();

        private readonly Func<T> _itemCreator = null;
        private readonly Action<T> _itemClearer = null;

        public Pool(Func<T> itemCreator)
        {
            _itemCreator = itemCreator ?? throw new ArgumentNullException("itemCreator");
        }

        public Pool(Func<T> itemCreator, Action<T> itemClearer) : this(itemCreator)
        {
            _itemClearer = itemClearer ?? throw new ArgumentNullException("itemClearer");
        }

        public T Rent()
        {
            if (_items.TryTake(out var item))
                return item;

            return _itemCreator();
        }

        public void Return(T item)
        {
            _itemClearer?.Invoke(item);

            _items.Add(item);
        }

        public int Count()
        {
            return _items.Count;
        }
    }
}
