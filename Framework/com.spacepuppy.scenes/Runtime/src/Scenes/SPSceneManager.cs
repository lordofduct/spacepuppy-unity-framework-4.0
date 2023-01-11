using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using com.spacepuppy.Async;

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

        private SceneUnloadedEventArgs _unloadArgs;
        private ActiveSceneChangedEventArgs _activeChangeArgs;

        private HashSet<LoadSceneOptions> _activeLoadOptions = new HashSet<LoadSceneOptions>();
        private System.EventHandler<LoadSceneOptions> _sceneLoadOptionsCompleteCallback;
        private System.EventHandler<LoadSceneOptions> _sceneLoadOptionsBeforeLoadSceneCalledCallback;

        #endregion

        #region CONSTRUCTOR

        protected override void OnValidAwake()
        {
            SceneManager.sceneUnloaded += this.OnSceneUnloaded;
            SceneManager.sceneLoaded += this.OnSceneLoaded;
            SceneManager.activeSceneChanged += this.OnActiveSceneChanged;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            SceneManager.sceneUnloaded -= this.OnSceneUnloaded;
            SceneManager.sceneLoaded -= this.OnSceneLoaded;
            SceneManager.activeSceneChanged -= this.OnActiveSceneChanged;
        }

        #endregion

        #region ISceneManager Interface

        public event System.EventHandler<LoadSceneOptions> BeforeSceneLoaded;
        public event System.EventHandler<SceneUnloadedEventArgs> BeforeSceneUnloaded;
        public event System.EventHandler<SceneUnloadedEventArgs> SceneUnloaded;
        public event System.EventHandler<LoadSceneOptions> SceneLoaded;
        public event System.EventHandler<ActiveSceneChangedEventArgs> ActiveSceneChanged;
        public event System.EventHandler<LoadSceneOptions> BeganLoad;
        public event System.EventHandler<LoadSceneOptions> CompletedLoad;

        public virtual void LoadScene(LoadSceneOptions options)
        {
            if (options == null) throw new System.ArgumentNullException(nameof(options));

            if(GameLoop.InvokeRequired)
            {
                GameLoop.UpdateHandle.Invoke(() => this.LoadScene(options));
            }
            else
            {
                if (_activeLoadOptions.Add(options))
                {
                    if (_sceneLoadOptionsBeforeLoadSceneCalledCallback == null) _sceneLoadOptionsBeforeLoadSceneCalledCallback = (s, e) =>
                    {
                        this.OnBeforeSceneLoaded(e);
                    };
                    if (_sceneLoadOptionsCompleteCallback == null) _sceneLoadOptionsCompleteCallback = (s, e) =>
                    {
                        _activeLoadOptions.Remove(e);
                        options.BeforeSceneLoadCalled -= _sceneLoadOptionsBeforeLoadSceneCalledCallback;
                        options.Complete -= _sceneLoadOptionsCompleteCallback;
                        this.OnCompletedLoad(e);
                    };

                    options.BeforeSceneLoadCalled += _sceneLoadOptionsBeforeLoadSceneCalledCallback;
                    options.Complete += _sceneLoadOptionsCompleteCallback;
                    this.OnBeganLoad(options);
                    options.Begin(this);
                }
            }
        }

        public virtual AsyncOperation UnloadScene(Scene scene)
        {
            this.OnBeforeSceneUnloaded(scene);
            return SceneManager.UnloadSceneAsync(scene);
        }

        public virtual Scene GetActiveScene()
        {
            return SceneManager.GetActiveScene();
        }

        public virtual bool SceneExists(string sceneName, bool excludeInactive = false)
        {
            if (excludeInactive)
            {
                var sc = SceneManager.GetSceneByName(sceneName);
                return sc.IsValid();
            }
            else
            {
                return SceneUtility.GetBuildIndexByScenePath(sceneName) >= 0;
            }
        }

        public virtual LoadSceneInternalResult LoadSceneInternal(string sceneName, LoadSceneParameters parameters, LoadSceneBehaviour behaviour) => SceneManagerUtils.LoadSceneInternal(sceneName, parameters, behaviour);

        #endregion

        #region EventHandlers

        protected virtual void OnBeganLoad(LoadSceneOptions handle)
        {
            this.BeganLoad?.Invoke(this, handle);
        }

        protected virtual void OnBeforeSceneLoaded(LoadSceneOptions handle)
        {
            this.BeforeSceneLoaded?.Invoke(this, handle);
        }

        protected virtual void OnBeforeSceneUnloaded(Scene scene)
        {
            var d = this.BeforeSceneUnloaded;
            if (d == null) return;

            var e = _unloadArgs;
            _unloadArgs = null;
            if (e == null)
                e = new SceneUnloadedEventArgs(scene);
            else
                e.Scene = scene;

            d(this, e);

            _unloadArgs = e;
            _unloadArgs.Scene = default(Scene);
        }

        protected virtual void OnSceneUnloaded(Scene scene)
        {
            var d = this.SceneUnloaded;
            if (d == null) return;

            var e = _unloadArgs;
            _unloadArgs = null;
            if (e == null)
                e = new SceneUnloadedEventArgs(scene);
            else
                e.Scene = scene;

            d(this, e);

            _unloadArgs = e;
            _unloadArgs.Scene = default(Scene);
        }

        protected virtual void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            var d1 = this.SceneLoaded;
            var d2 = this.CompletedLoad;
            if (d1 == null && d2 == null) return;

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

        }
        protected virtual void OnCompletedLoad(LoadSceneOptions options)
        {
            this.CompletedLoad?.Invoke(this, options);
        }

        protected virtual void OnActiveSceneChanged(Scene lastScene, Scene nextScene)
        {
            var d = this.ActiveSceneChanged;
            if (d == null) return;

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

            d(this, e);

            _activeChangeArgs = e;
            _activeChangeArgs.LastScene = default(Scene);
            _activeChangeArgs.NextScene = default(Scene);
        }

        #endregion

    }

}
