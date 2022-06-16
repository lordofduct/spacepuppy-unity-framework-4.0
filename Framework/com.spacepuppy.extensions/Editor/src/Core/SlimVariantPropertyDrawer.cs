using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

using com.spacepuppyeditor;
using com.spacepuppy;
using System.Reflection;
using com.spacepuppy.Utils;

namespace com.spacepuppyeditor.Core
{

    [CustomPropertyDrawer(typeof(SlimVariant))]
    public class SlimVariantPropertyDrawer : PropertyDrawer
    {

        public const string PROP_VALUE = "_value";

        private VariantType _lastVariantType;

        public VariantType? RestrictToType { get; set; }
        private VariantType? RestrictToType_Resolved
        {
            get
            {
                var attrib = this.fieldInfo?.GetCustomAttribute<SlimVariant.Config>();
                return attrib != null ? attrib.RestrictToType : this.RestrictToType;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position = SPEditorGUI.SafePrefixLabel(position, label);

            var restrictTo = this.RestrictToType_Resolved;
            if (restrictTo != null)
            {
                this.DrawValueField(position, property, restrictTo.Value);
            }
            else
            {
                var r0 = new Rect(position.xMin, position.yMin, Mathf.Min(95f, position.width * 0.5f), position.height);
                var r1 = new Rect(r0.xMax + 2f, r0.yMin, position.width - r0.width - 1f, r0.height);

                var innerval = GetInnerValue(property.FindPropertyRelative(PROP_VALUE));
                var vtype = innerval != null ? VariantReference.GetVariantType(innerval) : _lastVariantType;
                EditorGUI.BeginChangeCheck();
                vtype = (VariantType)SPEditorGUI.EnumPopup(r0, vtype);
                _lastVariantType = vtype;
                if (EditorGUI.EndChangeCheck())
                {
                    switch(vtype)
                    {
                        case VariantType.String:
                            property.FindPropertyRelative(PROP_VALUE).managedReferenceValue = SlimVariant.CreateInnerValuewrapper(string.Empty);
                            break;
                        default:
                            property.FindPropertyRelative(PROP_VALUE).managedReferenceValue = SlimVariant.CreateInnerValuewrapper(TypeUtil.GetDefaultValue(VariantReference.GetTypeFromVariantType(vtype)));
                            break;
                    }
                }

                this.DrawValueField(r1, property, vtype);
            }
        }

        protected void DrawValueField(Rect position, SerializedProperty property, VariantType vtype)
        {
            var valueProp = property.FindPropertyRelative(PROP_VALUE);

            object innerval;
            SlimVariant.IValue ivalue;

            switch (vtype)
            {
                case VariantType.Null:
                    valueProp.managedReferenceValue = null;
                    break;
                case VariantType.Object:
                    innerval = GetInnerValue(valueProp) as UnityEngine.Object;
                    EditorGUI.BeginChangeCheck();
                    innerval = EditorGUI.ObjectField(position, innerval as UnityEngine.Object, typeof(UnityEngine.Object), true);
                    if (EditorGUI.EndChangeCheck())
                    {
                        valueProp.managedReferenceValue = SlimVariant.CreateInnerValuewrapper(innerval);
                    }
                    break;
                case VariantType.GameObject:
                    innerval = GetInnerValue(valueProp) as UnityEngine.Object;
                    EditorGUI.BeginChangeCheck();
                    innerval = EditorGUI.ObjectField(position, innerval as UnityEngine.Object, typeof(UnityEngine.GameObject), true);
                    if (EditorGUI.EndChangeCheck())
                    {
                        valueProp.managedReferenceValue = SlimVariant.CreateInnerValuewrapper(innerval);
                    }
                    break;
                case VariantType.Component:
                    innerval = GetInnerValue(valueProp) as UnityEngine.Object;
                    EditorGUI.BeginChangeCheck();
                    innerval = EditorGUI.ObjectField(position, innerval as UnityEngine.Object, typeof(UnityEngine.Component), true);
                    if (EditorGUI.EndChangeCheck())
                    {
                        valueProp.managedReferenceValue = SlimVariant.CreateInnerValuewrapper(innerval);
                    }
                    break;
                case VariantType.String:
                case VariantType.Boolean:
                case VariantType.Integer:
                case VariantType.Float:
                case VariantType.Double:
                    var vtp = VariantReference.GetTypeFromVariantType(vtype);
                    innerval = ConvertUtil.ToPrim(GetInnerValue(valueProp), vtp);
                    EditorGUI.BeginChangeCheck();
                    innerval = SPEditorGUI.DefaultPropertyField(position, GUIContent.none, innerval, vtp);
                    if (EditorGUI.EndChangeCheck())
                    {
                        valueProp.managedReferenceValue = SlimVariant.CreateInnerValuewrapper(innerval);
                    }
                    break;
                case VariantType.DateTime:
                case VariantType.Vector2:
                case VariantType.Vector3:
                case VariantType.Vector4:
                case VariantType.Quaternion:
                case VariantType.Color:
                case VariantType.LayerMask:
                case VariantType.Rect:
                case VariantType.Numeric:
                case VariantType.Ref:
                    {
                        var objval = EditorHelper.GetTargetObjectOfProperty(valueProp);
                        EditorGUI.BeginChangeCheck();
                        objval = SPEditorGUI.DefaultPropertyField(position, GUIContent.none, objval, VariantReference.GetTypeFromVariantType(vtype));
                        if (EditorGUI.EndChangeCheck())
                        {
                            valueProp.managedReferenceValue = objval;
                        }
                    }
                    break;
            }
        }

        private static object GetInnerValue(SerializedProperty valueProp)
        {
            var innerval = EditorHelper.GetTargetObjectOfProperty(valueProp);
            if (innerval is SlimVariant.IValue ival)
                return ival.Value;
            else
                return innerval;
        }

    }

}
