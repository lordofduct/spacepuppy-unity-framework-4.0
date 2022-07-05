using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Utils;
using com.spacepuppy.Dynamic;

namespace com.spacepuppy.DataBinding
{
    public class EnableTargetBinder : ContentBinder
    {

        #region Fields

        [SerializeField]
        [SelectableObject]
        private Component _target;
        [SerializeField]
        private bool _invertEnabled;

        #endregion

        #region Properties

        public Component Target
        {
            get => _target;
            set => _target = value;
        }

        public bool InvertEnabled
        {
            get => _invertEnabled;
            set => _invertEnabled = value;
        }

        #endregion

        #region Methods

        public override void Bind(DataBindingContext context, object source)
        {
            if (!_target) return;

            bool enabled = context.GetBoundValue<bool>(source, this.Key);
            if (_invertEnabled) enabled = !enabled;
            switch (_target)
            {
                case Transform t:
                    t.gameObject.SetActive(enabled);
                    break;
                default:
                    _target.SetEnabled(enabled);
                    break;
            }
        }

        #endregion

    }
}
