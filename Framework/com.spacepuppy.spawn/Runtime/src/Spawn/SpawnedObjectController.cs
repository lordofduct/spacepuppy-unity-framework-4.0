using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Collections;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Spawn
{

    /// <summary>
    /// Handle for managing spawnable/spawned prefabs. This will be automatically attached to prefab's registered/spawned by SpawnPool. 
    /// This can be manually attached to prefabs at editor time to configure the entity priority OR for associating the PrefabId to the asset's globalid. 
    /// This is latter option is very important for Addressables/AssetBundles since the bundle the asset comes from is in a different domain 
    /// and can utilize the PrefabId to locate the prefab in the pool.
    /// </summary>
    public class SpawnedObjectController : SPComponent, IKillableEntity, INameable
    {

        public const float KILLABLEENTITYPRIORITY = 0f;

        public event System.EventHandler OnSpawned;
        public event System.EventHandler OnDespawned;
        public event System.EventHandler OnKilled;

        #region Fields

        [SerializeField]
        [ShortUid.Config(LinkToGlobalId = true, ReadOnly = true)]
        private ShortUid _prefabId;

        [SerializeField]
        private float _killableEntityPriority = KILLABLEENTITYPRIORITY;

        [System.NonSerialized()]
        private ISpawnPool _pool;
        [System.NonSerialized]
        private GameObject _prefab;
        
        [System.NonSerialized()]
        private bool _isSpawned;

        #endregion

        #region CONSTRUCTOR

        public SpawnedObjectController()
        {
            _nameCache = new NameCache.UnityObjectNameCache(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if(!GameLoop.ApplicationClosing && _pool != null)
            {
                _pool.Purge(this);
            }
            this.OnKilled?.Invoke(this, System.EventArgs.Empty);
            _pool = null;
            _prefab = null;
        }

        #endregion

        #region Properties

        public bool IsSpawned
        {
            get { return _isSpawned; }
        }

        /// <summary>
        /// The pool that created this object.
        /// </summary>
        public ISpawnPool Pool
        {
            get { return _pool; }
        }

        public GameObject Prefab
        {
            get { return _prefab; }
        }

        public ulong PrefabID => _prefabId.Value;

        public bool IsCached { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// This method ONLY called by ISpawnPool
        /// </summary>
        public void SetSpawned()
        {
            _isSpawned = true;
            this.gameObject.SetActive(true);
            if (this.OnSpawned != null) this.OnSpawned(this, System.EventArgs.Empty);
        }

        /// <summary>
        /// This method ONLY called by ISpawnPool
        /// </summary>
        public void SetDespawned()
        {
            _isSpawned = false;
            this.gameObject.SetActive(false);
            if (this.OnDespawned != null) this.OnDespawned(this, System.EventArgs.Empty);
        }

        public void Purge()
        {
            if (_pool != null) _pool.Purge(this);
        }

        /// <summary>
        /// Creates a new spawned object from the available cache in the ISpawnPool this was spawned from.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        public SpawnedObjectController CloneObject()
        {
            if (_pool == null) throw new System.InvalidOperationException($"Can not clone an uninitialized {nameof(SpawnedObjectController)} as it has not {nameof(ISpawnPool)} associated with it. If you want to duplicate this object, just call Instantiate.");

            if (_prefab)
                return _pool.SpawnAsController(_prefab, this.transform.position, this.transform.rotation);
            else
                return _pool.SpawnAsController(this.gameObject, this.transform.position, this.transform.rotation);
        }

        #endregion

        #region IKillableEntity Interface

        public bool IsDead
        {
            get { return !_isSpawned; }
        }

        void IKillableEntity.OnPreKill(ref com.spacepuppy.KillableEntityToken token, UnityEngine.GameObject target)
        {
            //if not initialized, if this is dead, or if it's not the root of this entity being killed... exit now
            if (_pool == null || !ObjUtil.IsObjectAlive(this) || this.gameObject != target) return;

            token.ProposeKillCandidate(this, _killableEntityPriority);
        }

        void IKillableEntity.OnKill(KillableEntityToken token)
        {
            if (_pool != null && !object.ReferenceEquals(token.Candidate, this)) _pool.Purge(this);

            try
            {
                this.OnKilled?.Invoke(this, System.EventArgs.Empty);
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        void IKillableEntity.OnElectedKillCandidate()
        {
            if (!_pool.Despawn(this))
            {
                Destroy(this.gameObject);
                return;
            }

            //TODO - need a cleaner way of doing this
            using (var lst = TempCollection.GetList<Rigidbody>())
            {
                this.transform.GetComponentsInChildren<Rigidbody>(lst);
                var e = lst.GetEnumerator();
                while (e.MoveNext())
                {
                    e.Current.velocity = Vector3.zero;
                    e.Current.angularVelocity = Vector3.zero;
                }
            }
            using (var lst = TempCollection.GetList<Rigidbody2D>())
            {
                this.transform.GetComponentsInChildren<Rigidbody2D>(lst);
                var e = lst.GetEnumerator();
                while (e.MoveNext())
                {
                    e.Current.velocity = Vector2.zero;
                    e.Current.angularVelocity = 0f;
                }
            }
        }

        #endregion

        #region INameable Interface

        private NameCache.UnityObjectNameCache _nameCache;
        public new string name
        {
            get { return _nameCache.Name; }
            set { _nameCache.Name = value; }
        }
        string INameable.Name
        {
            get { return _nameCache.Name; }
            set { _nameCache.Name = value; }
        }
        public bool CompareName(string nm)
        {
            return _nameCache.CompareName(nm);
        }
        void INameable.SetDirty()
        {
            _nameCache.SetDirty();
        }

        #endregion

        #region Factory

        public static SpawnedObjectController InitializeController(GameObject gameObject, ISpawnPool pool, GameObject prefab, bool cached)
        {
            if (pool == null) throw new System.ArgumentNullException(nameof(pool));

            var controller = gameObject.AddOrGetComponent<SpawnedObjectController>();
            controller._pool = pool;
            controller.IsCached = cached;

            if (prefab)
            {
                var ctrl = prefab.GetComponent<SpawnedObjectController>();
                controller._prefab = prefab;
                controller._prefabId = ctrl ? ctrl._prefabId : new ShortUid(0, (uint)prefab.GetInstanceID());
            }
            else
            {
                controller._prefab = null;
                controller._prefabId = default;
            }
            return controller;
            
        }

        public static SpawnedObjectController InitializePrefab(GameObject prefab)
        {
            if (prefab == null) throw new System.ArgumentNullException(nameof(prefab));

            var controller = prefab.AddOrGetComponent<SpawnedObjectController>();
            controller._prefab = prefab;
            controller.IsCached = false;
            if (controller._prefabId.Value == 0UL)
            {
                controller._prefabId = new ShortUid(0, (uint)prefab.GetInstanceID());
            }
            return controller;
        }

        #endregion

    }

}
