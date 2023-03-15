#pragma warning disable 0649 // variable declared but not used.

using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Events;
using com.spacepuppy.Utils;
using System.Runtime.CompilerServices;

namespace com.spacepuppy.Spawn.Events
{

    public class i_Spawn : AutoTriggerable, IObservableTrigger, ISpawnPoint
    {

        public const string TRG_ONSPAWNED = "OnSpawned";
        
        #region Fields

        [SerializeField()]
        [Tooltip("If left empty the default SpawnPool will be used instead.")]
        private SpawnPoolRef _spawnPool = new SpawnPoolRef();

        [SerializeField]
        [RespectsIProxy()]
        [TypeRestriction(typeof(Transform), AllowProxy = true, HideTypeDropDown = true)]
        private UnityEngine.Object _spawnedObjectParent;

        [SerializeField()]
        //[WeightedValueCollection("Weight", "_prefab")]
        [SpawnablePrefabEntryCollection]
        [Tooltip("Objects available for spawning. When spawn is called with no arguments a prefab is selected at random.")]
        private List<SpawnablePrefabEntry> _prefabs;

        [SerializeField]
        private RandomRef _rng;

        [SerializeField()]
        [SPEvent.Config("spawned object (GameObject)")]
        private OnSpawnEvent _onSpawnedObject = new OnSpawnEvent();

        #endregion

        #region Properties

        public ISpawnPool SpawnPool
        {
            get { return _spawnPool.Value; }
            set { _spawnPool.Value = value; }
        }

        public List<SpawnablePrefabEntry> Prefabs
        {
            get { return _prefabs; }
        }
        
        public OnSpawnEvent OnSpawnedObject
        {
            get { return _onSpawnedObject; }
        }

        public IRandom RNG
        {
            get { return _rng.Value; }
            set { _rng.Value = value; }
        }

        #endregion

        #region Methods

        public GameObject Spawn()
        {
            GameObject instance;
            this.Spawn(out instance, _rng.Value);
            return instance;
        }
        public GameObject Spawn(IRandom rng, bool forceIncrementRng = false)
        {
            GameObject instance;
            this.Spawn(out instance, rng, forceIncrementRng);
            return instance;
        }
        public bool Spawn(out GameObject instance, IRandom rng, bool forceIncrementRng = false)
        {
            if (!this.CanTrigger || _prefabs == null || _prefabs.Count == 0)
            {
                instance = null;
                return false;
            }

            if (_prefabs.Count == 1)
            {
                if (forceIncrementRng) rng?.Next();
                return this.Spawn(_prefabs[0], out instance);
            }
            else
            {
                return this.Spawn(_prefabs.PickRandom((o) => o.Weight, rng), out instance);
            }
        }


        public GameObject Spawn(int index)
        {
            if (!this.enabled || _prefabs == null || index < 0 || index >= _prefabs.Count) return null;

            GameObject instance;
            this.Spawn(_prefabs[index], out instance);
            return instance;
        }
        public bool Spawn(out GameObject instance, int index)
        {
            if (!this.enabled || _prefabs == null || index < 0 || index >= _prefabs.Count)
            {
                instance = null;
                return false;
            }

            return this.Spawn(_prefabs[index], out instance);
        }


        public GameObject Spawn(string name)
        {
            if (!this.enabled || _prefabs == null || _prefabs.Count == 0) return null;

            for (int i = 0; i < _prefabs.Count; i++)
            {
                if (_prefabs[i].Prefab != null && _prefabs[i].Prefab.CompareName(name))
                {
                    GameObject instance;
                    this.Spawn(_prefabs[i], out instance);
                    return instance;
                }
            }
            return null;
        }
        public bool Spawn(out GameObject instance, string name)
        {
            if (!this.enabled || _prefabs == null || _prefabs.Count == 0)
            {
                instance = null;
                return false;
            }

            for (int i = 0; i < _prefabs.Count; i++)
            {
                if (_prefabs[i].Prefab != null && _prefabs[i].Prefab.CompareName(name)) return this.Spawn(_prefabs[i], out instance);
            }
            instance = null;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool Spawn(SpawnablePrefabEntry entry, out GameObject instance)
        {
            if (entry.Prefab == null)
            {
                instance = null;
                return false;
            }

            var pool = _spawnPool.ValueOrDefault;
            if (!entry.Spawn(out instance, pool, this.transform.position, this.transform.rotation, ObjUtil.GetAsFromSource<Transform>(_spawnedObjectParent, true)))
            {
                instance = null;
                return false;
            }

            if (_onSpawnedObject?.HasReceivers ?? false)
            {
                _onSpawnedObject.ActivateTrigger(this, instance);
            }
            return true;
        }

        #endregion


        #region ITriggerable Interface

        public override bool CanTrigger
        {
            get { return base.CanTrigger && _prefabs != null && _prefabs.Count > 0; }
        }

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            return this.Spawn(out _, _rng.Value);
        }

        #endregion

        #region IObserverableTarget Interface

        BaseSPEvent[] IObservableTrigger.GetEvents()
        {
            return new BaseSPEvent[] { _onSpawnedObject };
        }

        #endregion

        #region ISpawnPoint Interface

        BaseSPEvent ISpawnPoint.OnSpawned => _onSpawnedObject;

        void ISpawnPoint.Spawn() => this.Spawn(out _, _rng.Value);

        #endregion

        #region Special Types

        [System.Serializable]
        public class OnSpawnEvent : SPDelegate<GameObject>
        {
            public OnSpawnEvent() : base(TRG_ONSPAWNED)
            {

            }
        }

        #endregion

    }

}
