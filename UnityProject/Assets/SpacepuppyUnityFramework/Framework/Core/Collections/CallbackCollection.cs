using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace com.spacepuppy.Collections
{
    public class CallbackCollection<T> : ICollection<T>
    {

        #region CONSTRUCTOR

        public CallbackCollection()
        {

        }

        public CallbackCollection(Action<T> addCallback)
        {
            this.AddCallback = addCallback;
            this.RemoveCallback = null;
        }

        public CallbackCollection(Action<T> addCallback, Action<T> removeCallback)
        {
            this.AddCallback = addCallback;
            this.RemoveCallback = removeCallback;
        }

        #endregion

        #region Properties

        public Action<T> AddCallback { get; set; }

        public Action<T> RemoveCallback { get; set; }

        #endregion

        #region ICollection Interface

        public int Count { get { return 0; } }

        bool ICollection<T>.IsReadOnly { get { return false; } }

        public void Add(T item)
        {
            this.AddCallback?.Invoke(item);
        }

        public void Clear()
        {
            //do nothing
        }

        public bool Contains(T item)
        {
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            //do nothing
        }

        public bool Remove(T item)
        {
            this.RemoveCallback?.Invoke(item);
            return true;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Enumerable.Empty<T>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Enumerable.Empty<T>().GetEnumerator();
        }

        #endregion

    }

    public class TempCallbackCollection<T> : CallbackCollection<T>, ITempCollection<T>
    {

        private const int MAX_SIZE_INBYTES = 1024;

        #region Fields

        private static ObjectCachePool<TempCallbackCollection<T>> _pool = new ObjectCachePool<TempCallbackCollection<T>>(-1, () => new TempCallbackCollection<T>());

        #endregion

        #region CONSTRUCTOR

        public TempCallbackCollection()
            : base()
        {
            var tp = typeof(T);
            int sz = (tp.IsValueType && !tp.IsEnum) ? System.Runtime.InteropServices.Marshal.SizeOf(tp) : 4;
        }

        #endregion

        #region IDisposable Interface

        public void Dispose()
        {
            this.AddCallback = null;
            this.RemoveCallback = null;
            _pool.Release(this);
        }

        #endregion

        #region Static Methods

        public static TempCallbackCollection<T> GetCallbackCollection(Action<T> addCallback, Action<T> removeCallback = null)
        {
            var coll = _pool.GetInstance();
            coll.AddCallback = addCallback;
            coll.RemoveCallback = removeCallback;
            return coll;
        }

        #endregion

    }

}
