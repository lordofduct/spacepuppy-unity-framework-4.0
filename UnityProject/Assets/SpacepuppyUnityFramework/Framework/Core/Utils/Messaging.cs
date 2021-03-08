using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy.Collections;

namespace com.spacepuppy.Utils
{

    public static class Messaging
    {

        #region Standard Execute Methods

        public static void Signal<T>(this GameObject go, System.Action<T> functor, bool includeDisabledComponents = false) where T : class
        {
            if (functor == null) throw new System.ArgumentNullException("functor");

            using (var lst = TempCollection.GetList<T>())
            {
                go.GetComponents<T>(lst);
                if (lst.Count > 0)
                {
                    for (int i = 0; i < lst.Count; i++)
                    {
                        if (includeDisabledComponents || TargetIsValid(lst[i]))
                            functor(lst[i]);
                    }
                }
            }
        }

        public static void Signal<TInterface, TArg>(this GameObject go, TArg arg, System.Action<TInterface, TArg> functor, bool includeDisabledComponents = false) where TInterface : class
        {
            if (functor == null) throw new System.ArgumentNullException("functor");

            using (var lst = TempCollection.GetList<TInterface>())
            {
                go.GetComponents<TInterface>(lst);
                if (lst.Count > 0)
                {
                    for (int i = 0; i < lst.Count; i++)
                    {
                        if (includeDisabledComponents || TargetIsValid(lst[i]))
                            functor(lst[i], arg);
                    }
                }
            }
        }

        public static void Signal(this GameObject go, System.Type receiverType, System.Action<Component> functor, bool includeDisabledComponents = false)
        {
            if (functor == null) throw new System.ArgumentNullException("functor");

            using (var lst = TempCollection.GetList<Component>())
            {
                go.GetComponents(receiverType, lst);
                if (lst.Count > 0)
                {
                    for (int i = 0; i < lst.Count; i++)
                    {
                        if (includeDisabledComponents || TargetIsValid(lst[i]))
                            functor(lst[i]);
                    }
                }
            }
        }

        /// <summary>
        /// Broadcast message to all children of a GameObject
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="go"></param>
        /// <param name="functor"></param>
        /// <param name="includeInactive"></param>
        public static void Broadcast<T>(this GameObject go, System.Action<T> functor, bool includeInactiveObjects = false, bool includeDisabledComponents = false) where T : class
        {
            if (functor == null) throw new System.ArgumentNullException("functor");

            using (var lst = TempCollection.GetList<T>())
            {
                go.GetComponentsInChildren<T>(includeInactiveObjects, lst);
                if (lst.Count > 0)
                {
                    for (int i = 0; i < lst.Count; i++)
                    {
                        if (includeDisabledComponents || TargetIsValid(lst[i]))
                            functor(lst[i]);
                    }
                }
            }
        }

        /// <summary>
        /// Broadcast message to all children of a GameObject
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <typeparam name="TArg"></typeparam>
        /// <param name="go"></param>
        /// <param name="arg"></param>
        /// <param name="functor"></param>
        /// <param name="includeInactive"></param>
        public static void Broadcast<TInterface, TArg>(this GameObject go, TArg arg, System.Action<TInterface, TArg> functor, bool includeInactiveObjects = false, bool includeDisabledComponents = false) where TInterface : class
        {
            if (functor == null) throw new System.ArgumentNullException("functor");

            using (var lst = TempCollection.GetList<TInterface>())
            {
                go.GetComponentsInChildren<TInterface>(includeInactiveObjects, lst);
                if (lst.Count > 0)
                {
                    for (int i = 0; i < lst.Count; i++)
                    {
                        if (includeDisabledComponents || TargetIsValid(lst[i]))
                            functor(lst[i], arg);
                    }
                }
            }
        }

        /// <summary>
        /// Broadcast message to all children of a GameObject
        /// </summary>
        /// <param name="go"></param>
        /// <param name="functor"></param>
        /// <param name="includeInactive"></param>
        public static void Broadcast(this GameObject go, System.Type receiverType, System.Action<Component> functor, bool includeInactiveObjects = false, bool includeDisabledComponents = false)
        {
            if (functor == null) throw new System.ArgumentNullException("functor");

            //a alloc free version of GetComponentsInChildren by Type doesn't exist
            var arr = go.GetComponentsInChildren(receiverType, includeInactiveObjects);
            if (arr?.Length > 0)
            {
                for (int i = 0; i < arr.Length; i++)
                {
                    if (includeDisabledComponents || TargetIsValid(arr[i]))
                        functor(arr[i]);
                }
            }
        }

        public static void SignalUpwards<T>(this GameObject go, System.Action<T> functor, bool includeDisabledComponents = false) where T : class
        {
            var p = go.transform;
            while (p != null)
            {
                Signal<T>(p.gameObject, functor, includeDisabledComponents);
                p = p.parent;
            }
        }

        public static void SignalUpwards<TInterface, TArg>(this GameObject go, TArg arg, System.Action<TInterface, TArg> functor, bool includeDisabledComponents = false) where TInterface : class
        {
            var p = go.transform;
            while (p != null)
            {
                Signal<TInterface, TArg>(p.gameObject, arg, functor, includeDisabledComponents);
                p = p.parent;
            }
        }

        public static void SignalUpwards(this GameObject go, System.Type receiverType, System.Action<Component> functor, bool includeDisabledComponents = false)
        {
            var p = go.transform;
            while (p != null)
            {
                Signal(p.gameObject, receiverType, functor, includeDisabledComponents);
                p = p.parent;
            }
        }

        #endregion

        #region Global Execute

        /// <summary>
        /// Register a listener for a global Broadcast.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listener"></param>
        public static void RegisterGlobal<T>(T listener) where T : class
        {
            if (listener == null) throw new System.ArgumentNullException("listener");
            GlobalMessagePool<T>.Add(listener);
        }

        /// <summary>
        /// Register a listener for a global broadcast.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listener"></param>
        public static void UnregisterGlobal<T>(T listener) where T : class
        {
            if (object.ReferenceEquals(listener, null)) throw new System.ArgumentNullException("listener");
            GlobalMessagePool<T>.Remove(listener);
        }

        public static bool ContainsGlobalListener<T>(T listener) where T : class
        {
            if (object.ReferenceEquals(listener, null)) throw new System.ArgumentNullException("listener");
            return GlobalMessagePool<T>.Contains(listener);
        }

        /// <summary>
        /// Broadcast a message globally to all registered for T. This is faster than FindAndBroadcast, but requires manual registering/unregistering.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="functor"></param>
        /// <param name="includeDisabledComponents"></param>
        public static void Broadcast<T>(System.Action<T> functor) where T : class
        {
            if (functor == null) throw new System.ArgumentNullException("functor");

            GlobalMessagePool<T>.Signal(functor);
        }

        /// <summary>
        /// Broadcast a message globally to all registered for T. This is faster than FindAndBroadcast, but requires manual registering/unregistering.
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <typeparam name="TArg"></typeparam>
        /// <param name="arg"></param>
        /// <param name="functor"></param>
        /// <param name="includeDisabledComponents"></param>
        public static void Broadcast<TInterface, TArg>(TArg arg, System.Action<TInterface, TArg> functor) where TInterface : class
        {
            if (functor == null) throw new System.ArgumentNullException("functor");

            GlobalMessagePool<TInterface>.Signal<TArg>(arg, functor);
        }

        /// <summary>
        /// Broadcast a message globally to all that match T. This can be slow, use sparingly.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="functor"></param>
        public static void FindAndBroadcast<T>(System.Action<T> functor, bool includeDisabledComponents = false) where T : class
        {
            if (functor == null) throw new System.ArgumentNullException("functor");

            using (var coll = TempCollection.GetSet<T>())
            {
                ObjUtil.FindObjectsOfInterface<T>(coll);
                GlobalMessagePool<T>.CopyReceivers(coll);
                var e = coll.GetEnumerator();
                while (e.MoveNext())
                {
                    if (includeDisabledComponents || TargetIsValid(e.Current))
                        functor(e.Current);
                }
            }
        }

        /// <summary>
        /// Broadcast a message globally to all that match T. This can be slow, use sparingly.
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <typeparam name="TArg"></typeparam>
        /// <param name="arg"></param>
        /// <param name="functor"></param>
        public static void FindAndBroadcast<TInterface, TArg>(TArg arg, System.Action<TInterface, TArg> functor, bool includeDisabledComponents = false) where TInterface : class
        {
            if (functor == null) throw new System.ArgumentNullException("functor");

            using (var coll = TempCollection.GetSet<TInterface>())
            {
                ObjUtil.FindObjectsOfInterface<TInterface>(coll);
                GlobalMessagePool<TInterface>.CopyReceivers(coll);
                var e = coll.GetEnumerator();
                while (e.MoveNext())
                {
                    if (includeDisabledComponents || TargetIsValid(e.Current))
                        functor(e.Current, arg);
                }
            }
        }

        #endregion

        #region Broadcast Token

        /// <summary>
        /// Create a MessageToken to invoke at a later point.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="go"></param>
        /// <returns></returns>
        public static MessageToken<T> CreateSignalToken<T>(GameObject go) where T : class
        {
            if (object.ReferenceEquals(go, null)) throw new System.ArgumentNullException("go");

            return new MessageToken<T>(() => go.GetComponents<T>());
        }

        /// <summary>
        /// Create a MessageToken to invoke at a later point. If no targets found null is returned.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="go"></param>
        /// <returns></returns>
        public static MessageToken<T> CreateSignalUpwardsToken<T>(GameObject go) where T : class
        {
            if (object.ReferenceEquals(go, null)) throw new System.ArgumentNullException("go");

            return new MessageToken<T>(() =>
            {
                using (var lst = TempCollection.GetList<T>())
                {
                    var p = go.transform;
                    while (p != null)
                    {
                        p.GetComponents<T>(lst);
                        p = p.parent;
                    }

                    return lst.ToArray();
                }
            });
        }

        /// <summary>
        /// Create a MessageToken to invoke at a later point. If no targets found null is returned.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="go"></param>
        /// <param name="includeInactiveObjects"></param>
        /// <param name="includeDisabledComponents"></param>
        /// <returns></returns>
        public static MessageToken<T> CreateBroadcastToken<T>(GameObject go, bool includeInactiveObjects = false, bool includeDisabledComponents = false) where T : class
        {
            if (object.ReferenceEquals(go, null)) throw new System.ArgumentNullException("go");

            return new MessageToken<T>(() => go.GetComponentsInChildren<T>(includeInactiveObjects));
        }

        /// <summary>
        /// Create a MessageToken to invoke at a later point. If no targets found null is returned.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static MessageToken<T> CreateBroadcastToken<T>() where T : class
        {
            if (GlobalMessagePool<T>.Count == 0) return null;

            return new MessageToken<T>(() => GlobalMessagePool<T>.CopyReceivers());
        }

        /// <summary>
        /// Create a MessageToken to invoke at a later point. If no targets found null is returned.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="includeDisabledComponents"></param>
        /// <returns></returns>
        public static MessageToken<T> CreateFindAndBroadcastToken<T>(bool includeDisabledComponents = false) where T : class
        {
            return new MessageToken<T>(() =>
            {
                using (var lst = TempCollection.GetSet<T>())
                {
                    ObjUtil.FindObjectsOfInterface<T>(lst);
                    if (lst.Count == 0) return null;

                    if (includeDisabledComponents)
                        return lst.ToArray();

                    using (var lst2 = TempCollection.GetList<T>(lst.Count))
                    {
                        var e = lst.GetEnumerator();
                        while (e.MoveNext())
                        {
                            if (TargetIsValid(e.Current)) lst.Add(e.Current);
                        }

                        return lst2.ToArray();
                    }
                }
            });
        }

        public class MessageToken<T> where T : class
        {

            private System.Func<T[]> _getTargets;
            private T[] _targets;

            internal MessageToken(System.Func<T[]> getTargets)
            {
                if (getTargets == null) throw new System.ArgumentNullException("getTargets");
                _getTargets = getTargets;
            }

            public int Count
            {
                get { return GetTargets().Length; }
            }

            public void Invoke(System.Action<T> functor)
            {
                if (functor == null) throw new System.ArgumentNullException("functor");

                foreach (var t in GetTargets())
                {
                    functor(t);
                }
            }

            public void Invoke<TArg>(TArg arg, System.Action<T, TArg> functor)
            {
                if (functor == null) throw new System.ArgumentNullException("functor");

                foreach (var t in GetTargets())
                {
                    functor(t, arg);
                }
            }

            public void SetDirty()
            {
                _targets = null;
            }

            private T[] GetTargets()
            {
                if (_targets == null) _targets = _getTargets();
                return _targets;
            }

        }

        #endregion




        #region Internal Utils

        private static bool TargetIsValid(object obj)
        {
            if (obj is Behaviour) return (obj as Behaviour).isActiveAndEnabled;
            return true;
        }

        /// <summary>
        /// TODO - currently is not thread safe, need to add a check to make sure on the main thread OR make it thread safe.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private static class GlobalMessagePool<T> where T : class
        {

            private enum ExecutingState
            {
                None,
                Executing,
                CleaningUp
            }

            private static HashSet<T> _receivers;
            private static ExecutingState _state;
            private static TempHashSet<T> _toAdd;
            private static TempHashSet<T> _toRemove;

            public static int Count
            {
                get { return _receivers?.Count ?? 0; }
            }

            public static void Add(T listener)
            {
                if (_receivers == null) _receivers = new HashSet<T>();

                switch (_state)
                {
                    case ExecutingState.None:
                        _receivers.Add(listener);
                        break;
                    case ExecutingState.Executing:
                        if (_toAdd == null) _toAdd = TempCollection.GetSet<T>();
                        _toAdd.Add(listener);
                        break;
                    case ExecutingState.CleaningUp:
                        _receivers.Add(listener);
                        break;
                }
            }

            public static void Remove(T listener)
            {
                if (_receivers == null || _receivers.Count == 0) return;

                switch (_state)
                {
                    case ExecutingState.None:
                        _receivers.Remove(listener);
                        break;
                    case ExecutingState.Executing:
                        if (_toRemove == null) _toRemove = TempCollection.GetSet<T>();
                        _toRemove.Add(listener);
                        break;
                    case ExecutingState.CleaningUp:
                        _receivers.Remove(listener);
                        break;
                }
            }

            public static bool Contains(T listener)
            {
                return _receivers != null && _receivers.Contains(listener);
            }

            public static T[] CopyReceivers()
            {
                if (_receivers == null || _receivers.Count == 0) return ArrayUtil.Empty<T>();
                return _receivers.ToArray();
            }

            public static int CopyReceivers(ICollection<T> coll)
            {
                if (_receivers == null || _receivers.Count == 0) return 0;

                int cnt = coll.Count;
                var e = _receivers.GetEnumerator();
                while (e.MoveNext())
                {
                    coll.Add(e.Current);
                }
                return coll.Count - cnt;
            }

            public static void Signal(System.Action<T> functor)
            {
                if (_state != ExecutingState.None) throw new System.InvalidOperationException("Can not globally broadcast a message currently executing.");
                if (_receivers == null || _receivers.Count == 0) return;

                _state = ExecutingState.Executing;
                try
                {
                    var e = _receivers.GetEnumerator();
                    while (e.MoveNext())
                    {
                        if (e.Current is UnityEngine.Object && (e.Current as UnityEngine.Object) == null)
                        {
                            //skip & remove destroyed objects
                            Remove(e.Current);
                        }
                        else
                        {
                            try
                            {
                                functor(e.Current);
                            }
                            catch (System.Exception ex)
                            {
                                Debug.LogException(ex);
                            }
                        }
                    }
                }
                finally
                {
                    _state = ExecutingState.CleaningUp;

                    if (_toRemove != null)
                    {
                        var e = _toRemove.GetEnumerator();
                        while (e.MoveNext())
                        {
                            _receivers.Remove(e.Current);
                        }
                        _toRemove.Dispose();
                        _toRemove = null;
                    }

                    if (_toAdd != null)
                    {
                        var e = _toAdd.GetEnumerator();
                        while (e.MoveNext())
                        {
                            _receivers.Add(e.Current);
                        }
                        _toAdd.Dispose();
                        _toAdd = null;
                    }

                    _state = ExecutingState.None;
                }
            }

            public static void Signal<TArg>(TArg arg, System.Action<T, TArg> functor)
            {
                if (_state != ExecutingState.None) throw new System.InvalidOperationException("Can not globally broadcast a message currently executing.");
                if (_receivers == null || _receivers.Count == 0) return;

                _state = ExecutingState.Executing;
                try
                {
                    var e = _receivers.GetEnumerator();
                    while (e.MoveNext())
                    {
                        if (e.Current is UnityEngine.Object && (e.Current as UnityEngine.Object) == null)
                        {
                            //skip & remove destroyed objects
                            Remove(e.Current);
                        }
                        else
                        {
                            try
                            {
                                functor(e.Current, arg);
                            }
                            catch (System.Exception ex)
                            {
                                Debug.LogException(ex);
                            }
                        }
                    }
                }
                finally
                {
                    _state = ExecutingState.CleaningUp;

                    if (_toRemove != null)
                    {
                        var e = _toRemove.GetEnumerator();
                        while (e.MoveNext())
                        {
                            _receivers.Remove(e.Current);
                        }
                        _toRemove.Dispose();
                        _toRemove = null;
                    }

                    if (_toAdd != null)
                    {
                        var e = _toAdd.GetEnumerator();
                        while (e.MoveNext())
                        {
                            _receivers.Add(e.Current);
                        }
                        _toAdd.Dispose();
                        _toAdd = null;
                    }

                    _state = ExecutingState.None;
                }
            }

        }

        #endregion

    }

}
