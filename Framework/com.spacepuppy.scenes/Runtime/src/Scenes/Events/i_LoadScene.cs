#pragma warning disable 0649 // variable declared but not used.

using UnityEngine;
using UnityEngine.SceneManagement;
using com.spacepuppy.Events;
using com.spacepuppy.Async;

namespace com.spacepuppy.Scenes.Events
{

    public class i_LoadScene : AutoTriggerable
    {

        #region Fields

        [SerializeField]
        [Tooltip("Prefix with # to load by index.")]
        private SceneRef _scene;
        [SerializeField]
        private LoadSceneMode _mode;
        [SerializeField]
        [EnumPopupExcluding((int)LoadSceneBehaviour.AsyncAndWait)]
        private LoadSceneBehaviour _behaviour;

        [SerializeField]
        [Tooltip("A token used to persist data across scenes.")]
        VariantReference _persistentToken = new VariantReference();

        [Space(10f)]
        [Infobox("If the targets of this complete event get destroyed during the load they will not activate.")]
        [SerializeField]
        private SPEvent _onComplete = new SPEvent("OnComplete");

        #endregion

        #region Properties

        public SceneRef Scene
        {
            get => _scene;
            set => _scene = value;
        }

        public LoadSceneMode Mode
        {
            get => _mode;
            set => _mode = value;
        }

        public LoadSceneBehaviour Behaviour
        {
            get => _behaviour;
            set => _behaviour = value.RestrictAsyncAndAwait();
        }

        public VariantReference PersistentToken => _persistentToken;

        public SPEvent OnComplete => _onComplete;

        #endregion

        #region Methods

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;
            if (string.IsNullOrEmpty(_scene.SceneName)) return false;

            var persistentToken = IProxyExtensions.ReduceIfProxy(_persistentToken.Value);

            IRadicalWaitHandle handle = _scene.LoadScene(_mode, _behaviour.RestrictAsyncAndAwait(), persistentToken);
            if(_onComplete.HasReceivers && handle != null)
            {
                handle.OnComplete(o => _onComplete.ActivateTrigger(this, null));
            }

            return true;
        }

        #endregion

    }

}
