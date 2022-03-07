using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Utils;

namespace com.spacepuppy.DataBinding
{

    public class SpriteBinder : ContentBinder
    {

        #region Fields

        [SerializeField]
        private UnityEngine.UI.Image _target;

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

        public override void Bind(object source, object value)
        {
            if (!_target) return;

            switch(value)
            {
                case Sprite spr:
                    this.Sprite = spr;
                    break;
#if SP_ADDRESSABLES
                case UnityEngine.AddressableAssets.AssetReference assref:
                    {
                        var handle = com.spacepuppy.Addressables.AddressableUtils.LoadOrGetAssetAsync<Sprite>(assref);
                        if (handle.IsComplete)
                        {
                            this.Sprite = handle.GetResult();
                        }
                        else
                        {
                            handle.OnComplete(h =>
                            {
                                this.Sprite = h.GetResult();
                            });
                        }
                    }
                    break;
#endif
            }
        }

        #endregion

    }

}
