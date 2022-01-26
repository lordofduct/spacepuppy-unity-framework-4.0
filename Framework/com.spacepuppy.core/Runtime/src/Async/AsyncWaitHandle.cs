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

        public static readonly AsyncWaitHandle CompletedHandle = default;

        #region Fields

        private readonly IAsyncWaitHandleProvider _provider;
        public readonly object Token;

        #endregion

        #region CONSTRUCTOR

        public AsyncWaitHandle(IAsyncWaitHandleProvider provider, object token)
        {
            _provider = provider;
            Token = token;
        }

        #endregion

        #region Properties

        public IAsyncWaitHandleProvider Provider => _provider;

        /// <summary>
        /// Is the operation complete. 
        /// Most handles being wrapped are NOT thread safe, only call this from main thread. 
        /// If on a thread await the task returned by GetAwaitable rather than checking this or calling 'OnComplete' to register a callback.
        /// </summary>
        public bool IsComplete => _provider?.IsComplete(this) ?? true;

        /// <summary>
        /// The progress of the operation, not all AsyncWaitHandle's have progress and may return 0/1 based on completion. 
        /// Most handles being wrapped are NOT thread safe, only call this from main thread. 
        /// </summary>
        public float Progress => _provider?.GetProgress(this) ?? 1f;

        #endregion

        #region Methods

        public AsyncWaitHandle<T> Convert<T>()
        {
            if (_provider is IAsyncWaitHandleProvider<T> p)
            {
                return new AsyncWaitHandle<T>(p, Token);
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
                result = new AsyncWaitHandle<T>(p, Token);
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
        public object AsYieldInstruction()
        {
            return _provider?.GetYieldInstruction(this);
        }

        /// <summary>
        /// Register callback that will fire on completion. Will be fired immediately if the handle is complete. 
        /// This will be thread safe.
        /// </summary>
        /// <param name="callback"></param>
        public void OnComplete(System.Action<AsyncWaitHandle> callback)
        {
            _provider?.OnComplete(this, callback);
        }

        /// <summary>
        /// Get a task that can be awaited until the handle completes. 
        /// This will be thread safe.
        /// </summary>
        /// <returns></returns>
        public System.Threading.Tasks.Task AsTask()
        {
            return _provider?.GetTask(this);
        }

#if SP_UNITASK
        /// <summary>
        /// Get a unitask that can be awaited until the handle completes.
        /// This will be thread safe.
        /// </summary>
        /// <returns></returns>
        public UniTask AsUniTask()
        {
            if (_provider is AsyncUtil.IUniTaskAsyncWaitHandleProvider p)
            {
                return p.GetUniTask(this);
            }
            else
            {
                return GetUniTask();
            }
        }
        private async UniTask GetUniTask()
        {
            if (!PlayerLoopHelper.IsMainThread)
            {
                await UniTask.SwitchToMainThread();
            }

            while (!this.IsComplete)
            {
                await UniTask.Yield();
            }
        }

        public UniTask.Awaiter GetAwaiter()
        {
            if (_provider is AsyncUtil.IUniTaskAsyncWaitHandleProvider p)
            {
                return p.GetUniTask(this).GetAwaiter();
            }
            else
            {
                return GetUniTask().GetAwaiter();
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
                return _provider.GetResult(this);
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
            return handle.AsTask() ?? Task.CompletedTask;
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
        public readonly object Token;
        private T _result;

        #endregion

        #region CONSTRUCTOR

        public AsyncWaitHandle(T result)
        {
            _provider = null;
            Token = null;
            _result = result;
        }

        public AsyncWaitHandle(IAsyncWaitHandleProvider<T> provider, object token)
        {
            _provider = provider;
            Token = token;
            _result = default;
        }

        #endregion

        #region Properties

        public IAsyncWaitHandleProvider<T> Provider => _provider;

        /// <summary>
        /// Is the operation complete. 
        /// Most handles being wrapped are NOT thread safe, only call this from main thread. 
        /// If on a thread await the task returned by GetAwaitable rather than checking this or calling 'OnComplete' to register a callback.
        /// </summary>
        public bool IsComplete => _provider?.IsComplete(this) ?? true;

        /// <summary>
        /// The progress of the operation, not all AsyncWaitHandle's have progress and may return 0/1 based on completion. 
        /// Most handles being wrapped are NOT thread safe, only call this from main thread. 
        /// </summary>
        public float Progress => _provider?.GetProgress(this) ?? 1f;

        #endregion

        #region Methods

        /// <summary>
        /// A yield instruction that can be used by a coroutine. 
        /// Most handles being wrapped are NOT thread safe, only call this from main thread. 
        /// </summary>
        /// <returns></returns>
        public object AsYieldInstruction()
        {
            return _provider?.GetYieldInstruction(this);
        }

        /// <summary>
        /// Register callback that will fire on completion. Will be fired immediately if the handle is complete. 
        /// This will be thread safe.
        /// </summary>
        /// <param name="callback"></param>
        public void OnComplete(System.Action<AsyncWaitHandle<T>> callback)
        {
            if (_provider == null)
            {
                callback(this);
            }
            else
            {
                _provider.OnComplete(this, callback);
            }
        }

        /// <summary>
        /// Get a task that can be awaited until the handle completes. 
        /// This will be thread safe.
        /// </summary>
        /// <returns></returns>
        public System.Threading.Tasks.Task<T> AsTask()
        {
            if (_provider == null)
            {
                return System.Threading.Tasks.Task<T>.FromResult(_result);
            }
            else
            {
                return _provider.GetTask(this);
            }
        }

#if SP_UNITASK
        /// <summary>
        /// Get a unitask that can be awaited until the handle completes.
        /// This will be thread safe.
        /// </summary>
        /// <returns></returns>
        public UniTask<T> AsUniTask()
        {
            if (_provider is AsyncUtil.IUniTaskAsyncWaitHandleProvider<T> p)
            {
                return p.GetUniTask(this);
            }
            else
            {
                return GetUniTask();
            }
        }
        private async UniTask<T> GetUniTask()
        {
            if (!PlayerLoopHelper.IsMainThread)
            {
                await UniTask.SwitchToMainThread();
            }

            while (!this.IsComplete)
            {
                await UniTask.Yield();
            }

            return this.GetResult();
        }

        public UniTask<T>.Awaiter GetAwaiter()
        {
            if (_provider == null)
            {
                return new UniTask<T>(_result).GetAwaiter();
            }
            else if (_provider is AsyncUtil.IUniTaskAsyncWaitHandleProvider<T> p)
            {
                return p.GetUniTask(this).GetAwaiter();
            }
            else
            {
                return GetUniTask().GetAwaiter();
            }
        }
#endif

        /// <summary>
        /// Get the result, if any, of the handle after the handle has completed.
        /// </summary>
        /// <returns></returns>
        public T GetResult()
        {
            if (_provider != null)
            {
                return _provider.GetResult(this);
            }
            else
            {
                return _result;
            }
        }

        #endregion

        #region Operators

        public static implicit operator AsyncWaitHandle(AsyncWaitHandle<T> handle)
        {
            return new AsyncWaitHandle(handle._provider, handle.Token);
        }

        public static implicit operator Task<T>(AsyncWaitHandle<T> handle)
        {
            return handle.AsTask() ?? Task.FromResult<T>(handle.GetResult());
        }

        #endregion

        #region Static Methods

        public static AsyncWaitHandle<T> Result(T result)
        {
            return new AsyncWaitHandle<T>(result);
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
        float GetProgress(AsyncWaitHandle handle);
        /// <summary>
        /// Provides if the underlying handle is complete. 
        /// This is not required to be thread safe.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        bool IsComplete(AsyncWaitHandle handle);
        /// <summary>
        /// Provides the underlying handle's yield instruction for a coroutine. 
        /// This is not required to be thread safe since it's only used in coroutines.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        object GetYieldInstruction(AsyncWaitHandle handle);
        /// <summary>
        /// Provides a task that can be awaited until the underlying handle is complete. 
        /// This MUST be thread safe. 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task GetTask(AsyncWaitHandle handle);
        /// <summary>
        /// Provides a hook to attach a callback delegate that will be called when the underlying handle is complete. 
        /// This MUST be thread safe.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="callback"></param>
        void OnComplete(AsyncWaitHandle handle, System.Action<AsyncWaitHandle> callback);
        /// <summary>
        /// Provides a reference to the result provided by the underlying handle, if any. Otherwise return the handle itself.
        /// This MUST be thread safe.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        object GetResult(AsyncWaitHandle handle);
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
        void OnComplete(AsyncWaitHandle<T> handle, System.Action<AsyncWaitHandle<T>> callback);
        /// <summary>
        /// Provides a reference to the result provided by the underlying handle, if any. Otherwise return the handle itself.
        /// This MUST be thread safe.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        T GetResult(AsyncWaitHandle<T> handle);
        /// <summary>
        /// Provides a task that can be awaited until the underlying handle is complete. 
        /// This MUST be thread safe. 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<T> GetTask(AsyncWaitHandle<T> handle);
    }

}
