using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

using com.spacepuppy.Utils;
using com.spacepuppy.Async;

namespace com.spacepuppy.Addressables
{

    public static class AddressableUtils
    {

        public static AsyncWaitHandle AsAsyncWaitHandle(this AsyncOperationHandle handle)
        {
            return new AsyncWaitHandle(AsyncOperationHandleProvider.Default, handle);
        }

        public static AsyncWaitHandle<TObject> AsAsyncWaitHandle<TObject>(this AsyncOperationHandle<TObject> handle)
        {
            return new AsyncWaitHandle<TObject>(AsyncOperationHandleProvider<TObject>.Default, handle);
        }

        /// <summary>
        /// Returns true if the AssetReference has a configured target. That target may not necessarily be valid, but one is configured.
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        public static bool HasTargetGuid(this AssetReference asset)
        {
            return !string.IsNullOrEmpty(asset?.AssetGUID);
        }

        public static AsyncOperationHandle<TObject> LoadAssetSPManagedAsync<TObject>(this AssetReference reference)
        {
            var handle = reference.LoadAssetAsync<TObject>();
            handle.Completed += (h) =>
            {
                if(h.Status == AsyncOperationStatus.Succeeded)
                {
                    var go = GameObjectUtil.GetGameObjectFromSource(h.Result);
                    if (go != null)
                    {
                        go.AddOrGetComponent<AddressableKillHandle>();
                    }
                }
            };
            return handle;
        }

        public static AsyncOperationHandle<GameObject> InstantiateSPManagedAsync(this AssetReference reference, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            var handle = reference.InstantiateAsync(position, rotation, parent);
            handle.Completed += (h) =>
            {
                if (h.Status == AsyncOperationStatus.Succeeded)
                {
                    h.Result.AddOrGetComponent<AddressableKillHandle>();
                }
            };
            return handle;
        }

        public static AsyncOperationHandle<GameObject> InstantiateSPManagedAsync<TObject>(this AssetReference reference, Transform parent = null, bool instantiateInWorldSpace = false)
        {
            var handle = reference.InstantiateAsync(parent, instantiateInWorldSpace);
            handle.Completed += (h) =>
            {
                if (h.Status == AsyncOperationStatus.Succeeded)
                {
                    h.Result.AddOrGetComponent<AddressableKillHandle>();
                }
            };
            return handle;
        }


        #region Special Typers

        private class AsyncOperationHandleProvider : IAsyncWaitHandleProvider
        {
            public static readonly AsyncOperationHandleProvider Default = new AsyncOperationHandleProvider();

            public System.Threading.Tasks.Task GetAwaitable(object token)
            {
                if(token is AsyncOperationHandle h)
                {
                    return h.Task;
                }
                else
                {
                    return System.Threading.Tasks.Task.CompletedTask;
                }
            }

            public object GetYieldInstruction(object token)
            {
                if (token is AsyncOperationHandle)
                {
                    return token;
                }
                else
                {
                    return null;
                }
            }

            public bool IsComplete(object token)
            {
                if (token is AsyncOperationHandle h)
                {
                    return h.IsDone;
                }
                else
                {
                    return true;
                }
            }

            public void OnComplete(object token, System.Action<AsyncWaitHandle> callback)
            {
                if (token is AsyncOperationHandle h)
                {
                    h.Completed += (aoh) =>
                    {
                        callback(aoh.AsAsyncWaitHandle());
                    };
                }
            }

            public object GetResult(object token)
            {
                if (token is AsyncOperationHandle h)
                {
                    return h.Result;
                }
                else
                {
                    return null;
                }
            }

        }

        private class AsyncOperationHandleProvider<TObject> : IAsyncWaitHandleProvider<TObject>
        {
            public static readonly AsyncOperationHandleProvider<TObject> Default = new AsyncOperationHandleProvider<TObject>();

            public System.Threading.Tasks.Task<TObject> GetAwaitable(object token)
            {
                if (token is AsyncOperationHandle<TObject> h)
                {
                    return h.Task;
                }
                else
                {
                    return System.Threading.Tasks.Task<TObject>.FromResult(default(TObject));
                }
            }

            System.Threading.Tasks.Task IAsyncWaitHandleProvider.GetAwaitable(object token)
            {
                if (token is AsyncOperationHandle<TObject> h)
                {
                    return h.Task;
                }
                else
                {
                    return System.Threading.Tasks.Task.CompletedTask;
                }
            }

            public object GetYieldInstruction(object token)
            {
                if (token is AsyncOperationHandle<TObject>)
                {
                    return token;
                }
                else
                {
                    return null;
                }
            }

            public bool IsComplete(object token)
            {
                if (token is AsyncOperationHandle<TObject> h)
                {
                    return h.IsDone;
                }
                else
                {
                    return true;
                }
            }

            public void OnComplete(object token, System.Action<AsyncWaitHandle> callback)
            {
                if (token is AsyncOperationHandle<TObject> h)
                {
                    h.Completed += (aoh) =>
                    {
                        callback(aoh.AsAsyncWaitHandle());
                    };
                }
            }

            public void OnComplete(object token, System.Action<AsyncWaitHandle<TObject>> callback)
            {
                if (token is AsyncOperationHandle<TObject> h)
                {
                    h.Completed += (aoh) =>
                    {
                        callback(aoh.AsAsyncWaitHandle());
                    };
                }
            }

            public TObject GetResult(object token)
            {
                if (token is AsyncOperationHandle<TObject> h)
                {
                    return h.Result;
                }
                else
                {
                    return default(TObject);
                }
            }

            object IAsyncWaitHandleProvider.GetResult(object token)
            {
                if (token is AsyncOperationHandle<TObject> h)
                {
                    return h.Result;
                }
                else
                {
                    return null;
                }
            }
        }

        #endregion

    }

}
