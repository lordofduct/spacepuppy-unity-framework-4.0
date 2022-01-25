using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Utils;

namespace com.spacepuppy.Waypoints
{

    public class TransformControlPoint : MonoBehaviour, IWeightedControlPoint, IGameObjectSource
    {

        #region Fields

        [SerializeField()]
        private WaypointPathComponent _owner;

        #endregion

        #region Properties

        public WaypointPathComponent Owner
        {
            get { return _owner; }
        }

        #endregion

        #region Methods

        public void Initialize(WaypointPathComponent owner)
        {
            _owner = owner;
        }

        #endregion

        #region IWaypoint Interface

        public Vector3 Position
        {
            get
            {
                return (ObjUtil.IsAlive(_owner.TransformRelativeTo)) ? this.transform.GetRelativePosition(_owner.TransformRelativeTo) : this.transform.position;
            }
            set
            {
                if (!object.ReferenceEquals(_owner, null) && _owner.TransformRelativeTo != null)
                    this.transform.localPosition = value;
                else
                    this.transform.position = value;
            }
        }

        public Vector3 Heading
        {
            get
            {
                return (ObjUtil.IsAlive(_owner.TransformRelativeTo)) ? this.transform.GetRelativeRotation(_owner.TransformRelativeTo) * Vector3.forward : this.transform.forward;
            }
            set
            {
                if (!object.ReferenceEquals(_owner, null) && _owner.TransformRelativeTo != null)
                    this.transform.localRotation = Quaternion.LookRotation(value);
                else
                    this.transform.rotation = Quaternion.LookRotation(value);
            }
        }

        public float Strength
        {
            get
            {
                return this.transform.localScale.z;
            }
            set
            {
                this.transform.localScale = Vector3.one * value;
            }
        }

        #endregion

        #region IComponent Interface

        GameObject IGameObjectSource.gameObject
        {
            get
            {
                return this.gameObject;
            }
        }

        Transform IGameObjectSource.transform
        {
            get
            {
                return this.transform;
            }
        }


        #endregion

    }

}
