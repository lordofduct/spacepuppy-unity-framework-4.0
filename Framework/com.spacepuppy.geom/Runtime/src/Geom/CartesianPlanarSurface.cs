using UnityEngine;
using System.Collections.Generic;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Geom
{

    [CreateAssetMenu(fileName = "CartesianPlanarSurface", menuName = "Spacepuppy/Geom/CartesianPlanarSurface")]
    public sealed class CartesianPlanarSurface : ScriptableObject, ISerializationCallbackReceiver, IPlanarSurface
    {

        public enum CartesianPlane
        {
            XY = 0,
            XZ = 1,
            YZ = 2,
        }

        #region Fields

        [SerializeField]
        private CartesianPlane _plane;

        [System.NonSerialized]
        private Vector3 _normal;

        [SerializeField]
        private UnityEngine.Plane _blargh;

        #endregion

        #region CONSTRUCTOR

        private void OnEnable()
        {
            this.SyncNormal();
        }

        #endregion

        #region Properties

        public CartesianPlane Plane
        {
            get => _plane;
            set
            {
                _plane = value;
                this.SyncNormal();
            }
        }

        public Vector3 SurfaceNormal => _normal;

        #endregion

        #region Methods

        void SyncNormal()
        {
            switch (_plane)
            {
                case CartesianPlane.XY:
                    _normal = Vector3.forward;
                    break;
                case CartesianPlane.XZ:
                    _normal = Vector3.up;
                    break;
                case CartesianPlane.YZ:
                    _normal = Vector3.right;
                    break;
            }
        }

        public Vector3 GetSurfaceNormal() => _normal;

        public Vector2 ProjectVectorTo2D(Vector3 v)
        {
            switch (_plane)
            {
                case CartesianPlane.XY:
                    return VectorUtil.ToXY(v);
                case CartesianPlane.XZ:
                    return VectorUtil.ToXZ(v);
                case CartesianPlane.YZ:
                    return VectorUtil.ToZY(v);
                default:
                    return VectorUtil.ToXY(v);
            }
        }

        public Vector3 ProjectVectorTo3D(Vector2 v)
        {
            switch (_plane)
            {
                case CartesianPlane.XY:
                    return VectorUtil.FromXY(v);
                case CartesianPlane.XZ:
                    return VectorUtil.FromXZ(v);
                case CartesianPlane.YZ:
                    return VectorUtil.FromZY(v);
                default:
                    return VectorUtil.FromXY(v);
            }
        }

        #endregion

        #region IPlanarSurface Interface

        public Vector3 GetSurfaceNormal(Vector2 location) => _normal;

        public Vector3 GetSurfaceNormal(Vector3 location) => _normal;

        public Vector2 ProjectVectorTo2D(Vector3 location, Vector3 v)
        {
            switch (_plane)
            {
                case CartesianPlane.XY:
                    return VectorUtil.ToXY(v);
                case CartesianPlane.XZ:
                    return VectorUtil.ToXZ(v);
                case CartesianPlane.YZ:
                    return VectorUtil.ToZY(v);
                default:
                    return VectorUtil.ToXY(v);
            }
        }

        public Vector3 ProjectVectorTo3D(Vector3 location, Vector2 v)
        {
            switch (_plane)
            {
                case CartesianPlane.XY:
                    return VectorUtil.FromXY(v);
                case CartesianPlane.XZ:
                    return VectorUtil.FromXZ(v);
                case CartesianPlane.YZ:
                    return VectorUtil.FromZY(v);
                default:
                    return VectorUtil.FromXY(v);
            }
        }

        public Vector2 ProjectPosition2D(Vector3 v)
        {
            switch (_plane)
            {
                case CartesianPlane.XY:
                    return VectorUtil.ToXY(v);
                case CartesianPlane.XZ:
                    return VectorUtil.ToXZ(v);
                case CartesianPlane.YZ:
                    return VectorUtil.ToZY(v);
                default:
                    return VectorUtil.ToXY(v);
            }
        }

        public Vector3 ProjectPosition3D(Vector2 v)
        {
            switch (_plane)
            {
                case CartesianPlane.XY:
                    return VectorUtil.FromXY(v);
                case CartesianPlane.XZ:
                    return VectorUtil.FromXZ(v);
                case CartesianPlane.YZ:
                    return VectorUtil.FromZY(v);
                default:
                    return VectorUtil.FromXY(v);
            }
        }

        public Vector3 ClampToSurface(Vector3 v)
        {
            switch (_plane)
            {
                case CartesianPlane.XY:
                    return v.SetZ(0f);
                case CartesianPlane.XZ:
                    return v.SetY(0f);
                case CartesianPlane.YZ:
                    return v.SetX(0f);
                default:
                    return v.SetZ(0f);
            }
        }

        #endregion

        #region ISerializationCallbackReceiver Interface

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            this.SyncNormal();
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            //do nothing
        }

        #endregion

#if UNITY_EDITOR
        private void OnValidate()
        {
            this.SyncNormal();
        }
#endif

    }

}
