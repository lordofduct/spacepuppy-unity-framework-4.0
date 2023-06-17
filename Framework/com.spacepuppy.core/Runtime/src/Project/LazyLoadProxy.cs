using UnityEngine;
using System.Collections.Generic;
using System;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Project
{

    public class LazyLoadProxy<T> : ScriptableObject, IProxy where T : UnityEngine.Object
    {

        #region Fields

        [SerializeField]
        public LazyLoadReference<T> Target;

        #endregion

        #region Properties

        #endregion

        #region Methods

        public void Configure(T value)
        {
            Target = new LazyLoadReference<T>(value);
        }

        #endregion

        #region IProxy Interface

        ProxyParams IProxy.Params => ProxyParams.QueriesTarget;

        public object GetTargetInternal(Type expectedType, object arg)
        {
            if (!Target.isSet || Target.isBroken) return null;

            return ObjUtil.GetAsFromSource(expectedType, Target.asset);
        }

        public Type GetTargetType() => typeof(T);

        #endregion

    }

    public class LazyLoadProxy : LazyLoadProxy<UnityEngine.Object> { }

}
