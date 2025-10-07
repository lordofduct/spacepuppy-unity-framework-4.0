using System;
using System.Collections.Generic;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Collections
{

    /// <summary>
    /// Creates a pool that will cache instances of objects for later use so that you don't have to construct them again. 
    /// There is a max cache size, if set to 0 or less, it uses the default size (see: DEFAULT_CACHESIZE).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectCachePool<T> : ICachePool<T> where T : class
    {

        public const int DEFAULT_CACHESIZE = 64; //1024;

        #region Fields

        private HashSet<T> _inactive;

        private int _cacheSize;
        private Func<T> _constructorDelegate;
        private Action<T> _resetObjectDelegate;
        private bool _resetOnGet;

        #endregion

        #region CONSTRUCTOR

        public ObjectCachePool(int cacheSize)
        {
            this.CacheSize = cacheSize;
            //_inactive = (_cacheSize <= 0) ? new Bag<T>() : new Bag<T>(_cacheSize);
            _inactive = new HashSet<T>();
            _constructorDelegate = this.SimpleConstructor;
            _resetObjectDelegate = null;
        }

        public ObjectCachePool(int cacheSize, Func<T> constructorDelegate)
        {
            this.CacheSize = cacheSize;
            //_inactive = (_cacheSize <= 0) ? new Bag<T>() : new Bag<T>(_cacheSize);
            _inactive = new HashSet<T>();
            _constructorDelegate = (constructorDelegate != null) ? constructorDelegate : this.SimpleConstructor;
            _resetObjectDelegate = null;
        }

        public ObjectCachePool(int cacheSize, Func<T> constructorDelegate, Action<T> resetObjectDelegate)
        {
            this.CacheSize = cacheSize;
            //_inactive = (_cacheSize <= 0) ? new Bag<T>() : new Bag<T>(_cacheSize);
            _inactive = new HashSet<T>();
            _constructorDelegate = (constructorDelegate != null) ? constructorDelegate : this.SimpleConstructor;
            _resetObjectDelegate = resetObjectDelegate;
        }

        public ObjectCachePool(int cacheSize, Func<T> constructorDelegate, Action<T> resetObjectDelegate, bool resetOnGet)
        {
            this.CacheSize = cacheSize;
            //_inactive = (_cacheSize <= 0) ? new Bag<T>() : new Bag<T>(_cacheSize);
            _inactive = new HashSet<T>();
            _constructorDelegate = (constructorDelegate != null) ? constructorDelegate : this.SimpleConstructor;
            _resetObjectDelegate = resetObjectDelegate;
            _resetOnGet = resetOnGet;
        }

        private T SimpleConstructor()
        {
            return Activator.CreateInstance<T>();
        }

        #endregion

        #region Properties

        public int CacheSize
        {
            get { return _cacheSize; }
            set
            {
                _cacheSize = value > 0 ? value : DEFAULT_CACHESIZE;
            }
        }

        public bool ResetOnGet
        {
            get { return _resetOnGet; }
            set { _resetOnGet = value; }
        }

        public int InactiveCount
        {
            get { return _inactive.Count; }
        }

        #endregion

        #region Methods

        public bool TryGetInstance(out T result)
        {
            result = null;
            lock (_inactive)
            {
                if (_inactive.Count > 0)
                {
                    result = _inactive.Pop();
                }
            }
            if (result != null)
            {
                if (_resetOnGet && _resetObjectDelegate != null)
                    _resetObjectDelegate(result);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Tries to filter out candidates based on some filter, otherwise returns the last available.
        /// </summary>
        /// <typeparam name="TArg"></typeparam>
        /// <param name="arg"></param>
        /// <param name="result"></param>
        /// <param name="filterpredicate"></param>
        /// <returns></returns>
        public bool TryGetInstance<TArg>(TArg arg, out T result, System.Func<T, TArg, bool> filterpredicate)
        {
            if (filterpredicate == null) throw new System.ArgumentNullException(nameof(filterpredicate));

            result = null;
            lock (_inactive)
            {
                if (_inactive.Count > 0)
                {
                    var e = _inactive.GetEnumerator();
                    while (e.MoveNext())
                    {
                        if (filterpredicate(e.Current, arg))
                        {
                            result = e.Current;
                            _inactive.Remove(result);
                            goto ElectedCandidate;
                        }
                    }
                    result = _inactive.Pop();
                }
            }

        ElectedCandidate:
            if (result != null)
            {
                if (_resetOnGet && _resetObjectDelegate != null)
                    _resetObjectDelegate(result);
                return true;
            }
            else
            {
                return false;
            }
        }

        public T GetInstance()
        {
            T result = null;
            lock (_inactive)
            {
                if (_inactive.Count > 0)
                {
                    result = _inactive.Pop();
                }
            }
            if (result != null)
            {
                if (_resetOnGet && _resetObjectDelegate != null)
                    _resetObjectDelegate(result);
                return result;
            }
            else
            {
                return _constructorDelegate();
            }
        }

        public T GetInstance<TArg>(TArg arg, System.Func<T, TArg, bool> filterpredicate)
        {
            if (filterpredicate == null) throw new System.ArgumentNullException(nameof(filterpredicate));

            T result = null;
            lock (_inactive)
            {
                if (_inactive.Count > 0)
                {
                    var e = _inactive.GetEnumerator();
                    while (e.MoveNext())
                    {
                        if (filterpredicate(e.Current, arg))
                        {
                            result = e.Current;
                            _inactive.Remove(result);
                            goto ElectedCandidate;
                        }
                    }
                    result = _inactive.Pop();
                }
            }

        ElectedCandidate:
            if (result != null)
            {
                if (_resetOnGet && _resetObjectDelegate != null)
                    _resetObjectDelegate(result);
                return result;
            }
            else
            {
                return _constructorDelegate();
            }
        }

        public bool Release(T obj)
        {
            if (obj == null) throw new System.ArgumentNullException("obj");

            if (!_resetOnGet && _resetObjectDelegate != null && _inactive.Count < _cacheSize) _resetObjectDelegate(obj);

            lock (_inactive)
            {
                if (_inactive.Count < _cacheSize)
                {
                    _inactive.Add(obj);
                    return true;
                }
            }

            return false;
        }

        void ICachePool<T>.Release(T obj)
        {
            this.Release(obj);
        }

        public bool IsTreatedAsInactive(T obj)
        {
            return _inactive.Contains(obj);
        }

        #endregion

    }

}
