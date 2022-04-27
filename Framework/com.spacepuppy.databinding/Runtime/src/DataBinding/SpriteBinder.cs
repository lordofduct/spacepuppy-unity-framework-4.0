using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Utils;
using com.spacepuppy.Addressables;

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
        private int _asyncLoadHash;
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

            var handle = com.spacepuppy.Addressables.AddressableUtils.LoadOrGetAssetAsync<Sprite>(assref);
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
        }

#endif


        #endregion

    }

}
