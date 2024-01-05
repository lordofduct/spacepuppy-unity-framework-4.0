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


    public class SPSceneManager : ServiceScriptableObject<ISceneManager>, ISceneManager
    {

        #region Fields

        private StandardSPSceneManagerImplementation _implementation;

        #endregion

        #region CONSTRUCTOR

        public SPSceneManager()
        {
            _implementation = new StandardSPSceneManagerImplementation();
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

        public virtual void LoadScene(LoadSceneOptions options)
        {
            if (options == null) throw new System.ArgumentNullException(nameof(options));

            if (GameLoop.InvokeRequired)
            {
                GameLoop.UpdateHandle.Invoke(() => this.LoadScene(options));
            }
            else
            {
                _implementation.LoadScene(options);
            }
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

        public void LoadScene(LoadSceneOptions options)
        {
            if (_disposed) return;
            if (options == null) throw new System.ArgumentNullException(nameof(options));

            if (GameLoop.InvokeRequired)
            {
                GameLoop.UpdateHandle.Invoke(() => this.LoadScene(options));
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
                    options.Begin(_owner);
                }
            }
        }

        public AsyncWaitHandle UnloadScene(Scene scene)
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
            if (Messaging.HasRegisteredGlobalListener<ISceneManagerBeganLoadGlobalHandler>())
            {
                Messaging.Broadcast<ISceneManagerBeganLoadGlobalHandler, LoadSceneOptions>(handle, (o, a) => o.OnSceneManagerBeganLoad(a));
            }
        }

        protected virtual void OnBeforeSceneLoaded(LoadSceneOptions handle)
        {
            this.BeforeSceneLoaded?.Invoke(this, handle);
            if (Messaging.HasRegisteredGlobalListener<IBeforeSceneLoadedGlobalHandler>())
            {
                Messaging.Broadcast<IBeforeSceneLoadedGlobalHandler, LoadSceneOptions>(handle, (o, a) => o.OnBeforeSceneLoaded(a));
            }
        }

        protected virtual void OnBeforeSceneUnloaded(Scene scene)
        {
            var d = this.BeforeSceneUnloaded;
            if (d == null && !Messaging.HasRegisteredGlobalListener<IBeforeSceneUnloadedGlobalHandler>()) return;

            var e = _unloadArgs;
            _unloadArgs = null;
            if (e == null)
                e = new SceneUnloadedEventArgs(scene);
            else
                e.Scene = scene;

            d?.Invoke(this, e);
            Messaging.Broadcast<IBeforeSceneUnloadedGlobalHandler, System.ValueTuple<ISceneManager, SceneUnloadedEventArgs>>((_owner, e), (o, a) => o.OnBeforeSceneUnloaded(a.Item1, a.Item2));

            _unloadArgs = e;
            _unloadArgs.Scene = default(Scene);
        }

        protected virtual void OnSceneUnloaded(Scene scene)
        {
            var d = this.SceneUnloaded;
            if (d == null && !Messaging.HasRegisteredGlobalListener<ISceneUnloadedGlobalHandler>()) return;

            var e = _unloadArgs;
            _unloadArgs = null;
            if (e == null)
                e = new SceneUnloadedEventArgs(scene);
            else
                e.Scene = scene;

            d?.Invoke(this, e);
            Messaging.Broadcast<ISceneUnloadedGlobalHandler, System.ValueTuple<ISceneManager, SceneUnloadedEventArgs>>((_owner, e), (o, a) => o.OnSceneUnloaded(a.Item1, a.Item2));

            _unloadArgs = e;
            _unloadArgs.Scene = default(Scene);
        }

        protected virtual void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            var d1 = this.SceneLoaded;
            var d2 = this.CompletedLoad;
            if (d1 == null && d2 == null && !Messaging.HasRegisteredGlobalListener<ISceneLoadedGlobalHandler>()) return;

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
                d2?.Invoke(this, handle);
            }
            else
            {
                d1?.Invoke(this, handle);
            }

            if (Messaging.HasRegisteredGlobalListener<ISceneLoadedGlobalHandler>())
            {
                Messaging.Broadcast<ISceneLoadedGlobalHandler, LoadSceneOptions>(handle, (o, a) => o.OnSceneLoaded(a));
            }
        }
        protected virtual void OnCompletedLoad(LoadSceneOptions options)
        {
            this.CompletedLoad?.Invoke(this, options);
        }

        protected virtual void OnActiveSceneChanged(Scene lastScene, Scene nextScene)
        {
            var d = this.ActiveSceneChanged;
            if (d == null && !Messaging.HasRegisteredGlobalListener<IActiveSceneChangedGlobalHandler>()) return;

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
            Messaging.Broadcast<IActiveSceneChangedGlobalHandler, System.ValueTuple<ISceneManager, ActiveSceneChangedEventArgs>>((_owner, e), (o, a) => o.OnActiveSceneChanged(a.Item1, a.Item2));

            _activeChangeArgs = e;
            _activeChangeArgs.LastScene = default(Scene);
            _activeChangeArgs.NextScene = default(Scene);
        }

        #endregion

    }

    internal class InternalSceneManager : StandardSPSceneManagerImplementation, ISceneManager
    {

        public static readonly InternalSceneManager Instance = new InternalSceneManager();

        private InternalSceneManager() : base()
        {
            this.Initialize(this);
        }

        public LoadSceneInternalResult LoadSceneInternal(SceneRef scene, LoadSceneParameters parameters, LoadSceneBehaviour behaviour) => SceneManagerUtils.LoadSceneInternal(scene, parameters, behaviour);

        //not treated as an actual service, this are ignored
        event System.EventHandler IService.ServiceUnregistered { add { } remove { } }
        void IService.OnServiceRegistered(System.Type serviceTypeRegisteredAs) => throw new System.InvalidOperationException();
        void IService.OnServiceUnregistered() => throw new System.InvalidOperationException();

    }

}
