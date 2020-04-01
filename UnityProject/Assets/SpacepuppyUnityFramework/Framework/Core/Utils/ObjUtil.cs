using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy.Collections;

namespace com.spacepuppy.Utils
{

    public static class ObjUtil
    {

        #region Fields

        private static System.Func<UnityEngine.Object, bool> _isObjectAlive;

        #endregion

        #region CONSTRUCTOR

        static ObjUtil()
        {
            try
            {
                var tp = typeof(UnityEngine.Object);
                var meth = tp.GetMethod("IsNativeObjectAlive", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                if (meth != null)
                {
                    var d = System.Delegate.CreateDelegate(typeof(System.Func<UnityEngine.Object, bool>), meth) as System.Func<UnityEngine.Object, bool>;
                    _isObjectAlive = (a) => !object.ReferenceEquals(a, null) && d(a);
                }
                else
                    _isObjectAlive = (a) => a != null;
            }
            catch
            {
                //incase there was a change to the UnityEngine.dll
                _isObjectAlive = (a) => a != null;
                UnityEngine.Debug.LogWarning("This version of Spacepuppy Framework does not support the version of Unity it's being used with. (ObjUtil)");
                //throw new System.InvalidOperationException("This version of Spacepuppy Framework does not support the version of Unity it's being used with.");
            }
        }

        #endregion

        #region Casting

        public static object ReduceIfProxy(this object obj)
        {
            if (obj is IProxy) return (obj as IProxy).GetTarget();

            return obj;
        }

        public static object ReduceIfProxy(this object obj, object arg)
        {
            if (obj is IProxy) return (obj as IProxy).GetTarget(arg);

            return obj;
        }


        public static T GetAsFromSource<T>(object obj) where T : class
        {
            if (obj == null) return null;
            if (obj is T) return obj as T;
            if (obj is IComponent)
            {
                var c = (obj as IComponent).component;
                if (c is T) return c as T;
            }
            var go = GameObjectUtil.GetGameObjectFromSource(obj);
            if (go is T) return go as T;

            //if (go != null && ComponentUtil.IsAcceptableComponentType(typeof(T))) return go.GetComponentAlt<T>();
            if (go != null)
            {
                var tp = typeof(T);
                //TODO - SPEntity
                //if (typeof(SPEntity).IsAssignableFrom(tp))
                //    return SPEntity.Pool.GetFromSource(tp, go) as T;
                //else if (ComponentUtil.IsAcceptableComponentType(tp))
                if (ComponentUtil.IsAcceptableComponentType(tp))
                    return go.GetComponent(tp) as T;
            }

            return null;
        }

        public static bool GetAsFromSource<T>(object obj, out T result, bool respectProxy = false) where T : class
        {
            result = null;
            if (obj == null) return false;

            if (respectProxy && obj is IProxy)
            {
                obj = (obj as IProxy).GetTarget();
                if (obj == null) return false;
            }

            if (obj is T)
            {
                result = obj as T;
                return true;
            }
            if (obj is IComponent)
            {
                var c = (obj as IComponent).component;
                if (c is T)
                {
                    result = c as T;
                    return true;
                }
            }
            var go = GameObjectUtil.GetGameObjectFromSource(obj);
            if (go is T)
            {
                result = go as T;
                return true;
            }

            //if (go != null && ComponentUtil.IsAcceptableComponentType(typeof(T))) return go.GetComponentAlt<T>();
            if (go != null)
            {
                var tp = typeof(T);
                //TODO - SPEntity
                //if (typeof(SPEntity).IsAssignableFrom(tp))
                //{
                //    var uobj = SPEntity.Pool.GetFromSource(tp, go);
                //    if (uobj == null) return false;

                //    result = uobj as T;
                //    return result != null;
                //}
                //else if (ComponentUtil.IsAcceptableComponentType(tp))
                if (ComponentUtil.IsAcceptableComponentType(tp))
                {
                    var uobj = go.GetComponent(tp);
                    if (uobj == null) return false;

                    result = uobj as T;
                    return result != null;
                }
            }

            return false;
        }

        public static object GetAsFromSource(System.Type tp, object obj)
        {
            if (obj == null) return null;

            var otp = obj.GetType();
            if (TypeUtil.IsType(otp, tp)) return obj;
            if (obj is IComponent)
            {
                var c = (obj as IComponent).component;
                if (!object.ReferenceEquals(c, null) && TypeUtil.IsType(c.GetType(), tp)) return c;
            }

            var go = GameObjectUtil.GetGameObjectFromSource(obj);
            if (tp == typeof(UnityEngine.GameObject)) return go;

            if (go != null)
            {
                //TODO - SPEntity
                //if (typeof(SPEntity).IsAssignableFrom(tp))
                //    return SPEntity.Pool.GetFromSource(tp, go);
                //else if (ComponentUtil.IsAcceptableComponentType(tp))
                if (ComponentUtil.IsAcceptableComponentType(tp))
                    return go.GetComponent(tp);
            }

            return null;
        }

        public static bool GetAsFromSource(System.Type tp, object obj, out object result, bool respectProxy = false)
        {
            result = null;
            if (obj == null) return false;

            if (respectProxy && obj is IProxy)
            {
                obj = (obj as IProxy).GetTarget();
                if (obj == null) return false;
            }

            var otp = obj.GetType();
            if (TypeUtil.IsType(otp, tp))
            {
                result = obj;
                return true;
            }
            if (obj is IComponent)
            {
                var c = (obj as IComponent).component;
                if (!object.ReferenceEquals(c, null) && TypeUtil.IsType(c.GetType(), tp))
                {
                    result = c;
                    return true;
                }
            }

            var go = GameObjectUtil.GetGameObjectFromSource(obj);
            if (tp == typeof(UnityEngine.GameObject))
            {
                result = go;
                return true;
            }

            if (go != null)
            {
                //TODO - SPEntity
                //if (typeof(SPEntity).IsAssignableFrom(tp))
                //{
                //    var uobj = SPEntity.Pool.GetFromSource(tp, go);
                //    if (uobj == null) return false;

                //    result = uobj;
                //    return true;
                //}
                //else if (ComponentUtil.IsAcceptableComponentType(tp))
                if (ComponentUtil.IsAcceptableComponentType(tp))
                {
                    var uobj = go.GetComponent(tp);
                    if (uobj == null) return false;

                    result = uobj;
                    return true;
                }
            }

            return false;
        }


        public static T GetAsFromSource<T>(object obj, bool respectProxy) where T : class
        {
            if (obj == null) return null;

            if (respectProxy && obj is IProxy)
            {
                obj = (obj as IProxy).GetTarget();
                if (obj == null) return null;
            }

            if (obj is T) return obj as T;
            if (obj is IComponent)
            {
                var c = (obj as IComponent).component;
                if (c is T) return c as T;
            }
            var go = GameObjectUtil.GetGameObjectFromSource(obj);
            if (go is T) return go as T;

            //if (go != null && ComponentUtil.IsAcceptableComponentType(typeof(T))) return go.GetComponentAlt<T>();
            if (go != null)
            {
                var tp = typeof(T);
                //TODO - SPEntity
                //if (typeof(SPEntity).IsAssignableFrom(tp))
                //    return SPEntity.Pool.GetFromSource(tp, go) as T;
                //else if (ComponentUtil.IsAcceptableComponentType(tp))
                if (ComponentUtil.IsAcceptableComponentType(tp))
                    return go.GetComponent(tp) as T;
            }

            return null;
        }

        public static object GetAsFromSource(System.Type tp, object obj, bool respectProxy)
        {
            if (obj == null) return null;

            if (respectProxy && obj is IProxy)
            {
                obj = (obj as IProxy).GetTarget();
                if (obj == null) return null;
            }

            var otp = obj.GetType();
            if (TypeUtil.IsType(otp, tp)) return obj;
            if (obj is IComponent)
            {
                var c = (obj as IComponent).component;
                if (!object.ReferenceEquals(c, null) && TypeUtil.IsType(c.GetType(), tp)) return c;
            }

            var go = GameObjectUtil.GetGameObjectFromSource(obj);
            if (tp == typeof(UnityEngine.GameObject)) return go;

            if (go != null)
            {
                //TODO - SPEntity
                //if (typeof(SPEntity).IsAssignableFrom(tp))
                //    return SPEntity.Pool.GetFromSource(tp, go);
                //else if (ComponentUtil.IsAcceptableComponentType(tp))
                if (ComponentUtil.IsAcceptableComponentType(tp))
                    return go.GetComponent(tp);
            }

            return null;
        }


        public static T[] GetAllFromSource<T>(object obj, bool includeChildren = false) where T : class
        {
            if (obj == null) return ArrayUtil.Empty<T>();

            using (var set = TempCollection.GetSet<T>())
            {
                if (obj is T) set.Add(obj as T);
                if (obj is IComponent)
                {
                    var c = (obj as IComponent).component;
                    if (c is T) set.Add(c as T);
                }

                var go = GameObjectUtil.GetGameObjectFromSource(obj);
                if (go is T) set.Add(go as T);

                //if (go != null && ComponentUtil.IsAcceptableComponentType(typeof(T))) go.GetComponentsAlt<T>(set);
                if (go != null)
                {
                    var tp = typeof(T);
                    //TODO - SPEntity
                    //if (typeof(SPEntity).IsAssignableFrom(tp))
                    //{
                    //    var entity = SPEntity.Pool.GetFromSource(tp, go) as T;
                    //    if (entity != null) set.Add(entity);
                    //}
                    //else if (typeof(UnityEngine.GameObject).IsAssignableFrom(tp))
                    if (typeof(UnityEngine.GameObject).IsAssignableFrom(tp))
                    {
                        if (includeChildren)
                        {
                            using (var lst = TempCollection.GetList<UnityEngine.Transform>())
                            {
                                go.GetComponentsInChildren<UnityEngine.Transform>(lst);

                                var e = lst.GetEnumerator();
                                while (e.MoveNext())
                                {
                                    set.Add(e.Current.gameObject as T);
                                }
                            }
                        }
                    }
                    if (ComponentUtil.IsAcceptableComponentType(tp))
                    {
                        if (includeChildren)
                            go.GetChildComponents<T>(set, true);
                        else
                            go.GetComponents<T>(set);
                    }
                }

                return set.Count > 0 ? set.ToArray() : ArrayUtil.Empty<T>();
            }
        }

        public static object[] GetAllFromSource(System.Type tp, object obj, bool includeChildren = false)
        {
            if (obj == null) return ArrayUtil.Empty<object>();

            using (var set = TempCollection.GetSet<object>())
            {
                var otp = obj.GetType();
                if (TypeUtil.IsType(otp, tp)) set.Add(obj);
                if (obj is IComponent)
                {
                    var c = (obj as IComponent).component;
                    if (!object.ReferenceEquals(c, null) && TypeUtil.IsType(c.GetType(), tp)) set.Add(c);
                }

                var go = GameObjectUtil.GetGameObjectFromSource(obj);
                if (go != null)
                {
                    //TODO - SPEntity
                    //if (typeof(SPEntity).IsAssignableFrom(tp))
                    //{
                    //    var entity = SPEntity.Pool.GetFromSource(tp, go);
                    //    if (entity != null) set.Add(entity);
                    //}
                    //else if (typeof(UnityEngine.GameObject).IsAssignableFrom(tp))
                    if (typeof(UnityEngine.GameObject).IsAssignableFrom(tp))
                    {
                        if (includeChildren)
                        {
                            using (var lst = TempCollection.GetList<UnityEngine.Transform>())
                            {
                                go.GetComponentsInChildren<UnityEngine.Transform>(lst);

                                var e = lst.GetEnumerator();
                                while (e.MoveNext())
                                {
                                    set.Add(e.Current.gameObject);
                                }
                            }
                        }
                        else
                        {
                            set.Add(go);
                        }
                    }
                    else if (ComponentUtil.IsAcceptableComponentType(tp))
                    {
                        using (var lst = TempCollection.GetList<UnityEngine.Component>())
                        {
                            if (includeChildren)
                                ComponentUtil.GetChildComponents(go, tp, lst, true);
                            else
                                go.GetComponents(tp, lst);

                            var e = lst.GetEnumerator();
                            while (e.MoveNext())
                            {
                                set.Add(e.Current);
                            }
                        }
                    }
                }

                return set.Count > 0 ? set.ToArray() : ArrayUtil.Empty<object>();
            }
        }

        public static bool IsType(object obj, System.Type tp)
        {
            if (obj == null) return false;

            return TypeUtil.IsType(obj.GetType(), tp);
        }

        public static bool IsType(object obj, System.Type tp, bool respectProxy)
        {
            if (obj == null) return false;

            if (respectProxy && obj is IProxy)
            {
                return TypeUtil.IsType((obj as IProxy).GetTargetType(), tp);
            }
            else
            {
                return TypeUtil.IsType(obj.GetType(), tp);
            }
        }

        #endregion

        #region Destruction Methods

        public static void SmartDestroy(UnityEngine.Object obj)
        {
            if (obj.IsNullOrDestroyed()) return;

            if (UnityEngine.Application.isEditor && !UnityEngine.Application.isPlaying)
            {
                UnityEngine.Object.DestroyImmediate(obj);
            }
            else
            {
                /*
                 * TODO - IKillable Interface
                 * 
                if (obj is UnityEngine.GameObject)
                    (obj as UnityEngine.GameObject).Kill();
                else if (obj is UnityEngine.Transform)
                    (obj as UnityEngine.Transform).gameObject.Kill();
                else
                    UnityEngine.Object.Destroy(obj);
                 */
                UnityEngine.Object.Destroy(obj);
            }
        }

        public static System.Func<UnityEngine.Object, bool> IsObjectAlive
        {
            get { return _isObjectAlive; }
        }

        /// <summary>
        /// Returns true if the object is either a null reference or has been destroyed by unity. 
        /// This will respect ISPDisposable over all else.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsNullOrDestroyed(this System.Object obj)
        {
            if (object.ReferenceEquals(obj, null)) return true;

            if (obj is ISPDisposable)
                return (obj as ISPDisposable).IsDisposed;
            else if (obj is UnityEngine.Object)
                return !_isObjectAlive(obj as UnityEngine.Object);
            else if (obj is UnityEngine.TrackedReference)
                return (obj as UnityEngine.TrackedReference) == null;
            else if (obj is IComponent)
                return !_isObjectAlive((obj as IComponent).component);
            else if (obj is IGameObjectSource)
                return !_isObjectAlive((obj as IGameObjectSource).gameObject);

            return false;
        }

        /// <summary>
        /// Unlike IsNullOrDestroyed, this only returns true if the managed object half of the object still exists, 
        /// but the unmanaged half has been destroyed by unity. 
        /// This will respect ISPDisposable over all else.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsDestroyed(this System.Object obj)
        {
            if (object.ReferenceEquals(obj, null)) return false;

            if (obj is ISPDisposable)
                return (obj as ISPDisposable).IsDisposed;
            else if (obj is UnityEngine.Object)
                return !_isObjectAlive(obj as UnityEngine.Object);
            else if (obj is UnityEngine.TrackedReference)
                return (obj as UnityEngine.TrackedReference) == null;
            else if (obj is IComponent)
                return !_isObjectAlive((obj as IComponent).component);
            else if (obj is IGameObjectSource)
                return !_isObjectAlive((obj as IGameObjectSource).gameObject);


            //if (obj is UnityEngine.Object)
            //    return (obj as UnityEngine.Object) == null;
            //else if (obj is IComponent)
            //    return (obj as IComponent).component == null;
            //else if (obj is IGameObjectSource)
            //    return (obj as IGameObjectSource).gameObject == null;

            return false;
        }

        public static bool IsAlive(this System.Object obj)
        {
            if (object.ReferenceEquals(obj, null)) return false;

            if (obj is ISPDisposable)
                return !(obj as ISPDisposable).IsDisposed;
            else if (obj is UnityEngine.Object)
                return _isObjectAlive(obj as UnityEngine.Object);
            else if (obj is IComponent)
                return _isObjectAlive((obj as IComponent).component);
            else if (obj is IGameObjectSource)
                return _isObjectAlive((obj as IGameObjectSource).gameObject);


            //if (obj is UnityEngine.Object)
            //    return (obj as UnityEngine.Object) != null;
            //else if (obj is IComponent)
            //    return (obj as IComponent).component != null;
            //else if (obj is IGameObjectSource)
            //    return (obj as IGameObjectSource).gameObject != null;

            return true;
        }

        /// <summary>
        /// Returns true if the object passed in is one of the many UnityEngine.Object's that unity will create that are invalid and report being null, but ReferenceEquals reports not being null. 
        /// This is opposed to 'IsDestroyed' since this object should have never existed in the first place.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsInvalidObject(UnityEngine.Object obj)
        {
            //note - we use 'GetHashCode' because it returns the instanceId BUT doesn't do the stupid main thread test
            return !object.ReferenceEquals(obj, null) && obj.GetHashCode() == 0;
        }

        /// <summary>
        /// Returns true if the Unity object passed in is not null but isn't necessarily alive/destroyed. 
        /// This is a compliment to IsInvalidObject for use when you want to determine if some value was ever once valid. 
        /// For example if in the inspector you leave a UnityEngin.Object field null, the serializer will actually populate it with an 'Invalid Object'. 
        /// If you ever needed to differentiate between if that value was ever a value but since destroyed VS being an invalid object, this will help.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsValidObject(UnityEngine.Object obj)
        {
            //note - we use 'GetHashCode' because it returns the instanceId BUT doesn't do the stupid main thread test
            return !object.ReferenceEquals(obj, null) && obj.GetHashCode() != 0;
        }

        #endregion

    }

}
 