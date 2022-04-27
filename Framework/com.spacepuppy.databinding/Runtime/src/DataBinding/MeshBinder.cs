using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Utils;

namespace com.spacepuppy.DataBinding
{

    public class MeshBinder : ContentBinder
    {

        #region Fields

        [SerializeField]
        [TypeRestriction(typeof(MeshFilter), typeof(MeshCollider))]
        private Component _target;

        #endregion

        #region Properties

        public Component Target
        {
            get => _target;
            set
            {
                switch(value)
                {
                    case MeshFilter filter:
                        _target = filter;
                        break;
                    case MeshCollider mc:
                        _target = mc;
                        break;
                    default:
                        _target = null;
                        break;
                }
            }
        }

        #endregion

        #region Methods

        public override void Bind(DataBindingContext context, object source)
        {
            var mesh = context.GetBoundValue<Mesh>(source, this.Key);
            switch(_target)
            {
                case MeshFilter filter:
                    filter.sharedMesh = mesh;
                    break;
                case MeshCollider mc:
                    mc.sharedMesh = mesh;
                    break;
            }
        }

        #endregion

    }
}
