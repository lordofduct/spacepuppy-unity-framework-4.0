using System;
using System.Collections.Generic;

namespace com.spacepuppy.Collections
{
    public class TempQueue<T> : Queue<T>, ITempCollection<T>
    {

        private const int MAX_SIZE_INBYTES = 1024;

        #region Fields

        private static ObjectCachePool<TempQueue<T>> _pool = new ObjectCachePool<TempQueue<T>>(8, () => new TempQueue<T>());

        #endregion

        #region CONSTRUCTOR

        public TempQueue()
            : base()
        {
        }

        public TempQueue(IEnumerable<T> e)
            : base(e)
        {
        }

        #endregion

        #region ICollection Interface

        bool ICollection<T>.IsReadOnly { get { return false; } }

        void ICollection<T>.Add(T item)
        {
            this.Enqueue(item);
        }

        bool ICollection<T>.Remove(T item)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IDisposable Interface

        public void Dispose()
        {
            this.Clear();
            _pool.Release(this);
        }

        #endregion

        #region Static Methods

        public static TempQueue<T> GetQueue()
        {
            return _pool.GetInstance();
        }

        public static TempQueue<T> GetQueue(IEnumerable<T> e)
        {
            TempQueue<T> result;
            if (_pool.TryGetInstance(out result))
            {
                var le = LightEnumerator.Create<T>(e);
                while (le.MoveNext())
                {
                    result.Enqueue(le.Current);
                }
            }
            else
            {
                result = new TempQueue<T>(e);
            }
            return result;
        }

        #endregion

    }
}
