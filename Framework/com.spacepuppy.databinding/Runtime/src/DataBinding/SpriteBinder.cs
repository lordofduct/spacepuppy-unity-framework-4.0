using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Utils;
#if SP_ADDRESSABLES
using com.spacepuppy.Addressables;
#endif

namespace com.spacepuppy.DataBinding
{

    public class SpriteBinder : ContentBinder
    {

        #region Fields

        [SerializeField]
        private UnityEngine.UI.Image _target;

        [SerializeField]
        private bool _bindIfNull;

        #endregion

        #region CONSTRUCTOR

        #endregion

        #region Properties

        public UnityEngine.UI.Image Target
        {
            get => _target;
            set => _target = value;
        }

        public Sprite Sprite
        {
            get => _target ? _target.sprite : null;
            set
            {
                if (_target) _target.sprite = value;
            }
        }

        #endregion

        #region Methods

        public override void Bind(DataBindingContext context, object source)
        {
            if (!_target) return;

            switch (context.GetBoundValue(source, this.Key))
            {
                case Sprite spr:
                    this.Sprite = spr;
                    break;
#if SP_ADDRESSABLES
                case UnityEngine.AddressableAssets.AssetReference assref:
                    {
                        _asyncLoadHash++;
                        this.DoAssetLoad(assref, _asyncLoadHash);
                    }
                    break;
#endif
                default:
                    if (_bindIfNull)
                    {
                        this.Sprite = null;
                    }
                    break;
            }
        }


#if SP_ADDRESSABLES

        [System.NonSerialized]
        private Sprite _addressableSpriteToBeReleased;
        [System.NonSerialized]
        private int _asyncLoadHash;

        protected override void OnDestroy()
        {
            base.OnDestroy();

            this.TryReleaseSprite();
        }

        private void TryReleaseSprite()
        {
            var s = _addressableSpriteToBeReleased;
            _addressableSpriteToBeReleased = null;
            if (s)
            {
                if (this.Sprite == s) this.Sprite = null;
                UnityEngine.AddressableAssets.Addressables.Release(s);
            }
        }

        private void DoAssetLoad(UnityEngine.AddressableAssets.AssetReference assref, int hash)
        {
            if (!assref.IsConfigured())
            {
                if (_bindIfNull)
                {
                    this.Sprite = null;
                }
                return;
            }

            /*
             * REDACTED - see note below
            var handle = assref.LoadOrGetAssetAsync<Sprite>();
            if (handle.IsComplete)
            {
                this.Sprite = handle.GetResult();
            }
            else
            {
                handle.OnComplete(h =>
                {
                    if (_asyncLoadHash == hash)
                    {
                        this.Sprite = h.GetResult();
                    }
                });
            }
            */

            //NOTE - we will load the asset directly so that the SpriteBinder can release it. Otherwise if the source prematurely releases it we lose the sprite.
            this.TryReleaseSprite();
            var handle = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<Sprite>(assref.RuntimeKey);
            handle.Completed += (h) =>
            {
                if (h.Status != UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                {
                    if (_bindIfNull && _asyncLoadHash == hash)
                    {
                        this.Sprite = null;
                    }
                    return;
                }

                if (_asyncLoadHash == hash)
                {
                    _addressableSpriteToBeReleased = h.Result;
                    this.Sprite = _addressableSpriteToBeReleased;
                }
                else
                {
                    UnityEngine.AddressableAssets.Addressables.Release(h);
                }
            };
        }
#endif

        #endregion

    }

}
