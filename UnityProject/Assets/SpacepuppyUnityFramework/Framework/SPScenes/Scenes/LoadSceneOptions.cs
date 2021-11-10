using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace com.spacepuppy.Scenes
{

    public abstract class LoadSceneOptions : System.EventArgs, IProgressingYieldInstruction, IRadicalWaitHandle, ISPDisposable
    {
        public static readonly System.Action<ISceneLoadedGlobalHandler, LoadSceneOptions> OnSceneLoadedFunctor = (o, d) => o.OnSceneLoaded(d);

        public event System.EventHandler<LoadSceneOptions> BeforeLoad;
        public event System.EventHandler<LoadSceneOptions> Complete;

        #region Properties

        /// <summary>
        /// The primary scene being loaded by these options. If the options load more than 1 scene, this should be the dominant scene.
        /// </summary>
        public abstract Scene Scene { get; }

        public abstract LoadSceneMode Mode { get; }

        /// <summary>
        /// Use this to pass some token between scenes. 
        /// Anything that handles the ISceneLoadedMessageReceiver will receiver a reference to this handle, and therefore this token.
        /// The token should not be something that is destroyed by the load process.
        /// </summary>
        public object PersistentToken
        {
            get;
            set;
        }

        #endregion

        #region Methods

        public abstract void Begin(ISceneManager manager);

        public abstract bool HandlesScene(Scene scene);

        protected UnityLoadResults LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, LoadSceneBehaviour behaviour = LoadSceneBehaviour.Async)
        {
            switch (behaviour)
            {
                case LoadSceneBehaviour.Standard:
                    {
                        SceneManager.LoadScene(sceneName, mode);
                        var scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
                        return new UnityLoadResults()
                        {
                            Op = null,
                            Scene = scene,
                        };
                    }
                case LoadSceneBehaviour.Async:
                    {
                        var op = SceneManager.LoadSceneAsync(sceneName, mode);
                        var scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
                        return new UnityLoadResults()
                        {
                            Op = op,
                            Scene = scene,
                        };
                    }
                case LoadSceneBehaviour.AsyncAndWait:
                    {
                        var op = SceneManager.LoadSceneAsync(sceneName, mode);
                        op.allowSceneActivation = false;
                        var scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
                        return new UnityLoadResults()
                        {
                            Op = op,
                            Scene = scene,
                        };
                    }
                default:
                    throw new System.InvalidOperationException("Unsupported LoadSceneBehaviour.");
            }
        }

        protected UnityLoadResults LoadScene(int index, LoadSceneMode mode = LoadSceneMode.Single, LoadSceneBehaviour behaviour = LoadSceneBehaviour.Async)
        {
            switch (behaviour)
            {
                case LoadSceneBehaviour.Standard:
                    {
                        SceneManager.LoadScene(index, mode);
                        var scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
                        return new UnityLoadResults()
                        {
                            Op = null,
                            Scene = scene,
                        };
                    }
                case LoadSceneBehaviour.Async:
                    {
                        var op = SceneManager.LoadSceneAsync(index, mode);
                        var scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
                        return new UnityLoadResults()
                        {
                            Op = op,
                            Scene = scene,
                        };
                    }
                case LoadSceneBehaviour.AsyncAndWait:
                    {
                        var op = SceneManager.LoadSceneAsync(index, mode);
                        op.allowSceneActivation = false;
                        var scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
                        return new UnityLoadResults()
                        {
                            Op = op,
                            Scene = scene,
                        };
                    }
                default:
                    throw new System.InvalidOperationException("Unsupported LoadSceneBehaviour.");
            }
        }

        protected virtual void OnBeforeLoad()
        {
            this.BeforeLoad?.Invoke(this, this);
        }

        protected virtual void OnComplete()
        {
            //we purge these handlers as this should only happen once. This is to obey the way IRadicalWaitHandle.OnComplete works
            var d = this.Complete;
            this.Complete = null;
            d?.Invoke(this, this);
        }

        #endregion

        #region IProgressingAsyncOperation Interface

        public abstract float Progress
        {
            get;
        }

        public abstract bool IsComplete
        {
            get;
        }

        bool IRadicalYieldInstruction.Tick(out object yieldObject)
        {
            return this.Tick(out yieldObject);
        }
        protected abstract bool Tick(out object yieldObject);

        #endregion

        #region IRadicalWaitHandle Interface

        bool IRadicalWaitHandle.Cancelled
        {
            get { return !_disposed; }
        }

        void IRadicalWaitHandle.OnComplete(System.Action<IRadicalWaitHandle> callback)
        {
            if (callback == null) return;
            this.Complete += (s,e) => callback(e);
        }

        #endregion

        #region IDisposable Interface

        private bool _disposed;
        public bool IsDisposed
        {
            get { return _disposed; }
        }

        public virtual void Dispose()
        {
            _disposed = true;
        }

        ~LoadSceneOptions()
        {
            //make sure we clean ourselves up
            this.Dispose();
        }

        #endregion

        #region Special Types

        protected struct UnityLoadResults
        {
            public AsyncOperation Op;
            public Scene Scene;
        }

        #endregion

    }

    public class UnmanagedSceneLoadedEventArgs : LoadSceneOptions
    {

        private Scene _scene;
        private LoadSceneMode _mode;

        public UnmanagedSceneLoadedEventArgs(Scene scene, LoadSceneMode mode)
        {
            _scene = scene;
        }

        public override Scene Scene { get { return _scene; } }

        public override LoadSceneMode Mode {  get { return _mode; } }

        public override float Progress { get { return 1f; } }

        public override bool IsComplete { get { return true; } }

        public override void Begin(ISceneManager manager)
        {
            throw new System.NotSupportedException();
        }

        public override bool HandlesScene(Scene scene)
        {
            return _scene == scene;
        }

        protected override bool Tick(out object yieldObject)
        {
            yieldObject = null;
            return false;
        }
    }

}
