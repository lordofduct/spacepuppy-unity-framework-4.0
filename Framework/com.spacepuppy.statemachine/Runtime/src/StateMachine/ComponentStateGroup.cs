using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy.Utils;

namespace com.spacepuppy.StateMachine
{

    public class ComponentStateGroup<T> : IStateGroup<T> where T : class
    {

        #region Fields

        private GameObject _container;
        private T _current;

        #endregion

        #region CONSTRUCTOR

        public ComponentStateGroup(GameObject container)
        {
            if (container == null) throw new System.ArgumentNullException(nameof(container));
            _container = container;
        }

        #endregion

        #region Properties

        public GameObject Container
        {
            get { return _container; }
        }

        #endregion

        #region Methods

        public bool Contains<TSub>() where TSub : class, T
        {
            if (_container == null) return false;
            return _container.HasComponent<TSub>();
        }

        public bool Contains(System.Type tp)
        {
            if (_container == null) return false;
            if (tp == null || !TypeUtil.IsType(tp, typeof(T))) return false;
            return _container.HasComponent(tp);
        }

        public bool Contains(T state)
        {
            if (_container == null) return false;
            return _container == GameObjectUtil.GetGameObjectFromSource(state);
        }

        public T GetStateAt(int index)
        {
            if (_container == null) return null;
            if (index < 0) throw new System.IndexOutOfRangeException();

            using (var lst = com.spacepuppy.Collections.TempCollection.GetList<T>())
            {
                _container.GetComponents<T>(lst);
                if (index < lst.Count) return lst[index];
                else throw new System.IndexOutOfRangeException();
            }
        }

        public TSub GetState<TSub>() where TSub : class, T
        {
            if (_container == null) return null;
            return _container.GetComponent<TSub>();
        }

        public T GetState(System.Type tp)
        {
            if (tp == null || !TypeUtil.IsType(tp, typeof(T))) return default(T);
            if (_container == null) return default(T);
            return _container.GetComponent(tp) as T;
        }

        public T GetNext(T current)
        {
            if (_container == null) return null;
            //return this.GetValueAfterOrDefault(current, true);
            using (var lst = com.spacepuppy.Collections.TempCollection.GetList<T>())
            {
                _container.GetComponents<T>(lst);
                return lst.GetValueAfterOrDefault(current, true);
            }
        }

        #endregion

        #region State Machine Interface

        public event StateChangedEventHandler<T> StateChanged;

        public int Count
        {
            get
            {
                using (var lst = com.spacepuppy.Collections.TempCollection.GetList<T>())
                {
                    _container.GetComponents<T>(lst);
                    return lst.Count;
                }
            }
        }

        public T Current { get { return _current; } }

        public T ChangeState(T state)
        {
            if (object.ReferenceEquals(state, _current)) return _current;
            if (!(state is null) && !this.Contains(state)) throw new System.ArgumentException("state must be a member of this state group.", nameof(state));

            var old = _current;
            _current = state;
            this.StateChanged?.Invoke(this, new StateChangedEventArgs<T>(old, _current));
            return _current;
        }

        #endregion

        #region IRadicalEnumerable Interface

        public int Enumerate(ICollection<T> coll)
        {
            if (coll == null) return 0;
            if (_container == null) return 0;

            int cnt = 0;
            using (var lst = com.spacepuppy.Collections.TempCollection.GetList<T>())
            {
                _container.GetComponents<T>(lst);
                var e = lst.GetEnumerator();
                while (e.MoveNext())
                {
                    cnt++;
                    coll.Add(e.Current);
                }
            }
            return cnt;
        }

        public int Enumerate(System.Action<T> callback)
        {
            if (callback == null) return 0;
            if (_container == null) return 0;

            int cnt = 0;
            using (var lst = com.spacepuppy.Collections.TempCollection.GetList<T>())
            {
                _container.GetComponents<T>(lst);
                var e = lst.GetEnumerator();
                while (e.MoveNext())
                {
                    cnt++;
                    callback(e.Current);
                }
            }
            return cnt;
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (_container == null) return Enumerable.Empty<T>().GetEnumerator();
            return (_container.GetComponents<T>() as IEnumerable<T>).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            if (_container == null) return Enumerable.Empty<T>().GetEnumerator();
            return _container.GetComponents<T>().GetEnumerator();
        }

        #endregion

        #region Static Interface

        public static IEnumerable<T> GetComponentsOnTarg(GameObject container)
        {
            return container.GetComponents<T>();
        }

        public static IEnumerable<TSub> GetComponentsOnTarg<TSub>(GameObject container) where TSub : class, T
        {
            return container.GetComponents<TSub>();
        }

        public static IEnumerable<T> GetComponentsOnTarg(System.Type tp, GameObject container)
        {
            if (!TypeUtil.IsType(tp, typeof(T))) throw new TypeArgumentMismatchException(tp, typeof(T), "tp");

            return container.GetComponents(tp).Cast<T>();
        }

        #endregion

    }

}
