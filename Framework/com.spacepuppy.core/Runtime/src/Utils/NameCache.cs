using UnityEngine;
using System.Collections.Generic;

namespace com.spacepuppy.Utils
{

    public static class NameCache
    {

        /// <summary>
        /// Attempts to reduce garbage when comparing names.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool CompareName(this UnityEngine.Object obj, string name)
        {
            if (obj == null) return false;

            if (obj is INameable) return (obj as INameable).CompareName(name);

            var go = GameObjectUtil.GetGameObjectFromSource(obj);
#if UNITY_EDITOR
            if (Application.isPlaying && go != null) return go.AddOrGetComponent<GameObjectNameCache>().CompareName(name);
#else
            if (go != null) return go.AddOrGetComponent<GameObjectNameCache>().CompareName(name);
#endif

            return obj.name == name;
        }

        public static bool CompareName(this UnityEngine.Object obj, string name, bool respectProxy)
        {
            if (respectProxy && obj is IProxy) obj = obj.ReduceIfProxy() as UnityEngine.Object;
            if (obj == null) return false;

            if (obj is INameable) return (obj as INameable).CompareName(name);

            var go = GameObjectUtil.GetGameObjectFromSource(obj);
#if UNITY_EDITOR
            if (Application.isPlaying && go != null) return go.AddOrGetComponent<GameObjectNameCache>().CompareName(name);
#else
            if (go != null) return go.AddOrGetComponent<GameObjectNameCache>().CompareName(name);
#endif

            return obj.name == name;
        }

        public static bool CompareName(this GameObject go, string name)
        {
            if (go == null) return false;

#if UNITY_EDITOR
            if (Application.isPlaying && go != null) return go.AddOrGetComponent<GameObjectNameCache>().CompareName(name);
#else
            if (go != null) return go.AddOrGetComponent<GameObjectNameCache>().CompareName(name);
#endif

            return go.name == name;
        }

        public static string GetCachedName(UnityEngine.Object obj)
        {
            if (obj == null) return null;
            if (obj is INameable) return (obj as INameable).Name;

            var go = GameObjectUtil.GetGameObjectFromSource(obj);
#if UNITY_EDITOR
            if (Application.isPlaying && go != null) return go.AddOrGetComponent<GameObjectNameCache>().Name;
#else
            if (go != null) return go.AddOrGetComponent<GameObjectNameCache>().Name;
#endif

            return obj.name;
        }

        public static string GetCachedName(UnityEngine.Object obj, bool respectProxy)
        {
            if (respectProxy && obj is IProxy) obj = obj.ReduceIfProxy() as UnityEngine.Object;
            if (obj == null) return null;

            if (obj is INameable) return (obj as INameable).Name;

            var go = GameObjectUtil.GetGameObjectFromSource(obj);
#if UNITY_EDITOR
            if (Application.isPlaying && go != null) return go.AddOrGetComponent<GameObjectNameCache>().Name;
#else
            if (go != null) return go.AddOrGetComponent<GameObjectNameCache>().Name;
#endif

            return obj.name;
        }

        public static string GetCachedName(GameObject go)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return go ? go.name : string.Empty;
#endif
            return go ? go.AddOrGetComponent<GameObjectNameCache>().Name : string.Empty;
        }

        public static void SetCachedName(GameObject go, string name)
        {
            if (!go) return;

#if UNITY_EDITOR
            if (Application.isPlaying && go.GetComponent<GameObjectNameCache>(out GameObjectNameCache cache))
            {
                cache.Name = name;
            }
            else
            {
                go.name = name;
            }
#else
            if (go.GetComponent<GameObjectNameCache>(out GameObjectNameCache cache))
            {
                cache.Name = name;
            }
            else
            {
                go.name = name;
            }
#endif
        }

        /// <summary>
        /// Set the cached name dirty so that the next time it's read it's updated to the current state of the object.
        /// Call this if you rename an object.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="respectProxy"></param>
        public static void SetDirty(UnityEngine.Object obj, bool respectProxy = false)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            if (respectProxy && obj is IProxy) obj = obj.ReduceIfProxy() as UnityEngine.Object;
            if (obj == null) return;

            if (obj is INameable)
            {
                (obj as INameable).SetDirty();
                return;
            }

            var go = GameObjectUtil.GetGameObjectFromSource(obj);
            if (go && go.GetComponent(out GameObjectNameCache cache)) cache.SetDirty();
        }

        public static void SetDirty(GameObject go)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            if (go && go.GetComponent(out GameObjectNameCache cache)) cache.SetDirty();
        }


        #region Special Types

        public class UnityObjectNameCache : INameable
        {

            private UnityEngine.Object _obj;
            private string _nameCache;

            public UnityObjectNameCache(UnityEngine.Object obj)
            {
                if (object.ReferenceEquals(obj, null)) throw new System.ArgumentNullException("obj");
                _obj = obj;
            }

            public string Name
            {
                get
                {
                    if (_nameCache == null) _nameCache = _obj.name ?? string.Empty;
                    return _nameCache;
                }
                set
                {
                    _nameCache = value ?? string.Empty;
                    _obj.name = _nameCache;
                }
            }

            public bool CompareName(string nm)
            {
                if (_nameCache == null) _nameCache = _obj.name ?? string.Empty;
                return _nameCache == nm;
            }

            public void SetDirty()
            {
                _nameCache = null;
            }

        }

        private class GameObjectNameCache : MonoBehaviour, INameable
        {
            private string _nameCache;

            public string Name
            {
                get
                {
                    if (_nameCache == null) _nameCache = this.gameObject.name ?? string.Empty;
                    return _nameCache;
                }
                set
                {
                    _nameCache = value ?? string.Empty;
                    this.gameObject.name = _nameCache;
                }
            }

            public bool CompareName(string nm)
            {
                if (_nameCache == null) _nameCache = this.gameObject.name ?? string.Empty;
                return _nameCache == nm;
            }

            public void SetDirty()
            {
                _nameCache = null;
            }
        }

        #endregion

    }

}
