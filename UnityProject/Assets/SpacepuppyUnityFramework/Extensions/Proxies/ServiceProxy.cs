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

        bool IProxy.QueriesTarget
        {
            get { return false; }
        }

        public object GetTarget()
        {
            return Services.Find(_serviceType.Type);
        }

        public object GetTarget(object arg)
        {
            return Services.Find(_serviceType.Type);
        }

        public object GetTargetAs(System.Type tp)
        {
            return ObjUtil.GetAsFromSource(tp, Services.Find(_serviceType.Type));
        }

        public object GetTargetAs(System.Type tp, object arg)
        {
            return ObjUtil.GetAsFromSource(tp, Services.Find(_serviceType.Type));
        }

        public System.Type GetTargetType()
        {
            return _serviceType.Type;
        }

    }

}
