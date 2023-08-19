using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy.Collections;
using com.spacepuppy.Utils;
using com.spacepuppy.Project;
using System.Runtime.CompilerServices;

namespace com.spacepuppy.Spawn
{

    public interface ISpawnPool
    {

        SpawnedObjectController SpawnAsController(GameObject prefab, Transform par = null, System.Action<SpawnedObjectController> beforeSignalSpawnCallback = null);

        SpawnedObjectController SpawnAsController(GameObject prefab, Vector3 position, Quaternion rotation, Transform par = null, System.Action<SpawnedObjectController> beforeSignalSpawnCallback = null);

        bool Despawn(SpawnedObjectController cntrl);
        bool Purge(SpawnedObjectController cntrl);

    }

    public static class SpawnPoolExtensions
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GameObject Spawn(this ISpawnPool pool, GameObject prefab, Transform par = null, System.Action<SpawnedObjectController> beforeSignalSpawnCallback = null)
        {
            return pool.SpawnAsController(prefab, par, beforeSignalSpawnCallback)?.gameObject;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GameObject Spawn(this ISpawnPool pool, GameObject prefab, Vector3 position, Quaternion rotation, Transform par = null, System.Action<SpawnedObjectController> beforeSignalSpawnCallback = null)
        {
            return pool.SpawnAsController(prefab, position, rotation, par, beforeSignalSpawnCallback)?.gameObject;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Spawn<T>(this ISpawnPool pool, T prefab, Transform par = null, System.Action<SpawnedObjectController> beforeSignalSpawnCallback = null) where T : Component
        {
            return pool.SpawnAsController(prefab.gameObject, par, beforeSignalSpawnCallback)?.GetComponent<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Spawn<T>(this ISpawnPool pool, T prefab, Vector3 position, Quaternion rotation, Transform par = null, System.Action<SpawnedObjectController> beforeSignalSpawnCallback = null) where T : Component
        {
            return pool.SpawnAsController(prefab.gameObject, position, rotation, par, beforeSignalSpawnCallback)?.GetComponent<T>();
        }

    }

    [System.Serializable]
    public class SpawnPoolRef : SerializableInterfaceRef<ISpawnPool>
    {
        public ISpawnPool ValueOrDefault => this.Value.IsAlive() ? this.Value : SpawnPool.DefaultPool;
    }

    public class SpawnPool : SPComponent, ISpawnPool, ICollection<IPrefabCache>
    {

        #region Static Multiton Interface

        public const string DEFAULT_SPAWNPOOL_NAME = "Spacepuppy.PrimarySpawnPool";

        private static ISpawnPool _defaultPool;
        public static readonly MultitonPool<ISpawnPool> Pools = new MultitonPool<ISpawnPool>();

        public static ISpawnPool DefaultPool
        {
            get
            {
                return _defaultPool.IsAlive() ? _defaultPool : FindOrCreatePrimaryPool();
            }
        }

        public static ISpawnPool FindOrCreatePrimaryPool()
        {
            if (_defaultPool.IsAlive()) return _defaultPool;

            if (Pools.Count > 0)
            {
                ISpawnPool point = null;
                var e = Pools.GetEnumerator();
                while (e.MoveNext())
                {
                    if (e.Current is Component c && c.CompareName(DEFAULT_SPAWNPOOL_NAME))
                    {
                        point = e.Current;
                        break;
                    }
                }

                if (!object.ReferenceEquals(point, null))
                {
                    _defaultPool = point;
                    return _defaultPool;
                }
            }

            var go = new GameObject(DEFAULT_SPAWNPOOL_NAME);
            _defaultPool = go.AddComponent<SpawnPool>();
            return _defaultPool;
        }

        public static ISpawnPool CreatePrimaryPool(bool dontDestroyOnLoad = false)
        {
            if (_defaultPool.IsAlive()) throw new System.InvalidOperationException("A primary SpawnPool already exists, call 'HasRegisteredPrimaryPool' to confirm one doesn't exist before calling CreatePrimaryPool.");

            var go = new GameObject(DEFAULT_SPAWNPOOL_NAME);
            _defaultPool = go.AddComponent<SpawnPool>();
            if (dontDestroyOnLoad) DontDestroyOnLoad(go);
            return _defaultPool;
        }

        public static T CreatePrimaryPool<T>(bool dontDestroyOnLoad = false) where T : Component, ISpawnPool
        {
            if (_defaultPool.IsAlive()) throw new System.InvalidOperationException("A primary SpawnPool already exists, call 'HasRegisteredPrimaryPool' to confirm one doesn't exist before calling CreatePrimaryPool.");

            var go = new GameObject(DEFAULT_SPAWNPOOL_NAME);
            _defaultPool = go.AddComponent<T>();
            if (dontDestroyOnLoad) DontDestroyOnLoad(go);
            return _defaultPool as T;
        }

        public static void RegisterPrimaryPool(ISpawnPool pool)
        {
            if (_defaultPool.IsAlive())
            {
                if (!object.ReferenceEquals(_defaultPool, pool)) throw new System.InvalidOperationException("A primary SpawnPool already exists, call 'HasRegisteredPrimaryPool' to confirm one doesn't exist before calling RegisterPrimaryPool.");
                return;
            }

            _defaultPool = pool;
        }

        public static bool HasRegisteredPrimaryPool => _defaultPool.IsAlive();

        #endregion

        #region Fields

        [SerializeField()]
        [ReorderableArray(DrawElementAtBottom = true, ChildPropertyToDrawAsElementLabel = "Name", ChildPropertyToDrawAsElementEntry = "_prefab")]
        private List<PrefabCache> _registeredPrefabs = new List<PrefabCache>();

        [Space(20f)]
        [SerializeField]
        [ReorderableArray(DrawElementAtBottom = true, ChildPropertyToDrawAsElementEntry = "_assetSet")]
        private AssetSetCache[] _registeredAssetSets;

        [System.NonSerialized()]
        private Dictionary<ulong, PrefabCache> _prefabToCache = new Dictionary<ulong, PrefabCache>();

        #endregion

        #region CONSTRUCTOR

        protected override void Awake()
        {
            base.Awake();

            Pools.AddReference(this);
            if (this.CompareName(DEFAULT_SPAWNPOOL_NAME) && _defaultPool == null)
            {
                _defaultPool = this;
            }
        }

        protected override void Start()
        {
            base.Start();

            var e = _registeredPrefabs.GetEnumerator();
            while (e.MoveNext())
            {
                if (e.Current.Prefab == null) continue;

                this.OnLoadingCache(e.Current);
                e.Current.Load(this);
                _prefabToCache[e.Current.PrefabID] = e.Current;
            }

            if (_registeredAssetSets?.Length > 0)
            {
                for (int i = 0; i < _registeredAssetSets.Length; i++)
                {
                    if (_registeredAssetSets[i].AssetSet == null) continue;

                    foreach (var prefab in _registeredAssetSets[i].AssetSet.LoadAssets<GameObject>())
                    {
                        try
                        {
                            this.Register(prefab, string.Empty, _registeredAssetSets[i].CacheSize, _registeredAssetSets[i].ResizeBuffer, _registeredAssetSets[i].LimitAmount);
                        }
                        catch (System.ArgumentNullException)
                        {
                            //do nothing
                        }
                        catch (System.ArgumentException)
                        {
                            Debug.LogWarning("AssetSet contains prefab that was already registered.", prefab);
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                    }
                }
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (object.ReferenceEquals(this, _defaultPool))
            {
                _defaultPool = null;
            }
            Pools.RemoveReference(this);

            var e = _registeredPrefabs.GetEnumerator();
            while (e.MoveNext())
            {
                this.OnUnloadingCache(e.Current);
                e.Current.Clear();
            }
        }

        #endregion

        #region Properties

        private string _cachedName;
        public new string name
        {
            get
            {
                if (_cachedName == null) _cachedName = this.gameObject.name;
                return _cachedName;
            }
            set
            {
                this.gameObject.name = value;
                _cachedName = value;
            }
        }

        #endregion

        #region Methods

        public IPrefabCache Register(GameObject prefab, string sname, int cacheSize = 0, int resizeBuffer = 1, int limitAmount = 1)
        {
            if (object.ReferenceEquals(prefab, null)) throw new System.ArgumentNullException("prefab");

            var ctrl = prefab.GetComponent<SpawnedObjectController>();
            if (ctrl && _prefabToCache.ContainsKey(ctrl.PrefabID)) throw new System.ArgumentException("Already manages prefab.", "prefab");

            var cache = new PrefabCache(prefab, sname)
            {
                CacheSize = cacheSize,
                ResizeBuffer = resizeBuffer,
                LimitAmount = limitAmount
            };

            this.OnLoadingCache(cache);
            _registeredPrefabs.Add(cache);
            _prefabToCache[cache.PrefabID] = cache;
            cache.Load(this);
            return cache;
        }

        public bool UnRegister(GameObject prefab)
        {
            var cache = this.FindPrefabCacheInternal(prefab);
            if (cache == null) return false;

            return this.UnRegister(cache);
        }

        public bool UnRegister(ulong prefabId)
        {
            PrefabCache cache;
            if (!_prefabToCache.TryGetValue(prefabId, out cache)) return false;

            return this.UnRegister(cache);
        }

        public bool UnRegister(IPrefabCache cache)
        {
            var obj = cache as PrefabCache;
            if (obj == null) return false;
            if (obj.Owner != this) return false;

            this.OnUnloadingCache(obj);
            obj.Clear();
            _registeredPrefabs.Remove(obj);
            _prefabToCache.Remove(obj.PrefabID);
            return true;
        }

        public bool Contains(ulong prefabId)
        {
            return _prefabToCache.ContainsKey(prefabId);
        }

        public bool Contains(string sname)
        {
            var e = _registeredPrefabs.GetEnumerator();
            while (e.MoveNext())
            {
                if (e.Current.Name == sname)
                {
                    return true;
                }
            }
            return false;
        }

        public IPrefabCache FindPrefabCache(GameObject prefab)
        {
            return FindPrefabCacheInternal(prefab);
        }

        public IPrefabCache FindPrefabCache(ulong prefabId)
        {
            PrefabCache result;
            if (_prefabToCache.TryGetValue(prefabId, out result)) return result;
            return null;
        }




        public GameObject SpawnByIndex(int index, Transform par = null, System.Action<SpawnedObjectController> beforeSignalSpawnCallback = null)
        {
            if (index < 0 || index >= _registeredPrefabs.Count) throw new System.IndexOutOfRangeException();

            var cache = _registeredPrefabs[index];
            var pos = (par != null) ? par.position : Vector3.zero;
            var rot = (par != null) ? par.rotation : Quaternion.identity;
            var obj = cache.Spawn(pos, rot, par);
            beforeSignalSpawnCallback?.Invoke(obj);
            this.SignalSpawned(obj);
            return obj.gameObject;
        }

        public GameObject SpawnByIndex(int index, Vector3 position, Quaternion rotation, Transform par = null, System.Action<SpawnedObjectController> beforeSignalSpawnCallback = null)
        {
            if (index < 0 || index >= _registeredPrefabs.Count) throw new System.IndexOutOfRangeException();

            var cache = _registeredPrefabs[index];
            var obj = cache.Spawn(position, rotation, par);
            beforeSignalSpawnCallback?.Invoke(obj);
            this.SignalSpawned(obj);
            return obj.gameObject;
        }

        public GameObject Spawn(string sname, Transform par = null, System.Action<SpawnedObjectController> beforeSignalSpawnCallback = null)
        {
            PrefabCache cache = null;
            var e = _registeredPrefabs.GetEnumerator();
            while (e.MoveNext())
            {
                if (e.Current.Name == sname)
                {
                    cache = e.Current;
                    break;
                }
            }
            if (cache == null) return null;

            var pos = (par != null) ? par.position : Vector3.zero;
            var rot = (par != null) ? par.rotation : Quaternion.identity;
            var obj = cache.Spawn(pos, rot, par);
            beforeSignalSpawnCallback?.Invoke(obj);
            this.SignalSpawned(obj);
            return obj.gameObject;
        }

        public GameObject Spawn(string sname, Vector3 position, Quaternion rotation, Transform par = null, System.Action<SpawnedObjectController> beforeSignalSpawnCallback = null)
        {
            PrefabCache cache = null;
            var e = _registeredPrefabs.GetEnumerator();
            while (e.MoveNext())
            {
                if (e.Current.Name == sname)
                {
                    cache = e.Current;
                    break;
                }
            }
            if (cache == null) return null;

            var obj = cache.Spawn(position, rotation, par);
            beforeSignalSpawnCallback?.Invoke(obj);
            this.SignalSpawned(obj);
            return obj.gameObject;
        }

        public GameObject Spawn(GameObject prefab, Transform par = null, System.Action<SpawnedObjectController> beforeSignalSpawnCallback = null)
        {
            var controller = SpawnAsController(prefab, par, beforeSignalSpawnCallback);
            return (controller != null) ? controller.gameObject : null;
        }

        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform par = null, System.Action<SpawnedObjectController> beforeSignalSpawnCallback = null)
        {
            var controller = SpawnAsController(prefab, position, rotation, par, beforeSignalSpawnCallback);
            return (controller != null) ? controller.gameObject : null;
        }

        public T Spawn<T>(T prefab, Transform parent = null, System.Action<SpawnedObjectController> beforeSignalSpawnCallback = null) where T : Component
        {
            var controller = SpawnAsController(prefab.gameObject, parent, beforeSignalSpawnCallback);
            return (controller != null) ? controller.GetComponent<T>() : null;
        }

        public T Spawn<T>(T prefab, Vector3 position, Quaternion rotation, Transform par = null, System.Action<SpawnedObjectController> beforeSignalSpawnCallback = null) where T : Component
        {
            var controller = SpawnAsController(prefab.gameObject, position, rotation, par, beforeSignalSpawnCallback);
            return (controller != null) ? controller.GetComponent<T>() : null;
        }

        public virtual SpawnedObjectController SpawnAsController(GameObject prefab, Transform par = null, System.Action<SpawnedObjectController> beforeSignalSpawnCallback = null)
        {
            if (prefab == null || !this.PreSpawnValidation(prefab)) return null;

            var cache = this.FindPrefabCacheInternal(prefab);
            var pos = (cache != null) ? cache.Prefab.transform.position : prefab.transform.position;
            var rot = (cache != null) ? cache.Prefab.transform.rotation : prefab.transform.rotation;

            if (cache != null)
            {
                var controller = cache.Spawn(pos, rot, par);
                beforeSignalSpawnCallback?.Invoke(controller);
                this.SignalSpawned(controller);
                return controller;
            }
            else if (prefab)
            {
                var controller = this.CreateInstanceInternal(prefab, pos, rot, par, false);
                if (controller)
                {
                    controller.SetSpawned();
                    beforeSignalSpawnCallback?.Invoke(controller);
                    this.SignalSpawned(controller);
                    return controller;
                }
            }

            return null;
        }

        public virtual SpawnedObjectController SpawnAsController(GameObject prefab, Vector3 position, Quaternion rotation, Transform par = null, System.Action<SpawnedObjectController> beforeSignalSpawnCallback = null)
        {
            if (prefab == null || !this.PreSpawnValidation(prefab)) return null;

            var cache = this.FindPrefabCacheInternal(prefab);
            if (cache != null)
            {
                var controller = cache.Spawn(position, rotation, par);
                beforeSignalSpawnCallback?.Invoke(controller);
                this.SignalSpawned(controller);
                return controller;
            }
            else if (prefab)
            {
                var controller = this.CreateInstanceInternal(prefab, position, rotation, par, false);
                if (controller)
                {
                    controller.SetSpawned();
                    beforeSignalSpawnCallback?.Invoke(controller);
                    this.SignalSpawned(controller);
                    return controller;
                }
            }

            return null;
        }

        public GameObject SpawnByPrefabId(ulong prefabId, Transform par = null, System.Action<SpawnedObjectController> beforeSignalSpawnCallback = null)
        {
            var controller = SpawnAsControllerByPrefabId(prefabId, par, beforeSignalSpawnCallback);
            return (controller != null) ? controller.gameObject : null;
        }

        public GameObject SpawnByPrefabId(ulong prefabId, Vector3 position, Quaternion rotation, Transform par = null, System.Action<SpawnedObjectController> beforeSignalSpawnCallback = null)
        {
            var controller = SpawnAsControllerByPrefabId(prefabId, position, rotation, par, beforeSignalSpawnCallback);
            return (controller != null) ? controller.gameObject : null;
        }

        public virtual SpawnedObjectController SpawnAsControllerByPrefabId(ulong prefabId, Transform par = null, System.Action<SpawnedObjectController> beforeSignalSpawnCallback = null)
        {
            PrefabCache cache;
            if (!_prefabToCache.TryGetValue(prefabId, out cache) || !this.PreSpawnValidation(cache.Prefab)) return null;

            var pos = (par != null) ? par.position : Vector3.zero;
            var rot = (par != null) ? par.rotation : Quaternion.identity;
            var controller = cache.Spawn(pos, rot, par);
            beforeSignalSpawnCallback?.Invoke(controller);
            this.SignalSpawned(controller);
            return controller;
        }

        public virtual SpawnedObjectController SpawnAsControllerByPrefabId(ulong prefabId, Vector3 position, Quaternion rotation, Transform par = null, System.Action<SpawnedObjectController> beforeSignalSpawnCallback = null)
        {
            PrefabCache cache;
            if (!_prefabToCache.TryGetValue(prefabId, out cache) || !this.PreSpawnValidation(cache.Prefab)) return null;
            var controller = cache.Spawn(position, rotation, par);
            beforeSignalSpawnCallback?.Invoke(controller);
            this.SignalSpawned(controller);
            return controller;
        }



        public bool Despawn(SpawnedObjectController cntrl)
        {
            if (Object.ReferenceEquals(cntrl, null)) throw new System.ArgumentNullException(nameof(cntrl));

            PrefabCache cache;
            if (!_prefabToCache.TryGetValue(cntrl.PrefabID, out cache) || !cache.ContainsActive(cntrl))
            {
                return false;
            }

            this.gameObject.Broadcast<IOnDespawnHandler, SpawnedObjectController>(cntrl, (o, c) => o.OnDespawn(c));
            cntrl.gameObject.Broadcast<IOnDespawnHandler, SpawnedObjectController>(cntrl, (o, c) => o.OnDespawn(c));
            Messaging.Broadcast<IOnDespawnGlobalHandler, SpawnedObjectController>(cntrl, (o, c) => o.OnDespawn(c));
            return cache.Despawn(cntrl);
        }



        public bool Purge(GameObject obj)
        {
            if (object.ReferenceEquals(obj, null)) throw new System.ArgumentNullException("obj");

            var cntrl = obj.GetComponent<SpawnedObjectController>();
            if (cntrl == null) return false;

            return this.Purge(cntrl);
        }

        public bool Purge(SpawnedObjectController cntrl)
        {
            if (object.ReferenceEquals(cntrl, null)) throw new System.ArgumentNullException("cntrl");

            PrefabCache cache;
            if (!_prefabToCache.TryGetValue(cntrl.PrefabID, out cache) || !cache.Contains(cntrl))
            {
                return false;
            }

            return cache.Purge(cntrl);
        }




        protected virtual SpawnedObjectController CreateInstanceInternal(GameObject prefab, Vector3 pos, Quaternion rot, Transform par, bool cached)
        {
            return SpawnedObjectController.InitializeController(Instantiate(prefab, pos, rot, par), this, prefab, cached);
        }


        /// <summary>
        /// Match an object to its prefab if this pool manages the GameObject.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private PrefabCache FindPrefabCacheInternal(GameObject obj)
        {
            var controller = obj.GetComponent<SpawnedObjectController>();
            if (controller == null) return null;

            PrefabCache result;
            if (_prefabToCache.TryGetValue(controller.PrefabID, out result)) return result;

            return null;
        }

        protected virtual bool PreSpawnValidation(GameObject prefab) => true;
        protected virtual void SignalSpawned(SpawnedObjectController cntrl)
        {
            this.gameObject.Broadcast<IOnSpawnHandler, SpawnedObjectController>(cntrl, (o, c) => o.OnSpawn(c));
            cntrl.gameObject.Broadcast<IOnSpawnHandler, SpawnedObjectController>(cntrl, (o, c) => o.OnSpawn(c));
            Messaging.Broadcast<IOnSpawnGlobalHandler, SpawnedObjectController>(cntrl, (o, c) => o.OnSpawn(c));
        }

        protected virtual void OnLoadingCache(IPrefabCache cache) { }
        protected virtual void OnUnloadingCache(IPrefabCache cache) { }

        #endregion

        #region ICollection Interface

        public int Count
        {
            get { return _registeredPrefabs.Count; }
        }

        bool ICollection<IPrefabCache>.IsReadOnly
        {
            get { return false; }
        }

        void ICollection<IPrefabCache>.Add(IPrefabCache item)
        {
            throw new System.NotSupportedException();
        }

        public bool Contains(IPrefabCache item)
        {
            var obj = item as PrefabCache;
            if (item == null) return false;

            return _registeredPrefabs.Contains(item);
        }

        public void Clear()
        {
            var e = _registeredPrefabs.GetEnumerator();
            while (e.MoveNext())
            {
                this.OnUnloadingCache(e.Current);
                e.Current.Clear();
            }

            _registeredPrefabs.Clear();
            _prefabToCache.Clear();
        }

        bool ICollection<IPrefabCache>.Remove(IPrefabCache item)
        {
            return this.UnRegister(item);
        }

        void ICollection<IPrefabCache>.CopyTo(IPrefabCache[] array, int arrayIndex)
        {
            for (int i = 0; i < _registeredPrefabs.Count; i++)
            {
                if (i >= array.Length) return;

                array[arrayIndex + i] = _registeredPrefabs[i];
            }
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<IPrefabCache> IEnumerable<IPrefabCache>.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        #region Special Types

        [System.Serializable()]
        private sealed class PrefabCache : IPrefabCache
        {

            #region Fields

            [SerializeField]
            private string _itemName;
            [SerializeField]
            private GameObject _prefab;

            [Tooltip("The starting CacheSize.")]
            public int CacheSize = 0;
            [Tooltip("How much should the cache resize by if an empty/used cache is spawned from.")]
            [Min(1)]
            public int ResizeBuffer = 1;
            [Tooltip("The maximum number of instances allowed to be cached, 0 or less means infinite.")]
            [NegativeIsInfinity(ZeroIsAlsoInfinity = true)]
            public int LimitAmount = 0;

            [System.NonSerialized()]
            private SpawnPool _owner;
            [System.NonSerialized()]
            private HashSet<SpawnedObjectController> _instances;
            [System.NonSerialized()]
            private HashSet<SpawnedObjectController> _activeInstances;

            #endregion

            #region CONSTRUCTOR

            private PrefabCache()
            {
                _instances = new HashSet<SpawnedObjectController>(ObjectReferenceEqualityComparer<SpawnedObjectController>.Default);
                _activeInstances = new HashSet<SpawnedObjectController>(ObjectReferenceEqualityComparer<SpawnedObjectController>.Default);
            }

            public PrefabCache(GameObject prefab, string name)
            {
                _prefab = prefab;
                _itemName = name;
                _instances = new HashSet<SpawnedObjectController>(ObjectReferenceEqualityComparer<SpawnedObjectController>.Default);
                _activeInstances = new HashSet<SpawnedObjectController>(ObjectReferenceEqualityComparer<SpawnedObjectController>.Default);
            }

            #endregion

            #region Properties

            public SpawnPool Owner
            {
                get { return _owner; }
            }

            public string Name
            {
                get { return _itemName; }
            }

            public GameObject Prefab
            {
                get { return _prefab; }
            }

            public ulong PrefabID { get; private set; }

            int IPrefabCache.CacheSize
            {
                get { return this.CacheSize; }
                set { this.CacheSize = value; }
            }

            int IPrefabCache.ResizeBuffer
            {
                get { return this.ResizeBuffer; }
                set { this.ResizeBuffer = value; }
            }

            int IPrefabCache.LimitAmount
            {
                get { return this.LimitAmount; }
                set { this.LimitAmount = value; }
            }

            public int Count
            {
                get { return _instances.Count + _activeInstances.Count; }
            }

            #endregion

            #region Methods

            internal bool Contains(SpawnedObjectController cntrl)
            {
                return _instances.Contains(cntrl) || _activeInstances.Contains(cntrl);
            }

            internal bool ContainsActive(SpawnedObjectController cntrl)
            {
                return _activeInstances.Contains(cntrl);
            }

            internal void Load(SpawnPool owner)
            {
                this.Clear();
                _owner = owner;
                if (_prefab == null) return;

                this.PrefabID = SpawnedObjectController.InitializePrefab(_prefab).PrefabID;
                for (int i = 0; i < this.CacheSize; i++)
                {
                    _instances.Add(this.CreateCachedInstance());
                }
            }

            internal void Clear()
            {
                if (_instances.Count > 0)
                {
                    var e = _instances.GetEnumerator();
                    while (e.MoveNext())
                    {
                        Object.Destroy(e.Current.gameObject);
                    }
                    _instances.Clear();
                }

                _activeInstances.Clear();
            }

            internal SpawnedObjectController Spawn(Vector3 pos, Quaternion rot, Transform par)
            {
                if (_instances.Count == 0)
                {
                    int cnt = this.Count;
                    int newSize = Mathf.Max(cnt + 1, cnt + this.ResizeBuffer);
                    if (this.LimitAmount > 0) newSize = Mathf.Min(newSize, this.LimitAmount);

                    if (newSize > cnt)
                    {
                        for (int i = cnt; i < newSize; i++)
                        {
                            _instances.Add(this.CreateCachedInstance());
                        }
                    }
                }

                if (_instances.Count > 0)
                {
                    var cntrl = _instances.Pop();

                    _activeInstances.Add(cntrl);

                    cntrl.transform.SetParent(par, false);
                    cntrl.transform.position = pos;
                    cntrl.transform.rotation = rot;
                    cntrl.SetSpawned();

                    return cntrl;
                }
                else if (this.Prefab)
                {
                    var controller = _owner.CreateInstanceInternal(this.Prefab, pos, rot, par, false);
                    if (controller)
                    {
                        controller.SetSpawned();
                        return controller;
                    }
                }

                return null;
            }

            internal bool Despawn(SpawnedObjectController cntrl)
            {
                if (!_activeInstances.Remove(cntrl)) return false;

                cntrl.SetDespawned();
                cntrl.transform.SetParent(_owner.transform, false);
                cntrl.transform.localPosition = Vector3.zero;
                cntrl.transform.rotation = Quaternion.identity;

                _instances.Add(cntrl);
                return true;
            }

            internal bool Purge(SpawnedObjectController cntrl)
            {
                if (_activeInstances.Remove(cntrl))
                    return true;
                if (_instances.Remove(cntrl))
                    return true;

                return false;
            }

            private SpawnedObjectController CreateCachedInstance()
            {
                if (_prefab)
                {
                    var controller = _owner.CreateInstanceInternal(_prefab, Vector3.zero, Quaternion.identity, _owner.transform, true);

                    controller.gameObject.SetActive(false);
                    controller.name = (!string.IsNullOrEmpty(_itemName) ? _itemName : controller.name) + "(CachedInstance)";
                    controller.transform.SetParent(_owner.transform, false);
                    controller.transform.localPosition = Vector3.zero;
                    controller.transform.rotation = Quaternion.identity;

                    return controller;
                }

                return null;
            }

            #endregion

        }

        [System.Serializable]
        private sealed class AssetSetCache
        {
            [SerializeField]
            private AssetBundleRef _assetSet = new AssetBundleRef();
            public IAssetSet AssetSet => _assetSet.Value;

            [Tooltip("The starting CacheSize.")]
            public int CacheSize = 0;
            [Tooltip("How much should the cache resize by if an empty/used cache is spawned from.")]
            [Min(1)]
            public int ResizeBuffer = 1;
            [Tooltip("The maximum number of instances allowed to be cached, 0 or less means infinite.")]
            [NegativeIsInfinity(ZeroIsAlsoInfinity = true)]
            public int LimitAmount = 0;

        }

        public struct Enumerator : IEnumerator<IPrefabCache>
        {

            private List<PrefabCache>.Enumerator _e;

            public Enumerator(SpawnPool pool)
            {
                if (pool == null) throw new System.ArgumentNullException("pool");

                _e = pool._registeredPrefabs.GetEnumerator();
            }

            public IPrefabCache Current
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

            public bool MoveNext()
            {
                return _e.MoveNext();
            }

            public void Dispose()
            {
                _e.Dispose();
            }

            void System.Collections.IEnumerator.Reset()
            {
                //DO NOTHING
            }
        }

        #endregion

    }

}
