using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#if SP_UNITASK
using Cysharp.Threading.Tasks;
#endif

namespace com.spacepuppy.Async
{

    /// <summary>
    /// This struct is intended to represent an asynchronous wait token while reducing garbage collection as much as possible. 
    /// By handing it an IAsyncWaitHandleProvider and a token, it will call the appropriate provider method with the token 
    /// to return the desired wait handle (yieldinstruction, task, callback).
    /// 
    /// This is used to unify disparate APIs that may return different kinds of wait handles. This way we can create contracts 
    /// that return AsyncWaitHandles regardless of the actual workflow underneath (coroutine, task, etc).
    /// 
    /// Every sort of handle out there that can be treated as an AsyncWaitHandle should provide an extension method named 
    /// 'AsAsyncWaitHandle' that creates an AsyncWaitHandle with the appropriate provider and token.
    /// </summary>
    public struct AsyncWaitHandle
    {

        #region Fields

        private IAsyncWaitHandleProvider _provider;
        private object _token;

        #endregion

        #region CONSTRUCTOR

        public AsyncWaitHandle(IAsyncWaitHandleProvider provider, object token)
        {
            _provider = provider;
            _token = token;
        }

        #endregion

        #region Properties

        public IAsyncWaitHandleProvider Provider => _provider;

        public object Token => _token;

        public bool IsValid => !object.ReferenceEquals(_provider, null);

        /// <summary>
        /// Is the operation complete. 
        /// Most handles being wrapped are NOT thread safe, only call this from main thread. 
        /// If on a thread await the task returned by GetAwaitable rather than checking this or calling 'OnComplete' to register a callback.
        /// </summary>
        public bool IsComplete => _provider?.IsComplete(_token) ?? false;

        /// <summary>
        /// The progress of the operation, not all AsyncWaitHandle's have progress and may return 0/1 based on completion. 
        /// Most handles being wrapped are NOT thread safe, only call this from main thread. 
        /// </summary>
        public float Progress => _provider?.GetProgress(_token) ?? 0f;

        #endregion

        #region Methods

        public AsyncWaitHandle<T> Convert<T>()
        {
            if(_provider is IAsyncWaitHandleProvider<T> p)
            {
                return new AsyncWaitHandle<T>(p, _token);
            }
            else
            {
                throw new System.InvalidCastException();
            }
        }

        public bool TryConvert<T>(out AsyncWaitHandle<T> result)
        {
            if (_provider is IAsyncWaitHandleProvider<T> p)
            {
                result = new AsyncWaitHandle<T>(p, _token);
                return true;
            }
            else
            {
                result = default(AsyncWaitHandle<T>);
                return false;
            }
        }

        /// <summary>
        /// A yield instruction that can be used by a coroutine. 
        /// Most handles being wrapped are NOT thread safe, only call this from main thread. 
        /// </summary>
        /// <returns></returns>
        public object GetYieldInstruction()
        {
            return _provider?.GetYieldInstruction(_token);
        }

        /// <summary>
        /// Register callback that will fire on completion. Will be fired immediately if the handle is complete. 
        /// This will be thread safe.
        /// </summary>
        /// <param name="callback"></param>
        public void OnComplete(System.Action<AsyncWaitHandle> callback)
        {
            _provider?.OnComplete(_token, callback);
        }

        /// <summary>
        /// Get a task that can be awaited until the handle completes. 
        /// This will be thread safe.
        /// </summary>
        /// <returns></returns>
        public System.Threading.Tasks.Task GetTask()
        {
            return _provider?.GetTask(_token);
        }

#if SP_UNITASK
        /// <summary>
        /// Get a unitask that can be awaited until the handle completes.
        /// This will be thread safe.
        /// </summary>
        /// <returns></returns>
        public async UniTask GetUniTask(CancellationToken token = default(CancellationToken))
        {
            if (!PlayerLoopHelper.IsMainThread)
            {
                await UniTask.SwitchToMainThread();
                token.ThrowIfCancellationRequested();
            }

            while(!this.IsComplete)
            {
                await UniTask.Yield();
                token.ThrowIfCancellationRequested();
            }
        }
#endif

        /// <summary>
        /// Get the result, if any, of the handle after the handle has completed.
        /// </summary>
        /// <returns></returns>
        public object GetResult()
        {
            if (_provider != null)
            {
                return _provider.GetResult(_token);
            }
            else
            {
                return null;
            }
        }

#endregion

#region Operators

        public static implicit operator Task(AsyncWaitHandle handle)
        {
            return handle.GetTask() ?? Task.CompletedTask;
        }

#endregion

    }

    /// <summary>
    /// This struct is intended to represent an asynchronous wait token while reducing garbage collection as much as possible. 
    /// By handing it an IAsyncWaitHandleProvider and a token, it will call the appropriate provider method with the token 
    /// to return the desired wait handle (yieldinstruction, task, callback).
    /// 
    /// This is used to unify disparate APIs that may return different kinds of wait handles. This way we can create contracts 
    /// that return AsyncWaitHandles regardless of the actual workflow underneath (coroutine, task, etc).
    /// 
    /// Every sort of handle out there that can be treated as an AsyncWaitHandle should provide an extension method named 
    /// 'AsAsyncWaitHandle' that creates an AsyncWaitHandle with the appropriate provider and token.
    /// </summary>
    public struct AsyncWaitHandle<T>
    {

#region Fields

        private IAsyncWaitHandleProvider<T> _provider;
        private object _token;

#endregion

#region CONSTRUCTOR

        public AsyncWaitHandle(IAsyncWaitHandleProvider<T> provider, object token)
        {
            _provider = provider;
            _token = token;
        }

#endregion

#region Properties

        public IAsyncWaitHandleProvider<T> Provider => _provider;

        public object Token => _token;

        public bool IsValid => !object.ReferenceEquals(_provider, null);

        /// <summary>
        /// Is the operation complete. 
        /// Most handles being wrapped are NOT thread safe, only call this from main thread. 
        /// If on a thread await the task returned by GetAwaitable rather than checking this or calling 'OnComplete' to register a callback.
        /// </summary>
        public bool IsComplete => _provider?.IsComplete(_token) ?? false;

        /// <summary>
        /// The progress of the operation, not all AsyncWaitHandle's have progress and may return 0/1 based on completion. 
        /// Most handles being wrapped are NOT thread safe, only call this from main thread. 
        /// </summary>
        public float Progress => _provider?.GetProgress(_token) ?? 0f;

#endregion

#region Methods

        /// <summary>
        /// A yield instruction that can be used by a coroutine. 
        /// Most handles being wrapped are NOT thread safe, only call this from main thread. 
        /// </summary>
        /// <returns></returns>
        public object GetYieldInstruction()
        {
            return _provider?.GetYieldInstruction(_token);
        }

        /// <summary>
        /// Register callback that will fire on completion. Will be fired immediately if the handle is complete. 
        /// This will be thread safe.
        /// </summary>
        /// <param name="callback"></param>
        public void OnComplete(System.Action<AsyncWaitHandle<T>> callback)
        {
            _provider?.OnComplete(_token, callback);
        }

        /// <summary>
        /// Get a task that can be awaited until the handle completes. 
        /// This will be thread safe.
        /// </summary>
        /// <returns></returns>
        public System.Threading.Tasks.Task<T> GetTask()
        {
            return _provider?.GetTask(_token);
        }

#if SP_UNITASK
        /// <summary>
        /// Get a unitask that can be awaited until the handle completes.
        /// This will be thread safe.
        /// </summary>
        /// <returns></returns>
        public async UniTask<T> GetUniTask(CancellationToken token = default(CancellationToken))
        {
            if (!PlayerLoopHelper.IsMainThread)
            {
                await UniTask.SwitchToMainThread();
                token.ThrowIfCancellationRequested();
            }

            while (!this.IsComplete)
            {
                await UniTask.Yield();
                token.ThrowIfCancellationRequested();
            }

            return this.GetResult();
        }
#endif

        /// <summary>
        /// Get the result, if any, of the handle after the handle has completed.
        /// </summary>
        /// <returns></returns>
        public T GetResult()
        {
            if(_provider != null)
            {
                return _provider.GetResult(_token);
            }
            else
            {
                return default(T);
            }
        }

#endregion

#region Operators

        public static implicit operator AsyncWaitHandle(AsyncWaitHandle<T> handle)
        {
            return new AsyncWaitHandle(handle._provider, handle._token);
        }

        public static implicit operator Task<T>(AsyncWaitHandle<T> handle)
        {
            return handle.GetTask() ?? Task.FromResult<T>(handle.GetResult());
        }

#endregion

    }

    /// <summary>
    /// Converts the token portion of an AsyncWaitHandle to its appropriate data. 
    /// </summary>
    public interface IAsyncWaitHandleProvider
    {
        /// <summary>
        /// Provides the progress of the underlying handle. 
        /// This should return 1 if IsComplete returns true. 
        /// If this underlying handle does not have progress return 0 until complete. 
        /// This is not required to be thread safe. 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        float GetProgress(object token);
        /// <summary>
        /// Provides if the underlying handle is complete. 
        /// This is not required to be thread safe.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        bool IsComplete(object token);
        /// <summary>
        /// Provides the underlying handle's yield instruction for a coroutine. 
        /// This is not required to be thread safe since it's only used in coroutines.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        object GetYieldInstruction(object token);
        /// <summary>
        /// Provides a task that can be awaited until the underlying handle is complete. 
        /// This MUST be thread safe. 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task GetTask(object token);
        /// <summary>
        /// Provides a hook to attach a callback delegate that will be called when the underlying handle is complete. 
        /// This MUST be thread safe.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="callback"></param>
        void OnComplete(object token, System.Action<AsyncWaitHandle> callback);
        /// <summary>
        /// Provides a reference to the result provided by the underlying handle, if any. Otherwise return the handle itself.
        /// This MUST be thread safe.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        object GetResult(object token);
    }

    /// <summary>
    /// Converts the token portion of an AsyncWaitHandle to its appropriate data. 
    /// </summary>
    public interface IAsyncWaitHandleProvider<T> : IAsyncWaitHandleProvider
    {
        /// <summary>
        /// Provides a hook to attach a callback delegate that will be called when the underlying handle is complete. 
        /// This MUST be thread safe.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="callback"></param>
        void OnComplete(object token, System.Action<AsyncWaitHandle<T>> callback);
        /// <summary>
        /// Provides a reference to the result provided by the underlying handle, if any. Otherwise return the handle itself.
        /// This MUST be thread safe.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        new T GetResult(object token);
        /// <summary>
        /// Provides a task that can be awaited until the underlying handle is complete. 
        /// This MUST be thread safe. 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        new Task<T> GetTask(object token);
    }

}
