using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

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

        public MonoBehaviour Hook { get { return GameLoop.Hook; } }

        public void LoadScene(LoadSceneOptions options)
        {
            if (options == null) throw new System.ArgumentNullException(nameof(options));

            if(_activeLoadOptions.Add(options))
            {
                if(_sceneLoadOptionsCompleteCallback == null) _sceneLoadOptionsCompleteCallback = (s, e) =>
                {
                    _activeLoadOptions.Remove(e);
                    this.OnSceneLoaded(e);
                };

                this.OnBeforeSceneLoaded(options);
                options.Complete += _sceneLoadOptionsCompleteCallback;
                options.Begin(this);
            }
        }

        public LoadSceneWaitHandle LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, LoadSceneBehaviour behaviour = LoadSceneBehaviour.Async, object persistentToken = null)
        {
            var handle = new LoadSceneWaitHandle(sceneName, mode, behaviour, persistentToken);
            this.LoadScene(handle);
            return handle;
        }

        public LoadSceneWaitHandle LoadScene(int sceneBuildIndex, LoadSceneMode mode = LoadSceneMode.Single, LoadSceneBehaviour behaviour = LoadSceneBehaviour.Async, object persistentToken = null)
        {
            if (sceneBuildIndex < 0 || sceneBuildIndex >= SceneManager.sceneCountInBuildSettings) throw new System.IndexOutOfRangeException("sceneBuildIndex");

            var handle = new LoadSceneWaitHandle(sceneBuildIndex, mode, behaviour, persistentToken);
            this.LoadScene(handle);
            return handle;
        }

        public AsyncOperation UnloadScene(Scene scene)
        {
            this.OnBeforeSceneUnloaded(scene);
            return SceneManager.UnloadSceneAsync(scene);
        }

        public Scene GetActiveScene()
        {
            return SceneManager.GetActiveScene();
        }

        public bool SceneExists(string sceneName, bool excludeInactive = false)
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

        #endregion

        #region EventHandlers

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
            LoadSceneOptions handle = null;
            if(_activeLoadOptions.Count > 0)
            {
                var e = _activeLoadOptions.GetEnumerator();
                while(e.MoveNext())
                {
                    if(e.Current.HandlesScene(scene))
                    {
                        handle = e.Current;
                        break;
                    }
                }
            }

            var d = this.SceneLoaded;
            if (d == null) return;

            if (handle == null)
            {
                handle = new UnmanagedSceneLoadedEventArgs(scene);
            }

            d(this, handle);
        }
        protected virtual void OnSceneLoaded(LoadSceneOptions options)
        {
            this.SceneLoaded?.Invoke(this, options);
        }

        protected virtual void OnActiveSceneChanged(Scene lastScene, Scene nextScene)
        {
            var d = this.ActiveSceneChanged;
            if (d == null) return;

            var e = _activeChangeArgs;
            _activeChangeArgs = null;
            if (e == null)
                e = new ActiveSceneChangedEventArgs(lastScene, nextScene);
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
