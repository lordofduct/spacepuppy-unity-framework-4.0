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
        /// <summary>
        /// Returns true if InstantiateStampAsync MUST be called and InstantiateStamp will throw an exception.
        /// </summary>
        bool IsAsync { get; }

        /// <summary>
        /// Create an instance of the ui that will be databound.
        /// </summary>
        /// <param name="parent">The container it should be instantiated inside of</param>
        /// <param name="datasource">The datasource that will be bound, this can be used if the stamp created depends on what is in the datasource</param>
        /// <returns></returns>
        GameObject InstantiateStamp(Transform parent, object datasource);
        /// <summary>
        /// Create an instance of the ui that will be databound asynchronously.
        /// </summary>
        /// <param name="parent">The container it should be instantiated inside of</param>
        /// <param name="datasource">The datasource that will be bound, this can be used if the stamp created depends on what is in the datasource</param>
        /// <returns></returns>
        AsyncWaitHandle<GameObject> InstantiateStampAsync(Transform parent, object datasource);

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

        public GameObject InstantiateStamp(Transform parent, object datasource)
        {
            if (_stampPrefab == null) return null;

            return UnityEngine.Object.Instantiate(_stampPrefab, parent);
        }

        public AsyncWaitHandle<GameObject> InstantiateStampAsync(Transform parent, object datasource)
        {
            return new AsyncWaitHandle<GameObject>(InstantiateStamp(parent, datasource));
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

        public GameObject InstantiateStamp(Transform parent, object datasource)
        {
            throw new System.NotSupportedException("This IStampSource is asynchronous only.");
        }

        public AsyncWaitHandle<GameObject> InstantiateStampAsync(Transform parent, object datasource)
        {
            if (!_stampPrefabReference.IsConfigured()) return new AsyncWaitHandle<GameObject>(null);

            return _stampPrefabReference.InstantiateSPManagedAsync(parent).AsAsyncWaitHandle();
        }

        #endregion

    }

#endif

}
