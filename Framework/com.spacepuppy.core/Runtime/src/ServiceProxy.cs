using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Utils;

namespace com.spacepuppy
{

    [CreateAssetMenu(fileName = "ServiceProxy", menuName = "Spacepuppy/Proxy/ServiceProxy")]
    public class ServiceProxy : ScriptableObject, IProxy
    {

        [SerializeField]
        [TypeReference.Config(typeof(IService), allowAbstractClasses = true, allowInterfaces = true)]
        private TypeReference _serviceType;

        ProxyParams IProxy.Params => ProxyParams.QueriesTarget;

        public object GetTargetInternal(System.Type expectedType, object arg)
        {
            return Services.Find(_serviceType.Type);
        }

        public System.Type GetTargetType()
        {
            return _serviceType.Type;
        }

    }

}
