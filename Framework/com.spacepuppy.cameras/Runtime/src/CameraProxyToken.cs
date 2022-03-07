using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy.Dynamic;
using com.spacepuppy.Events;
using com.spacepuppy.Utils;
using com.spacepuppy.Cameras;

namespace com.spacepuppy
{

    [System.Serializable]
    public struct CameraProxy : IProxy
    {

        public enum TargetCamera
        {
            Main = SearchBy.Nothing,
            ByTag = SearchBy.Tag,
            ByName = SearchBy.Name,
            WithType = SearchBy.Type,
        }

        #region Fields

        public TargetCamera Target;
        public string QueryString;

        #endregion

        #region IProxy Interface

        public ProxyParams Params => ProxyParams.QueriesTarget;

        public object GetTargetInternal(System.Type expectedType, object arg)
        {
            ICamera result = null;
            switch (this.Target)
            {
                case TargetCamera.Main:
                    result = CameraPool.Main;
                    break;
                case TargetCamera.ByName:
                case TargetCamera.ByTag:
                case TargetCamera.WithType:
                    result = ObjUtil.Find(CameraPool.All, (SearchBy)this.Target, this.QueryString);
                    break;
                default:
                    result = CameraPool.Main;
                    break;
            }

            return result != null && TypeUtil.IsType(expectedType, typeof(Camera)) ? result.camera : (object)result;
        }

        public System.Type GetTargetType()
        {
            return typeof(ICamera);
        }

        #endregion
    }

    [CreateAssetMenu(fileName = "CameraProxy", menuName = "Spacepuppy/Proxy/CameraProxy")]
    public class CameraProxyToken : ScriptableObject, IProxy
    {

        #region Fields

        [SerializeField]
        [DisplayFlat]
        private CameraProxy _proxy;

        #endregion

        #region IProxy Interface

        public ProxyParams Params => _proxy.Params;

        public object GetTargetInternal(System.Type expectedType, object arg)
        {
            return _proxy.GetTargetInternal(expectedType, arg);
        }

        public System.Type GetTargetType()
        {
            return _proxy.GetTargetType();
        }

        #endregion

    }

}
