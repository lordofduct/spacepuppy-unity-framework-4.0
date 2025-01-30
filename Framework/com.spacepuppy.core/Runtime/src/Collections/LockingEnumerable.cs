using UnityEngine;
using System.Collections.Generic;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Collections
{

    /// <summary>
    /// Call lock before enumerating to enumerate the version of it before lock. You can still modify the collection, but the enumeration 
    /// won't change until unlocked.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LockingEnumerable<T> : ICollection<T>
    {

        enum States
        {
            Unlocked = 0,
            Locked = 1,
            Altered = 2,
        }

        #region Fields

        private ICollection<T> _coll;
        private States _state;
        private List<T> _buffer;
      
        #endregion

        #region CONSTRUCTOR

        public LockingEnumerable()
        {
            _coll = new List<T>();
        }

        public LockingEnumerable(ICollection<T> inner)
        {
            if (inner == null) throw new System.ArgumentNullException(nameof(inner));
            _coll = inner;
        }

        #endregion

        #region Properties

        public ICollection<T> InnerCollection => _coll;

        public bool Locked => _state != States.Unlocked;

        #endregion

        #region Methods

        public void Lock()
        {
            if (_state != States.Unlocked) return;

            _state = States.Locked;
        }

        public void Unlock()
        {
            switch (_state)
            {
                case States.Unlocked:
                case States.Locked:
                    _state = States.Unlocked;
                    _buffer?.Clear();
                    break;
                case States.Altered:
                    _state = States.Unlocked;
                    _coll.Clear();
                    _coll.AddRange(_buffer);
                    _buffer.Clear();
                    break;
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

        public void Add(T item)
        {
            switch (_state)
            {
                case States.Unlocked:
                    _coll.Add(item);
                    break;
                case States.Locked:
                    if (_buffer == null)
                    {
                        _state = States.Altered;
                        _buffer = new(_coll);
                        _buffer.Add(item);
                    }
                    else
                    {
                        _state = States.Altered;
                        _buffer.AddRange(_coll);
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

        public bool Remove(T item)
        {
            switch (_state)
            {
                case States.Unlocked:
                    return _coll.Remove(item);
                case States.Locked:
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
                        return _buffer.Remove(item);
                    }
                case States.Altered:
                    return _buffer.Remove(item);
                default:
                    return false;
            }
        }

        public bool Contains(T item)
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

        public void CopyTo(T[] array, int arrayIndex)
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

        public IEnumerator<T> GetEnumerator()
        {
            return _coll.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _coll.GetEnumerator();
        }

        #endregion

    }

}
