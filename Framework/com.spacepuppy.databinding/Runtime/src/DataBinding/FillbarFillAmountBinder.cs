using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Geom;
using com.spacepuppy.Utils;

namespace com.spacepuppy.DataBinding
{

    public class FillbarFillAmountBinder : ContentBinder
    {

        #region Fields

        [SerializeField]
        private Image _fillbar;

        [SerializeField]
        private Interval _range = Interval.MinMax(0f, 1f);

        #endregion

        #region Methods

        public override void Bind(DataBindingContext context, object source)
        {
            float val = context.GetBoundValue<float>(source, this.Key);
            val = Mathf.Clamp01(_range.CalculatePercentage(val));
            if (_fillbar) _fillbar.fillAmount = val;
        }

        #endregion

    }
}
