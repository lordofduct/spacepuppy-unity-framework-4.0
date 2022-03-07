using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Utils;
using com.spacepuppy.Events;
using com.spacepuppy.Async;

#if SP_ADDRESSABLES
using UnityEngine.AddressableAssets;
using com.spacepuppy.Addressables;
#endif

namespace com.spacepuppy.DataBinding
{

    public interface IStampSource
    {
        bool IsAsync { get; }

        GameObject InstantiateStamp(Transform parent);
        AsyncWaitHandle<GameObject> InstantiateStampAsync(Transform parent);

    }

    [System.Serializable]
    public class GameObjectStampSource : IStampSource
    {

        #region Fields

        [SerializeField]
        private GameObject _stampPrefab;

        #endregion

        #region CONSTRUCTOR

        public GameObjectStampSource()
        {
            _stampPrefab = null;
        }

        public GameObjectStampSource(GameObject go)
        {
            _stampPrefab = go;
        }

        #endregion

        #region Properties

        public GameObject StampPrefab
        {
            get => _stampPrefab;
            set => _stampPrefab = value;
        }

        #endregion

        #region IStampSource Interface

        public bool IsAsync => false;

        public GameObject InstantiateStamp(Transform parent)
        {
            if (_stampPrefab == null) return null;

            return UnityEngine.Object.Instantiate(_stampPrefab, parent);
        }

        public AsyncWaitHandle<GameObject> InstantiateStampAsync(Transform parent)
        {
            return new AsyncWaitHandle<GameObject>(InstantiateStamp(parent));
        }

        #endregion

    }

#if SP_ADDRESSABLES

    [System.Serializable]
    public class AddressableStampSource : IStampSource
    {

        #region Fields

        [SerializeField]
        private AssetReferenceGameObject _stampPrefabReference;

        #endregion

        #region CONSTRUCTOR

        public AddressableStampSource()
        {
            _stampPrefabReference = null;
        }

        public AddressableStampSource(AssetReferenceGameObject assetRef)
        {
            _stampPrefabReference = assetRef;
        }

        #endregion

        #region Properties

        public AssetReferenceGameObject StampPrefab
        {
            get => _stampPrefabReference;
            set => _stampPrefabReference = value;
        }

        #endregion

        #region IStampSource Interface

        public bool IsAsync => true;

        public GameObject InstantiateStamp(Transform parent)
        {
            throw new System.NotSupportedException("This IStampSource is asynchronous only.");
        }

        public AsyncWaitHandle<GameObject> InstantiateStampAsync(Transform parent)
        {
            if (!_stampPrefabReference.IsConfigured()) return new AsyncWaitHandle<GameObject>(null);

            return _stampPrefabReference.InstantiateSPManagedAsync(parent).AsAsyncWaitHandle();
        }

        #endregion

    }

#endif

}
