using UnityEngine;
using System.Collections.Generic;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Dynamic
{
    public class StateModifierNode : MonoBehaviour, IStateModifier
    {

        #region Fields

        [SerializeField()]
        [TypeReference.Config(typeof(Component), allowAbstractClasses = true, allowInterfaces = true, dropDownStyle = TypeDropDownListingStyle.ComponentMenu)]
        private TypeReference _targetType;

        [SerializeField]
        private bool _respectProxy = true;

        [SerializeField()]
        private VariantCollection _settings = new VariantCollection();

        #endregion

        #region Properties

        public System.Type TargetType
        {
            get => _targetType.Type;
            set => _targetType.Type = value;
        }

        public bool RespectProxy
        {
            get => _respectProxy;
            set => _respectProxy = value;
        }

        public VariantCollection Settings => _settings;

        #endregion

        #region IStateModifier Interface

        void IStateModifier.CopyTo(object targ)
        {
            if (ObjUtil.GetAsFromSource(_targetType, targ, out var obj, _respectProxy))
            {
                _settings.CopyTo(obj);
            }
            else if(targ is IStateToken)
            {
                _settings.CopyTo(targ);
            }
        }

        void IStateModifier.LerpTo(object targ, float t)
        {
            if (ObjUtil.GetAsFromSource<Camera>(targ, out var obj, _respectProxy))
            {
                _settings.LerpTo(obj, t);
            }
            else if(targ is IStateToken)
            {
                _settings.LerpTo(targ, t);
            }
        }

        #endregion

    }
}
