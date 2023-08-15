using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Utils;

namespace com.spacepuppyeditor.Core
{

    [CustomPropertyDrawer(typeof(AnimationCurveConstraintAttribute))]
    [CustomPropertyDrawer(typeof(AnimationCurveEaseScaleAttribute))]
    public class AnimationCurveConstraintPropertyDrawer : PropertyDrawer
    {

        private AnimationCurveRefPropertyDrawer _innerCurveRefDrawer;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUIUtility.singleLineHeight;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.AnimationCurve:
                    {
                        property.animationCurveValue = CurveField(position, label, property.animationCurveValue);
                    }
                    break;
                case SerializedPropertyType.ManagedReference:
                    {
                        if (property.GetManagedReferenceType() == typeof(AnimationCurveRef))
                        {
                            (_innerCurveRefDrawer ??= new AnimationCurveRefPropertyDrawer() { InternalCurveDrawer = this }).OnGUI(position, property, label);
                        }
                        else
                        {
                            SPEditorGUI.DefaultPropertyField(position, property, label);
                        }
                    }
                    break;
                default:
                    SPEditorGUI.DefaultPropertyField(position, property, label);
                    break;
            }
        }

        public AnimationCurve CurveField(Rect position, GUIContent label, AnimationCurve curve)
        {
            if (this.attribute is AnimationCurveConstraintAttribute)
            {
                var attrib = this.attribute as AnimationCurveConstraintAttribute;
                var ranges = new Rect(attrib.x, attrib.y, attrib.width, attrib.height);
                return EditorGUI.CurveField(position, label, curve, attrib.color, ranges);
            }
            else if (this.attribute is AnimationCurveEaseScaleAttribute)
            {
                var attrib = this.attribute as AnimationCurveEaseScaleAttribute;
                var ranges = new Rect(0f, -Mathf.Max(0f, attrib.overscan), 1f, Mathf.Max(1f, 1f + attrib.overscan * 2f));
                return EditorGUI.CurveField(position, label, curve, attrib.color, ranges);
            }
            else
            {
                return EditorGUI.CurveField(position, label, curve);
            }
        }

    }

}
