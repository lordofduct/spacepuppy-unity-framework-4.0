using UnityEngine;
using System.Collections.Generic;

namespace com.spacepuppy.Spawn
{

    public class SpawnablePrefabEntryCollectionAttribute : WeightedValueCollectionAttribute
    {
        public SpawnablePrefabEntryCollectionAttribute() : base(SpawnablePrefabEntry.PROP_WEIGHT, SpawnablePrefabEntry.PROP_PREFAB) { }
    }

    [System.Serializable]
    public struct SpawnablePrefabEntry
    {

        public const string PROP_WEIGHT = nameof(_weight);
        public const string PROP_PREFAB = nameof(_prefab);


        [SerializeField]
        [UnityEngine.Serialization.FormerlySerializedAs("Weight")]
        private float _weight;
        [SerializeField]
        [UnityEngine.Serialization.FormerlySerializedAs("Prefab")]
        [TypeRestriction(typeof(ISpawnable), typeof(GameObject), HideTypeDropDownIfSingle = true)]
        private UnityEngine.Object _prefab;

        public SpawnablePrefabEntry(GameObject prefab, float weight)
        {
            _prefab = prefab;
            _weight = weight;
        }

        public SpawnablePrefabEntry(ISpawnable prefab, float weight)
        {
            _prefab = prefab as UnityEngine.Object;
            _weight = weight;
        }

        #region Properties

        public float Weight
        {
            get => _weight;
            set => _weight = value;
        }

        public UnityEngine.Object Prefab
        {
            get => _prefab;
            set
            {
                switch (value)
                {
                    case UnityEngine.GameObject:
                        _prefab = value;
                        break;
                    case ISpawnable:
                        _prefab = value;
                        break;
                    default:
                        _prefab = null;
                        break;
                }
            }
        }

        #endregion


        public GameObject Spawn(ISpawnPool pool, Vector3 position, Quaternion rotation, Transform parent)
        {
            switch (_prefab)
            {
                case UnityEngine.GameObject go:
                    return pool.Spawn(go, position, rotation, parent);
                case ISpawnable sp:
                    GameObject instance;
                    sp.Spawn(out instance, pool, position, rotation, parent);
                    return instance;
                default:
                    return null;
            }
        }

        public bool Spawn(out GameObject instance, ISpawnPool pool, Vector3 position, Quaternion rotation, Transform parent)
        {
            switch (_prefab)
            {
                case UnityEngine.GameObject go:
                    instance = pool.Spawn(go, position, rotation, parent);
                    return instance != null;
                case ISpawnable sp:
                    sp.Spawn(out instance, pool, position, rotation, parent);
                    return instance;
                default:
                    instance = null;
                    return false;
            }
        }

    }
}
