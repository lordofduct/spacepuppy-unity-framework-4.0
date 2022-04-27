using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Utils;
using com.spacepuppy.Dynamic;

namespace com.spacepuppy.DataBinding
{
    public class PropertyBinder : ContentBinder
    {

        #region Fields

        [SerializeField]
        [SelectableObject]
        private UnityEngine.Object _target;

        [SerializeField]
        private string _targetProperty;

        #endregion

        #region Methods

        public override void Bind(DataBindingContext context, object source)
        {
            if(_target)
            {
                DynamicUtil.SetValue(_target, _targetProperty, context.GetBoundValue(source, this.Key));
            }
        }

        #endregion

    }
}
