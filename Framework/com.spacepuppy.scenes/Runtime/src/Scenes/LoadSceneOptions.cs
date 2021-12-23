using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using com.spacepuppy.Dynamic;
using com.spacepuppy.Async;
using log4net.Util;

namespace com.spacepuppy.Scenes
{

    public enum LoadSceneOptionsStatus
    {
        Unused = 0,
        Running = 1,
        Complete = 2,
        Error = 3,
    }

    /// <summary>
    /// Base abstract class for all LoadSceneOptions (it's not an interface since it has to be an EventArgs and there is a lot of helper methods to facilitate functionality). 
    /// 
    /// When implmenting:
    /// Implement all the abstract methods, as well as the Clone method. It is important that when cloning your LoadSceneOptions that any state info used during loading are reset. 
    /// It is best to call base.Clone from your override as it cleans up the inherited state as well. Things to clean up include registered event handlers, status's, wait handles, etc. 
    /// 
    /// Then when implementing 'DoBegin' make sure to call SignalComplete when finished, or SignalError on failure, otherwise the wait handle will block forever.
    /// 
    /// Other methods remain virtual for more advanced implementations of LoadSceneOptions. 
    /// </summary>
    public abstract class LoadSceneOptions : System.EventArgs, IProgressingYieldInstruction, IRadicalWaitHandle, IRadicalEnumerator, ISPDisposable, System.ICloneable
    {

        private static readonly System.Action<ISceneLoadedGlobalHandler, LoadSceneOptions> OnSceneLoadedFunctor = (o, d) => o.OnSceneLoaded(d);

        public event System.EventHandler<LoadSceneOptions> BeforeLoad;
        public event System.EventHandler<LoadSceneOptions> Complete;

        #region Properties

        public LoadSceneOptionsStatus Status
        {
            get;
            private set;
        }

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

        /// <summary>
        /// Begins loading the scene on the main thread. If called on another thread, will block until the next frame on mainthread.
        /// </summary>
        /// <param name="manager"></param>
        public void Begin(ISceneManager manager)
        {
            if(GameLoop.InvokeRequired)
            {
                GameLoop.UpdateHandle.Invoke(() => this.Begin(manager));
            }
            else
            {
                if (this.Status != LoadSceneOptionsStatus.Unused)
                {
                    throw new System.InvalidOperationException("LoadSceneOptions should be ran only once. Clone if you need a copy.");
                }

                this.Status = LoadSceneOptionsStatus.Running;
                this.DoBegin(manager);
            }
        }

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

        protected void SignalComplete()
        {
            this.Status = LoadSceneOptionsStatus.Complete;

            var d = this.Complete;
            this.Complete = null;
            try
            {
                d?.Invoke(this, this);
            }
            catch(System.Exception ex)
            {
                Debug.LogException(ex);
            }
            try
            {
                com.spacepuppy.Utils.Messaging.Broadcast(this, OnSceneLoadedFunctor);
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        protected void SignalError()
        {
            this.Status = LoadSceneOptionsStatus.Error;

            var d = this.Complete;
            this.Complete = null;
            try
            {
                d?.Invoke(this, this);
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
            try
            {
                com.spacepuppy.Utils.Messaging.Broadcast(this, OnSceneLoadedFunctor);
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        #endregion

        #region Abstract Interface

        /// <summary>
        /// The primary scene being loaded by these options. If the options load more than 1 scene, this should be the dominant scene.
        /// </summary>
        public abstract Scene Scene { get; }

        /// <summary>
        /// How the primary scene returned by the 'Scene' property was loaded.
        /// </summary>
        public abstract LoadSceneMode Mode { get; }

        public abstract float Progress
        {
            get;
        }

        protected abstract void DoBegin(ISceneManager manager);

        public abstract bool HandlesScene(Scene scene);

        #endregion

        #region ICloneable Interface

        object System.ICloneable.Clone()
        {
            return this.Clone();
        }

        public virtual LoadSceneOptions Clone()
        {
            var result = this.MemberwiseClone() as LoadSceneOptions;
            result.Status = LoadSceneOptionsStatus.Unused;
            result.BeforeLoad = null;
            result.Complete = null;
            return result;
        }

        #endregion

        #region IProgressingAsyncOperation Interface

        public virtual bool IsComplete
        {
            get => this.Status >= LoadSceneOptionsStatus.Complete;
        }

        bool IRadicalYieldInstruction.Tick(out object yieldObject)
        {
            return this.Tick(out yieldObject);
        }
        protected virtual bool Tick(out object yieldObject)
        {
            yieldObject = null;
            return !this.IsComplete;
        }

        #endregion

        #region IRadicalWaitHandle Interface

        bool IRadicalWaitHandle.Cancelled
        {
            get { return !_disposed; }
        }

        void IRadicalWaitHandle.OnComplete(System.Action<IRadicalWaitHandle> callback)
        {
            this.RegisterWaitHandleOnComplete(callback);
        }

        protected virtual void RegisterWaitHandleOnComplete(System.Action<IRadicalWaitHandle> callback)
        {
            if (callback == null) return;
            this.Complete += (s, e) => callback(e);
        }

        #endregion

        #region IEnumerator Interface

        object System.Collections.IEnumerator.Current => null;

        bool System.Collections.IEnumerator.MoveNext()
        {
            object inst;
            return this.Tick(out inst);
        }

        void System.Collections.IEnumerator.Reset()
        {
            //do nothing
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

            public float Progress => Op?.progress ?? 0f;

            public bool IsComplete => Op?.isDone ?? false;

            public async System.Threading.Tasks.Task<Scene> GetAwaitable()
            {
                await Op.AsAsyncWaitHandle().GetTask();
                return this.Scene;
            }

            public object GetYieldInstruction()
            {
                return Op;
            }

            public void OnComplete(System.Action<UnityLoadResults> callback)
            {
                var r = this;
                Op.AsAsyncWaitHandle().OnComplete((h) => callback(r));
            }

            public static readonly UnityLoadResults Empty = new UnityLoadResults();
        }

        #endregion

    }

    public class UnmanagedSceneLoadedEventArgs : LoadSceneOptions
    {

        private Scene _scene;
        private LoadSceneMode _mode;

        internal UnmanagedSceneLoadedEventArgs(Scene scene, LoadSceneMode mode)
        {
            _scene = scene;
        }

        public override Scene Scene { get { return _scene; } }

        public override LoadSceneMode Mode {  get { return _mode; } }

        public override float Progress { get { return 1f; } }

        public override bool IsComplete { get { return true; } }

        protected override void DoBegin(ISceneManager manager)
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
