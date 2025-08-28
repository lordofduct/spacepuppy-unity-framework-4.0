using UnityEngine;
using UnityEngine.UI;

using com.spacepuppy;
using com.spacepuppy.Dynamic.Accessors;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Tween
{

    public static class FindAccessor
    {

        public static void RegisterFastAccessorsWithTweenCurveFactory(TweenCurveFactory.TweenMemberAccessorFactory factory)
        {
            factory.RegisterPerminentlyCachedAccessor(typeof(Transform).GetProperty(nameof(Transform.position)), FindAccessor.TransformPosition);
            factory.RegisterPerminentlyCachedAccessor(typeof(Transform).GetProperty(nameof(Transform.localPosition)), FindAccessor.TransformLocalPosition);
            factory.RegisterPerminentlyCachedAccessor(typeof(Transform).GetProperty(nameof(Transform.localScale)), FindAccessor.TransformLocalScale);
            factory.RegisterPerminentlyCachedAccessor(typeof(Transform).GetProperty(nameof(Transform.eulerAngles)), FindAccessor.TransformEulerAngles);
            factory.RegisterPerminentlyCachedAccessor(typeof(Transform).GetProperty(nameof(Transform.localEulerAngles)), FindAccessor.TransformLocalEulerAngles);
            factory.RegisterPerminentlyCachedAccessor(typeof(Transform).GetProperty(nameof(Transform.rotation)), FindAccessor.TransformRotation);
            factory.RegisterPerminentlyCachedAccessor(typeof(Transform).GetProperty(nameof(Transform.localRotation)), FindAccessor.TransformLocalRotation);
            factory.RegisterPerminentlyCachedAccessor(typeof(RectTransform).GetProperty(nameof(RectTransform.sizeDelta)), FindAccessor.RectTransformSizeDelta);
            factory.RegisterPerminentlyCachedAccessor(typeof(UnityEngine.UI.Text).GetProperty(nameof(UnityEngine.UI.Text.text)), FindAccessor.TextGraphicText);
            factory.RegisterPerminentlyCachedAccessor(typeof(CanvasGroup).GetProperty(nameof(CanvasGroup.alpha)), FindAccessor.CanvasGroupAlpha);
            factory.RegisterPerminentlyCachedAccessor(typeof(Image).GetProperty(nameof(Image.color)), FindAccessor.ImageColor);

            //TODO - add support for nested properties
            //factory.RegisterPerminentlyCachedAccessor(typeof(Transform).GetProperty("position.x"), FindAccessor.TransformPositionX);
            //factory.RegisterPerminentlyCachedAccessor(typeof(Transform).GetProperty("position.y"), FindAccessor.TransformPositionY);
            //factory.RegisterPerminentlyCachedAccessor(typeof(Transform).GetProperty("position.z"), FindAccessor.TransformPositionZ);
            //factory.RegisterPerminentlyCachedAccessor(typeof(Transform).GetProperty("localPosition.x"), FindAccessor.TransformLocalPositionX);
            //factory.RegisterPerminentlyCachedAccessor(typeof(Transform).GetProperty("localPosition.y"), FindAccessor.TransformLocalPositionY);
            //factory.RegisterPerminentlyCachedAccessor(typeof(Transform).GetProperty("localPosition.z"), FindAccessor.TransformLocalPositionZ);
        }

        #region Transform

        private static IMemberAccessor<Vector3> _transformPosition;
        public static IMemberAccessor<Vector3> TransformPosition { get { return _transformPosition ?? (_transformPosition = new GetterSetterMemberAccessor<Transform, Vector3>(t => t.position, (t, v) => t.position = v)); } }
        public static IMemberAccessor<Vector3> position_ref(this Transform t) { return TransformPosition; }
        private static IMemberAccessor<float> _transformPositionX;
        public static IMemberAccessor<float> TransformPositionX { get { return _transformPositionX ?? (_transformPositionX = new GetterSetterMemberAccessor<Transform, float>(t => t.position.x, (t, v) => t.position = t.position.SetX(v))); } }
        public static IMemberAccessor<float> positionX_ref(this Transform t) { return TransformPositionX; }
        private static IMemberAccessor<float> _transformPositionY;
        public static IMemberAccessor<float> TransformPositionY { get { return _transformPositionY ?? (_transformPositionY = new GetterSetterMemberAccessor<Transform, float>(t => t.position.y, (t, v) => t.position = t.position.SetY(v))); } }
        public static IMemberAccessor<float> positionY_ref(this Transform t) { return TransformPositionY; }
        private static IMemberAccessor<float> _transformPositionZ;
        public static IMemberAccessor<float> TransformPositionZ { get { return _transformPositionZ ?? (_transformPositionZ = new GetterSetterMemberAccessor<Transform, float>(t => t.position.z, (t, v) => t.position = t.position.SetZ(v))); } }
        public static IMemberAccessor<float> positionZ_ref(this Transform t) { return TransformPositionZ; }


        private static IMemberAccessor<Vector3> _transformLocalPosition;
        public static IMemberAccessor<Vector3> TransformLocalPosition { get { return _transformLocalPosition ?? (_transformLocalPosition = new GetterSetterMemberAccessor<Transform, Vector3>(t => t.localPosition, (t, v) => t.localPosition = v)); } }
        public static IMemberAccessor<Vector3> localPosition_ref(this Transform t) { return TransformLocalPosition; }
        private static IMemberAccessor<float> _transformLocalPositionX;
        public static IMemberAccessor<float> TransformLocalPositionX { get { return _transformLocalPositionX ?? (_transformLocalPositionX = new GetterSetterMemberAccessor<Transform, float>(t => t.localPosition.x, (t, v) => t.localPosition = t.localPosition.SetX(v))); } }
        public static IMemberAccessor<float> localPositionX_ref(this Transform t) { return TransformLocalPositionX; }
        private static IMemberAccessor<float> _transformLocalPositionY;
        public static IMemberAccessor<float> TransformLocalPositionY { get { return _transformLocalPositionY ?? (_transformLocalPositionY = new GetterSetterMemberAccessor<Transform, float>(t => t.localPosition.y, (t, v) => t.localPosition = t.localPosition.SetY(v))); } }
        public static IMemberAccessor<float> localPositionY_ref(this Transform t) { return TransformLocalPositionY; }
        private static IMemberAccessor<float> _transformLocalPositionZ;
        public static IMemberAccessor<float> TransformLocalPositionZ { get { return _transformLocalPositionZ ?? (_transformLocalPositionZ = new GetterSetterMemberAccessor<Transform, float>(t => t.localPosition.z, (t, v) => t.localPosition = t.localPosition.SetZ(v))); } }
        public static IMemberAccessor<float> localPositionZ_ref(this Transform t) { return TransformLocalPositionZ; }


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

        private static IMemberAccessor<Vector2> _rectTransformOffsetMax;
        public static IMemberAccessor<Vector2> RectTransformOffsetMax { get { return _rectTransformOffsetMax ?? (_rectTransformOffsetMax = new GetterSetterMemberAccessor<RectTransform, Vector2>(t => t.offsetMax, (t, v) => t.offsetMax = v)); } }
        public static IMemberAccessor<Vector2> offsetMax_ref(this RectTransform t) { return RectTransformOffsetMax; }

        private static IMemberAccessor<Vector2> _rectTransformOffsetMin;
        public static IMemberAccessor<Vector2> RectTransformOffsetMin { get { return _rectTransformOffsetMin ?? (_rectTransformOffsetMin = new GetterSetterMemberAccessor<RectTransform, Vector2>(t => t.offsetMin, (t, v) => t.offsetMin = v)); } }
        public static IMemberAccessor<Vector2> offsetMin_ref(this RectTransform t) { return RectTransformOffsetMin; }

        #endregion

        #region UnityEngine.UI.Text

        private static IMemberAccessor<string> _textGraphicText;
        public static IMemberAccessor<string> TextGraphicText { get { return _textGraphicText ?? (_textGraphicText = new GetterSetterMemberAccessor<UnityEngine.UI.Text, string>(t => t.text, (t, v) => t.text = v)); } }
        public static IMemberAccessor<string> text_ref(this UnityEngine.UI.Text t) { return TextGraphicText; }

        #endregion

        #region CanvasGroup

        private static IMemberAccessor<float> _canvasGroupAlpha;
        public static IMemberAccessor<float> CanvasGroupAlpha { get { return _canvasGroupAlpha ?? (_canvasGroupAlpha = new GetterSetterMemberAccessor<CanvasGroup, float>(c => c.alpha, (c, v) => c.alpha = v)); } }
        public static IMemberAccessor<float> alpha_ref(this CanvasGroup c) { return CanvasGroupAlpha; }

        #endregion

        #region Image

        private static IMemberAccessor<Color> _imageColor;
        public static IMemberAccessor<Color> ImageColor { get { return _imageColor ?? (_imageColor = new GetterSetterMemberAccessor<Image, Color>(img => img.color, (img, v) => img.color = v)); } }
        public static IMemberAccessor<Color> color_ref(this Image img) { return ImageColor; }

        #endregion

        //TODO - add more helpers to quickly lookup accessors for common properties/fields on commonly tweened objects

    }

}
