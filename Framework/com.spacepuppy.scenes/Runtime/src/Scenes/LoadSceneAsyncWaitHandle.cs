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
    public class LoadSceneWaitHandle : LoadSceneOptions
    {

        #region Fields

        private string _sceneName;
        private LoadSceneMode _mode;
        private LoadSceneBehaviour _behaviour;
        private UnityLoadResults _loadResults;

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

        public override Scene Scene
        {
            get { return _loadResults.Scene; }
        }

        public bool ReadyAndWaitingToActivate
        {
            get { return _loaded && !object.ReferenceEquals(_loadResults.Op, null) && !_loadResults.Op.isDone; }
        }

        #endregion

        #region Methods

#if SP_UNITASK
        protected override void DoBegin(ISceneManager manager)
        {
            _ = this.DoBeginUniTask(manager);
        }

        private async UniTaskVoid DoBeginUniTask(ISceneManager manager)
        {
#else
        protected override async void DoBegin(ISceneManager manager)
        {
#endif
            try
            {
                this.OnBeforeLoad();
                _loadResults = this.LoadScene(_sceneName, _mode, _behaviour);

#if SP_UNITASK
                switch (_behaviour)
                {
                    case LoadSceneBehaviour.Standard:
                        await UniTask.Yield();
                        break;
                    case LoadSceneBehaviour.Async:
                        await _loadResults.Op;
                        break;
                    case LoadSceneBehaviour.AsyncAndWait:
                        while (!_loadResults.Op.isDone && _loadResults.Op.progress < 0.9f)
                        {
                            await UniTask.Yield();
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
                        await _loadResults.GetTask();
                        break;
                    case LoadSceneBehaviour.AsyncAndWait:
                        while (!_loadResults.Op.isDone && _loadResults.Op.progress < 0.9f)
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
                if (_loadResults.Op != null)
                {
                    while (!_loadResults.Op.isDone)
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
                _loadResults = UnityLoadResults.Empty;
                this.SignalError();
                throw ex;
            }
        }

        public override bool HandlesScene(Scene scene)
        {
            return scene == _loadResults.Scene;
        }

        public void WaitToActivate()
        {
            if (_behaviour >= LoadSceneBehaviour.Async)
            {
                _behaviour = LoadSceneBehaviour.AsyncAndWait;
                var op = _loadResults.Op;
                if (op != null && !op.isDone)
                {
                    op.allowSceneActivation = false;
                }
            }
        }

        public void ActivateScene()
        {
            if (_loadResults.Op != null) _loadResults.Op.allowSceneActivation = true;
        }

        public override LoadSceneOptions Clone()
        {
            var result = base.Clone() as LoadSceneWaitHandle;
            result._loadResults = UnityLoadResults.Empty;
            result._loaded = false;
            result._loadedCallback = null;
            return result;
        }

#endregion

#region IProgressingAsyncOperation Interface

        public override float Progress
        {
            get
            {
                return _loadResults.Op?.progress ?? (this.Status >= LoadSceneOptionsStatus.Complete ? 1f : 0f);
            }
        }

        public override bool IsComplete => _loaded;

        protected override bool Tick(out object yieldObject)
        {
            yieldObject = null;
            return !_loaded;
        }

        protected override void RegisterWaitHandleOnComplete(Action<IRadicalWaitHandle> callback)
        {
            if (callback == null) return;

            if(_loaded)
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
