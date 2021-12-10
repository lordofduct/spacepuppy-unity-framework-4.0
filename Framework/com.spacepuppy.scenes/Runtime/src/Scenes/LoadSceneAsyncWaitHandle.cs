using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace com.spacepuppy.Scenes
{

    /// <summary>
    /// Blocks until load is complete. If Behaviour is LoadAndWait, it'll stop blocking, but the scene isn't actually loaded until ActivateScene is called.
    /// </summary>
    public class LoadSceneWaitHandle : LoadSceneOptions
    {

        #region Fields

        private string _sceneName;
        private LoadSceneMode _mode;
        private LoadSceneBehaviour _behaviour;
        private UnityLoadResults _loadResults;

        private bool _loaded;

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

        public override void Begin(ISceneManager manager)
        {
            (manager?.Hook ?? GameLoop.Hook).StartCoroutine(this.DoLoad());
        }

        private System.Collections.IEnumerator DoLoad()
        {
            this.OnBeforeLoad();

            _loadResults = this.LoadScene(_sceneName, _mode, _behaviour);
            switch(_behaviour)
            {
                case LoadSceneBehaviour.Standard:
                    yield return null;
                    break;
                case LoadSceneBehaviour.Async:
                    yield return _loadResults.Op;
                    break;
                case LoadSceneBehaviour.AsyncAndWait:
                    while(!_loadResults.Op.isDone && _loadResults.Op.progress < 0.9f)
                    {
                        yield return null;
                    }
                    break;
            }

            _loaded = true;

            if (_loadResults.Op != null)
            {
                while (!_loadResults.Op.isDone)
                {
                    yield return null;
                }
            }

            this.OnComplete();
            com.spacepuppy.Utils.Messaging.Broadcast(this, OnSceneLoadedFunctor);
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

        #endregion

        #region IProgressingAsyncOperation Interface

        public override float Progress
        {
            get
            {
                return _loadResults.Op?.progress ?? (_loaded ? 1f : 0f);
            }
        }

        public override bool IsComplete
        {
            get
            {
                return _loaded;
            }
        }

        protected override bool Tick(out object yieldObject)
        {
            yieldObject = null;
            return !_loaded;
        }

        #endregion

    }

}
