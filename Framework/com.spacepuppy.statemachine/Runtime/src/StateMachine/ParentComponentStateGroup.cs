using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy.Utils;

namespace com.spacepuppy.StateMachine
{

    public class ParentComponentStateGroup<T> : IStateGroup<T> where T : class
    {

        #region Fields

        private GameObject _container;
        private bool _includeStatesOnContainer;
        private bool _isStatic;

        private List<T> _states = new List<T>();
        private bool _clean;

        private T _current;

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Create a ParentComponentStateSupplier
        /// </summary>
        /// <param name="container"></param>
        /// <param name="includeStatesOnContainer"></param>
        /// <param name="isStatic">Set true if the hierarchy doesn't change.</param>
        public ParentComponentStateGroup(GameObject container, bool includeStatesOnContainer, bool isStatic)
        {
            if (container == null) throw new System.ArgumentNullException("container");
            _container = container;
            _includeStatesOnContainer = includeStatesOnContainer;
            _isStatic = isStatic;
        }

        #endregion

        #region Properties

        public GameObject Container
        {
            get { return _container; }
        }

        public int Count
        {
            get
            {
                this.SyncStates();
                return _states.Count;
            }
        }

        public bool IncludeStatesOnContainer
        {
            get { return _includeStatesOnContainer; }
        }

        /// <summary>
        /// Set true if the hierarchy doesn't change.
        /// </summary>
        public bool IsStatic
        {
            get { return _isStatic; }
            set
            {
                if (_isStatic == value) return;
                _isStatic = value;
                if (!_isStatic) this.SetDirty();
            }
        }
        #endregion

        #region Methods

        public void SetDirty()
        {
            _clean = false;
        }

        private void SyncStates()
        {
            if (_isStatic)
            {
                if (!_clean)
                {
                    _states.Clear();
                    if (_container != null) ParentComponentStateGroup<T>.GetComponentsOnTarg(_container, _states, _includeStatesOnContainer);
                    _clean = true;
                }
            }
            else
            {
                _states.Clear();
                if (_container != null) ParentComponentStateGroup<T>.GetComponentsOnTarg(_container, _states, _includeStatesOnContainer);
            }
        }

        public bool Contains<TSub>() where TSub : class, T
        {
            //if (_container == null) return false;
            //T comp = this.GetStates<TSub>().FirstOrDefault();
            //return !comp.IsNullOrDestroyed();

            this.SyncStates();
            var e = _states.GetEnumerator();
            while (e.MoveNext())
            {
                if (e.Current is TSub && !e.Current.IsNullOrDestroyed()) return true;
            }
            return false;
        }

        public bool Contains(System.Type tp)
        {
            if (tp == null) throw new System.ArgumentNullException("tp");
            //if (_container == null) return false;
            //var comp = this.GetStates(tp).FirstOrDefault();
            //return !comp.IsNullOrDestroyed();

            this.SyncStates();
            var e = _states.GetEnumerator();
            while (e.MoveNext())
            {
                if (TypeUtil.IsType(e.Current.GetType(), tp) && !e.Current.IsNullOrDestroyed()) return true;
            }
            return false;
        }

        public bool Contains(T state)
        {
            if (_container == null) return false;
            var go = GameObjectUtil.GetGameObjectFromSource(state);
            if (go != null)
            {
                if (_includeStatesOnContainer && _container == go) return true;
                if (_container.transform == go.transform.parent) return true;
            }

            return false;
        }

        public T GetStateAt(int index)
        {
            if (index < 0) throw new System.IndexOutOfRangeException();

            this.SyncStates();
            if (index < _states.Count) return _states[index];
            else throw new System.IndexOutOfRangeException();
        }

        public TSub GetState<TSub>() where TSub : class, T
        {
            //if (_container == null) return null;
            //TSub comp = this.GetStates<TSub>().FirstOrDefault();
            //if (!comp.IsNullOrDestroyed())
            //    return comp;
            //else
            //    return null;

            this.SyncStates();
            var e = _states.GetEnumerator();
            while (e.MoveNext())
            {
                if (e.Current is TSub && !e.Current.IsNullOrDestroyed()) return e.Current as TSub;
            }
            return null;
        }

        public T GetState(System.Type tp)
        {
            if (tp == null) return null;

            //if (_container == null) return null;
            //T comp = this.GetStates(tp).FirstOrDefault();
            //if (!comp.IsNullOrDestroyed())
            //    return comp;
            //else
            //    return null;

            this.SyncStates();
            var e = _states.GetEnumerator();
            while (e.MoveNext())
            {
                if (TypeUtil.IsType(e.Current.GetType(), tp) && !e.Current.IsNullOrDestroyed()) return e.Current;
            }
            return null;
        }

        public T GetNext(T current)
        {
            //if (_container == null) return null;
            //return this.GetValueAfterOrDefault(current, true);

            this.SyncStates();
            return _states.GetValueAfterOrDefault(current, true);
        }

        #endregion

        #region State Machine Interface

        public event StateChangedEventHandler<T> StateChanged;

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

            using (var lst = com.spacepuppy.Collections.TempCollection.GetList<T>())
            {
                GetComponentsOnTarg(_container, lst, _includeStatesOnContainer);
                for(int i = 0; i < lst.Count; i++)
                {
                    coll.Add(lst[i]);
                }
                return lst.Count;
            }
        }

        public int Enumerate(System.Action<T> callback)
        {
            if (callback == null) return 0;
            if (_container == null) return 0;

            using (var lst = com.spacepuppy.Collections.TempCollection.GetList<T>())
            {
                GetComponentsOnTarg(_container, lst, _includeStatesOnContainer);
                for (int i = 0; i < lst.Count; i++)
                {
                    callback(lst[i]);
                }
                return lst.Count;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (_container == null) return System.Linq.Enumerable.Empty<T>().GetEnumerator();
            return (GetComponentsOnTarg(_container, _includeStatesOnContainer) as IEnumerable<T>).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            if (_container == null) return System.Linq.Enumerable.Empty<T>().GetEnumerator();
            return GetComponentsOnTarg(_container, _includeStatesOnContainer).GetEnumerator();
        }

        #endregion

        #region Static Interface

        public static T[] GetComponentsOnTarg(GameObject container, bool includeComponentsOnContainer)
        {
            using (var set = com.spacepuppy.Collections.TempCollection.GetSet<T>())
            {
                GetComponentsOnTarg(container, set, includeComponentsOnContainer);
                return set.ToArray();
            }
        }

        public static void GetComponentsOnTarg(GameObject container, ICollection<T> coll, bool includeComponentsOnContainer)
        {
            if (includeComponentsOnContainer)
            {
                ComponentUtil.GetComponents<T>(container, coll);
            }

            for (int i = 0; i < container.transform.childCount; i++)
            {
                ComponentUtil.GetComponents<T>(container.transform.GetChild(i), coll);
            }
        }

        public static TSub[] GetComponentsOnTarg<TSub>(GameObject container, bool includeComponentsOnContainer) where TSub : class, T
        {
            using (var set = com.spacepuppy.Collections.TempCollection.GetSet<TSub>())
            {
                if (includeComponentsOnContainer)
                {
                    ComponentUtil.GetComponents<TSub>(container, set);
                }

                for (int i = 0; i < container.transform.childCount; i++)
                {
                    ComponentUtil.GetComponents<TSub>(container.transform.GetChild(i), set);
                }

                return set.ToArray();
            }
        }

        public static T[] GetComponentsOnTarg(System.Type tp, GameObject container, bool includeComponentsOnContainer)
        {
            if (!TypeUtil.IsType(tp, typeof(T))) throw new TypeArgumentMismatchException(tp, typeof(T), "tp");

            using (var set = com.spacepuppy.Collections.TempCollection.GetSet<T>())
            {
                System.Func<Component, T> filter = (c) =>
                {
                    if (object.ReferenceEquals(c, null)) return null;

                    if (tp.IsAssignableFrom(c.GetType())) return c as T;
                    else return null;
                };

                if (includeComponentsOnContainer)
                {
                    ComponentUtil.GetComponents<T>(container, set, filter);
                }

                for (int i = 0; i < container.transform.childCount; i++)
                {
                    ComponentUtil.GetComponents<T>(container.transform.GetChild(i), set, filter);
                }

                return set.ToArray();
            }
        }

        #endregion

    }

}
