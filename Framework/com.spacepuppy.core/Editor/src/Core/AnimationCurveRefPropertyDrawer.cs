using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Utils;
using com.spacepuppyeditor.Windows;

namespace com.spacepuppyeditor.Core
{

    [CustomPropertyDrawer(typeof(AnimationCurveRef))]
    public class AnimationCurveRefPropertyDrawer : PropertyDrawer
    {

        enum CurveType
        {
            Null = 0,
            Asset = 1,
            Curve = 2,
        }

        internal AnimationCurveConstraintPropertyDrawer InternalCurveDrawer { get; set; }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUIUtility.singleLineHeight;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.serializedObject.isEditingMultipleObjects)
            {
                EditorGUI.LabelField(position, label, EditorHelper.TempContent("Multi-Object editing not supported."));
                return;
            }

            const float DROPDOWN_WIDTH = 75f;

            var drawArea = SPEditorGUI.SafePrefixLabel(position, label);
            var labelArea = new Rect(position.xMin, position.yMin, position.width - drawArea.width - 1f, position.height);
            Rect dropdownArea;
            float halfwidth = labelArea.width / 2f;
            if (halfwidth > DROPDOWN_WIDTH)
            {
                dropdownArea = new Rect(labelArea.xMax - DROPDOWN_WIDTH, labelArea.yMin, DROPDOWN_WIDTH, labelArea.height);
            }
            else
            {
                dropdownArea = new Rect(labelArea.xMax - halfwidth, labelArea.yMin, halfwidth, labelArea.height);
            }

            var curveProp = property.FindPropertyRelative("_curve");
            var obj = GetInnerValue(curveProp);
            var curvetype = GetCurveType(obj);

            EditorGUI.BeginChangeCheck();
            curvetype = (CurveType)EditorGUI.EnumPopup(dropdownArea, curvetype);
            if (EditorGUI.EndChangeCheck())
            {
                switch (curvetype)
                {
                    case CurveType.Null:
                    case CurveType.Asset:
                        curveProp.managedReferenceValue = null;
                        obj = null;
                        break;
                    case CurveType.Curve:
                        obj = AnimationCurve.Constant(0f, 1f, 1f);
                        curveProp.managedReferenceValue = AnimationCurveRef.CreateInnerValuewrapper(obj);
                        break;
                }
            }

            switch (GetCurveType(obj))
            {
                case CurveType.Null:
                case CurveType.Asset:
                    EditorGUI.BeginChangeCheck();
                    obj = SPEditorGUI.AdvancedObjectField(drawArea, GUIContent.none, obj as UnityEngine.Object, typeof(IAnimationCurve), true, false);
                    if (EditorGUI.EndChangeCheck())
                    {
                        curveProp.managedReferenceValue = AnimationCurveRef.CreateInnerValuewrapper(obj);
                    }
                    break;
                case CurveType.Curve:
                    EditorGUI.BeginChangeCheck();
                    if (this.InternalCurveDrawer != null)
                    {
                        obj = this.InternalCurveDrawer.CurveField(drawArea, GUIContent.none, obj as AnimationCurve ?? AnimationCurve.Constant(0f, 1f, 1f));
                    }
                    else
                    {
                        obj = EditorGUI.CurveField(drawArea, obj as AnimationCurve ?? AnimationCurve.Constant(0f, 1f, 1f));
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        curveProp.managedReferenceValue = AnimationCurveRef.CreateInnerValuewrapper(obj);
                    }
                    break;
            }
        }

        static CurveType GetCurveType(object obj)
        {
            switch (obj)
            {
                case AnimationCurve:
                    return CurveType.Curve;
                case IAnimationCurve:
                case UnityEngine.Object:
                    return CurveType.Asset;
                default:
                    return CurveType.Null;
            }
        }

        private static object GetInnerValue(SerializedProperty valueProp)
        {
            var innerval = EditorHelper.GetTargetObjectOfProperty(valueProp);
            if (innerval is AnimationCurveRef.IValue ival)
                return ival.Value;
            else
                return innerval;
        }

    }

}
