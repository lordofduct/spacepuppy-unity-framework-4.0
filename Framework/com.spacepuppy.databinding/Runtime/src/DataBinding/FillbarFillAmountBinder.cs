using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Utils;

namespace com.spacepuppy.DataBinding
{

    public class FillbarFillAmountBinder : ContentBinder
    {

        #region Fields

        [SerializeField]
        private Image _fillbar;

        #endregion

        #region Methods

        public override void Bind(DataBindingContext context, object source)
        {
            if (_fillbar) _fillbar.fillAmount = Mathf.Clamp01(context.GetBoundValue<float>(source, this.Key));
        }

        #endregion

    }
}
