using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy.Utils;

namespace com.spacepuppy.Collections
{

    public class LockingEnumerable
    {

        public static LockingList<T> Create<T>(List<T> coll)
        {
            if (coll == null) throw new System.ArgumentNullException(nameof(coll));
            return new LockingList<T>(coll);
        }

        public static LockingHashSet<T> Create<T>(HashSet<T> coll)
        {
            if (coll == null) throw new System.ArgumentNullException(nameof(coll));
            return new LockingHashSet<T>(coll);
        }

        public static LockingDictionary<TKey, TValue> Create<TKey, TValue>(Dictionary<TKey, TValue> coll)
        {
            if (coll == null) throw new System.ArgumentNullException(nameof(coll));
            return new LockingDictionary<TKey, TValue>(coll);
        }

        //TODO - we don't yet support dictionary...
        //
        //public static LockingEnumerable<TCollection, TValue> Create<TCollection, TValue>(TCollection coll) where TCollection : ICollection<TValue>
        //{
        //    if (coll == null) throw new System.ArgumentNullException(nameof(coll));
        //    switch (coll)
        //    {
        //        case List<TValue> lst:
        //            return new LockingList<TValue>(lst) as LockingEnumerable<TCollection, TValue>;
        //        case HashSet<TValue> hash:
        //            return new LockingHashSet<TValue>(hash) as LockingEnumerable<TCollection, TValue>;
        //        default:
        //            return new LockingEnumerable<TCollection, TValue>(coll);
        //    }
        //}

    }

    /// <summary>
    /// Call lock before enumerating to enumerate the version of it before lock. You can still modify the collection, but the enumeration 
    /// won't change until unlocked.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LockingEnumerable<TCollection, TValue> : LockingEnumerable, ICollection<TValue> where TCollection : ICollection<TValue>
    {

        protected internal enum States
        {
            Unlocked = 0,
            Locked = 1,
            Altered = 2,
        }

        #region Fields

        private TCollection _coll;
        private States _state;
        private List<TValue> _buffer;
      
        #endregion

        #region CONSTRUCTOR

        public LockingEnumerable(TCollection inner)
        {
            if (inner == null) throw new System.ArgumentNullException(nameof(inner));
            _coll = inner;
        }

        #endregion

        #region Properties

        public TCollection InnerCollection => _coll;

        public bool Locked => _state != States.Unlocked;

        protected States State => _state;

        /// <summary>
        /// A reference to the underlying 'altered' buffer, this is only populated if the state is 'States.Altered'.
        /// </summary>
        protected List<TValue> Buffer => _state == States.Altered ? _buffer : null;

        #endregion

        #region Methods

        public void Lock()
        {
            if (_state != States.Unlocked) return;

            _state = States.Locked;
        }

        public virtual bool Unlock()
        {
            switch (_state)
            {
                case States.Unlocked:
                case States.Locked:
                    _state = States.Unlocked;
                    _buffer?.Clear();
                    return false;
                case States.Altered:
                    _state = States.Unlocked;
                    _coll.Clear();
                    _coll.AddRange(_buffer);
                    _buffer.Clear();
                    return true;
                default:
                    return false;
            }
        }

        protected void TransitionToAlteredState()
        {
            _state = States.Altered;
            if (_buffer == null)
            {
                _buffer = new();
                _buffer.AddRange(_coll);
            }
            else
            {
                _buffer.AddRange(_coll);
            }
        }

        #endregion

        #region ICollection Interface

        public int Count
        {
            get
            {
                switch (_state)
                {
                    case States.Unlocked:
                    case States.Locked:
                        return _coll.Count;
                    case States.Altered:
                        return _buffer.Count;
                    default:
                        return _coll.Count;
                }
            }
        }

        public bool IsReadOnly => _coll.IsReadOnly;

        public void Add(TValue item)
        {
            switch (_state)
            {
                case States.Unlocked:
                    _coll.Add(item);
                    break;
                case States.Locked:
                    {
                        this.TransitionToAlteredState();
                        _buffer.Add(item);
                    }
                    break;
                case States.Altered:
                    {
                        _buffer.Add(item);
                    }
                    break;
            }
        }

        public void Clear()
        {
            switch (_state)
            {
                case States.Unlocked:
                    _coll.Clear();
                    break;
                case States.Locked:
                    {
                        _state = States.Altered;
                        if (_buffer == null)
                        {
                            _buffer = new();
                        }
                        else
                        {
                            _buffer.Clear();
                        }
                    }
                    break;
                case States.Altered:
                    {
                        _buffer.Clear();
                    }
                    break;
            }
        }

        public bool Remove(TValue item)
        {
            switch (_state)
            {
                case States.Unlocked:
                    return _coll.Remove(item);
                case States.Locked:
                    {
                        this.TransitionToAlteredState();
                        return _buffer.Remove(item);
                    }
                case States.Altered:
                    return _buffer.Remove(item);
                default:
                    return false;
            }
        }

        public bool Contains(TValue item)
        {
            switch (_state)
            {
                case States.Unlocked:
                case States.Locked:
                    return _coll.Contains(item);
                case States.Altered:
                    return _buffer.Contains(item);
                default:
                    return false;
            }
        }

        public void CopyTo(TValue[] array, int arrayIndex)
        {
            switch (_state)
            {
                case States.Unlocked:
                case States.Locked:
                    _coll.CopyTo(array, arrayIndex);
                    break;
                case States.Altered:
                    _buffer.CopyTo(array, arrayIndex);
                    break;
            }
        }

        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
        {
            return _coll.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _coll.GetEnumerator();
        }

        #endregion

    }

    public class LockingList<T> : LockingEnumerable<List<T>, T>
    {

        public LockingList() : base(new List<T>()) { }
        public LockingList(List<T> coll) : base(coll) { }

        public List<T>.Enumerator GetEnumerator() => this.InnerCollection.GetEnumerator();

    }

    public class LockingHashSet<T> : LockingEnumerable<HashSet<T>, T>
    {

        public LockingHashSet() : base(new HashSet<T>()) { }
        public LockingHashSet(HashSet<T> coll) : base(coll) { }

        public HashSet<T>.Enumerator GetEnumerator() => this.InnerCollection.GetEnumerator();

        public new bool Add(T item)
        {
            switch (this.State)
            {
                case States.Unlocked:
                    return this.InnerCollection.Add(item);
                case States.Locked:
                    if (!this.Contains(item))
                    {
                        this.TransitionToAlteredState();
                        this.Buffer.Add(item);
                        return true;
                    }
                    break;
                case States.Altered:
                    if (!this.Contains(item))
                    {
                        this.Buffer.Add(item);
                        return true;
                    }
                    break;
            }

            return false;
        }

    }

    public class LockingDictionary<TKey, TValue> : LockingEnumerable<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>
    {

        private KeyColl _keys;
        private ValueColl _values;

        public LockingDictionary(Dictionary<TKey, TValue> dict) : base(dict) { }

        public ICollection<TKey> Keys
        {
            get
            {
                switch (this.State)
                {
                    case LockingEnumerable<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>.States.Altered:
                        {
                            return (_keys ??= new KeyColl(this));
                        }
                    default:
                        return this.InnerCollection.Keys;
                }
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                switch (this.State)
                {
                    case LockingEnumerable<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>.States.Altered:
                        {
                            return (_values ??= new ValueColl(this));
                        }
                    default:
                        return this.InnerCollection.Values;
                }
            }
        }

        public bool ContainsKey(TKey key)
        {
            switch (this.State)
            {
                case LockingEnumerable<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>.States.Unlocked:
                case LockingEnumerable<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>.States.Locked:
                    return this.InnerCollection.ContainsKey(key);
                case LockingEnumerable<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>.States.Altered:
                    return this.GetBufferIndexOfKey(key) >= 0;
                default:
                    return false;
            }
        }

        public bool ContainsValue(TValue value)
        {
            switch (this.State)
            {
                case LockingEnumerable<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>.States.Unlocked:
                case LockingEnumerable<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>.States.Locked:
                    return this.InnerCollection.ContainsValue(value);
                case LockingEnumerable<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>.States.Altered:
                    {
                        int cnt = this.Buffer?.Count ?? 0;
                        for (int i = 0; i < cnt; i++)
                        {
                            if (EqualityComparer<TValue>.Default.Equals(this.Buffer[i].Value, value)) return true;
                        }
                        return false;
                    }
                default:
                    return false;
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                return this.InnerCollection[key];
            }
            set
            {
                switch (this.State)
                {
                    case LockingEnumerable<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>.States.Unlocked:
                        this.InnerCollection[key] = value;
                        break;
                    case LockingEnumerable<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>.States.Locked:
                        {
                            this.TransitionToAlteredState();
                            int index = this.GetBufferIndexOfKey(key);
                            if (index < 0)
                            {
                                this.Buffer.Add(new KeyValuePair<TKey, TValue>(key, value));
                            }
                            else
                            {
                                this.Buffer[index] = new KeyValuePair<TKey, TValue>(key, value);
                            }
                        }
                        break;
                    case LockingEnumerable<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>.States.Altered:
                        {
                            this.TransitionToAlteredState();
                            int index = this.GetBufferIndexOfKey(key);
                            if (index < 0)
                            {
                                this.Buffer.Add(new KeyValuePair<TKey, TValue>(key, value));
                            }
                            else
                            {
                                this.Buffer[index] = new KeyValuePair<TKey, TValue>(key, value);
                            }
                        }
                        break;
                }
            }
        }

        public void Add(TKey key, TValue value)
        {
            switch (this.State)
            {
                case LockingEnumerable<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>.States.Unlocked:
                    this.InnerCollection.Add(key, value);
                    break;
                case LockingEnumerable<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>.States.Locked:
                    if (this.InnerCollection.ContainsKey(key))
                    {
                        //duplicate!
                        throw new System.ArgumentException("An element with the same key already exists in the Dictionary<TKey,TValue>.");
                    }
                    else
                    {
                        this.TransitionToAlteredState();
                        this.Buffer.Add(new KeyValuePair<TKey, TValue>(key, value));
                    }
                    break;
                case LockingEnumerable<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>.States.Altered:
                    {
                        int index = this.GetBufferIndexOfKey(key);
                        if (index < 0)
                        {
                            this.Buffer.Add(new KeyValuePair<TKey, TValue>(key, value));
                        }
                        else
                        {
                            //duplicate!
                            throw new System.ArgumentException("An element with the same key already exists in the Dictionary<TKey,TValue>.");
                        }
                    }
                    break;
            }
        }

        public bool TryAdd(TKey key, TValue value)
        {
            switch (this.State)
            {
                case LockingEnumerable<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>.States.Unlocked:
                    return this.InnerCollection.TryAdd(key, value);
                case LockingEnumerable<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>.States.Locked:
                    if (this.InnerCollection.ContainsKey(key))
                    {
                        //duplicate!
                        return false;
                    }
                    else
                    {
                        this.TransitionToAlteredState();
                        this.Buffer.Add(new KeyValuePair<TKey, TValue>(key, value));
                        return true;
                    }
                case LockingEnumerable<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>.States.Altered:
                    {
                        int index = this.GetBufferIndexOfKey(key);
                        if (index < 0)
                        {
                            this.Buffer.Add(new KeyValuePair<TKey, TValue>(key, value));
                            return true;
                        }
                        else
                        {
                            //duplicate!
                            return false;
                        }
                    }
                default:
                    return false;
            }
        }

        public bool Remove(TKey key)
        {
            switch (this.State)
            {
                case LockingEnumerable<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>.States.Unlocked:
                    return this.InnerCollection.Remove(key);
                case LockingEnumerable<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>.States.Locked:
                    if (this.InnerCollection.TryGetValue(key, out TValue value))
                    {
                        return this.Remove(new KeyValuePair<TKey, TValue>(key, value));
                    }
                    else
                    {
                        return false;
                    }
                case LockingEnumerable<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>.States.Altered:
                    {
                        int index = this.GetBufferIndexOfKey(key);
                        if (index >= 0)
                        {
                            this.Buffer.RemoveAt(index);
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                default:
                    return false;
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            switch (this.State)
            {
                case LockingEnumerable<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>.States.Unlocked:
                case LockingEnumerable<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>.States.Locked:
                    return this.InnerCollection.TryGetValue(key, out value);
                case LockingEnumerable<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>.States.Altered:
                    {
                        int index = this.GetBufferIndexOfKey(key);
                        if (index < 0)
                        {
                            value = default;
                            return false;
                        }
                        else
                        {
                            value = this.Buffer[index].Value;
                            return true;
                        }
                    }
                default:
                    value = default;
                    return false;
            }
        }

        int GetBufferIndexOfKey(TKey key)
        {
            var comparer = this.InnerCollection?.Comparer ?? EqualityComparer<TKey>.Default;
            int cnt = this.Buffer?.Count ?? 0;
            for (int i = 0; i < cnt; i++)
            {
                if (comparer.Equals(this.Buffer[i].Key, key)) return i;
            }
            return -1;
        }


        class KeyColl : ICollection<TKey>
        {

            private LockingDictionary<TKey, TValue> _owner;
            public KeyColl(LockingDictionary<TKey, TValue> owner) { _owner = owner; }

            public int Count => _owner.Count;

            public bool IsReadOnly => true;

            public void Add(TKey item) => throw new System.NotSupportedException();
            public void Clear() => throw new System.NotSupportedException();
            public bool Remove(TKey item) => throw new System.NotSupportedException();


            public bool Contains(TKey item) => _owner?.ContainsKey(item) ?? false;

            public void CopyTo(TKey[] array, int arrayIndex)
            {
                if (_owner == null) return;

                switch (_owner.State)
                {
                    case LockingEnumerable<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>.States.Altered:
                        {
                            int cnt = _owner.Buffer?.Count ?? 0;
                            for (int i = 0; i < cnt; i++)
                            {
                                int j = arrayIndex + i;
                                if (j < array.Length)
                                {
                                    array[j] = _owner.Buffer[i].Key;
                                }
                                else
                                {
                                    return;
                                }
                            }
                        }
                        break;
                    default:
                        _owner.InnerCollection.Keys.CopyTo(array, arrayIndex);
                        break;
                }
            }

            public IEnumerator<TKey> GetEnumerator()
            {
                if (_owner == null) return Enumerable.Empty<TKey>().GetEnumerator();

                switch (_owner.State)
                {
                    case LockingEnumerable<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>.States.Altered:
                        return _owner.Buffer.Select(o => o.Key).GetEnumerator();
                    default:
                        return _owner.InnerCollection.Keys.GetEnumerator();
                }
            }
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this.GetEnumerator();

        }

        class ValueColl : ICollection<TValue>
        {

            private LockingDictionary<TKey, TValue> _owner;
            public ValueColl(LockingDictionary<TKey, TValue> owner) { _owner = owner; }

            public int Count => _owner.Count;

            public bool IsReadOnly => true;

            public void Add(TValue item) => throw new System.NotSupportedException();
            public void Clear() => throw new System.NotSupportedException();
            public bool Remove(TValue item) => throw new System.NotSupportedException();


            public bool Contains(TValue item) => _owner?.ContainsValue(item) ?? false;

            public void CopyTo(TValue[] array, int arrayIndex)
            {
                if (_owner == null) return;

                switch (_owner.State)
                {
                    case LockingEnumerable<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>.States.Altered:
                        {
                            int cnt = _owner.Buffer?.Count ?? 0;
                            for (int i = 0; i < cnt; i++)
                            {
                                int j = arrayIndex + i;
                                if (j < array.Length)
                                {
                                    array[j] = _owner.Buffer[i].Value;
                                }
                                else
                                {
                                    return;
                                }
                            }
                        }
                        break;
                    default:
                        _owner.InnerCollection.Values.CopyTo(array, arrayIndex);
                        break;
                }
            }

            public IEnumerator<TValue> GetEnumerator()
            {
                if (_owner == null) return Enumerable.Empty<TValue>().GetEnumerator();

                switch (_owner.State)
                {
                    case LockingEnumerable<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>.States.Altered:
                        return _owner.Buffer.Select(o => o.Value).GetEnumerator();
                    default:
                        return _owner.InnerCollection.Values.GetEnumerator();
                }
            }
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this.GetEnumerator();

        }

    }

}
