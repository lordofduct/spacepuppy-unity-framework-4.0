using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using com.spacepuppy.Async;

#if SP_UNITASK
using Cysharp.Threading.Tasks;
#endif

namespace com.spacepuppy.Scenes
{

    /// <summary>
    /// Blocks until load is complete. If Behaviour is LoadAndWait, it'll stop blocking as a yield instruction, 
    /// but the scene isn't actually loaded until ActivateScene is called. At this point its Status will finally 
    /// read Complete.
    /// </summary>
    public sealed class LoadSceneWaitHandle : LoadSceneOptions
    {

        #region Fields

        private string _sceneName;
        private LoadSceneMode _mode;
        private LoadSceneBehaviour _behaviour;

        private bool _loaded;
        private System.Action<IRadicalWaitHandle> _loadedCallback;

        #endregion

        #region CONSTRUCTOR

        public LoadSceneWaitHandle(string sceneName, LoadSceneMode mode, LoadSceneBehaviour behaviour, object persistentToken = null)
        {
            _sceneName = sceneName;
            _mode = mode;
            _behaviour = behaviour;
            _loaded = false;
            this.PersistentToken = persistentToken;
        }

        public LoadSceneWaitHandle(int buildIndex, LoadSceneMode mode, LoadSceneBehaviour behaviour, object persistentToken = null)
        {
            _sceneName = SceneUtility.GetScenePathByBuildIndex(buildIndex);
            _mode = mode;
            _behaviour = behaviour;
            _loaded = false;
            this.PersistentToken = persistentToken;
        }

        #endregion

        #region Properties

        public string SceneName
        {
            get { return _sceneName; }
        }

        public override LoadSceneMode Mode
        {
            get { return _mode; }
        }

        public LoadSceneBehaviour Behaviour
        {
            get { return _behaviour; }
        }

        public bool ReadyAndWaitingToActivate
        {
            get
            {
                return _loaded && !object.ReferenceEquals(this.PrimaryLoadResult.Op, null) && !this.PrimaryLoadResult.Op.isDone;
            }
        }

        #endregion

        #region Methods

#if SP_UNITASK
        public UniTask.Awaiter GetAwaiter()
        {
            return this.AsUniTask().GetAwaiter();
        }

        protected override void DoBegin(ISceneManager manager)
        {
            _ = this.DoBeginUniTask(manager);
        }

        private async UniTaskVoid DoBeginUniTask(ISceneManager manager)
        {
#else
        public System.Runtime.CompilerServices.TaskAwaiter GetAwaiter()
        {
            return this.AsTask().GetAwaiter();
        }

        protected override async void DoBegin(ISceneManager manager)
        {
#endif
            try
            {
                var handle = this.LoadScene(_sceneName, _mode, _behaviour);

#if SP_UNITASK
                switch (_behaviour)
                {
                    case LoadSceneBehaviour.Standard:
                        await UniTask.Yield();
                        break;
                    case LoadSceneBehaviour.Async:
                        await handle.AsUniTask();
                        break;
                    case LoadSceneBehaviour.AsyncAndWait:
                        if (handle.Op != null)
                        {
                            while (!handle.Op.isDone && handle.Op.progress < 0.9f)
                            {
                                await UniTask.Yield();
                            }
                        }
                        else
                        {
                            await handle.AsUniTask();
                        }
                        break;
                }
#else
                switch (_behaviour)
                {
                    case LoadSceneBehaviour.Standard:
                        await Task.Yield();
                        break;
                    case LoadSceneBehaviour.Async:
                        await handle.AsTask();
                        break;
                    case LoadSceneBehaviour.AsyncAndWait:
                        while (!handle.Op.isDone && handle.Op.progress < 0.9f)
                        {
                            await Task.Yield();
                        }
                        break;
                }
#endif

                //signal loaded
                _loaded = true;
                var d = _loadedCallback;
                _loadedCallback = null;
                try
                {
                    d?.Invoke(this);
                }
                catch (System.Exception ex)
                {
                    //just capture this exception, we don't care that the handle failed
                    Debug.LogException(ex);
                }

                //wait for it to signal complete
                if (handle.Op != null)
                {
                    while (!handle.Op.isDone)
                    {
#if SP_UNITASK
                        await UniTask.Yield();
#else
                        await Task.Yield();
#endif
                    }
                }

                this.SignalComplete();
            }
            catch (System.Exception ex)
            {
                this.SignalError();
                throw ex;
            }
        }

        public void WaitToActivate()
        {
            if (_behaviour >= LoadSceneBehaviour.Async)
            {
                _behaviour = LoadSceneBehaviour.AsyncAndWait;
                var op = this.PrimaryLoadResult.Op;
                if (op != null && !op.isDone)
                {
                    op.allowSceneActivation = false;
                }
            }
        }

        public void ActivateScene()
        {
            var op = this.PrimaryLoadResult.Op;
            if (op != null) op.allowSceneActivation = true;
        }

        public override LoadSceneOptions Clone()
        {
            var result = (LoadSceneWaitHandle)base.Clone();
            result._loaded = false;
            result._loadedCallback = null;
            return result;
        }

        #endregion

        #region IProgressingAsyncOperation Interface

        public override bool IsComplete => _loaded;

        protected override bool Tick(out object yieldObject)
        {
            yieldObject = null;
            return !_loaded;
        }

        protected override void RegisterWaitHandleOnComplete(Action<IRadicalWaitHandle> callback)
        {
            if (callback == null) return;

            if (_loaded)
            {
                base.RegisterWaitHandleOnComplete(callback);
            }
            else
            {
                _loadedCallback += callback;
            }
        }

        #endregion

    }

}
