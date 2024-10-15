using UnityEngine;
using System.Collections.Generic;
using com.spacepuppy.Project;
using com.spacepuppy.Spawn;
using System.Linq;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Spawn
{

    [CreateAssetMenu(fileName = "SpawnableAssetSet", menuName = "Spacepuppy/Spawnable Asset Set")]
    public class SpawnableAssetSet : QueryableAssetSet, ISpawnable
    {

#if UNITY_EDITOR
        public const string PROP_WEIGHTS = nameof(_weights);
        public const string PROP_LOGIC = nameof(_logic);
#endif

        #region Fields

        [SerializeField]
        private float[] _weights;

        [SerializeReference, SerializeRefPicker(typeof(ISelectionLogic), AllowNull = true, DisplayBox = true, AlwaysExpanded = true)]
        private ISelectionLogic _logic = DefaultLogic;

        #endregion

        #region Properties

        public ISelectionLogic Logic
        {
            get => _logic;
            set => _logic = value;
        }

        #endregion

        #region Methods

        public IEnumerable<SpawnablePrefabEntry> EnumerateSpawnableEntries()
        {
            for (int i = 0; i < this.Assets.Length; i++)
            {
                var prefab = this.Assets[i];
                float w = _weights.Length > i ? _weights[i] : 0f;
                switch (prefab)
                {
                    case null:
                        yield return new SpawnablePrefabEntry((GameObject)null, w);
                        break;
                    case GameObject go:
                        yield return new SpawnablePrefabEntry(go, w);
                        break;
                    case ISpawnable spawnable:
                        yield return new SpawnablePrefabEntry(spawnable, w);
                        break;
                    case Component c:
                        yield return new SpawnablePrefabEntry(c.gameObject, w);
                        break;
                }
            }
        }

        public override void ResetAssets(IEnumerable<Object> assets)
        {
            base.ResetAssets(assets);
            _weights = new float[this.Assets.Length];
            for (int i = 0; i < _weights.Length; i++) _weights[i] = 1f;
        }

        public virtual void ResetAssets(IEnumerable<SpawnablePrefabEntry> entries)
        {
            base.ResetAssets(entries.Select(o => o.Prefab));
            _weights = entries.Select(o => o.Weight).ToArray();
        }

        #endregion

        #region ISpawnable Interface

        bool ISpawnable.Spawn(out UnityEngine.GameObject instance, com.spacepuppy.Spawn.ISpawnPool pool, UnityEngine.Vector3 position, UnityEngine.Quaternion rotation, UnityEngine.Transform parent)
        {
            return (_logic ?? DefaultLogic).Spawn(out instance, this, pool, position, rotation, parent);
        }

        #endregion

        #region Special Types

        private static readonly PickRandomFromSpawnableAssetSet DefaultLogic = new PickRandomFromSpawnableAssetSet();

        public interface ISelectionLogic
        {
            bool Spawn(out UnityEngine.GameObject instance, SpawnableAssetSet assetset, com.spacepuppy.Spawn.ISpawnPool pool, UnityEngine.Vector3 position, UnityEngine.Quaternion rotation, UnityEngine.Transform parent);
        }

        [System.Serializable]
        public class PickRandomFromSpawnableAssetSet : ISelectionLogic
        {

            [SerializeField]
            private RandomRef _rng;

            public IRandom RNG
            {
                get => _rng.Value;
                set => _rng.Value = value;
            }

            public bool Spawn(out UnityEngine.GameObject instance, SpawnableAssetSet assetset, com.spacepuppy.Spawn.ISpawnPool pool, UnityEngine.Vector3 position, UnityEngine.Quaternion rotation, UnityEngine.Transform parent)
            {
                var prefab = assetset ? assetset.EnumerateSpawnableEntries().PickRandom(this.RNG) : default;
                return prefab.Spawn(out instance, pool, position, rotation, parent);
            }
        }

        #endregion

    }

}
