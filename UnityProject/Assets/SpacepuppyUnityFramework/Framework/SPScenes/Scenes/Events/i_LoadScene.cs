#pragma warning disable 0649 // variable declared but not used.

using UnityEngine;
using UnityEngine.SceneManagement;
using com.spacepuppy.Events;

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
        private LoadSceneBehaviour _behaviour;

        [SerializeField]
        [Tooltip("A token used to persist data across scenes.")]
        VariantReference _persistentToken;

        [Space(10f)]
        [Infobox("If the targets of this complete event get destroyed during the load they will not activate.")]
        [SerializeField]
        private SPEvent _onComplete;

        #endregion

        #region Properties

        public SceneRef Scene
        {
            get { return _scene; }
            set { _scene = value; }
        }

        public LoadSceneMode Mode
        {
            get { return _mode; }
            set { _mode = value; }
        }

        public LoadSceneBehaviour Behaviour
        {
            get { return _behaviour; }
            set { _behaviour = value; }
        }

        public object PersistentToken
        {
            get { return _persistentToken; }
        }

        #endregion

        #region Methods

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;
            if (string.IsNullOrEmpty(_scene.SceneName)) return false;

            var persistentToken = com.spacepuppy.Utils.ObjUtil.ReduceIfProxy(_persistentToken.Value);

            IRadicalWaitHandle handle;
            var nm = _scene.SceneName ?? string.Empty;
            if (nm.StartsWith("#"))
            {
                nm = nm.Substring(1);
                int index;
                if (!int.TryParse(nm, out index))
                    return false;
                if (index < 0 || index >= SceneManager.sceneCountInBuildSettings)
                    return false;

                handle = SceneManagerUtils.LoadScene(index, _mode, _behaviour, persistentToken);
            }
            else
            {
                handle = SceneManagerUtils.LoadScene(nm, _mode, _behaviour, persistentToken);
            }

            if(_onComplete.HasReceivers && handle != null)
            {
                handle.OnComplete(o => _onComplete.ActivateTrigger(this, null));
            }

            return true;
        }

        #endregion

    }

}
