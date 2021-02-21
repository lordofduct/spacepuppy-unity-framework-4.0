using System;
using System.Collections;
using System.Collections.Generic;

namespace com.spacepuppy.Collections
{

    public interface IIndexedEnumerable<T> : IEnumerable<T>
    {

        int Count { get; }
        T this[int index] { get; }

        bool Contains(T item);
        void CopyTo(T[] array, int startIndex);
        int IndexOf(T item);

    }

    public class ReadOnlyList<T> : IIndexedEnumerable<T>, IList<T>
    {

        #region Fields

        private IList<T> _lst;

        #endregion

        #region CONSTRUCTOR

        public ReadOnlyList(IList<T> lst)
        {
            if (lst == null) throw new ArgumentNullException("lst");
            _lst = lst;
        }

        #endregion

        #region IIndexedEnumerable Interface

        public T this[int index] { get { return _lst[index]; } }

        public int Count { get { return _lst.Count; } }

        public bool Contains(T item)
        {
            return _lst.Contains(item);
        }

        public void CopyTo(T[] array, int startIndex)
        {
            _lst.CopyTo(array, startIndex);
        }

        public int IndexOf(T item)
        {
            return _lst.IndexOf(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _lst.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        #region IList Interface

        T IList<T>.this[int index] { get { return _lst[index]; } set { throw new NotSupportedException(); } }

        bool ICollection<T>.IsReadOnly { get { return true; } }

        int IIndexedEnumerable<T>.Count => throw new NotImplementedException();

        T IIndexedEnumerable<T>.this[int index] => throw new NotImplementedException();

        void ICollection<T>.Add(T item)
        {
            throw new NotSupportedException();
        }

        void ICollection<T>.Clear()
        {
            throw new NotSupportedException();
        }

        void IList<T>.Insert(int index, T item)
        {
            throw new NotSupportedException();
        }

        bool ICollection<T>.Remove(T item)
        {
            throw new NotSupportedException();
        }

        void IList<T>.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region Utils

        public static ReadOnlyList<T> Validate(ref ReadOnlyList<T> pointer, IList<T> consumable)
        {
            if (pointer == null) pointer = new ReadOnlyList<T>(consumable);
            else if (pointer._lst != consumable) pointer._lst = consumable;
            return pointer;
        }

        #endregion

    }

}
