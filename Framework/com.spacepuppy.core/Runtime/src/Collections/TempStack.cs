using System;
using System.Collections.Generic;

namespace com.spacepuppy.Collections
{
    public class TempStack<T> : Stack<T>, ITempCollection<T>
    {

        private const int MAX_SIZE_INBYTES = 1024;

        #region Fields

        private static ObjectCachePool<TempStack<T>> _pool = new ObjectCachePool<TempStack<T>>(8, () => new TempStack<T>());

        #endregion

        #region CONSTRUCTOR

        public TempStack()
            : base()
        {
        }

        public TempStack(IEnumerable<T> e)
            : base(e)
        {
        }

        #endregion

        #region ICollection Interface

        bool ICollection<T>.IsReadOnly { get { return false; } }

        void ICollection<T>.Add(T item)
        {
            this.Push(item);
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

        public static TempStack<T> GetStack()
        {
            return _pool.GetInstance();
        }

        public static TempStack<T> GetStack(IEnumerable<T> e)
        {
            TempStack<T> result;
            if (_pool.TryGetInstance(out result))
            {
                var le = LightEnumerator.Create<T>(e);
                while (le.MoveNext())
                {
                    result.Push(le.Current);
                }
            }
            else
            {
                result = new TempStack<T>(e);
            }
            return result;
        }

        #endregion

    }
}
