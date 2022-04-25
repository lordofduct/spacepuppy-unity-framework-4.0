using UnityEngine;
using System.Collections.Generic;

namespace com.spacepuppy
{

    public static class ExtensionMethods
    {

        public static AnimationCurve Clone(this AnimationCurve curve)
        {
            return new AnimationCurve(curve.keys)
            {
                postWrapMode = curve.postWrapMode,
                preWrapMode = curve.preWrapMode,
            };
        }

        public static void TweenTo(VariantCollection vcoll, com.spacepuppy.Tween.TweenHash hash, com.spacepuppy.Tween.Ease ease, float dur)
        {
            var e = vcoll.GetEnumerator();
            while (e.MoveNext())
            {
                var value = e.Current.Value;
                if (value == null) continue;

                switch (VariantReference.GetVariantType(value.GetType()))
                {
                    case VariantType.Integer:
                    case VariantType.Float:
                    case VariantType.Double:
                    case VariantType.Vector2:
                    case VariantType.Vector3:
                    case VariantType.Vector4:
                    case VariantType.Quaternion:
                    case VariantType.Color:
                    case VariantType.Rect:
                        hash.Prop(e.Current.Key).To(ease, dur, value);
                        break;
                }
            }
        }

    }

}