#if SP_ADDRESSABLES
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace com.spacepuppy.Addressables
{

    /// <summary>
    /// Releases a AsyncOperationHandle when Dispose is called. Allows AsyncOperationHandle to easily be used with the 'using (...)' paradigm.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct DisposableAsyncOperationHandle<T> : System.IDisposable
    {
        public AsyncOperationHandle<T> Handle;
        public T Result => this.Handle.Result;
        public AsyncOperationStatus Status => this.Handle.Status;

        public DisposableAsyncOperationHandle(AsyncOperationHandle<T> handle)
        {
            this.Handle = handle;
        }

        public void Dispose()
        {
            try
            {
                UnityEngine.AddressableAssets.Addressables.Release(this.Handle);
            }
            catch { }
        }
    }

    /// <summary>
    /// Releases a AsyncOperationHandle when Dispose is called. Allows AsyncOperationHandle to easily be used with the 'using (...)' paradigm.
    /// </summary>
    public struct DisposableAsyncOperationHandle : System.IDisposable
    {
        public AsyncOperationHandle Handle;
        public object Result => this.Handle.Result;
        public AsyncOperationStatus Status => this.Handle.Status;

        public DisposableAsyncOperationHandle(AsyncOperationHandle handle)
        {
            this.Handle = handle;
        }

        public void Dispose()
        {
            try
            {
                UnityEngine.AddressableAssets.Addressables.Release(this.Handle);
            }
            catch { }
        }
    }

}

#endif
