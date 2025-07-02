using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices.ComTypes;

namespace com.spacepuppy.Collections
{

    public class FiniteDeque<T> : IIndexedEnumerable<T>, ICollection<T>
    {

        #region Fields

        private T[] _buffer;
        private int _count;
        private int _rear;
        private int _version;

        #endregion

        #region CONSTRUCTOR

        public FiniteDeque(int size)
        {
            if (size <= 0) throw new System.ArgumentException("FiniteStack size must be positive and non-zero.", nameof(size));

            _buffer = new T[size];
            _count = 0;
            _rear = 0;
        }

        public FiniteDeque(int size, IEnumerable<T> values) : this(size)
        {
            if (values != null)
            {
                foreach (var v in values)
                {
                    this.Push(v);
                }
            }
        }

        #endregion

        #region Properties

        public int Count => _count;

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= _count) throw new IndexOutOfRangeException();
                return _buffer[(_rear + index) % _buffer.Length];
            }
            set
            {
                if (index < 0 || index >= _count) throw new IndexOutOfRangeException();
                _buffer[(_rear + index) % _buffer.Length] = value;
            }
        }

        public int Size
        {
            get => _buffer.Length;
        }

        #endregion

        #region Methods

        public void Clear()
        {
            Array.Clear(_buffer, 0, _buffer.Length);
            _rear = 0;
            _count = 0;
            _version++;
        }

        public bool Contains(T item)
        {
            var e = this.GetEnumerator();
            while (e.MoveNext())
            {
                if (EqualityComparer<T>.Default.Equals(item, e.Current))
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(T[] array, int startIndex)
        {
            if (_count == 0) return;

            int skip = 0;
            int sz = array.Length - startIndex;
            if (sz < _count)
            {
                skip = _count - sz;
            }

            var e = this.GetEnumerator();
            sz = 0;
            while (e.MoveNext() && startIndex < array.Length)
            {
                sz++;
                if (sz <= skip) continue;

                array[startIndex] = e.Current;
                startIndex++;
            }
        }

        public int IndexOf(T item)
        {
            if (_count == 0) return -1;

            var e = this.GetEnumerator();
            int index = 0;
            while (e.MoveNext())
            {
                if (EqualityComparer<T>.Default.Equals(item, e.Current))
                {
                    return index;
                }
                index++;
            }

            return -1;
        }

        public T PeekPop()
        {
            if (_count == 0) throw new InvalidOperationException("FiniteStack<T> is empty.");

            return _buffer[(_rear + _count - 1) % _buffer.Length];
        }

        public T PeekUnshift()
        {
            if (_count == 0) throw new InvalidOperationException("FiniteStack<T> is empty.");

            return _buffer[_rear];
        }

        public T Pop()
        {
            if (_count == 0) throw new InvalidOperationException("FiniteStack<T> is empty.");

            int head = (_rear + _count - 1) % _buffer.Length;
            var result = _buffer[head];
            _buffer[head] = default(T);
            _count--;
            _version++;
            return result;
        }

        public void Push(T item)
        {
            _buffer[(_rear + _count) % _buffer.Length] = item;
            if (_count < _buffer.Length)
            {
                _count++;
            }
            else
            {
                _rear = (_rear + 1) % _buffer.Length;
            }
            _version++;
        }

        public void Shift(T item)
        {
            int index = _rear - 1;
            if (_rear < 0) _rear += _buffer.Length;
            _buffer[index] = item;
            _rear = index;
            if (_count < _buffer.Length)
            {
                _count++;
            }
            _version++;
        }

        public T Unshift()
        {
            if (_count == 0) throw new InvalidOperationException("FiniteStack<T> is empty.");

            var result = _buffer[_rear];
            _buffer[_rear] = default(T);
            _count--;
            _rear = (_rear + 1) % _buffer.Length;
            _version++;
            return result;
        }

        public bool Remove(T item)
        {
            int index = this.IndexOf(item);
            if (index >= 0)
            {
                this.RemoveAt(index);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= _count) throw new IndexOutOfRangeException();

            if (index == 0)
            {
                this.Unshift();
            }
            else if (index == _count - 1)
            {
                this.Pop();
            }
            else if (index > 0 && index < _count)
            {
                int cnt = _count;
                _count = cnt - 1;
                _buffer[(_rear + index) % _buffer.Length] = default;
                for (int i = index + 1; i < cnt; i++)
                {
                    _buffer[(_rear + i - 1) % _buffer.Length] = _buffer[(_rear + i) % _buffer.Length];
                }
                _buffer[(_rear + cnt - 1) % _buffer.Length] = default;
                _version++;
            }
        }
        #endregion

        #region ICollection Interface

        bool ICollection<T>.IsReadOnly => false;

        void ICollection<T>.Add(T item)
        {
            this.Push(item);
        }

        #endregion

        #region IEnumerable Interface

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        #region Special Types

        public struct Enumerator : IEnumerator<T>
        {

            private FiniteDeque<T> _stack;
            private int _index;
            private int _version;

            public Enumerator(FiniteDeque<T> stack)
            {
                if (stack == null) throw new ArgumentNullException(nameof(stack));
                _stack = stack;
                _index = -1;
                _version = _stack._version;
            }

            public T Current => _stack[_index];

            object IEnumerator.Current => _stack[_index];

            public bool MoveNext()
            {
                if (_stack == null || _stack._version != _version) throw new InvalidOperationException("The collection was modified after the enumerator was created.");

                _index = Math.Min(_index + 1, _stack._count);
                return _index < _stack._count;
            }

            void IEnumerator.Reset()
            {
                _index = 0;
            }

            void IDisposable.Dispose()
            {
                _stack = null;
                _index = 0;
            }

        }

        #endregion

    }
}
