#if SP_ADDRESSABLES
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

using com.spacepuppy.Utils;
using com.spacepuppy.Async;
using System.Threading.Tasks;
using com.spacepuppy.Events;

namespace com.spacepuppy.Addressables
{

    public static class AddressableUtils
    {

        public static DisposableAsyncOperationHandle<T> AsDisposable<T>(this AsyncOperationHandle<T> handle)
        {
            return new DisposableAsyncOperationHandle<T>()
            {
                Handle = handle
            };
        }

        public static DisposableAsyncOperationHandle AsDisposable(this AsyncOperationHandle handle)
        {
            return new DisposableAsyncOperationHandle()
            {
                Handle = handle
            };
        }

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
        public static bool IsConfigured(this AssetReference asset)
        {
            return !string.IsNullOrEmpty(asset?.AssetGUID);
        }

        private static void RegisterSPManaged(object asset)
        {
            var go = GameObjectUtil.GetGameObjectFromSource(asset);
            if (go != null)
            {
                go.AddOrGetComponent<AddressableKillHandle>();
            }

        }

        public static AsyncOperationHandle<TObject> LoadAssetSPManagedAsync<TObject>(this AssetReference reference)
        {
            if (reference == null) throw new System.ArgumentNullException(nameof(reference));

            var handle = reference.LoadAssetAsync<TObject>();
            handle.Completed += (h) =>
            {
                if (h.Status == AsyncOperationStatus.Succeeded)
                {
                    RegisterSPManaged(h.Result);
                }
            };
            return handle;
        }

        public static AsyncWaitHandle<TObject> LoadOrGetAssetAsync<TObject>(this AssetReference reference) where TObject : class
        {
            if (reference == null) throw new System.ArgumentNullException(nameof(reference));

            if (reference.Asset || (reference.OperationHandle.IsValid() && reference.OperationHandle.IsDone))
            {
                return new AsyncWaitHandle<TObject>(ObjUtil.GetAsFromSource<TObject>(reference.Asset));
            }
            else if (reference.OperationHandle.IsValid())
            {
                return reference.OperationHandle.Convert<TObject>().AsAsyncWaitHandle();
            }
            else
            {
                return reference.LoadAssetAsync<TObject>().AsAsyncWaitHandle();
            }
        }

        public static AsyncWaitHandle<TObject> LoadOrGetAssetAsync<TObject>(this AssetReferenceT<TObject> reference) where TObject : UnityEngine.Object
        {
            return LoadOrGetAssetAsync<TObject>((AssetReference)reference);
        }

        public static AsyncWaitHandle<TObject> LoadOrGetAssetAsync<TObject>(this AssetReferenceIT<TObject> reference) where TObject : class
        {
            return LoadOrGetAssetAsync<TObject>((AssetReference)reference);
        }

        public static AsyncWaitHandle<TObject> LoadOrGetAssetSPManagedAsync<TObject>(this AssetReference reference) where TObject : class
        {
            if (reference == null) throw new System.ArgumentNullException(nameof(reference));

            if (reference.Asset || (reference.OperationHandle.IsValid() && reference.OperationHandle.IsDone))
            {
                var obj = ObjUtil.GetAsFromSource<TObject>(reference.Asset);
                if (obj != null)
                {
                    RegisterSPManaged(obj);
                }
                return new AsyncWaitHandle<TObject>(obj);
            }
            else if (reference.OperationHandle.IsValid())
            {
                var handle = reference.OperationHandle.Convert<TObject>();
                handle.Completed += (h) =>
                {
                    if (h.Status == AsyncOperationStatus.Succeeded)
                    {
                        RegisterSPManaged(h.Result);
                    }
                };
                return handle.AsAsyncWaitHandle();
            }
            else
            {
                var handle = reference.LoadAssetAsync<TObject>();
                handle.Completed += (h) =>
                {
                    if (h.Status == AsyncOperationStatus.Succeeded)
                    {
                        RegisterSPManaged(h.Result);
                    }
                };
                return handle.AsAsyncWaitHandle();
            }
        }

        public static AsyncWaitHandle<TObject> LoadOrGetAssetSPManagedAsync<TObject>(this AssetReferenceT<TObject> reference) where TObject : UnityEngine.Object
        {
            return LoadOrGetAssetSPManagedAsync<TObject>((AssetReference)reference);
        }

        public static AsyncWaitHandle<TObject> LoadOrGetAssetSPManagedAsync<TObject>(this AssetReferenceIT<TObject> reference) where TObject : class
        {
            return LoadOrGetAssetSPManagedAsync<TObject>((AssetReference)reference);
        }

        public static AsyncOperationHandle<GameObject> InstantiateSPManagedAsync(this AssetReference reference, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (reference == null) throw new System.ArgumentNullException(nameof(reference));

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
            if (reference == null) throw new System.ArgumentNullException(nameof(reference));

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

            public float GetProgress(AsyncWaitHandle handle)
            {
                if (handle.Token is AsyncOperationHandle h)
                {
                    return h.IsDone ? 1f : h.PercentComplete;
                }
                else
                {
                    throw new System.InvalidOperationException("An instance of AsyncOperationHandleProvider was associated with a token that was not an AsyncOperationHandle.");
                }
            }

            public System.Threading.Tasks.Task GetTask(AsyncWaitHandle handle)
            {
                if (handle.Token is AsyncOperationHandle h)
                {
                    if (GameLoop.InvokeRequired)
                    {
                        Task result = null;
                        GameLoop.UpdateHandle.Invoke(() => result = h.IsDone ? System.Threading.Tasks.Task.CompletedTask : h.Task);
                        return result ?? Task.CompletedTask;
                    }
                    else
                    {
                        return h.IsDone ? System.Threading.Tasks.Task.CompletedTask : h.Task;
                    }
                }
                else
                {
                    throw new System.InvalidOperationException("An instance of AsyncOperationHandleProvider was associated with a token that was not an AsyncOperationHandle.");
                }
            }

            public object GetYieldInstruction(AsyncWaitHandle handle)
            {
                if (handle.Token is AsyncOperationHandle)
                {
                    return handle.Token;
                }
                else
                {
                    throw new System.InvalidOperationException("An instance of AsyncOperationHandleProvider was associated with a token that was not an AsyncOperationHandle.");
                }
            }

            public bool IsComplete(AsyncWaitHandle handle)
            {
                if (handle.Token is AsyncOperationHandle h)
                {
                    return h.IsDone;
                }
                else
                {
                    throw new System.InvalidOperationException("An instance of AsyncOperationHandleProvider was associated with a token that was not an AsyncOperationHandle.");
                }
            }

            public void OnComplete(AsyncWaitHandle handle, System.Action<AsyncWaitHandle> callback)
            {
                if (handle.Token is AsyncOperationHandle h)
                {
                    if (GameLoop.InvokeRequired)
                    {
                        GameLoop.UpdateHandle.BeginInvoke(() =>
                        {
                            if (h.IsDone)
                            {
                                callback(h.AsAsyncWaitHandle());
                            }
                            else
                            {
                                h.Completed += (aoh) =>
                                {
                                    callback(aoh.AsAsyncWaitHandle());
                                };
                            }
                        });
                    }
                    else
                    {
                        if (h.IsDone)
                        {
                            callback(h.AsAsyncWaitHandle());
                        }
                        else
                        {
                            h.Completed += (aoh) =>
                            {
                                callback(aoh.AsAsyncWaitHandle());
                            };
                        }
                    }
                }
                else
                {
                    throw new System.InvalidOperationException("An instance of AsyncOperationHandleProvider was associated with a token that was not an AsyncOperationHandle.");
                }
            }

            public object GetResult(AsyncWaitHandle handle)
            {
                if (handle.Token is AsyncOperationHandle h)
                {
                    return h.Result;
                }
                else
                {
                    throw new System.InvalidOperationException("An instance of AsyncOperationHandleProvider was associated with a token that was not an AsyncOperationHandle.");
                }
            }

        }

        private class AsyncOperationHandleProvider<TObject> : IAsyncWaitHandleProvider<TObject>
        {
            public static readonly AsyncOperationHandleProvider<TObject> Default = new AsyncOperationHandleProvider<TObject>();

            public float GetProgress(AsyncWaitHandle handle)
            {
                if (handle.Token is AsyncOperationHandle<TObject> h)
                {
                    return h.IsDone ? 1f : h.PercentComplete;
                }
                else
                {
                    throw new System.InvalidOperationException("An instance of AsyncOperationHandleProvider<TObject> was associated with a token that was not an AsyncOperationHandle<TObject>.");
                }
            }

            public System.Threading.Tasks.Task<TObject> GetTask(AsyncWaitHandle<TObject> handle)
            {
                if (handle.Token is AsyncOperationHandle<TObject> h)
                {
                    if (GameLoop.InvokeRequired)
                    {
                        Task<TObject> result = null;
                        GameLoop.UpdateHandle.Invoke(() => result = h.IsDone ? Task.FromResult(h.Result) : h.Task);
                        return result ?? Task.FromResult(h.Result);
                    }
                    else
                    {
                        return h.IsDone ? Task.FromResult(h.Result) : h.Task;
                    }
                }
                else
                {
                    throw new System.InvalidOperationException("An instance of AsyncOperationHandleProvider<TObject> was associated with a token that was not an AsyncOperationHandle<TObject>.");
                }
            }

            System.Threading.Tasks.Task IAsyncWaitHandleProvider.GetTask(AsyncWaitHandle handle)
            {
                if (handle.Token is AsyncOperationHandle<TObject> h)
                {
                    if (GameLoop.InvokeRequired)
                    {
                        Task<TObject> result = null;
                        GameLoop.UpdateHandle.Invoke(() => result = h.IsDone ? Task.FromResult(h.Result) : h.Task);
                        return result ?? Task.FromResult(h.Result);
                    }
                    else
                    {
                        return h.IsDone ? Task.FromResult(h.Result) : h.Task;
                    }
                }
                else
                {
                    throw new System.InvalidOperationException("An instance of AsyncOperationHandleProvider<TObject> was associated with a token that was not an AsyncOperationHandle<TObject>.");
                }
            }

            public object GetYieldInstruction(AsyncWaitHandle handle)
            {
                if (handle.Token is AsyncOperationHandle<TObject>)
                {
                    return handle.Token;
                }
                else
                {
                    throw new System.InvalidOperationException("An instance of AsyncOperationHandleProvider<TObject> was associated with a token that was not an AsyncOperationHandle<TObject>.");
                }
            }

            public bool IsComplete(AsyncWaitHandle handle)
            {
                if (handle.Token is AsyncOperationHandle<TObject> h)
                {
                    return h.IsDone;
                }
                else
                {
                    throw new System.InvalidOperationException("An instance of AsyncOperationHandleProvider<TObject> was associated with a token that was not an AsyncOperationHandle<TObject>.");
                }
            }

            public void OnComplete(AsyncWaitHandle handle, System.Action<AsyncWaitHandle> callback)
            {
                if (handle.Token is AsyncOperationHandle<TObject> h)
                {
                    if (GameLoop.InvokeRequired)
                    {
                        GameLoop.UpdateHandle.BeginInvoke(() =>
                        {
                            if (h.IsDone)
                            {
                                callback(h.AsAsyncWaitHandle());
                            }
                            else
                            {
                                h.Completed += (aoh) =>
                                {
                                    callback(aoh.AsAsyncWaitHandle());
                                };
                            }
                        });
                    }
                    else
                    {
                        if (h.IsDone)
                        {
                            callback(h.AsAsyncWaitHandle());
                        }
                        else
                        {
                            h.Completed += (aoh) =>
                            {
                                callback(aoh.AsAsyncWaitHandle());
                            };
                        }
                    }
                }
                else
                {
                    throw new System.InvalidOperationException("An instance of AsyncOperationHandleProvider<TObject> was associated with a token that was not an AsyncOperationHandle<TObject>.");
                }
            }

            public void OnComplete(AsyncWaitHandle<TObject> handle, System.Action<AsyncWaitHandle<TObject>> callback)
            {
                if (handle.Token is AsyncOperationHandle<TObject> h)
                {
                    if (GameLoop.InvokeRequired)
                    {
                        GameLoop.UpdateHandle.BeginInvoke(() =>
                        {
                            if (h.IsDone)
                            {
                                callback(h.AsAsyncWaitHandle());
                            }
                            else
                            {
                                h.Completed += (aoh) =>
                                {
                                    callback(aoh.AsAsyncWaitHandle());
                                };
                            }
                        });
                    }
                    else
                    {
                        if (h.IsDone)
                        {
                            callback(h.AsAsyncWaitHandle());
                        }
                        else
                        {
                            h.Completed += (aoh) =>
                            {
                                callback(aoh.AsAsyncWaitHandle());
                            };
                        }
                    }
                }
                else
                {
                    throw new System.InvalidOperationException("An instance of AsyncOperationHandleProvider<TObject> was associated with a token that was not an AsyncOperationHandle<TObject>.");
                }
            }

            public TObject GetResult(AsyncWaitHandle<TObject> handle)
            {
                if (handle.Token is AsyncOperationHandle<TObject> h)
                {
                    return h.Result;
                }
                else
                {
                    throw new System.InvalidOperationException("An instance of AsyncOperationHandleProvider<TObject> was associated with a token that was not an AsyncOperationHandle<TObject>.");
                }
            }

            object IAsyncWaitHandleProvider.GetResult(AsyncWaitHandle handle)
            {
                if (handle.Token is AsyncOperationHandle<TObject> h)
                {
                    return h.Result;
                }
                else
                {
                    throw new System.InvalidOperationException("An instance of AsyncOperationHandleProvider<TObject> was associated with a token that was not an AsyncOperationHandle<TObject>.");
                }
            }
        }

        #endregion

    }

}
#endif