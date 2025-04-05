using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Collections;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Collections
{

    public interface IMultitonPool<T> : IEnumerable<T> where T : class
    {

        int Count { get; }

        void AddReference(T obj);
        bool RemoveReference(T obj);
        bool Contains(T obj);

        T Find(System.Func<T, bool> predicate);

        T[] FindAll(System.Func<T, bool> predicate);
        int FindAll(ICollection<T> coll, System.Func<T, bool> predicate);
        int FindAll<TSub>(ICollection<TSub> coll, System.Func<TSub, bool> predicate) where TSub : class, T;

        /// <summary>
        /// Use for linq if you want to enumerate a specific subtype.
        /// </summary>
        /// <typeparam name="TSub"></typeparam>
        /// <returns></returns>
        IEnumerable<TSub> Enumerate<TSub>() where TSub : class, T;

    }

    public class MultitonPool<T> : IMultitonPool<T> where T : class
    {

        #region Fields

        private LockingHashSet<T> _pool;
        private int _version;

        #endregion

        #region CONSTRUCTOR

        public MultitonPool()
        {
            _pool = new LockingHashSet<T>();
        }

        public MultitonPool(IEqualityComparer<T> comparer)
        {
            _pool = new LockingHashSet<T>(new HashSet<T>(comparer));
        }

        #endregion

        #region Properties

        public bool IsQuerying
        {
            get { return _pool.Locked; }
        }

        /// <summary>
        /// A value that changes if the collection any time the collection is modified. 
        /// Can be used to quickly recognize if the collection has changed. 
        /// </summary>
        public int Version => _version;

        #endregion

        #region IMultitonPool Interface

        public int Count
        {
            get { return _pool.Count; }
        }

        public virtual void Clear()
        {
            _pool.Clear();
            _version++;
        }

        public virtual void AddReference(T obj)
        {
            if (object.ReferenceEquals(obj, null)) throw new System.ArgumentNullException();

            if (_pool.Add(obj))
            {
                _version++;
            }
        }

        public virtual bool RemoveReference(T obj)
        {
            if (object.ReferenceEquals(obj, null)) throw new System.ArgumentNullException();

            if (_pool.Remove(obj))
            {
                _version++;
                return true;
            }
            return false;
        }

        public bool Contains(T obj)
        {
            return _pool.Contains(obj);
        }

        public T Find(System.Func<T, bool> predicate)
        {
            if (this.IsQuerying) throw new System.InvalidOperationException("MultitonPool is already in the process of a query.");

            try
            {
                _pool.Lock();
                var e = _pool.GetEnumerator();
                if (predicate == null)
                {
                    if (e.MoveNext())
                        return e.Current;
                    else
                        return null;
                }

                while (e.MoveNext())
                {
                    if (predicate(e.Current)) return e.Current;
                }
                return null;
            }
            finally
            {
                if (_pool.Unlock())
                {
                    _version++;
                }
            }
        }

        public TSub Find<TSub>(System.Func<TSub, bool> predicate) where TSub : class, T
        {
            if (this.IsQuerying) throw new System.InvalidOperationException("MultitonPool is already in the process of a query.");

            try
            {
                _pool.Lock();
                var e = _pool.GetEnumerator();
                if (predicate == null)
                {
                    while (e.MoveNext())
                    {
                        if (e.Current is TSub) return e.Current as TSub;
                    }
                }

                while (e.MoveNext())
                {
                    if (e.Current is TSub ent && predicate(ent)) return ent;
                }
                return null;
            }
            finally
            {
                if (_pool.Unlock())
                {
                    _version++;
                }
            }
        }

        public TSub Find<TSub, TArg>(TArg arg, System.Func<TSub, TArg, bool> predicate) where TSub : class, T
        {
            if (this.IsQuerying) throw new System.InvalidOperationException("MultitonPool is already in the process of a query.");

            try
            {
                _pool.Lock();
                var e = _pool.GetEnumerator();
                if (predicate == null)
                {
                    while (e.MoveNext())
                    {
                        if (e.Current is TSub) return e.Current as TSub;
                    }
                }

                while (e.MoveNext())
                {
                    if (e.Current is TSub ent && predicate(ent, arg)) return ent;
                }
                return null;
            }
            finally
            {
                if (_pool.Unlock())
                {
                    _version++;
                }
            }
        }

        public T[] FindAll(System.Func<T, bool> predicate)
        {
            if (this.IsQuerying) throw new System.InvalidOperationException("MultitonPool is already in the process of a query.");

            try
            {
                if (predicate == null)
                {
                    _pool.Lock();

                    T[] arr = new T[_pool.Count];
                    var e = _pool.GetEnumerator();
                    int i = 0;
                    while (e.MoveNext())
                    {
                        arr[i] = e.Current;
                        i++;
                    }
                    return arr;
                }
                else
                {
                    using (var lst = TempCollection.GetList<T>())
                    {
                        FindAll(lst, predicate);
                        return lst.ToArray();
                    }
                }
            }
            finally
            {
                if (_pool.Unlock())
                {
                    _version++;
                }
            }
        }

        public TSub[] FindAll<TSub>(System.Func<TSub, bool> predicate) where TSub : class, T
        {
            if (this.IsQuerying) throw new System.InvalidOperationException("MultitonPool is already in the process of a query.");

            using (var lst = TempCollection.GetList<TSub>())
            {
                FindAll<TSub>(lst, predicate);
                return lst.ToArray();
            }
        }

        public int FindAll(ICollection<T> coll, System.Func<T, bool> predicate)
        {
            if (coll == null) throw new System.ArgumentNullException("coll");
            if (this.IsQuerying) throw new System.InvalidOperationException("MultitonPool is already in the process of a query.");

            try
            {
                _pool.Lock();
                int cnt = 0;
                var e = _pool.GetEnumerator();
                if (predicate == null)
                {
                    while (e.MoveNext())
                    {
                        coll.Add(e.Current);
                        cnt++;
                    }
                }
                else
                {
                    while (e.MoveNext())
                    {
                        if (predicate(e.Current))
                        {
                            coll.Add(e.Current);
                            cnt++;
                        }
                    }
                }
                return cnt;
            }
            finally
            {
                if (_pool.Unlock())
                {
                    _version++;
                }
            }
        }

        public int FindAll<TSub>(ICollection<TSub> coll, System.Func<TSub, bool> predicate) where TSub : class, T
        {
            if (coll == null) throw new System.ArgumentNullException("coll");
            if (this.IsQuerying) throw new System.InvalidOperationException("MultitonPool is already in the process of a query.");

            try
            {
                _pool.Lock();
                int cnt = 0;
                var e = _pool.GetEnumerator();
                if (predicate == null)
                {
                    while (e.MoveNext())
                    {
                        if (e.Current is TSub)
                        {
                            coll.Add((TSub)e.Current);
                            cnt++;
                        }
                    }
                }
                else
                {
                    while (e.MoveNext())
                    {
                        if (e.Current is TSub && predicate((TSub)e.Current))
                        {
                            coll.Add((TSub)e.Current);
                            cnt++;
                        }
                    }
                }

                return cnt;
            }
            finally
            {
                if (_pool.Unlock())
                {
                    _version++;
                }
            }
        }

        public IEnumerable<TSub> Enumerate<TSub>() where TSub : class, T
        {
            var e = _pool.GetEnumerator();
            while (e.MoveNext())
            {
                if (e.Current is TSub) yield return e.Current as TSub;
            }
        }

        public IEnumerable<TSub> Enumerate<TSub>(System.Func<TSub, bool> predicate) where TSub : class, T
        {
            var e = _pool.GetEnumerator();
            if (predicate == null)
            {
                while (e.MoveNext())
                {
                    if (e.Current is TSub) yield return e.Current as TSub;
                }
            }
            else
            {
                while (e.MoveNext())
                {
                    if (e.Current is TSub o && predicate(o)) yield return o;
                }
            }
        }

        #endregion

        #region IEnumerable Interface

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        #region Special Types

        public struct Enumerator : IEnumerator<T>
        {

            #region Fields

            private HashSet<T>.Enumerator _e;

            #endregion

            #region CONSTRUCTOR

            public Enumerator(MultitonPool<T> multi)
            {
                if (multi == null) throw new System.ArgumentNullException();
                _e = multi._pool.GetEnumerator();
            }

            #endregion

            public T Current
            {
                get
                {
                    return _e.Current;
                }
            }

            object System.Collections.IEnumerator.Current
            {
                get
                {
                    return _e.Current;
                }
            }

            public void Dispose()
            {
                _e.Dispose();
            }

            public bool MoveNext()
            {
                return _e.MoveNext();
            }

            void System.Collections.IEnumerator.Reset()
            {
                (_e as System.Collections.IEnumerator).Reset();
            }
        }

        #endregion

    }

    public class UniqueToEntityMultitonPool<T> : MultitonPool<T> where T : class
    {

        #region CONSTRUCTOR

        public UniqueToEntityMultitonPool() : base()
        {
        }

        public UniqueToEntityMultitonPool(IEqualityComparer<T> comparer) : base(comparer)
        {
        }

        #endregion

        #region Methods

        public bool IsSource(object obj)
        {
            return GetFromSource(obj) != null;
        }

        public T GetFromSource(object obj)
        {
            if (obj == null) return null;
            if (obj is T) return this.Contains(obj as T) ? obj as T : null;

            var entity = SPEntity.Pool.GetFromSource(obj);
            if (entity == null) return null;
            //if (entity is T) return this.Contains(entity as T) ? entity as T : null;
            if (entity is T) return entity as T;

            var result = entity.GetComponentInChildren<T>();
            return this.Contains(result) ? result : null;
        }

        public bool GetFromSource(object obj, out T comp)
        {
            comp = GetFromSource(obj);
            return comp != null;
        }




        public bool IsSource<TSub>(object obj) where TSub : class, T
        {
            if (obj is TSub) return true;

            return GetFromSource<TSub>(obj) != null;
        }

        public TSub GetFromSource<TSub>(object obj) where TSub : class, T
        {
            if (obj == null) return null;
            if (obj is TSub) return obj as TSub;

            var entity = SPEntity.Pool.GetFromSource(obj);
            if (entity == null) return null;
            //if (entity is TSub) return this.Contains(entity as TSub) ? entity as TSub : null;
            if (entity is TSub) return entity as TSub;

            var result = entity.GetComponentInChildren<TSub>();
            return this.Contains(result) ? result : null;
        }

        public bool GetFromSource<TSub>(object obj, out TSub comp) where TSub : class, T
        {
            comp = GetFromSource<TSub>(obj);
            return comp != null;
        }


        public SPEntity FindEntity(System.Func<T, bool> predicate)
        {
            var e = this.GetEnumerator();
            while (e.MoveNext())
            {
                if ((predicate?.Invoke(e.Current) ?? true) &&
                    SPEntity.Pool.GetFromSource(e.Current, out SPEntity entity))
                {
                    return entity;
                }
            }
            return null;
        }

        public TEntity FindEntity<TEntity>(System.Func<T, bool> predicate) where TEntity : SPEntity
        {
            var e = this.GetEnumerator();
            while (e.MoveNext())
            {
                if ((predicate?.Invoke(e.Current) ?? true) &&
                    SPEntity.Pool.GetFromSource<TEntity>(e.Current, out TEntity entity))
                {
                    return entity;
                }
            }
            return null;
        }

        public SPEntity FindEntity<TArg>(TArg arg, System.Func<T, TArg, bool> predicate)
        {
            var e = this.GetEnumerator();
            while (e.MoveNext())
            {
                if ((predicate?.Invoke(e.Current, arg) ?? true) &&
                    SPEntity.Pool.GetFromSource(e.Current, out SPEntity entity))
                {
                    return entity;
                }
            }
            return null;
        }

        public TEntity FindEntity<TEntity, TArg>(TArg arg, System.Func<T, TArg, bool> predicate) where TEntity : SPEntity
        {
            var e = this.GetEnumerator();
            while (e.MoveNext())
            {
                if ((predicate?.Invoke(e.Current, arg) ?? true) &&
                    SPEntity.Pool.GetFromSource<TEntity>(e.Current, out TEntity entity))
                {
                    return entity;
                }
            }
            return null;
        }

        #endregion

    }

}
