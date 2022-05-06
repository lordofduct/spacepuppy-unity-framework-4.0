#if SP_ADDRESSABLES
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AddressableAssets;
using com.spacepuppy.Async;
using com.spacepuppy.Utils;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace com.spacepuppy.Addressables
{

    public struct AssetAddress
    {

        #region Fields

        private object _address;

        #endregion

        #region CONSTRUCTOR

        public AssetAddress(UnityEngine.Object obj)
        {
            _address = obj;
        }

        public AssetAddress(AssetReference assetref)
        {
            _address = assetref;
        }

        public AssetAddress(IResourceLocation location)
        {
            _address = location;
        }

        #endregion

        #region Properties

        public object Address => _address;

        #endregion

        #region Methods

        public bool IsAddressable()
        {
            switch (_address)
            {
                case UnityEngine.Object obj:
                    return false;
                case AssetReference assetref:
                case IResourceLocation loc:
                case string key:
                    return true;
                default:
                    return false;

            }
        }
        
        public bool IsConfigured()
        {
            switch (_address)
            {
                case UnityEngine.Object obj:
                    return true;
                case AssetReference assetref:
                    return assetref.IsConfigured();
                case IResourceLocation loc:
                    return true;
                case string key:
                    return true;
                default:
                    return false;
            }
        }

        public void Configure(UnityEngine.Object obj)
        {
            _address = obj;
        }

        public void Configure(AssetReference assetref)
        {
            _address = assetref;
        }

        public void Configure(IResourceLocation location)
        {
            _address = location;
        }

        public void Configure(string key)
        {
            _address = key;
        }

        public AsyncWaitHandle<T> LoadOrGetAssetAsync<T>() where T : class
        {
            switch(_address)
            {
                case T t:
                    return AsyncWaitHandle<T>.Result(t);
                case UnityEngine.Object obj:
                    return AsyncWaitHandle<T>.Result(ObjUtil.GetAsFromSource<T>(obj));
                case AssetReference assetref:
                    return assetref.LoadOrGetAssetAsync<T>();
                case IResourceLocation loc:
                    return UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<T>(loc).AsAsyncWaitHandle();
                case string key:
                    return UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<T>(key).AsAsyncWaitHandle();
                default:
                    return AsyncWaitHandle<T>.Empty;
            }
        }

        /// <summary>
        /// If the asset is loaded it is returned, otherwise it attempts to load. Only AssetReferences will be sp managed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public AsyncWaitHandle<T> LoadOrGetAssetSPManagedAsync<T>() where T : class
        {
            switch (_address)
            {
                case T t:
                    return AsyncWaitHandle<T>.Result(t);
                case UnityEngine.Object obj:
                    return AsyncWaitHandle<T>.Result(ObjUtil.GetAsFromSource<T>(obj));
                case AssetReference assetref:
                    return assetref.LoadOrGetAssetSPManagedAsync<T>();
                case IResourceLocation loc:
                    return AddressableUtils.LoadAssetSPManagedAsync<T>(loc).AsAsyncWaitHandle();
                case string key:
                    return AddressableUtils.LoadAssetSPManagedAsync<T>(key).AsAsyncWaitHandle();
                default:
                    return AsyncWaitHandle<T>.Empty;
            }
        }

        public AsyncWaitHandle<GameObject> InstantiateAsync(Transform parent = null, bool instantiateInWorldSpace = false)
        {
            switch (_address)
            {
                case GameObject go:
                    return AsyncWaitHandle<GameObject>.Result(GameObject.Instantiate(go, parent, instantiateInWorldSpace));
                case AssetReference assetref:
                    return assetref.InstantiateAsync(parent, instantiateInWorldSpace).AsAsyncWaitHandle();
                case IResourceLocation loc:
                    return UnityEngine.AddressableAssets.Addressables.InstantiateAsync(loc, parent, instantiateInWorldSpace).AsAsyncWaitHandle();
                case string key:
                    return UnityEngine.AddressableAssets.Addressables.InstantiateAsync(key, parent, instantiateInWorldSpace).AsAsyncWaitHandle();
                default:
                    return AsyncWaitHandle<GameObject>.Empty;
            }
        }

        public AsyncWaitHandle<GameObject> InstantiateAsync(Vector3 pos, Quaternion rot, Transform parent = null)
        {
            switch(_address)
            {
                case GameObject go:
                    return AsyncWaitHandle<GameObject>.Result(GameObject.Instantiate(go, pos, rot, parent));
                case AssetReference assetref:
                    return assetref.InstantiateAsync(pos, rot, parent).AsAsyncWaitHandle();
                case IResourceLocation loc:
                    return UnityEngine.AddressableAssets.Addressables.InstantiateAsync(loc, pos, rot, parent).AsAsyncWaitHandle();
                case string key:
                    return UnityEngine.AddressableAssets.Addressables.InstantiateAsync(key, pos, rot, parent).AsAsyncWaitHandle();
                default:
                    return AsyncWaitHandle<GameObject>.Empty;
            }
        }

        /// <summary>
        /// Instantiates the target as a GameObject. Only AssetReferences will be sp managed.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="instantiateInWorldSpace"></param>
        /// <returns></returns>
        public AsyncWaitHandle<GameObject> InstantiateSPManagedAsync(Transform parent = null, bool instantiateInWorldSpace = false)
        {
            switch (_address)
            {
                case GameObject go:
                    return AsyncWaitHandle<GameObject>.Result(GameObject.Instantiate(go, parent, instantiateInWorldSpace));
                case AssetReference assetref:
                    return assetref.InstantiateSPManagedAsync(parent, instantiateInWorldSpace).AsAsyncWaitHandle();
                case IResourceLocation loc:
                    return AddressableUtils.InstantiateSPManagedAsync(loc, parent, instantiateInWorldSpace).AsAsyncWaitHandle();
                case string key:
                    return AddressableUtils.InstantiateSPManagedAsync(key, parent, instantiateInWorldSpace).AsAsyncWaitHandle();
                default:
                    return AsyncWaitHandle<GameObject>.Empty;
            }
        }

        /// <summary>
        /// Instantiates the target as a GameObject. Only AssetReferences will be sp managed.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="rot"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public AsyncWaitHandle<GameObject> InstantiateSPManagedAsync(Vector3 pos, Quaternion rot, Transform parent = null)
        {
            switch (_address)
            {
                case GameObject go:
                    return AsyncWaitHandle<GameObject>.Result(GameObject.Instantiate(go, pos, rot, parent));
                case AssetReference assetref:
                    return assetref.InstantiateSPManagedAsync(pos, rot, parent).AsAsyncWaitHandle();
                case IResourceLocation loc:
                    return AddressableUtils.InstantiateSPManagedAsync(loc, pos, rot, parent).AsAsyncWaitHandle();
                case string key:
                    return AddressableUtils.InstantiateSPManagedAsync(key, pos, rot, parent).AsAsyncWaitHandle();
                default:
                    return AsyncWaitHandle<GameObject>.Empty;
            }
        }

        #endregion

    }

}
#endif
