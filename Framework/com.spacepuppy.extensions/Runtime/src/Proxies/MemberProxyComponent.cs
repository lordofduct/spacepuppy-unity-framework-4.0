using UnityEngine;
using System.Collections.Generic;

namespace com.spacepuppy
{

    public sealed class MemberProxyComponent : MonoBehaviour, IProxy
    {

        #region Fields

        [SerializeField]
        private MemberProxy _target;

        #endregion

        #region Properties

        public UnityEngine.Object Target
        {
            get { return _target.Target; }
            set { _target.Target = value; }
        }

        public string MemberName
        {
            get { return _target.MemberName; }
            set { _target.MemberName = value; }
        }

        public object Value
        {
            get { return _target.GetValue(); }
            set { _target.SetValue(value); }
        }

        #endregion

        #region Methods

        public object GetValue()
        {
            return _target.GetValue();
        }

        public T GetValue<T>()
        {
            return _target.GetValue<T>();
        }

        public bool SetValue(object value)
        {
            return _target.SetValue(value);
        }

        #endregion

        #region IProxy Interface

        ProxyParams IProxy.Params => ProxyParams.None;

        object IProxy.GetTargetInternal(System.Type expectedType, object arg)
        {
            return _target.GetValue();
        }

        System.Type IProxy.GetTargetType()
        {
            return _target.GetTargetType();
        }

        #endregion

    }

}
