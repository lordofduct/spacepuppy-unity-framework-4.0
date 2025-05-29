using UnityEngine;
using UnityEngine.Scripting;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.spacepuppy.Collections;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Utils
{

    public static class Messaging
    {

        #region Send By Method

        public enum MessageSendMethod
        {
            Broadcast = 0,
            Signal = 1,
            SignalUpward = 2,
            BroadcastEntity = 3,
            SignalEntity = 4,
        }

        [System.Serializable]
        public struct MessageSendCommand
        {
            public MessageSendMethod SendMethod;
            public bool IncludeInactiveObjects;
            public bool IncludeDisabledComponents;

            public MessageSendCommand(MessageSendMethod method, bool includeInactiveObjects = false, bool includeDisabledComponents = false)
            {
                this.SendMethod = method;
                this.IncludeInactiveObjects = includeInactiveObjects;
                this.IncludeDisabledComponents = includeDisabledComponents;
            }

            /// <summary>
            /// Returns true if 'target' is in the potential list of targets if a message were sent by 'sender' with this command's configuration.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="target"></param>
            /// <returns></returns>
            public bool IsInMessagePath(GameObject sender, GameObject target)
            {
                if (!sender || !target) return false;
                if (!IncludeInactiveObjects && !target.activeInHierarchy) return false;

                switch (this.SendMethod)
                {
                    case MessageSendMethod.Broadcast:
                        return target.transform.IsChildOf(sender.transform);
                    case MessageSendMethod.Signal:
                        return target == sender;
                    case MessageSendMethod.SignalUpward:
                        return sender.transform.IsChildOf(target.transform);
                    case MessageSendMethod.BroadcastEntity:
                        return target.transform.IsChildOf(SPEntity.Pool.GetFromSource(sender)?.transform ?? sender.transform);
                    case MessageSendMethod.SignalEntity:
                        return (SPEntity.Pool.GetFromSource(sender)?.gameObject ?? sender) == target;
                }

                return false;
            }

            public void Send<T>(GameObject go, System.Action<T> functor, System.Comparison<T> sort = null) where T : class
            {
                switch (this.SendMethod)
                {
                    case MessageSendMethod.Broadcast:
                        Broadcast<T>(go, functor, this.IncludeInactiveObjects, this.IncludeDisabledComponents, sort);
                        break;
                    case MessageSendMethod.Signal:
                        Signal<T>(go, functor, this.IncludeDisabledComponents, sort);
                        break;
                    case MessageSendMethod.SignalUpward:
                        SignalUpwards<T>(go, functor, this.IncludeDisabledComponents, sort);
                        break;
                    case MessageSendMethod.BroadcastEntity:
                        Broadcast<T>(SPEntity.Pool.GetFromSource(go)?.gameObject ?? go, functor, this.IncludeInactiveObjects, this.IncludeDisabledComponents, sort);
                        break;
                    case MessageSendMethod.SignalEntity:
                        Signal<T>(go, functor, this.IncludeDisabledComponents, sort);
                        break;
                }
            }

            public void Send<TInterface, TArg>(GameObject go, TArg arg, System.Action<TInterface, TArg> functor, System.Comparison<TInterface> sort = null) where TInterface : class
            {
                switch (this.SendMethod)
                {
                    case MessageSendMethod.Broadcast:
                        Broadcast<TInterface, TArg>(go, arg, functor, this.IncludeInactiveObjects, this.IncludeDisabledComponents, sort);
                        break;
                    case MessageSendMethod.Signal:
                        Signal<TInterface, TArg>(go, arg, functor, this.IncludeDisabledComponents, sort);
                        break;
                    case MessageSendMethod.SignalUpward:
                        SignalUpwards<TInterface, TArg>(go, arg, functor, this.IncludeDisabledComponents, sort);
                        break;
                    case MessageSendMethod.BroadcastEntity:
                        Broadcast<TInterface, TArg>(SPEntity.Pool.GetFromSource(go)?.gameObject ?? go, arg, functor, this.IncludeInactiveObjects, this.IncludeDisabledComponents, sort);
                        break;
                    case MessageSendMethod.SignalEntity:
                        Signal<TInterface, TArg>(SPEntity.Pool.GetFromSource(go)?.gameObject ?? go, arg, functor, this.IncludeDisabledComponents, sort);
                        break;
                }
            }

            public void Send(GameObject go, System.Type receiverType, System.Action<Component> functor, System.Comparison<Component> sort = null)
            {
                switch (this.SendMethod)
                {
                    case MessageSendMethod.Broadcast:
                        Broadcast(go, receiverType, functor, this.IncludeInactiveObjects, this.IncludeDisabledComponents, sort);
                        break;
                    case MessageSendMethod.Signal:
                        Signal(go, receiverType, functor, this.IncludeDisabledComponents, sort);
                        break;
                    case MessageSendMethod.SignalUpward:
                        SignalUpwards(go, receiverType, functor, this.IncludeDisabledComponents, sort);
                        break;
                    case MessageSendMethod.BroadcastEntity:
                        Broadcast(SPEntity.Pool.GetFromSource(go)?.gameObject ?? go, receiverType, functor, this.IncludeInactiveObjects, this.IncludeDisabledComponents, sort);
                        break;
                    case MessageSendMethod.SignalEntity:
                        Signal(SPEntity.Pool.GetFromSource(go)?.gameObject ?? go, receiverType, functor, this.IncludeDisabledComponents, sort);
                        break;
                }
            }

            /// <summary>
            /// If target is not in the message path of sender, then it'll subscribe to sender for the message if a subscription hook exists.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="sender"></param>
            /// <param name="target"></param>
            /// <returns></returns>
            public bool SubscribeIfNecessary<T>(GameObject sender, T target) where T : class
            {
                var go_target = GameObjectUtil.GetGameObjectFromSource(target);
                if (go_target && this.IsInMessagePath(sender, go_target)) return false;

                return sender.Subscribe<T>(target);
            }

        }

        #endregion

        #region Standard Execute Methods

        public static void Signal<T>(this GameObject go, System.Action<T> functor, bool includeDisabledComponents = false, System.Comparison<T> sort = null) where T : class
        {
            if (functor == null) throw new System.ArgumentNullException("functor");

            using (var lst = TempCollection.GetList<T>())
            {
                go.GetComponents<T>(lst);
                if (lst.Count > 0)
                {
                    if (sort != null) lst.Sort(sort);
                    for (int i = 0; i < lst.Count; i++)
                    {
                        if (includeDisabledComponents || TargetIsValid(lst[i]))
                        {
                            try
                            {
                                functor(lst[i]);
                            }
                            catch (System.Exception ex)
                            {
                                Debug.LogException(ex);
                            }
                        }
                    }
                }
            }
        }

        public static void Signal<TInterface, TArg>(this GameObject go, TArg arg, System.Action<TInterface, TArg> functor, bool includeDisabledComponents = false, System.Comparison<TInterface> sort = null) where TInterface : class
        {
            if (functor == null) throw new System.ArgumentNullException("functor");

            using (var lst = TempCollection.GetList<TInterface>())
            {
                go.GetComponents<TInterface>(lst);
                if (lst.Count > 0)
                {
                    if (sort != null) lst.Sort(sort);
                    for (int i = 0; i < lst.Count; i++)
                    {
                        if (includeDisabledComponents || TargetIsValid(lst[i]))
                        {
                            try
                            {
                                functor(lst[i], arg);
                            }
                            catch (System.Exception ex)
                            {
                                Debug.LogException(ex);
                            }
                        }
                    }
                }
            }
        }

        public static void Signal(this GameObject go, System.Type receiverType, System.Action<Component> functor, bool includeDisabledComponents = false, System.Comparison<Component> sort = null)
        {
            if (functor == null) throw new System.ArgumentNullException("functor");

            using (var lst = TempCollection.GetList<Component>())
            {
                go.GetComponents(receiverType, lst);
                if (lst.Count > 0)
                {
                    if (sort != null) lst.Sort(sort);
                    for (int i = 0; i < lst.Count; i++)
                    {
                        if (includeDisabledComponents || TargetIsValid(lst[i]))
                        {
                            try
                            {
                                functor(lst[i]);
                            }
                            catch (System.Exception ex)
                            {
                                Debug.LogException(ex);
                            }
                        }
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
        public static void Broadcast<T>(this GameObject go, System.Action<T> functor, bool includeInactiveObjects = false, bool includeDisabledComponents = false, System.Comparison<T> sort = null) where T : class
        {
            if (functor == null) throw new System.ArgumentNullException("functor");

            using (var lst = TempCollection.GetList<T>())
            {
                go.GetComponentsInChildren<T>(includeInactiveObjects, lst);
                if (lst.Count > 0)
                {
                    if (sort != null) lst.Sort(sort);
                    for (int i = 0; i < lst.Count; i++)
                    {
                        if (includeDisabledComponents || TargetIsValid(lst[i]))
                        {
                            try
                            {
                                functor(lst[i]);
                            }
                            catch (System.Exception ex)
                            {
                                Debug.LogException(ex);
                            }
                        }
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
        public static void Broadcast<TInterface, TArg>(this GameObject go, TArg arg, System.Action<TInterface, TArg> functor, bool includeInactiveObjects = false, bool includeDisabledComponents = false, System.Comparison<TInterface> sort = null) where TInterface : class
        {
            if (functor == null) throw new System.ArgumentNullException("functor");

            using (var lst = TempCollection.GetList<TInterface>())
            {
                go.GetComponentsInChildren<TInterface>(includeInactiveObjects, lst);
                if (lst.Count > 0)
                {
                    if (sort != null) lst.Sort(sort);
                    for (int i = 0; i < lst.Count; i++)
                    {
                        if (includeDisabledComponents || TargetIsValid(lst[i]))
                        {
                            try
                            {
                                functor(lst[i], arg);
                            }
                            catch (System.Exception ex)
                            {
                                Debug.LogException(ex);
                            }
                        }
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
        public static void Broadcast(this GameObject go, System.Type receiverType, System.Action<Component> functor, bool includeInactiveObjects = false, bool includeDisabledComponents = false, System.Comparison<Component> sort = null)
        {
            if (receiverType == null) throw new System.ArgumentNullException(nameof(receiverType));
            if (functor == null) throw new System.ArgumentNullException(nameof(functor));

            using (var lst = TempCollection.GetList<Component>())
            {
                go.GetComponentsInChildren<Component>(includeInactiveObjects, lst);
                if (lst.Count > 0)
                {
                    if (sort != null) lst.Sort(sort);
                    for (int i = 0; i < lst.Count; i++)
                    {
                        if (receiverType.IsInstanceOfType(lst[i]) && (includeDisabledComponents || TargetIsValid(lst[i])))
                        {
                            try
                            {
                                functor(lst[i]);
                            }
                            catch (System.Exception ex)
                            {
                                Debug.LogException(ex);
                            }
                        }
                    }
                }
            }
        }

        public static void SignalUpwards<T>(this GameObject go, System.Action<T> functor, bool includeDisabledComponents = false, System.Comparison<T> sort = null) where T : class
        {
            if (functor == null) throw new System.ArgumentNullException(nameof(functor));

            using (var lst = TempCollection.GetList<T>())
            {
                go.GetComponentsInParent<T>(includeDisabledComponents, lst);
                if (lst.Count > 0)
                {
                    if (sort != null) lst.Sort(sort);
                    for (int i = 0; i < lst.Count; i++)
                    {
                        if (includeDisabledComponents || TargetIsValid(lst[i]))
                        {
                            try
                            {
                                functor(lst[i]);
                            }
                            catch (System.Exception ex)
                            {
                                Debug.LogException(ex);
                            }
                        }
                    }
                }
            }
        }

        public static void SignalUpwards<TInterface, TArg>(this GameObject go, TArg arg, System.Action<TInterface, TArg> functor, bool includeDisabledComponents = false, System.Comparison<TInterface> sort = null) where TInterface : class
        {
            if (functor == null) throw new System.ArgumentNullException(nameof(functor));

            using (var lst = TempCollection.GetList<TInterface>())
            {
                go.GetComponentsInParent<TInterface>(includeDisabledComponents, lst);
                if (lst.Count > 0)
                {
                    if (sort != null) lst.Sort(sort);
                    for (int i = 0; i < lst.Count; i++)
                    {
                        if (includeDisabledComponents || TargetIsValid(lst[i]))
                        {
                            try
                            {
                                functor(lst[i], arg);
                            }
                            catch (System.Exception ex)
                            {
                                Debug.LogException(ex);
                            }
                        }
                    }
                }
            }
        }

        public static void SignalUpwards(this GameObject go, System.Type receiverType, System.Action<Component> functor, bool includeDisabledComponents = false, System.Comparison<Component> sort = null)
        {
            if (receiverType == null) throw new System.ArgumentNullException(nameof(receiverType));
            if (functor == null) throw new System.ArgumentNullException(nameof(functor));

            using (var lst = TempCollection.GetList<Component>())
            {
                go.GetComponentsInParent<Component>(includeDisabledComponents, lst);
                if (lst.Count > 0)
                {
                    if (sort != null) lst.Sort(sort);
                    for (int i = 0; i < lst.Count; i++)
                    {
                        if (receiverType.IsInstanceOfType(lst[i]) && (includeDisabledComponents || TargetIsValid(lst[i])))
                        {
                            try
                            {
                                functor(lst[i]);
                            }
                            catch (System.Exception ex)
                            {
                                Debug.LogException(ex);
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Global Execute

        /// <summary>
        /// Register a callback that will fire if the result of 'HasRegisteredGlobalListener<typeparamref name="T"/>' has changed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callback"></param>
        public static void AddHasRegisteredGlobalListenerChangedCallback<T>(System.Action callback) where T : class
        {
            GlobalMessagePool<T>.HasReceiversChanged += callback;
        }

        /// <summary>
        /// Remove a callback that would have fired if the result of 'HasRegisteredGlobalListener<typeparamref name="T"/>' has changed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callback"></param>
        public static void RemoveHasRegisteredGlobalListenerChangedCallback<T>(System.Action callback) where T : class
        {
            GlobalMessagePool<T>.HasReceiversChanged -= callback;
        }

        public static bool HasRegisteredGlobalListener<T>() where T : class => GlobalMessagePool<T>.Count > 0;

        public static bool IsExecutingGlobal<T>() where T : class => GlobalMessagePool<T>.IsExecuting;

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
        /// Broadcast a message globally to all registered for T. This is faster than FindAndBroadcast, but requires manual registering/unregistering.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="functor"></param>
        /// <param name="includeDisabledComponents"></param>
        public static void Broadcast<T>(System.Action<T> functor, System.Comparison<T> sort) where T : class
        {
            if (functor == null) throw new System.ArgumentNullException("functor");

            GlobalMessagePool<T>.Signal(functor, sort);
        }

        /// <summary>
        /// Broadcast a message globally to all registered for T. This is faster than FindAndBroadcast, but requires manual registering/unregistering.
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <typeparam name="TArg"></typeparam>
        /// <param name="arg"></param>
        /// <param name="functor"></param>
        /// <param name="includeDisabledComponents"></param>
        public static void Broadcast<TInterface, TArg>(TArg arg, System.Action<TInterface, TArg> functor, System.Comparison<TInterface> sort) where TInterface : class
        {
            if (functor == null) throw new System.ArgumentNullException("functor");

            GlobalMessagePool<TInterface>.Signal<TArg>(arg, functor, sort);
        }

        /// <summary>
        /// Broadcast a message globally to all that match T. This can be slow, use sparingly.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="functor"></param>
        public static void FindAndBroadcast<T>(System.Action<T> functor, bool includeDisabledComponents = false, System.Comparison<T> sort = null) where T : class
        {
            if (functor == null) throw new System.ArgumentNullException("functor");

            using (var coll = TempCollection.GetSet<T>())
            {
                ObjUtil.FindObjectsOfInterface<T>(coll);
                GlobalMessagePool<T>.CopyReceivers(coll);
                IEnumerable<T> iter = coll;
                if (sort != null) iter = iter.OrderBy(o => o, Comparer<T>.Create(sort));

                foreach (var o in iter)
                {
                    if (includeDisabledComponents || TargetIsValid(o))
                    {
                        try
                        {
                            functor(o);
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                    }
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
        public static void FindAndBroadcast<TInterface, TArg>(TArg arg, System.Action<TInterface, TArg> functor, bool includeDisabledComponents = false, System.Comparison<TInterface> sort = null) where TInterface : class
        {
            if (functor == null) throw new System.ArgumentNullException("functor");

            using (var coll = TempCollection.GetSet<TInterface>())
            {
                ObjUtil.FindObjectsOfInterface<TInterface>(coll);
                GlobalMessagePool<TInterface>.CopyReceivers(coll);
                IEnumerable<TInterface> iter = coll;
                if (sort != null) iter = iter.OrderBy(o => o, Comparer<TInterface>.Create(sort));

                foreach (var o in iter)
                {
                    if (includeDisabledComponents || TargetIsValid(o))
                    {
                        try
                        {
                            functor(o, arg);
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                    }
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
        public static MessageToken<T> CreateSignalToken<T>(GameObject go, bool includeDisabledComponents = false) where T : class
        {
            if (object.ReferenceEquals(go, null)) throw new System.ArgumentNullException("go");

            if (includeDisabledComponents)
            {
                return new MessageToken<T>(() => go.GetComponents<T>());
            }
            else
            {
                return new MessageToken<T>(() => go.GetComponents<T>().Where(o => TargetIsValid(o)).ToArray());
            } 
        }

        /// <summary>
        /// Create a MessageToken to invoke at a later point. If no targets found null is returned.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="go"></param>
        /// <returns></returns>
        public static MessageToken<T> CreateSignalUpwardsToken<T>(GameObject go, bool includeDisabledComponents = false) where T : class
        {
            if (object.ReferenceEquals(go, null)) throw new System.ArgumentNullException("go");

            if (includeDisabledComponents)
            {
                return new MessageToken<T>(() => go.GetComponentsInParent<T>());
            }
            else
            {
                return new MessageToken<T>(() => go.GetComponentsInParent<T>().Where(o => TargetIsValid(o)).ToArray());
            }
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

            if (includeDisabledComponents)
            {
                return new MessageToken<T>(() => go.GetComponentsInChildren<T>(includeInactiveObjects));
            }
            else
            {
                return new MessageToken<T>(() => go.GetComponentsInChildren<T>(includeInactiveObjects).Where(o => TargetIsValid(o)).ToArray());
            }
        }

        public static MessageToken<T> CreateBroadcastTokenIfReceiversExist<T>(GameObject go, bool includeInactiveObjects = false, bool includeDisabledComponents = false) where T : class
        {
            if (object.ReferenceEquals(go, null)) throw new System.ArgumentNullException("go");

            using (var lst = TempCollection.GetList<T>())
            {
                go.GetComponentsInChildren<T>(includeInactiveObjects, lst);
                int cnt = includeDisabledComponents ? lst.Count : lst.Count(o => TargetIsValid(o));

                if (cnt > 0)
                {
                    if (includeDisabledComponents)
                    {
                        return new MessageToken<T>(() => go.GetComponentsInChildren<T>(includeInactiveObjects));
                    }
                    else
                    {
                        return new MessageToken<T>(() => go.GetComponentsInChildren<T>(includeInactiveObjects).Where(o => TargetIsValid(o)).ToArray());
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Create a MessageToken to invoke at a later point. If no targets found null is returned.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static MessageToken<T> CreateBroadcastToken<T>() where T : class
        {
            return new MessageToken<T>(() => GlobalMessagePool<T>.CopyReceivers());
        }

        public static MessageToken<T> CreateBroadcastTokenIfReceiversExist<T>() where T : class
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

        public class MessageToken<T> : ISPDisposable where T : class
        {

            private System.Func<T[]> _getTargets;
            private T[] _targets;
            private System.Action<T> _paramterlessFunctorCache;

            internal MessageToken(System.Func<T[]> getTargets)
            {
                if (getTargets == null) throw new System.ArgumentNullException(nameof(getTargets));
                _getTargets = getTargets;
            }

            public int Count
            {
                get { return GetTargets().Length; }
            }

            public MessageToken<T> CacheFunctor(System.Action<T> functor)
            {
                if (this.IsDisposed) return this;
                _paramterlessFunctorCache = functor;
                return this;
            }

            public void InvokeCachedFunctor()
            {
                if (this.IsDisposed) return;
                if (_paramterlessFunctorCache != null) this.Invoke(_paramterlessFunctorCache);
            }

            public void Invoke(System.Action<T> functor)
            {
                if (functor == null) throw new System.ArgumentNullException(nameof(functor));
                if (this.IsDisposed) return;

                foreach (var t in GetTargets())
                {
                    try
                    {
                        functor(t);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
            }

            public void Invoke<TArg>(TArg arg, System.Action<T, TArg> functor)
            {
                if (functor == null) throw new System.ArgumentNullException(nameof(functor));
                if (this.IsDisposed) return;

                foreach (var t in GetTargets())
                {
                    {
                        try
                        {
                            functor(t, arg);
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                    }
                }
            }

            public void SetDirty()
            {
                _targets = null;
            }

            private T[] GetTargets()
            {
                if (_targets == null) _targets = _getTargets.Invoke();
                return _targets;
            }

            #region IDisposable Interface

            public bool IsDisposed => _getTargets == null;

            public void Dispose()
            {
                _targets = null;
                _getTargets = null;
                _paramterlessFunctorCache = null;
            }

            #endregion

        }

        #endregion

        #region Subscribe

        /// <summary>
        /// Tries to find a concrete SubscribableMessageHook for the message type, if it exists, it'll subscribe for that message on that gameobject. 
        /// You should create concrete SubscribableMessageHook<typeparamref name="T"/> types for each message you'd like to be able to subscribe to. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="go"></param>
        /// <param name="observer"></param>
        /// <returns>Returns true if subscription was successful</returns>
        public static bool Subscribe<T>(this GameObject go, T observer) where T : class => SubscribableMessageHook<T>.Subscribe(go, observer, out _);

        public static bool Subscribe<T>(this GameObject go, T observer, out ISubscribableMessageHook<T> hook) where T : class => SubscribableMessageHook<T>.Subscribe(go, observer, out hook);

        public static bool Subscribe<T, THook>(this GameObject go, T observer, out THook hook) where T : class where THook : Component, ISubscribableMessageHook<T> => SubscribableMessageHook<T>.Subscribe<THook>(go, observer, out hook);

        /// <summary>
        /// Removes a subscription for a message on a gameobject.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="go"></param>
        /// <param name="observer"></param>
        /// <returns>Returns true if the subscription was removed.</returns>
        public static bool Unsubscribe<T>(this GameObject go, T observer) where T : class => SubscribableMessageHook<T>.Unsubscribe(go, observer);

        #endregion


        #region Internal Utils

        private static bool TargetIsValid(object obj)
        {
            if (obj is Behaviour) return (obj as Behaviour).IsActiveAndEnabled_OrderAgnostic();
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

            public static event System.Action HasReceiversChanged;
            private static HashSet<T> _receivers;
            private static ExecutingState _state;
            private static TempHashSet<T> _toAdd;
            private static TempHashSet<T> _toRemove;

            public static int Count
            {
                get { return _receivers?.Count ?? 0; }
            }

            public static bool IsExecuting => _state != ExecutingState.None;

            public static void Add(T listener)
            {
                if (_receivers == null) _receivers = new HashSet<T>();

                switch (_state)
                {
                    case ExecutingState.None:
                        if (_receivers.Add(listener) && _receivers.Count == 1)
                        {
                            HasReceiversChanged?.Invoke();
                        }
                        break;
                    case ExecutingState.Executing:
                        if (_toAdd == null) _toAdd = TempCollection.GetSet<T>();
                        _toAdd.Add(listener);
                        break;
                    case ExecutingState.CleaningUp:
                        if (_receivers.Add(listener) && _receivers.Count == 1)
                        {
                            HasReceiversChanged?.Invoke();
                        }
                        break;
                }
            }

            public static void Remove(T listener)
            {
                if (_receivers == null || _receivers.Count == 0) return;

                switch (_state)
                {
                    case ExecutingState.None:
                        if (_receivers.Remove(listener) && _receivers.Count == 0)
                        {
                            HasReceiversChanged?.Invoke();
                        }
                        break;
                    case ExecutingState.Executing:
                        if (_toRemove == null) _toRemove = TempCollection.GetSet<T>();
                        _toRemove.Add(listener);
                        break;
                    case ExecutingState.CleaningUp:
                        if (_receivers.Remove(listener) && _receivers.Count == 0)
                        {
                            HasReceiversChanged?.Invoke();
                        }
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

                    int cnt = _receivers.Count;
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
                    if ((cnt > 0 && _receivers.Count == 0) || (cnt == 0 && _receivers.Count > 0))
                    {
                        HasReceiversChanged?.Invoke();
                    }
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

                    int cnt = _receivers.Count;
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
                    if ((cnt > 0 && _receivers.Count == 0) || (cnt == 0 && _receivers.Count > 0))
                    {
                        HasReceiversChanged?.Invoke();
                    }
                }
            }

            public static void Signal(System.Action<T> functor, System.Comparison<T> sort)
            {
                if (_state != ExecutingState.None) throw new System.InvalidOperationException("Can not globally broadcast a message currently executing.");
                if (_receivers == null || _receivers.Count == 0) return;

                _state = ExecutingState.Executing;
                try
                {
                    using (var lst = TempCollection.GetList<T>(_receivers))
                    {
                        lst.Sort(sort);
                        foreach (var o in lst)
                        {
                            if (o is UnityEngine.Object uo && uo == null)
                            {
                                _receivers.Remove(o);
                            }
                            else
                            {
                                try
                                {
                                    functor(o);
                                }
                                catch (System.Exception ex)
                                {
                                    Debug.LogException(ex);
                                }
                            }
                        }
                    }
                }
                finally
                {
                    _state = ExecutingState.None;
                    if (_receivers.Count == 0) HasReceiversChanged?.Invoke();
                }
            }

            public static void Signal<TArg>(TArg arg, System.Action<T, TArg> functor, System.Comparison<T> sort)
            {
                if (_state != ExecutingState.None) throw new System.InvalidOperationException("Can not globally broadcast a message currently executing.");
                if (_receivers == null || _receivers.Count == 0) return;

                _state = ExecutingState.Executing;
                try
                {
                    using (var lst = TempCollection.GetList<T>(_receivers))
                    {
                        lst.Sort(sort);
                        foreach (var o in lst)
                        {
                            if (o is UnityEngine.Object uo && uo == null)
                            {
                                _receivers.Remove(o);
                            }
                            else
                            {
                                try
                                {
                                    functor(o, arg);
                                }
                                catch (System.Exception ex)
                                {
                                    Debug.LogException(ex);
                                }
                            }
                        }
                    }
                }
                finally
                {
                    _state = ExecutingState.None;
                    if (_receivers.Count == 0) HasReceiversChanged?.Invoke();
                }
            }

        }

        #endregion

        #region Special Types

        public class SubscribableMessageConfigAttribute : System.Attribute
        {
            public int Precedence;
        }

        public interface ISubscribableMessageHook<T>
        {
            bool Subscribe(T observer);
            bool Unsubscribe(T observer);
        }

        public abstract class SubscribableMessageHook<T> : MonoBehaviour, ISubscribableMessageHook<T> where T : class
        {

            /// <summary>
            /// Set true if you'd like this hook to not auto destroy when no subscribers.
            /// </summary>
            [System.NonSerialized]
            private LockingHashSet<T> _observers = new();

            protected virtual void OnDestroy()
            {
                _observers = null;
            }

            /// <summary>
            /// Override this to keep the hook from being destroyed when the last observer is unsubscribed.
            /// </summary>
            /// <returns></returns>
            protected virtual bool PreserveOnUnsubscribe() => false;

            public int SubscriberCount => _observers?.Count ?? 0;

            [Preserve]
            public bool Subscribe(T observer)
            {
                if (observer == null || !ObjUtil.IsObjectAlive(this)) return false;

                if (_observers.Locked)
                {
                    if (_observers.Contains(observer)) return false;
                    _observers.Add(observer);
                    return true;
                }
                else
                {
                    return _observers.InnerCollection.Add(observer);
                }
            }

            [Preserve]
            public bool Unsubscribe(T observer)
            {
                if (!ObjUtil.IsObjectAlive(this)) return false;

                if (_observers.Locked)
                {
                    return observer != null && _observers.Remove(observer);
                }
                else
                {
                    bool result = observer != null && _observers.Remove(observer);
                    this.ValidateContinueUpdateLoop();
                    return result;
                }
            }

            public void Signal(System.Action<T> functor)
            {
                if (_observers == null || _observers.Count == 0) return;

                try
                {
                    _observers.Lock();
                    foreach (var o in _observers)
                    {
                        functor(o);
                    }
                }
                finally
                {
                    _observers.Unlock();
                    this.ValidateContinueUpdateLoop();
                }
            }

            public void Signal<TArg>(TArg arg, System.Action<T, TArg> functor)
            {
                if (_observers == null || _observers.Count == 0) return;

                try
                {
                    _observers.Lock();
                    foreach (var o in _observers)
                    {
                        functor(o, arg);
                    }
                }
                finally
                {
                    _observers.Unlock();
                    this.ValidateContinueUpdateLoop();
                }
            }

            protected virtual void ValidateContinueUpdateLoop()
            {
                if (this.SubscriberCount == 0 && !this.PreserveOnUnsubscribe())
                {
                    //we only destroy at the end of the frame if no new subscribers pick this up
                    if (GameLoop.LateUpdateWasCalled)
                    {
                        Destroy(this);
                    }
                    else
                    {
                        GameLoop.LateUpdateHandle.BeginInvoke(() =>
                        {
                            if (this.SubscriberCount == 0) Destroy(this);
                        });
                    }
                }
            }

            protected void LockObservers() { _observers.Lock(); }
            protected void UnlockObservers() { _observers.Unlock(); }
            protected HashSet<T>.Enumerator GetSubscriberEnumerator() => _observers != null ? _observers.GetEnumerator() : default;

            static readonly System.Type _hookType;
            static SubscribableMessageHook()
            {
                _hookType = TypeUtil.GetTypesAssignableFrom(typeof(ISubscribableMessageHook<T>)).Where(t => !t.IsGenericType && !t.IsAbstract).OrderByDescending(t => t.GetCustomAttribute<SubscribableMessageConfigAttribute>()?.Precedence ?? 0).FirstOrDefault();
            }

            internal static bool Subscribe(GameObject go, T observer, out ISubscribableMessageHook<T> hook)
            {
                if (_hookType == null)
                {
                    Debug.LogWarning($"Attempted to subscribe to message {typeof(T).Name} with no concrete SubscribableMessageHook in existence.");
                    hook = null;
                    return false;
                }
                if (go)
                {
                    hook = go.GetComponent<ISubscribableMessageHook<T>>();
                    if (hook.IsNullOrDestroyed() && _hookType != null) hook = go.AddComponent(_hookType) as ISubscribableMessageHook<T>;
                    return hook != null ? hook.Subscribe(observer) : false;
                }
                else
                {
                    hook = null;
                    return false;
                }
            }
            internal static bool Subscribe<THook>(GameObject go, T observer, out THook hook) where THook : Component, ISubscribableMessageHook<T>
            {
                if (go)
                {
                    hook = go.GetComponent<THook>();
                    if (hook == null) hook = go.AddComponent<THook>();
                    return hook != null ? hook.Subscribe(observer) : false;
                }
                else
                {
                    hook = null;
                    return false;
                }
            }
            internal static bool Unsubscribe(GameObject go, T observer) => _hookType != null && go && ((go.GetComponent(_hookType) as ISubscribableMessageHook<T>)?.Unsubscribe(observer) ?? false);

        }

        #endregion

    }

}
