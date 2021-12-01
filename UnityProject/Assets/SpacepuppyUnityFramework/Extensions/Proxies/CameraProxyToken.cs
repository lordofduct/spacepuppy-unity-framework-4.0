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

        public bool QueriesTarget => false;

        public object GetTarget()
        {
            switch(this.Target)
            {
                case TargetCamera.Main:
                    return CameraPool.Main;
                case TargetCamera.ByName:
                case TargetCamera.ByTag:
                case TargetCamera.WithType:
                    return ObjUtil.Find(CameraPool.All, (SearchBy)this.Target, this.QueryString);
                default:
                    return CameraPool.Main;
            }
        }

        public object GetTarget(object arg)
        {
            return this.GetTarget();
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

        public bool QueriesTarget => _proxy.QueriesTarget;

        public object GetTarget()
        {
            return _proxy.GetTarget();
        }

        public object GetTarget(object arg)
        {
            return _proxy.GetTarget(arg);
        }

        public System.Type GetTargetType()
        {
            return _proxy.GetTargetType();
        }

        #endregion

    }

}
