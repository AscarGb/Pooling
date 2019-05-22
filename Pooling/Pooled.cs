using System;
using System.Collections.Generic;
using System.Text;

namespace Pooling
{
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
                _pool.TryReturn(item);
                disposedValue = true;
            }
        }
    }
}