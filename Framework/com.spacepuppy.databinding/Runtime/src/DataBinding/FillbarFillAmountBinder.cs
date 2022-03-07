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

        public override void Bind(object source, object value)
        {
            if (_fillbar) _fillbar.fillAmount = Mathf.Clamp01(ConvertUtil.ToSingle(value));
        }

        #endregion

    }
}
