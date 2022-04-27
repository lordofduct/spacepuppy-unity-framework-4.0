using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Utils;

namespace com.spacepuppy.DataBinding
{

    public class FillbarColorGradientBinder : ContentBinder
    {

        #region Fields

        [SerializeField]
        private Image _fillbar;

        [SerializeField]
        private Gradient _gradient = new Gradient();

        #endregion

        #region Methods

        public override void Bind(DataBindingContext context, object source)
        {
            if(_fillbar)
            {
                _fillbar.color = _gradient.Evaluate(Mathf.Clamp01(context.GetBoundValue<float>(source, this.Key)));
            }
        }

        #endregion

    }
}
