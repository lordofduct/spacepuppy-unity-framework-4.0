using UnityEngine;

using com.spacepuppy;
using com.spacepuppy.Dynamic.Accessors;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Tween
{

    public static class FindAccessor
    {

        #region Transform

        private static IMemberAccessor<Vector3> _transformPosition;
        public static IMemberAccessor<Vector3> TransformPosition { get { return _transformPosition ?? (_transformPosition = new GetterSetterMemberAccessor<Transform, Vector3>(t => t.position, (t, v) => t.position = v)); } }
        public static IMemberAccessor<Vector3> position_ref(this Transform t) { return TransformPosition; }


        private static IMemberAccessor<Vector3> _transformLocalPosition;
        public static IMemberAccessor<Vector3> TransformLocalPosition { get { return _transformLocalPosition ?? (_transformLocalPosition = new GetterSetterMemberAccessor<Transform, Vector3>(t => t.localPosition, (t, v) => t.localPosition = v)); } }
        public static IMemberAccessor<Vector3> localPosition_ref(this Transform t) { return TransformLocalPosition; }


        private static IMemberAccessor<Quaternion> _transformRotation;
        public static IMemberAccessor<Quaternion> TransformRotation { get { return _transformRotation ?? (_transformRotation = new GetterSetterMemberAccessor<Transform, Quaternion>(t => t.rotation, (t, v) => t.rotation = v)); } }
        public static IMemberAccessor<Quaternion> rotation_ref(this Transform t) { return TransformRotation; }


        private static IMemberAccessor<Quaternion> _transformLocalRotation;
        public static IMemberAccessor<Quaternion> TransformLocalRotation { get { return _transformLocalRotation ?? (_transformLocalRotation = new GetterSetterMemberAccessor<Transform, Quaternion>(t => t.localRotation, (t, v) => t.localRotation = v)); } }
        public static IMemberAccessor<Quaternion> localRotation_ref(this Transform t) { return TransformLocalRotation; }

        #endregion

        //TODO - add more helpers to quickly lookup accessors for common properties/fields on commonly tweened objects

    }

}
