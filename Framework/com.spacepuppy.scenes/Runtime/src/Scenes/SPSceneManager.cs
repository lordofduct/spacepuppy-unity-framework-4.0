using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

using com.spacepuppy.Async;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Scenes
{

    /*
     * NOTES - some information to keep me reminded about scenes.
     * 
     * The Scene struct is really just a wrapper around an int/handle. All calls and comparisons reach through. 
     * So even though it's a struct, it acts more like a reference type.
     * 
     */


    public class SPSceneManager : ServiceComponent<ISceneManager>, ISceneManager
    {

        #region Fields

        private StandardSPSceneManagerImplementation _implementation;

        #endregion

        #region CONSTRUCTOR

        public SPSceneManager() : base(Services.AutoRegisterOption.Register, Services.MultipleServiceResolutionOption.UnregisterSelf, Services.UnregisterResolutionOption.DestroySelf)
        {
            _implementation = new StandardSPSceneManagerImplementation();
        }

        public SPSceneManager(Services.AutoRegisterOption autoRegister, Services.MultipleServiceResolutionOption multipleServiceResolution, Services.UnregisterResolutionOption unregisterResolution)
            : base(autoRegister, multipleServiceResolution, unregisterResolution)
        {
            _implementation = new StandardSPSceneManagerImplementation();
        }

        public SPSceneManager(StandardSPSceneManagerImplementation implementation) : base(Services.AutoRegisterOption.Register, Services.MultipleServiceResolutionOption.UnregisterSelf, Services.UnregisterResolutionOption.DestroySelf)
        {
            _implementation = implementation ?? new StandardSPSceneManagerImplementation();
        }

        public SPSceneManager(StandardSPSceneManagerImplementation implementation, Services.AutoRegisterOption autoRegister, Services.MultipleServiceResolutionOption multipleServiceResolution, Services.UnregisterResolutionOption unregisterResolution)
            : base(autoRegister, multipleServiceResolution, unregisterResolution)
        {
            _implementation = implementation ?? new StandardSPSceneManagerImplementation();
        }

        protected override void OnValidAwake()
        {
            _implementation?.Initialize(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _implementation?.Dispose();
        }

        #endregion

        #region Properties

        protected IReadOnlyCollection<LoadSceneOptions> ActiveLoadOptions => _implementation.ActivateLoadOptions;

        #endregion

        #region ISceneManager Interface

        public event System.EventHandler<LoadSceneOptions> BeforeSceneLoaded
        {
            add => _implementation.BeforeSceneLoaded += value;
            remove => _implementation.BeforeSceneLoaded -= value;
        }

        public event System.EventHandler<SceneUnloadedEventArgs> BeforeSceneUnloaded
        {
            add => _implementation.BeforeSceneUnloaded += value;
            remove => _implementation.BeforeSceneUnloaded -= value;
        }
        public event System.EventHandler<SceneUnloadedEventArgs> SceneUnloaded
        {
            add => _implementation.SceneUnloaded += value;
            remove => _implementation.SceneUnloaded -= value;
        }
        public event System.EventHandler<LoadSceneOptions> SceneLoaded
        {
            add => _implementation.SceneLoaded += value;
            remove => _implementation.SceneLoaded -= value;
        }
        public event System.EventHandler<ActiveSceneChangedEventArgs> ActiveSceneChanged
        {
            add => _implementation.ActiveSceneChanged += value;
            remove => _implementation.ActiveSceneChanged -= value;
        }
        public event System.EventHandler<LoadSceneOptions> BeganLoad
        {
            add => _implementation.BeganLoad += value;
            remove => _implementation.BeganLoad -= value;
        }
        public event System.EventHandler<LoadSceneOptions> CompletedLoad
        {
            add => _implementation.CompletedLoad += value;
            remove => _implementation.CompletedLoad -= value;
        }

        public virtual LoadSceneOptions LoadScene(LoadSceneOptions options)
        {
            if (options == null) throw new System.ArgumentNullException(nameof(options));

            return _implementation.LoadScene(options);
        }

        public virtual AsyncWaitHandle UnloadScene(Scene scene)
        {
            return _implementation.UnloadScene(scene);
        }

        public virtual LoadSceneInternalResult LoadSceneInternal(SceneRef scene, LoadSceneParameters parameters, LoadSceneBehaviour behaviour) => SceneManagerUtils.LoadSceneInternal(scene, parameters, behaviour);

        #endregion

    }

    /// <summary>
    /// When implementing ISceneManager it can composite this as an object taking the ISceneManager into the constructor 
    /// to behave the standard way.
    /// </summary>
    public class StandardSPSceneManagerImplementation : ISPDisposable
    {

        #region Fields

        private ISceneManager _owner;

        private SceneUnloadedEventArgs _unloadArgs;
        private ActiveSceneChangedEventArgs _activeChangeArgs;

        private HashSet<LoadSceneOptions> _activeLoadOptions = new HashSet<LoadSceneOptions>();
        private System.EventHandler<LoadSceneOptions> _sceneLoadOptionsCompleteCallback;
        private System.EventHandler<LoadSceneOptions> _sceneLoadOptionsBeforeLoadSceneCalledCallback;

        private bool _disposed;

        #endregion

        #region CONSTRUCTOR

        public StandardSPSceneManagerImplementation()
        {

        }

        public StandardSPSceneManagerImplementation(ISceneManager owner, bool initializeImmediately = false)
        {
            _owner = owner;
            if (initializeImmediately) this.Initialize(owner);
        }

        ~StandardSPSceneManagerImplementation()
        {
            this.Dispose();
        }

        public void Initialize(ISceneManager owner)
        {
            _owner = owner;
            SceneManager.sceneUnloaded += this.OnSceneUnloaded;
            SceneManager.sceneLoaded += this.OnSceneLoaded;
            SceneManager.activeSceneChanged += this.OnActiveSceneChanged;
        }

        #endregion

        #region Properties

        public ISceneManager Owner
        {
            get => _owner;
            set => _owner = value;
        }

        public IReadOnlyCollection<LoadSceneOptions> ActivateLoadOptions => _activeLoadOptions;

        public virtual bool DispatchGlobalHandlerMessages => true;

        #endregion

        #region Disposable Interface

        public bool IsDisposed => _disposed;

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                SceneManager.sceneUnloaded -= this.OnSceneUnloaded;
                SceneManager.sceneLoaded -= this.OnSceneLoaded;
                SceneManager.activeSceneChanged -= this.OnActiveSceneChanged;
                this.OnDisposed();
            }
        }

        protected virtual void OnDisposed() { }

        #endregion

        #region ISceneManager Interface

        public event System.EventHandler<LoadSceneOptions> BeforeSceneLoaded;
        public event System.EventHandler<SceneUnloadedEventArgs> BeforeSceneUnloaded;
        public event System.EventHandler<SceneUnloadedEventArgs> SceneUnloaded;
        public event System.EventHandler<LoadSceneOptions> SceneLoaded;
        public event System.EventHandler<ActiveSceneChangedEventArgs> ActiveSceneChanged;
        public event System.EventHandler<LoadSceneOptions> BeganLoad;
        public event System.EventHandler<LoadSceneOptions> CompletedLoad;

        public LoadSceneOptions LoadScene(LoadSceneOptions options)
        {
            if (_disposed) throw new System.ObjectDisposedException(nameof(StandardSPSceneManagerImplementation));
            if (options == null) throw new System.ArgumentNullException(nameof(options));

            if (GameLoop.InvokeRequired)
            {
                GameLoop.UpdateHandle.Invoke(() => this.LoadScene(options));
                return options;
            }
            else
            {
                if (_activeLoadOptions.Add(options))
                {
                    if (_sceneLoadOptionsBeforeLoadSceneCalledCallback == null) _sceneLoadOptionsBeforeLoadSceneCalledCallback = (s, e) =>
                    {
                        if (!_disposed) this.OnBeforeSceneLoaded(e);
                    };
                    if (_sceneLoadOptionsCompleteCallback == null) _sceneLoadOptionsCompleteCallback = (s, e) =>
                    {
                        _activeLoadOptions.Remove(e);
                        e.BeforeSceneLoadCalled -= _sceneLoadOptionsBeforeLoadSceneCalledCallback;
                        e.Complete -= _sceneLoadOptionsCompleteCallback;
                        if (!_disposed) this.OnCompletedLoad(e);
                    };

                    options.BeforeSceneLoadCalled += _sceneLoadOptionsBeforeLoadSceneCalledCallback;
                    options.Complete += _sceneLoadOptionsCompleteCallback;
                    this.OnBeganLoad(options);
                    this.SignalBeginOnOptions(options);
                }
                return options;
            }
        }

        protected virtual void SignalBeginOnOptions(LoadSceneOptions options)
        {
            options?.Begin(_owner);
        }

        public virtual AsyncWaitHandle UnloadScene(Scene scene)
        {
            if (_disposed) return default;
            this.OnBeforeSceneUnloaded(scene);
            return SceneManager.UnloadSceneAsync(scene).AsAsyncWaitHandle();
        }

        #endregion

        #region EventHandlers

        protected virtual void OnBeganLoad(LoadSceneOptions handle)
        {
            this.BeganLoad?.Invoke(this, handle);
            if (this.DispatchGlobalHandlerMessages && Messaging.HasRegisteredGlobalListener<ISceneManagerBeganLoadGlobalHandler>())
            {
                Messaging.Broadcast<ISceneManagerBeganLoadGlobalHandler, LoadSceneOptions>(handle, (o, a) => o.OnSceneManagerBeganLoad(a));
            }
        }

        protected virtual void OnBeforeSceneLoaded(LoadSceneOptions handle)
        {
            this.BeforeSceneLoaded?.Invoke(this, handle);
            if (this.DispatchGlobalHandlerMessages && Messaging.HasRegisteredGlobalListener<IBeforeSceneLoadedGlobalHandler>())
            {
                Messaging.Broadcast<IBeforeSceneLoadedGlobalHandler, LoadSceneOptions>(handle, (o, a) => o.OnBeforeSceneLoaded(a));
            }
        }

        protected virtual void OnBeforeSceneUnloaded(Scene scene)
        {
            var d = this.BeforeSceneUnloaded;
            if (d == null && !(this.DispatchGlobalHandlerMessages && Messaging.HasRegisteredGlobalListener<IBeforeSceneUnloadedGlobalHandler>())) return;

            var e = _unloadArgs;
            _unloadArgs = null;
            if (e == null)
                e = new SceneUnloadedEventArgs(scene);
            else
                e.Scene = scene;

            d?.Invoke(this, e);
            if (this.DispatchGlobalHandlerMessages) Messaging.Broadcast<IBeforeSceneUnloadedGlobalHandler, System.ValueTuple<ISceneManager, SceneUnloadedEventArgs>>((_owner, e), (o, a) => o.OnBeforeSceneUnloaded(a.Item1, a.Item2));

            _unloadArgs = e;
            _unloadArgs.Scene = default(Scene);
        }

        protected virtual void OnSceneUnloaded(Scene scene)
        {
            var d = this.SceneUnloaded;
            if (d == null && !(this.DispatchGlobalHandlerMessages && Messaging.HasRegisteredGlobalListener<ISceneUnloadedGlobalHandler>())) return;

            var e = _unloadArgs;
            _unloadArgs = null;
            if (e == null)
                e = new SceneUnloadedEventArgs(scene);
            else
                e.Scene = scene;

            d?.Invoke(this, e);
            if (this.DispatchGlobalHandlerMessages) Messaging.Broadcast<ISceneUnloadedGlobalHandler, System.ValueTuple<ISceneManager, SceneUnloadedEventArgs>>((_owner, e), (o, a) => o.OnSceneUnloaded(a.Item1, a.Item2));

            _unloadArgs = e;
            _unloadArgs.Scene = default(Scene);
        }

        protected virtual void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            var d1 = this.SceneLoaded;
            if (d1 == null && this.CompletedLoad == null && !(this.DispatchGlobalHandlerMessages && Messaging.HasRegisteredGlobalListener<ISceneLoadedGlobalHandler>())) return;

            LoadSceneOptions handle = null;
            if (_activeLoadOptions.Count > 0)
            {
                var e = _activeLoadOptions.GetEnumerator();
                while (e.MoveNext())
                {
                    if (e.Current.HandlesScene(scene))
                    {
                        handle = e.Current;
                        break;
                    }
                }
            }

            if (handle == null)
            {
                //signal loading unmanaged scene load
                handle = new UnmanagedSceneLoadedEventArgs(scene, mode);
                d1?.Invoke(this, handle);
                if (this.DispatchGlobalHandlerMessages && Messaging.HasRegisteredGlobalListener<ISceneLoadedGlobalHandler>())
                {
                    Messaging.Broadcast<ISceneLoadedGlobalHandler, LoadSceneOptions>(handle, (o, a) => o.OnSceneLoaded(a));
                }
                this.OnCompletedLoad(handle);
            }
            else
            {
                d1?.Invoke(this, handle);
                if (this.DispatchGlobalHandlerMessages && Messaging.HasRegisteredGlobalListener<ISceneLoadedGlobalHandler>())
                {
                    Messaging.Broadcast<ISceneLoadedGlobalHandler, LoadSceneOptions>(handle, (o, a) => o.OnSceneLoaded(a));
                }
            }
        }
        protected virtual void OnCompletedLoad(LoadSceneOptions options)
        {
            this.CompletedLoad?.Invoke(this, options);
            if (this.DispatchGlobalHandlerMessages && Messaging.HasRegisteredGlobalListener<ILoadSceneOptionsCompleteGlobalHandler>())
            {
                Messaging.Broadcast<ILoadSceneOptionsCompleteGlobalHandler, LoadSceneOptions>(options, (o, a) => o.OnComplete(a));
            }
        }

        protected virtual void OnActiveSceneChanged(Scene lastScene, Scene nextScene)
        {
            var d = this.ActiveSceneChanged;
            if (d == null && !(this.DispatchGlobalHandlerMessages && Messaging.HasRegisteredGlobalListener<IActiveSceneChangedGlobalHandler>())) return;

            var e = _activeChangeArgs;
            _activeChangeArgs = null;
            if (e == null)
            {
                e = new ActiveSceneChangedEventArgs(lastScene, nextScene);
            }
            else
            {
                e.LastScene = lastScene;
                e.NextScene = nextScene;
            }

            d?.Invoke(this, e);
            if (this.DispatchGlobalHandlerMessages) Messaging.Broadcast<IActiveSceneChangedGlobalHandler, System.ValueTuple<ISceneManager, ActiveSceneChangedEventArgs>>((_owner, e), (o, a) => o.OnActiveSceneChanged(a.Item1, a.Item2));

            _activeChangeArgs = e;
            _activeChangeArgs.LastScene = default(Scene);
            _activeChangeArgs.NextScene = default(Scene);
        }

        #endregion

    }

    /// <summary>
    /// Special implementation of ISceneManager used internally as a wrapper around unity standard load methods.
    /// </summary>
    internal class InternalSceneManager : StandardSPSceneManagerImplementation, ISceneManager
    {

        public static readonly InternalSceneManager Instance = new InternalSceneManager();

        #region Static Initializer

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        static void Init()
        {
            if (Services.SPInternal_GetDefaultService<ISceneManager>() == null)
            {
                Services.SPInternal_RegisterDefaultService<ISceneManager>(Instance);
            }
        }

        #endregion

        private bool _registered;

        private InternalSceneManager() : base()
        {
            this.Initialize(this);
        }

        public override bool DispatchGlobalHandlerMessages => _registered;

        public LoadSceneInternalResult LoadSceneInternal(SceneRef scene, LoadSceneParameters parameters, LoadSceneBehaviour behaviour) => SceneManagerUtils.LoadSceneInternal(scene, parameters, behaviour);

        //not treated as an actual service, this are ignored
        public event System.EventHandler ServiceUnregistered;
        void IService.OnServiceRegistered(System.Type serviceTypeRegisteredAs)
        {
            _registered = true;
        }
        void IService.OnServiceUnregistered()
        {
            _registered = false;
            this.ServiceUnregistered?.Invoke(this, System.EventArgs.Empty);
        }

    }

}
