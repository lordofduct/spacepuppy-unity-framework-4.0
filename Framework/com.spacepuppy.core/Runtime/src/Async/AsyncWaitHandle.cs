using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.spacepuppy.Async
{

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

        public bool IsComplete => _provider?.IsComplete(_token) ?? false;

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

        public object GetYieldInstruction()
        {
            return _provider?.GetYieldInstruction(_token);
        }

        public void OnComplete(System.Action<AsyncWaitHandle> callback)
        {
            _provider?.OnComplete(_token, callback);
        }

        public System.Threading.Tasks.Task GetAwaitable()
        {
            return _provider?.GetAwaitable(_token);
        }

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

    }

    /// <summary>
    /// This struct is intended to represent an asynchronous wait token while reducing garbage collection as much as possible. 
    /// By handing it an IAsyncWaitHandleProvider and a token, it will call the appropriate provider method with the token 
    /// to return the desired wait handle (yieldinstruction, task, callback).
    /// 
    /// This is used to unify disparate APIs that may return different kinds of wait handles.
    /// 
    /// For example the IAssetSet is intended to bring together various collections from AssetBundle, to Resources, to Addressables 
    /// to funciton polymorphically. But since all of them return different types for the async calls, this facilitate bringing them 
    /// all under one provider interface while keeping gc as minimal as possible (things like callbacks and tasks will inherently have 
    /// gc).
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

        public bool IsComplete => _provider?.IsComplete(_token) ?? false;

        #endregion

        #region Methods

        public object GetYieldInstruction()
        {
            return _provider?.GetYieldInstruction(_token);
        }

        public void OnComplete(System.Action<AsyncWaitHandle<T>> callback)
        {
            _provider?.OnComplete(_token, callback);
        }

        public System.Threading.Tasks.Task<T> GetAwaitable()
        {
            return _provider?.GetAwaitable(_token);
        }

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

        #endregion

    }

    public interface IAsyncWaitHandleProvider
    {
        bool IsComplete(object token);
        object GetYieldInstruction(object token);
        Task GetAwaitable(object token);
        void OnComplete(object token, System.Action<AsyncWaitHandle> callback);
        object GetResult(object token);
    }

    public interface IAsyncWaitHandleProvider<T> : IAsyncWaitHandleProvider
    {
        void OnComplete(object token, System.Action<AsyncWaitHandle<T>> callback);
        new T GetResult(object token);
        new Task<T> GetAwaitable(object token);
    }

}
