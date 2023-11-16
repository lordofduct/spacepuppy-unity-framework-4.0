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

        private static Dictionary<System.Type, System.Type[]> _interfaceToComponentMap;

        private static System.Type[] GetInterfaceComponentMap(System.Type tp)
        {
            if (!tp.IsInterface) throw new System.ArgumentException("Generic Type is not an interface.");

            System.Type[] arr;
            if (_interfaceToComponentMap != null && _interfaceToComponentMap.TryGetValue(tp, out arr))
                return arr;

            System.Type utp = typeof(UnityEngine.Object);
            arr = (from t in TypeUtil.GetTypesAssignableFrom(tp)
                   where utp.IsAssignableFrom(t)
                   select t).ToArray();
            if (_interfaceToComponentMap == null) _interfaceToComponentMap = new Dictionary<System.Type, System.Type[]>();
            _interfaceToComponentMap[tp] = arr;

            return arr;
        }

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
                {
                    _isObjectAlive = (a) => a != null;
                    UnityEngine.Debug.LogWarning("This version of Spacepuppy Framework does not support the version of Unity it's being used with. (ObjUtil)");
                }
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

        #region Find Methods

        public static T Find<T>(IEnumerable<T> objs, SearchBy search, string query) where T : class
        {
            GameObject go;
            foreach (var obj in objs)
            {
                switch (search)
                {
                    case SearchBy.Nothing:
                        return obj;
                    case SearchBy.Tag:
                        {
                            go = GameObjectUtil.GetGameObjectFromSource(obj);
                            if (go.HasTag(query)) return obj;
                        }
                        break;
                    case SearchBy.Name:
                        if (obj is INameable nm)
                        {
                            if (nm.CompareName(query)) return obj;
                        }
                        else
                        {
                            go = GameObjectUtil.GetGameObjectFromSource(obj);
                            if (go.name == query) return obj;
                        }
                        break;
                    case SearchBy.Type:
                        {
                            go = GameObjectUtil.GetGameObjectFromSource(obj);
                            var c = go.GetComponent(query);
                            if (c != null) return obj;
                        }
                        break;
                }
            }

            return null;
        }

        public static UnityEngine.Object Find(SearchBy search, string query)
        {
            switch (search)
            {
                case SearchBy.Nothing:
                    return null;
                case SearchBy.Tag:
                    return GameObjectUtil.FindWithMultiTag(query);
                case SearchBy.Name:
                    return UnityEngine.GameObject.Find(query);
                case SearchBy.Type:
                    return ObjUtil.FindObjectOfType(TypeUtil.FindType(query));
                default:
                    return null;
            }
        }

        public static T Find<T>(SearchBy search, string query) where T : class
        {

            if (typeof(T) == typeof(GameObject) || typeof(T) == typeof(Transform))
            {
                switch (search)
                {
                    case SearchBy.Tag:
                        return ObjUtil.GetAsFromSource<T>(GameObjectUtil.FindWithMultiTag(query));
                    case SearchBy.Name:
                        return ObjUtil.GetAsFromSource<T>(UnityEngine.GameObject.Find(query));
                    case SearchBy.Type:
                        return ObjUtil.GetAsFromSource<T>(ObjUtil.FindObjectOfType(TypeUtil.FindType(query)));
                    default:
                        return null;
                }
            }
            else
            {
                using (var lst = TempCollection.GetList<T>())
                {
                    if (FindAll<T>(search, query, lst) > 0)
                    {
                        return lst[0];
                    }
                    else
                    {
                        return default;
                    }
                }
            }
        }


        public static UnityEngine.Object[] FindAll(SearchBy search, string query)
        {
            switch (search)
            {
                case SearchBy.Nothing:
                    return ArrayUtil.Empty<UnityEngine.Object>();
                case SearchBy.Tag:
                    return GameObjectUtil.FindGameObjectsWithMultiTag(query);
                case SearchBy.Name:
                    {
                        using (var tmp = com.spacepuppy.Collections.TempCollection.GetList<UnityEngine.GameObject>())
                        {
                            GameObjectUtil.FindAllByName(query, tmp);
                            return tmp.ToArray();
                        }
                    }
                case SearchBy.Type:
                    return ObjUtil.FindObjectsOfType(TypeUtil.FindType(query));
                default:
                    return null;
            }
        }

        public static UnityEngine.Object[] FindAll(SearchBy search, string query, System.Type tp)
        {
            switch (search)
            {
                case SearchBy.Nothing:
                    return ArrayUtil.Empty<UnityEngine.Object>();
                case SearchBy.Tag:
                    {
                        using (var tmp = com.spacepuppy.Collections.TempCollection.GetList<UnityEngine.GameObject>())
                        using (var results = com.spacepuppy.Collections.TempCollection.GetList<UnityEngine.Object>())
                        {
                            GameObjectUtil.FindGameObjectsWithMultiTag(query, tmp);
                            var e = tmp.GetEnumerator();
                            while (e.MoveNext())
                            {
                                var o = ObjUtil.GetAsFromSource(tp, e.Current) as UnityEngine.Object;
                                if (o != null) results.Add(o);
                            }
                            return results.ToArray();
                        }
                    }
                case SearchBy.Name:
                    {
                        using (var tmp = com.spacepuppy.Collections.TempCollection.GetList<UnityEngine.GameObject>())
                        using (var results = com.spacepuppy.Collections.TempCollection.GetList<UnityEngine.Object>())
                        {
                            GameObjectUtil.FindAllByName(query, tmp);
                            var e = tmp.GetEnumerator();
                            while (e.MoveNext())
                            {
                                var o = ObjUtil.GetAsFromSource(tp, e.Current) as UnityEngine.Object;
                                if (o != null) results.Add(o);
                            }
                            return results.ToArray();
                        }
                    }
                case SearchBy.Type:
                    {
                        using (var results = com.spacepuppy.Collections.TempCollection.GetList<UnityEngine.Object>())
                        {
                            foreach (var o in ObjUtil.FindObjectsOfType(TypeUtil.FindType(query)))
                            {
                                var o2 = ObjUtil.GetAsFromSource(tp, o) as UnityEngine.Object;
                                if (o2 != null) results.Add(o2);
                            }
                            return results.ToArray();
                        }
                    }
                default:
                    return null;
            }
        }

        public static T[] FindAll<T>(SearchBy search, string query) where T : class
        {
            using (var results = com.spacepuppy.Collections.TempCollection.GetList<T>())
            {
                int cnt = FindAll<T>(search, query, results);
                return cnt > 0 ? results.ToArray() : ArrayUtil.Empty<T>();
            }
        }

        public static int FindAll<T>(SearchBy search, string query, ICollection<T> results) where T : class
        {
            int cnt;
            switch (search)
            {
                case SearchBy.Nothing:
                    return 0;
                case SearchBy.Tag:
                    {
                        using (var tmp = com.spacepuppy.Collections.TempCollection.GetList<UnityEngine.GameObject>())
                        {
                            GameObjectUtil.FindGameObjectsWithMultiTag(query, tmp);
                            var e = tmp.GetEnumerator();
                            cnt = 0;
                            while (e.MoveNext())
                            {
                                var o = ObjUtil.GetAsFromSource<T>(e.Current);
                                if (o != null)
                                {
                                    cnt++;
                                    results.Add(o);
                                }
                            }
                            return cnt;
                        }
                    }
                case SearchBy.Name:
                    {
                        using (var tmp = com.spacepuppy.Collections.TempCollection.GetList<UnityEngine.GameObject>())
                        {
                            GameObjectUtil.FindAllByName(query, tmp);
                            var e = tmp.GetEnumerator();
                            cnt = 0;
                            while (e.MoveNext())
                            {
                                var o = ObjUtil.GetAsFromSource<T>(e.Current);
                                if (o != null)
                                {
                                    cnt++;
                                    results.Add(o);
                                }
                            }
                            return cnt;
                        }
                    }
                case SearchBy.Type:
                    {
                        cnt = 0;
                        foreach (var o in ObjUtil.FindObjectsOfType(TypeUtil.FindType(query)))
                        {
                            var o2 = ObjUtil.GetAsFromSource<T>(o);
                            if (o2 != null)
                            {
                                cnt++;
                                results.Add(o2);
                            }
                        }
                        return cnt;
                    }
                default:
                    return 0;
            }
        }


        public static UnityEngine.Object FindObjectOfType(System.Type tp)
        {
            if (tp == null) return null;

            if (tp.IsInterface)
            {
                var map = GetInterfaceComponentMap(tp);
                if (map.Length == 0) return null;

                foreach (var ctp in map)
                {
                    var obj = UnityEngine.Object.FindObjectOfType(ctp);
                    if (obj != null) return obj;
                }
            }
            else
            {
                return UnityEngine.Object.FindObjectOfType(tp);
            }

            return null;
        }

        public static UnityEngine.Object[] FindObjectsOfType(System.Type tp)
        {
            if (tp == null) return ArrayUtil.Empty<UnityEngine.Object>();

            if (tp.IsInterface)
            {
                var map = GetInterfaceComponentMap(tp);
                using (var lst = TempCollection.GetList<UnityEngine.Object>())
                {
                    foreach (var ctp in map)
                    {
                        lst.AddRange(UnityEngine.Object.FindObjectsOfType(ctp));
                    }
                    return lst.ToArray();
                }
            }
            else
            {
                return UnityEngine.Object.FindObjectsOfType(tp);
            }
        }

        public static int FindObjectsOfType(System.Type tp, ICollection<UnityEngine.Object> lst)
        {
            if (tp == null) return 0;

            if (tp.IsInterface)
            {
                var map = GetInterfaceComponentMap(tp);
                int cnt = 0;
                foreach (var ctp in map)
                {
                    var arr = UnityEngine.Object.FindObjectsOfType(ctp);
                    cnt += arr.Length;
                    lst.AddRange(arr);
                }
                return cnt;
            }
            else
            {
                var arr = UnityEngine.Object.FindObjectsOfType(tp);
                foreach (var obj in arr)
                {
                    lst.Add(obj);
                }
                return arr.Length;
            }
        }


        public static bool TryFindObjectOfType<T>(out T result) where T : UnityEngine.Object
        {
            result = UnityEngine.Object.FindObjectOfType<T>();
            return result != null;
        }

        public static bool TryFindObjectOfType(System.Type tp, out UnityEngine.Object result)
        {
            result = FindObjectOfType(tp);
            return result != null;
        }


        public static T FindObjectOfInterface<T>() where T : class
        {
            var tp = typeof(T);
            var map = GetInterfaceComponentMap(tp);
            if (map.Length == 0) return null;

            foreach (var ctp in map)
            {
                var obj = UnityEngine.Object.FindObjectOfType(ctp);
                if (obj != null) return obj as T;
            }

            return null;
        }

        public static T[] FindObjectsOfInterface<T>() where T : class
        {
            var tp = typeof(T);
            var map = GetInterfaceComponentMap(tp);
            using (var lst = TempCollection.GetSet<T>())
            {
                foreach (var ctp in map)
                {
                    foreach (var obj in UnityEngine.Object.FindObjectsOfType(ctp))
                    {
                        lst.Add(obj as T);
                    }
                }
                return lst.ToArray();
            }
        }

        public static int FindObjectsOfInterface<T>(ICollection<T> lst) where T : class
        {
            var tp = typeof(T);
            var map = GetInterfaceComponentMap(tp);
            int cnt = 0;
            foreach (var ctp in map)
            {
                var arr = UnityEngine.Object.FindObjectsOfType(ctp);
                cnt += arr.Length;
                foreach (var obj in arr)
                {
                    lst.Add(obj as T);
                }
            }
            return cnt;
        }

        public static bool TryFindObjectOfInterface<T>(out T result) where T : class
        {
            result = FindObjectOfInterface<T>();
            return result != null;
        }

        #endregion

        #region Casting

        /// <summary>
        /// Returns true null if the UnityEngine.Object is dead, otherwise returns itself.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T SanitizeRef<T>(this T obj) where T : class
        {
            return (obj is UnityEngine.Object && (obj as UnityEngine.Object) == null) ? null : obj;
        }

        public static T GetAsFromSource<T>(object obj) where T : class
        {
            obj = obj.SanitizeRef();
            if (obj == null) return null;
            if (obj is T o1) return o1;
            if (obj is IComponent ic && ic.component is T o2) return o2;

            var go = GameObjectUtil.GetGameObjectFromSource(obj);
            if (go is T g) return g;

            if (go != null)
            {
                var tp = typeof(T);
                if (typeof(SPEntity).IsAssignableFrom(tp))
                    return SPEntity.Pool.GetFromSource(tp, go) as T;
                else if (ComponentUtil.IsAcceptableComponentType(tp))
                    return go.GetComponent(tp) as T;
            }

            return null;
        }

        public static T GetAsFromSource<T>(object obj, bool respectProxy) where T : class
        {
            obj = respectProxy ? obj.ReduceIfProxyAs(typeof(T)) : obj.SanitizeRef();
            if (obj == null) return null;

            if (obj is T o1) return o1;
            if (obj is IComponent ic && ic.component is T o2) return o2;

            var go = GameObjectUtil.GetGameObjectFromSource(obj);
            if (go is T g) return g;

            if (go != null)
            {
                var tp = typeof(T);
                if (typeof(SPEntity).IsAssignableFrom(tp))
                    return SPEntity.Pool.GetFromSource(tp, go) as T;
                else if (ComponentUtil.IsAcceptableComponentType(tp))
                    return go.GetComponent(tp) as T;
            }

            return null;
        }

        public static bool GetAsFromSource<T>(object obj, out T result, bool respectProxy = false) where T : class
        {
            result = null;

            obj = respectProxy ? obj.ReduceIfProxyAs(typeof(T)) : obj.SanitizeRef();
            if (obj == null) return false;

            if (obj is T o1)
            {
                result = o1;
                return true;
            }
            if (obj is IComponent ic && ic.component is T o2)
            {
                result = o2;
                return true;
            }

            var go = GameObjectUtil.GetGameObjectFromSource(obj);
            if (go is T g)
            {
                result = g;
                return true;
            }

            if (go != null)
            {
                var tp = typeof(T);
                if (typeof(SPEntity).IsAssignableFrom(tp))
                {
                    var uobj = SPEntity.Pool.GetFromSource(tp, go);
                    if (uobj == null) return false;

                    result = uobj as T;
                    return result != null;
                }
                else if (ComponentUtil.IsAcceptableComponentType(tp))
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
            obj = obj.SanitizeRef();
            if (obj == null) return null;

            if (tp.IsInstanceOfType(obj)) return obj;
            if (obj is IComponent)
            {
                var c = (obj as IComponent).component;
                if (!object.ReferenceEquals(c, null) && tp.IsInstanceOfType(c)) return c;
            }

            var go = GameObjectUtil.GetGameObjectFromSource(obj);
            if (tp == typeof(UnityEngine.GameObject)) return go;

            if (go != null)
            {
                if (typeof(SPEntity).IsAssignableFrom(tp))
                    return SPEntity.Pool.GetFromSource(tp, go);
                else if (ComponentUtil.IsAcceptableComponentType(tp))
                    return go.GetComponent(tp);
            }

            return null;
        }

        public static object GetAsFromSource(System.Type tp, object obj, bool respectProxy)
        {
            obj = respectProxy ? obj.ReduceIfProxyAs(tp) : obj.SanitizeRef();
            if (obj == null) return null;

            if (tp.IsInstanceOfType(obj)) return obj;
            if (obj is IComponent)
            {
                var c = (obj as IComponent).component;
                if (!object.ReferenceEquals(c, null) && tp.IsInstanceOfType(c)) return c;
            }

            var go = GameObjectUtil.GetGameObjectFromSource(obj);
            if (tp == typeof(UnityEngine.GameObject)) return go;

            if (go != null)
            {
                if (typeof(SPEntity).IsAssignableFrom(tp))
                    return SPEntity.Pool.GetFromSource(tp, go);
                else if (ComponentUtil.IsAcceptableComponentType(tp))
                    return go.GetComponent(tp);
            }

            return null;
        }

        public static bool GetAsFromSource(System.Type tp, object obj, out object result, bool respectProxy = false)
        {
            result = null;

            obj = respectProxy ? obj.ReduceIfProxyAs(tp) : obj.SanitizeRef();
            if (obj == null) return false;

            if (tp.IsInstanceOfType(obj))
            {
                result = obj;
                return true;
            }
            if (obj is IComponent)
            {
                var c = (obj as IComponent).component;
                if (!object.ReferenceEquals(c, null) && tp.IsInstanceOfType(c))
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
                if (typeof(SPEntity).IsAssignableFrom(tp))
                {
                    var uobj = SPEntity.Pool.GetFromSource(tp, go);
                    if (uobj == null) return false;

                    result = uobj;
                    return true;
                }
                else if (ComponentUtil.IsAcceptableComponentType(tp))
                {
                    var uobj = go.GetComponent(tp);
                    if (uobj == null) return false;

                    result = uobj;
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Returns a ref to the object as the first type in the array that it can resolve as.
        /// </summary>
        /// <param name="types"></param>
        /// <param name="obj"></param>
        /// <param name="respectProxy"></param>
        /// <returns></returns>
        public static object GetAsFromSource(System.Type[] types, object obj, bool respectProxy = false)
        {
            object result;
            GetAsFromSource(types, obj, out result, respectProxy);
            return result;
        }

        /// <summary>
        /// Returns a ref to the object as the first type in the array that it can resolve as.
        /// </summary>
        /// <param name="types"></param>
        /// <param name="obj"></param>
        /// <param name="respectProxy"></param>
        /// <returns></returns>
        public static bool GetAsFromSource(System.Type[] types, object obj, out object result, bool respectProxy = false)
        {
            if (types != null)
            {
                foreach (var tp in types)
                {
                    if (tp?.IsInstanceOfType(obj) ?? false)
                    {
                        result = obj;
                        return true;
                    }
                }

                foreach (var tp in types)
                {
                    if (GetAsFromSource(tp, obj, out result, respectProxy)) return true;
                }
            }

            result = null;
            return false;
        }

        public static bool IsType(object obj, System.Type tp)
        {
            if (obj == null) return false;

            return tp.IsInstanceOfType(obj);
        }

        public static bool IsType(object obj, System.Type tp, bool respectProxy)
        {
            if (obj == null) return false;

            if (respectProxy && obj is IProxy p)
            {
                if (p.PrioritizesSelfAsTarget() && ObjUtil.IsType(obj, tp)) return true;
                if (TypeUtil.IsType(p.GetTargetType(), tp)) return true;
#if UNITY_EDITOR
                if (Application.isPlaying || (p.Params & ProxyParams.QueriesTarget) == 0) return ObjUtil.IsType(p.GetTarget(), tp);
                return false;
#else
                return ObjUtil.IsType(p.GetTarget_ParamsRespecting(), tp);
#endif
            }
            else
            {
                return tp.IsInstanceOfType(obj);
            }
        }

        #endregion

        #region Casting All

        public static T[] GetAllFromSource<T>(object obj, bool includeChildren = false) where T : class
        {
            obj = obj.SanitizeRef();
            if (obj == null) return ArrayUtil.Empty<T>();

            using (var lst = TempCollection.GetList<T>())
            {
                GetAllFromSource<T>(lst, obj, includeChildren);
                return lst.ToArray();
            }
        }

        public static int GetAllFromSource<T>(ICollection<T> coll, object obj, bool includeChildren = false) where T : class
        {
            obj = obj.SanitizeRef();
            if (obj == null) return 0;

            using (var set = TempCollection.GetSet<T>())
            {
                if (obj is T && set.Add(obj as T)) coll.Add(obj as T);
                if (obj is IComponent)
                {
                    var c = (obj as IComponent).component;
                    if (c is T && set.Add(c as T)) coll.Add(c as T);
                }

                var go = GameObjectUtil.GetGameObjectFromSource(obj);
                if (go is T && set.Add(go as T)) coll.Add(go as T);

                //if (go != null && ComponentUtil.IsAcceptableComponentType(typeof(T))) go.GetComponentsAlt<T>(set);
                if (go != null)
                {
                    var tp = typeof(T);
                    if (typeof(SPEntity).IsAssignableFrom(tp))
                    {
                        var entity = SPEntity.Pool.GetFromSource(tp, go) as T;
                        if (entity != null && set.Add(entity)) coll.Add(entity);
                    }
                    else if (typeof(UnityEngine.GameObject).IsAssignableFrom(tp))
                    {
                        if (includeChildren)
                        {
                            using (var lst = TempCollection.GetList<UnityEngine.Transform>())
                            {
                                go.GetComponentsInChildren<UnityEngine.Transform>(lst);

                                var e = lst.GetEnumerator();
                                while (e.MoveNext())
                                {
                                    if (set.Add(e.Current.gameObject as T)) coll.Add(e.Current.gameObject as T);
                                }
                            }
                        }
                    }

                    if (ComponentUtil.IsAcceptableComponentType(tp))
                    {
                        using (var lst = TempCollection.GetList<T>())
                        {
                            if (includeChildren)
                                go.GetChildComponents<T>(lst, true);
                            else
                                go.GetComponents<T>(lst);

                            var e = lst.GetEnumerator();
                            while (e.MoveNext())
                            {
                                if (set.Add(e.Current)) coll.Add(e.Current);
                            }
                        }
                    }
                }

                return set.Count;
            }
        }

        public static object[] GetAllFromSource(System.Type tp, object obj, bool includeChildren = false)
        {
            using (var set = TempCollection.GetSet<object>())
            {
                GetAllFromSource(set, tp, obj, includeChildren);
                return set.Count > 0 ? set.ToArray() : ArrayUtil.Empty<object>();
            }
        }

        public static int GetAllFromSource(ICollection<object> coll, System.Type tp, object obj, bool includeChildren = false)
        {
            obj = obj.SanitizeRef();
            if (obj == null) return 0;

            bool dispose = false;
            HashSet<object> set = (coll as HashSet<object>);
            if (set == null)
            {
                set = TempCollection.GetSet<object>();
                dispose = true;
            }

            try
            {
                int initialCount = set.Count;

                if (tp.IsInstanceOfType(obj)) set.Add(obj);
                if (obj is IComponent)
                {
                    var c = (obj as IComponent).component;
                    if (!object.ReferenceEquals(c, null) && tp.IsInstanceOfType(c)) set.Add(c);
                }

                var go = GameObjectUtil.GetGameObjectFromSource(obj);
                if (go != null)
                {
                    if (typeof(SPEntity).IsAssignableFrom(tp))
                    {
                        var entity = SPEntity.Pool.GetFromSource(tp, go);
                        if (entity != null) set.Add(entity);
                    }
                    else if (typeof(UnityEngine.GameObject).IsAssignableFrom(tp))
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

                if (dispose)
                {
                    coll.AddRange(set);
                    return set.Count;
                }
                else
                {
                    return set.Count - initialCount;
                }
            }
            finally
            {
                if (dispose && set is System.IDisposable d) d.Dispose();
            }
        }

        public static object[] GetAllFromSource(System.Type[] types, object obj, bool includeChildren = false)
        {
            using (var set = TempCollection.GetSet<object>())
            {
                if (types != null)
                {
                    foreach (var tp in types)
                    {
                        GetAllFromSource(set, tp, obj, includeChildren);
                    }
                }
                return set.Count > 0 ? set.ToArray() : ArrayUtil.Empty<object>();
            }
        }

        public static int GetAllFromSource(ICollection<object> coll, System.Type[] types, object obj, bool includeChildren = false)
        {
            int totalCount = 0;
            if (types != null)
            {
                foreach (var tp in types)
                {
                    totalCount += GetAllFromSource(coll, tp, obj, includeChildren);
                }
            }
            return totalCount;
        }

        #endregion

        #region Destruction Methods

        /// <summary>
        /// Calls DestroyImmediate if in editor and NOT playing, otherwise attempts to call 'Kill', and if all else fails it calls Destroy.
        /// </summary>
        /// <remarks>
        /// This is intended for destroying an object in a context you're unsure of when it'll be called (editor vs runtime). 
        /// Say you have a level generator that clears out its contents on generation that runs in both the editor and at runtime, 
        /// this will resolve if you're destroying in the editor or not... and if it's not it'll attempt to call 'Kill' if applicable. 
        /// </remarks>
        /// <param name="obj"></param>
        public static void SmartDestroy(UnityEngine.Object obj)
        {
            if (obj.IsNullOrDestroyed()) return;

            try
            {
#if UNITY_EDITOR
                if (UnityEngine.Application.isEditor && !UnityEngine.Application.isPlaying)
                {
                    UnityEngine.Object.DestroyImmediate(obj);
                    return;
                }
#endif

                if (obj is UnityEngine.GameObject)
                    (obj as UnityEngine.GameObject).Kill();
                else if (obj is UnityEngine.Transform)
                    (obj as UnityEngine.Transform).gameObject.Kill();
                else
                    UnityEngine.Object.Destroy(obj);
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
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

            switch (obj)
            {
                case ISPDisposable spd:
                    return spd.IsDisposed;
                case UnityEngine.Object uob:
                    return !_isObjectAlive(uob);
                case UnityEngine.TrackedReference trf:
                    return trf == null;
                case IComponent c:
                    return !_isObjectAlive(c.component);
                case IGameObjectSource g:
                    return !_isObjectAlive(g.gameObject);
            }

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

            switch (obj)
            {
                case ISPDisposable spd:
                    return spd.IsDisposed;
                case UnityEngine.Object uob:
                    return !_isObjectAlive(uob);
                case UnityEngine.TrackedReference trf:
                    return trf == null;
                case IComponent c:
                    return !_isObjectAlive(c.component);
                case IGameObjectSource g:
                    return !_isObjectAlive(g.gameObject);
            }

            return false;
        }

        public static bool IsAlive(this System.Object obj)
        {
            if (object.ReferenceEquals(obj, null)) return false;

            switch (obj)
            {
                case ISPDisposable spd:
                    return !spd.IsDisposed;
                case UnityEngine.Object uob:
                    return _isObjectAlive(uob);
                case UnityEngine.TrackedReference trf:
                    return trf != null;
                case IComponent c:
                    return _isObjectAlive(c.component);
                case IGameObjectSource g:
                    return _isObjectAlive(g.gameObject);
            }

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

        #region Misc

        /// <summary>
        /// Returns true if 'obj' is 'parent', attached to 'parent', or part of the child hiearchy of 'parent'.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsRelatedTo(UnityEngine.Object parent, UnityEngine.Object obj)
        {
            if (parent == null || obj == null) return false;
            if (parent == obj) return true;

            var go = GameObjectUtil.GetGameObjectFromSource(parent);
            if (go == null) return false;

            var child = GameObjectUtil.GetGameObjectFromSource(obj);
            if (child == null) return false;

            if (go == child) return true;
            return child.transform.IsChildOf(go.transform);
        }

        public static bool EqualsAny(System.Object obj, params System.Object[] others)
        {
            return System.Array.IndexOf(others, obj) >= 0;
        }

        #endregion

    }

}
