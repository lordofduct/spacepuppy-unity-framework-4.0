using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Utils;

namespace com.spacepuppy.DataBinding
{

    public class MaterialBinder : ContentBinder
    {

        #region Fields

        [SerializeField]
        [TypeRestriction(typeof(Renderer), typeof(UnityEngine.UI.Graphic))]
        private Component _target;

        #endregion

        #region Properties

        public Component Target
        {
            get => _target;
            set
            {
                switch (value)
                {
                    case Renderer renderer:
                        _target = renderer;
                        break;
                    case UnityEngine.UI.Graphic gr:
                        _target = gr;
                        break;
                    default:
                        _target = null;
                        break;
                }
            }
        }

        #endregion

        #region Methods

        public override void Bind(object source, object value)
        {
            var mat = value as Material;
            switch (_target)
            {
                case Renderer renderer:
                    renderer.sharedMaterial = mat;
                    break;
                case UnityEngine.UI.Graphic gr:
                    gr.material = mat;
                    break;
            }
        }

        #endregion

    }
}
