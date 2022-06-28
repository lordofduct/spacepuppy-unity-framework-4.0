#if SP_ADDRESSABLES
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

using com.spacepuppy.Scenes;

#if SP_UNITASK
using Cysharp.Threading.Tasks;
#endif

namespace com.spacepuppy.Addressables
{

    [System.Serializable]
    public sealed class LoadAddressableSceneOptions : LoadSceneOptions
    {

        #region Fields

        [SerializeField]
        [Tooltip("Prefix with # to load by index.")]
        private AssetReferenceScene _scene;
        [SerializeField]
        private LoadSceneMode _mode;

        [SerializeField]
        [Tooltip("A token used to persist data across scenes.")]
        VariantReference _persistentToken = new VariantReference();

        #endregion

        #region Properties

        public AssetReferenceScene SceneReference
        {
            get => _scene;
            set => _scene = value;
        }

        public LoadSceneMode ConfiguredMode
        {
            get => _mode;
            set => _mode = value;
        }

        public VariantReference ConfiguredPersistentToken => _persistentToken;

        #endregion

        #region LoadSceneOptions Interface

        public override LoadSceneMode Mode => _mode;

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
                this.PersistentToken = IProxyExtensions.ReduceIfProxy(_persistentToken.Value);

                if (_scene == null)
                {
                    this.SignalError();
                    return;
                }

                var op = _scene.LoadSceneAsync(_mode, true);
#if SP_UNITASK
                await op;
#else
                await op.Task;
#endif

                this.RegisterHandlesScene(null, op.Result.Scene, _mode);

                this.SignalComplete();
            }
            catch (System.Exception ex)
            {
                this.SignalError();
                throw ex;
            }
        }

        #endregion

        #region ICloneable Interface

        public override LoadSceneOptions Clone()
        {
            var result = base.Clone() as LoadAddressableSceneOptions;
            result._scene = _scene?.Clone();
            result._persistentToken = _persistentToken.Clone();
            return result;
        }

        #endregion

    }

}
#endif
