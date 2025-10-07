using System.Threading.Tasks;

#if SP_UNITASK
using Cysharp.Threading.Tasks;
#endif

namespace com.spacepuppy.Async
{

    public interface IAsyncWaitHandle
    {
        bool IsComplete { get; }
        float Progress { get; }

        object AsYieldInstruction();
        void OnComplete(System.Action<IAsyncWaitHandle> callback);
        System.Threading.Tasks.Task AsTask();

#if SP_UNITASK
        UniTask AsUniTask();
        UniTask.Awaiter GetAwaiter();
#else
        System.Runtime.CompilerServices.TaskAwaiter GetAwaiter();
#endif

    }

#if SP_UNITASK

    public interface IUniTaskAsyncWaitHandleProvider : IAsyncWaitHandleProvider
    {
        UniTask GetUniTask(AsyncWaitHandle handle);
    }

    public interface IUniTaskAsyncWaitHandleProvider<T> : IUniTaskAsyncWaitHandleProvider, IAsyncWaitHandleProvider<T>
    {
        UniTask<T> GetUniTask(AsyncWaitHandle<T> handle);
    }

#endif

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
    public readonly struct AsyncWaitHandle : IAsyncWaitHandle
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
            if (callback == null) throw new System.ArgumentNullException(nameof(callback));
            if (_provider != null && !_provider.IsComplete(this))
            {
                _provider.OnComplete(this, callback);
            }
            else
            {
                callback.Invoke(this);
            }
        }
        void IAsyncWaitHandle.OnComplete(System.Action<IAsyncWaitHandle> callback) => _provider?.OnComplete(this, (a) => callback(a));

        /// <summary>
        /// Get a task that can be awaited until the handle completes. 
        /// This will be thread safe.
        /// </summary>
        /// <returns></returns>
        public System.Threading.Tasks.Task AsTask()
        {
            return _provider?.GetTask(this) ?? System.Threading.Tasks.Task.CompletedTask;
        }

#if SP_UNITASK
        /// <summary>
        /// Get a unitask that can be awaited until the handle completes.
        /// This will be thread safe.
        /// </summary>
        /// <returns></returns>
        public UniTask AsUniTask()
        {
            if (_provider is IUniTaskAsyncWaitHandleProvider p)
            {
                return p.GetUniTask(this);
            }
            else if (_provider != null)
            {
                return GetUniTask();
            }
            else
            {
                return UniTask.CompletedTask;
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
            if (_provider is IUniTaskAsyncWaitHandleProvider p)
            {
                return p.GetUniTask(this).GetAwaiter();
            }
            else if (_provider != null)
            {
                return GetUniTask().GetAwaiter();
            }
            else
            {
                return UniTask.CompletedTask.GetAwaiter();
            }
        }
#else
        public System.Runtime.CompilerServices.TaskAwaiter GetAwaiter()
        {
            return this.AsTask().GetAwaiter();
        }
#endif

        /// <summary>
        /// Get the result, if any, of the handle after the handle has completed.
        /// </summary>
        /// <returns></returns>
        public object GetResult() => _provider?.GetResult(this);

        #endregion

        #region Operators

        public static implicit operator Task(AsyncWaitHandle handle)
        {
            return handle.AsTask() ?? Task.CompletedTask;
        }

        #endregion

        #region Static Methods

        public static AsyncWaitHandle<T> Result<T>(T result) => new AsyncWaitHandle<T>(result);

        public static AsyncWaitHandle Empty => default(AsyncWaitHandle);

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
    public readonly struct AsyncWaitHandle<T> : IAsyncWaitHandle
    {

        #region Fields

        private readonly IAsyncWaitHandleProvider<T> _provider;
        public readonly object Token;
        private readonly T _result;

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
            if (callback == null) throw new System.ArgumentNullException(nameof(callback));
            if (_provider != null && !_provider.IsComplete(this))
            {
                _provider.OnComplete(this, callback);
            }
            else
            {
                callback.Invoke(this);
            }
        }
        void IAsyncWaitHandle.OnComplete(System.Action<IAsyncWaitHandle> callback) => _provider?.OnComplete(this, (a) => callback(a));

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
        System.Threading.Tasks.Task IAsyncWaitHandle.AsTask()
        {
            return this.AsTask();
        }

#if SP_UNITASK
        /// <summary>
        /// Get a unitask that can be awaited until the handle completes.
        /// This will be thread safe.
        /// </summary>
        /// <returns></returns>
        public UniTask<T> AsUniTask()
        {
            if (_provider is IUniTaskAsyncWaitHandleProvider<T> p)
            {
                return p.GetUniTask(this);
            }
            else if (_provider != null)
            {
                return GetUniTask();
            }
            else
            {
                return UniTask.FromResult<T>(_result);
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
        UniTask IAsyncWaitHandle.AsUniTask() => this.AsUniTask();

        public UniTask<T>.Awaiter GetAwaiter()
        {
            if (_provider is IUniTaskAsyncWaitHandleProvider<T> p)
            {
                return p.GetUniTask(this).GetAwaiter();
            }
            else if (_provider != null)
            {
                return GetUniTask().GetAwaiter();
            }
            else
            {
                return new UniTask<T>(_result).GetAwaiter();
            }
        }
        UniTask.Awaiter IAsyncWaitHandle.GetAwaiter() => ((UniTask)this.AsUniTask()).GetAwaiter();
#else
        public System.Runtime.CompilerServices.TaskAwaiter<T> GetAwaiter()
        {
            return this.AsTask().GetAwaiter();
        }
        System.Runtime.CompilerServices.TaskAwaiter IAsyncWaitHandle.GetAwaiter() => ((System.Threading.Tasks.Task)this.AsTask()).GetAwaiter();
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

        public static AsyncWaitHandle<T> Empty => default(AsyncWaitHandle<T>);

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
