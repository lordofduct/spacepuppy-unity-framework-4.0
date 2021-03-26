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


        private static IMemberAccessor<Vector3> _transformLocalScale;
        public static IMemberAccessor<Vector3> TransformLocalScale { get { return _transformLocalScale ?? (_transformLocalScale = new GetterSetterMemberAccessor<Transform, Vector3>(t => t.localScale, (t, v) => t.localScale = v)); } }
        public static IMemberAccessor<Vector3> localScale_ref(this Transform t) { return TransformLocalScale; }


        private static IMemberAccessor<Vector3> _transformEulerAngles;
        public static IMemberAccessor<Vector3> TransformEulerAngles { get { return _transformEulerAngles ?? (_transformEulerAngles = new GetterSetterMemberAccessor<Transform, Vector3>(t => t.eulerAngles, (t, v) => t.eulerAngles = v)); } }
        public static IMemberAccessor<Vector3> eulerAngles_ref(this Transform t) { return TransformEulerAngles; }


        private static IMemberAccessor<Vector3> _transformLocalEulerAngles;
        public static IMemberAccessor<Vector3> TransformLocalEulerAngles { get { return _transformLocalEulerAngles ?? (_transformLocalEulerAngles = new GetterSetterMemberAccessor<Transform, Vector3>(t => t.localEulerAngles, (t, v) => t.localEulerAngles = v)); } }
        public static IMemberAccessor<Vector3> localEulerAngles_ref(this Transform t) { return TransformLocalEulerAngles; }

        #endregion

        #region RectTransform

        private static IMemberAccessor<Vector2> _rectTransformSizeDelta;
        public static IMemberAccessor<Vector2> RectTransformSizeDelta { get { return _rectTransformSizeDelta ?? (_rectTransformSizeDelta = new GetterSetterMemberAccessor<RectTransform, Vector2>(t => t.sizeDelta, (t, v) => t.sizeDelta = v)); } }
        public static IMemberAccessor<Vector2> sizeDelta_ref(this RectTransform t) { return RectTransformSizeDelta; }

        #endregion

        //TODO - add more helpers to quickly lookup accessors for common properties/fields on commonly tweened objects

    }

}
