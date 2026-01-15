using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace com.spacepuppy.Collections
{

    /// <summary>
    /// A double-ended queue (deque).
    /// </summary>
    public class Deque<T> : IList<T>, IIndexedEnumerable<T>
    {

        private static T[] _insertHelperArray;
        private static T[] InsertHelperArray => _insertHelperArray ?? (_insertHelperArray = new T[1]);

        #region Fields

        private const int DefaultCapacity = 8;

        private T[] _buffer;
        private int _count;
        private int _rear;
        private int _version;

        private IEqualityComparer<T> _comparer;

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Initializes a new instance of the <see cref="Deque&lt;T&gt;"/> class with the specified capacity.
        /// </summary>
        /// <param name="capacity">The initial capacity. Must be greater than <c>0</c>.</param>
        public Deque(int capacity)
        {
            if (capacity < 1)
                throw new ArgumentOutOfRangeException("capacity", "Capacity must be greater than 0.");
            _buffer = new T[capacity];
            _comparer = EqualityComparer<T>.Default;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Deque&lt;T&gt;"/> class with the specified capacity.
        /// </summary>
        /// <param name="capacity">The initial capacity. Must be greater than <c>0</c>.</param>
        public Deque(int capacity, IEqualityComparer<T> comparer)
        {
            if (capacity < 1)
                throw new ArgumentOutOfRangeException("capacity", "Capacity must be greater than 0.");
            _buffer = new T[capacity];
            _comparer = comparer ?? EqualityComparer<T>.Default;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Deque&lt;T&gt;"/> class with the elements from the specified collection.
        /// </summary>
        /// <param name="collection">The collection.</param>
        public Deque(IEnumerable<T> collection)
        {
            int count = collection.Count();
            if (count > 0)
            {
                _buffer = new T[count];
                DoInsertRange(0, collection, count);
            }
            else
            {
                _buffer = new T[DefaultCapacity];
            }
            _comparer = EqualityComparer<T>.Default;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Deque&lt;T&gt;"/> class with the elements from the specified collection.
        /// </summary>
        /// <param name="collection">The collection.</param>
        public Deque(IEnumerable<T> collection, IEqualityComparer<T> comparer)
        {
            int count = collection.Count();
            if (count > 0)
            {
                _buffer = new T[count];
                DoInsertRange(0, collection, count);
            }
            else
            {
                _buffer = new T[DefaultCapacity];
            }
            _comparer = comparer ?? EqualityComparer<T>.Default;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Deque&lt;T&gt;"/> class.
        /// </summary>
        public Deque()
            : this(DefaultCapacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Deque&lt;T&gt;"/> class.
        /// </summary>
        public Deque(IEqualityComparer<T> comparer)
            : this(DefaultCapacity, comparer)
        {
        }

        #endregion

        #region Properties

        public int Capacity
        {
            get => _buffer.Length;
            set
            {
                if (value == _buffer.Length) return;
                if (value <= 0) throw new ArgumentOutOfRangeException("Capacity must be positive.", nameof(value));

                this.Resize(value);
            }
        }

        public int Count => _count;

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= _count) throw new IndexOutOfRangeException();
                return _buffer[DequeIndexToBufferIndex(index)];
            }
            set
            {
                if (index < 0 || index >= _count) throw new IndexOutOfRangeException();
                _buffer[DequeIndexToBufferIndex(index)] = value;
            }
        }

        public IEqualityComparer<T> Comparer => _comparer;

        #endregion

        #region Methods

        public void Clear()
        {
            Array.Clear(_buffer, 0, _buffer.Length);
            _count = 0;
            _rear = 0;
            _version++;
        }

        public bool Contains(T item)
        {
            var e = this.GetEnumerator();
            while (e.MoveNext())
            {
                if (_comparer.Equals(item, e.Current))
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(T[] array, int startIndex)
        {
            if (_count == 0) return;

            var e = this.GetEnumerator();
            while (e.MoveNext() && startIndex < array.Length)
            {
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
                if (_comparer.Equals(item, e.Current))
                {
                    return index;
                }
                index++;
            }

            return -1;
        }

        public void Insert(int index, T item)
        {
            if (index < 0 || index > _count) throw new IndexOutOfRangeException();

            if (index == 0)
            {
                this.Unshift(item);
            }
            else if (index == Count)
            {
                this.Push(item);
            }
            else
            {
                this.EnsureCapacity(_count + 1);

                InsertHelperArray[0] = item;
                this.DoInsertRange(index, InsertHelperArray, 1);
                InsertHelperArray[0] = default(T);
                _version++;
            }
        }

        public void InsertRange(int index, IEnumerable<T> values)
        {
            if (index < 0 || index > _count) throw new IndexOutOfRangeException();
            int cnt = values.Count();
            if (cnt == 0) return;

            // Overflow-safe check for "this.Count + collectionCount > this.Capacity"
            if (cnt > _buffer.Length - _count)
            {
                this.EnsureCapacity(checked(_count + cnt));
            }

            if (cnt == 0)
            {
                return;
            }

            this.DoInsertRange(index, values, cnt);
            _version++;
        }

        public T PeekPop()
        {
            if (_count == 0) throw new InvalidOperationException("Deque<T> is empty.");

            return _buffer[DequeIndexToBufferIndex(_count - 1)];
        }

        public T PeekUnshift()
        {
            if (_count == 0) throw new InvalidOperationException("Deque<T> is empty.");

            return _buffer[DequeIndexToBufferIndex(0)];
        }

        public T Pop()
        {
            if (_count == 0) throw new InvalidOperationException("Deque<T> is empty.");

            int bi = DequeIndexToBufferIndex(_count - 1);
            var result = _buffer[bi];
            _buffer[bi] = default(T);
            _count--;
            _version++;
            return result;
        }

        public void Push(T item)
        {
            this.ResizeIfNecessary();

            _buffer[DequeIndexToBufferIndex(Count)] = item;
            _count++;
            _version++;
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
                this.Shift();
            }
            else if (index == Count - 1)
            {
                this.Pop();
            }
            else
            {
                this.DoRemoveRange(index, 1);
                _version++;
            }
        }

        public void RemoveRange(int index, int count)
        {
            if (index < 0 || index >= _count) throw new IndexOutOfRangeException();

            if (_count == 0) return;

            this.DoRemoveRange(index, count);
            _version++;
        }

        public void Unshift(T item)
        {
            this.ResizeIfNecessary();

            _buffer[PreDecrement(1)] = item;
            _count++;
            _version++;
        }

        public T Shift()
        {
            if (_count == 0) throw new InvalidOperationException("Deque<T> is empty.");

            int bi = DequeIndexToBufferIndex(0);
            var result = _buffer[bi];
            _buffer[bi] = default(T);
            _count--;
            _rear = (_rear + 1) % _buffer.Length;
            _version++;
            return result;
        }

        #endregion

        #region Private Methods

        private void EnsureCapacity(int min)
        {
            if (_buffer.Length < min)
            {
                int len = ((_buffer.Length == 0) ? 4 : (_buffer.Length * 2));
                if ((uint)len > 2146435071u)
                {
                    len = 2146435071;
                }

                if (len < min)
                {
                    len = min;
                }

                this.Resize(len);
            }
        }

        private void Resize(int size)
        {
            T[] arr = new T[size];
            if (_rear + _count >= _buffer.Length) //issplit
            {
                // The existing buffer is split, so we have to copy it in parts
                int length = _buffer.Length - _rear;
                Array.Copy(_buffer, _rear, arr, 0, length);
                Array.Copy(_buffer, 0, arr, length, _count - length);
            }
            else
            {
                // The existing buffer is whole
                Array.Copy(_buffer, _rear, arr, 0, _count);
            }
            _buffer = arr;
            _rear = 0;
            _version++;
        }

        private void ResizeIfNecessary()
        {
            if (_count == _buffer.Length)
            {
                this.Resize(_buffer.Length * 2);
            }
        }

        private int DequeIndexToBufferIndex(int index)
        {
            return (index + _rear) % _buffer.Length;
        }

        private int PreDecrement(int value)
        {
            _rear -= value;
            if (_rear < 0)
                _rear += Capacity;
            return _rear;
        }

        private void DoInsertRange(int index, IEnumerable<T> values, int count)
        {
            // Make room in the existing list
            if (index < Count / 2)
            {
                // Inserting into the first half of the list

                // Move lower items down: [0, index) -> [Capacity - collectionCount, Capacity - collectionCount + index)
                // This clears out the low "index" number of items, moving them "collectionCount" places down;
                //   after rotation, there will be a "collectionCount"-sized hole at "index".
                int copyCount = index;
                int writeIndex = Capacity - count;
                for (int j = 0; j != copyCount; ++j)
                {
                    _buffer[DequeIndexToBufferIndex(writeIndex + j)] = _buffer[DequeIndexToBufferIndex(j)];
                }

                // Rotate to the new view
                this.PreDecrement(count);
            }
            else
            {
                // Inserting into the second half of the list

                // Move higher items up: [index, count) -> [index + collectionCount, collectionCount + count)
                int copyCount = Count - index;
                int writeIndex = index + count;
                for (int j = copyCount - 1; j != -1; --j)
                {
                    _buffer[DequeIndexToBufferIndex(writeIndex + j)] = _buffer[DequeIndexToBufferIndex(index + j)];
                }
            }

            // Copy new items into place
            int i = index;
            foreach (T item in values)
            {
                _buffer[DequeIndexToBufferIndex(i)] = item;
                ++i;
            }

            // Adjust valid count
            _count += count;
        }

        private void DoRemoveRange(int index, int count)
        {
            if (count > _count - index)
            {
                count = _count - index;
            }

            if (index == 0)
            {
                //removing from the beginning: move the _rear
                for (int i = index; i < count; i++)
                {
                    _buffer[DequeIndexToBufferIndex(index)] = default(T);
                }

                _rear = (_rear + count) % _buffer.Length;
                _count -= count;
            }
            else if (index == _count - count)
            {
                // Removing from the ending: trim the existing view
                for (int i = index; i < count; i++)
                {
                    _buffer[DequeIndexToBufferIndex(index)] = default(T);
                }

                _count -= count;
            }
            else
            {
                if ((index + (count / 2)) < _count / 2)
                {
                    // Removing from first half of list

                    // Move lower items up: [0, index) -> [collectionCount, collectionCount + index)
                    int copyCount = index;
                    int writeIndex = count;
                    for (int j = copyCount - 1; j != -1; --j)
                    {
                        _buffer[DequeIndexToBufferIndex(writeIndex + j)] = _buffer[DequeIndexToBufferIndex(j)];
                        _buffer[DequeIndexToBufferIndex(j)] = default(T);
                    }

                    // Rotate to new view
                    _rear = (_rear + count) % _buffer.Length;
                }
                else
                {
                    // Removing from second half of list

                    // Move higher items down: [index + collectionCount, count) -> [index, count - collectionCount)
                    int copyCount = _count - count - index;
                    int readIndex = index + count;
                    for (int j = 0; j != copyCount; ++j)
                    {
                        _buffer[DequeIndexToBufferIndex(index + j)] = _buffer[DequeIndexToBufferIndex(readIndex + j)];
                        _buffer[DequeIndexToBufferIndex(readIndex + j)] = default(T);
                    }
                }

                // Adjust valid count
                _count -= count;
            }
        }

        #endregion

        #region IList Interface

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

            private Deque<T> _stack;
            private int _index;
            private int _version;

            public Enumerator(Deque<T> stack)
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
