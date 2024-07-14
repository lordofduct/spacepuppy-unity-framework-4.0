using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Utils;

namespace com.spacepuppyeditor.Core
{

    [CustomPropertyDrawer(typeof(VectorInspectorAttribute))]
    public class VectorInspectorPropertyDrawer : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var attr = this.attribute as EulerRotationInspectorAttribute;
            switch (property.propertyType)
            {
                case SerializedPropertyType.Quaternion:
                    EditorGUI.BeginChangeCheck();
                    var qval = ConvertUtil.ToQuaternion(EditorGUI.Vector4Field(position, label, ConvertUtil.ToVector4(property.quaternionValue)));
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.quaternionValue = qval;
                    }
                    break;
                case SerializedPropertyType.Vector2:
                    EditorGUI.BeginChangeCheck();
                    var v2 = EditorGUI.Vector2Field(position, label, property.vector2Value);
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.vector2Value = v2;
                    }
                    break;
                case SerializedPropertyType.Vector3:
                    EditorGUI.BeginChangeCheck();
                    var v3 = EditorGUI.Vector3Field(position, label, property.vector3Value);
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.vector3Value = v3;
                    }
                    break;
                case SerializedPropertyType.Vector4:
                    EditorGUI.BeginChangeCheck();
                    var v4 = EditorGUI.Vector3Field(position, label, property.vector4Value);
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.vector4Value = v4;
                    }
                    break;
                default:
                    SPEditorGUI.DefaultPropertyField(position, property, label);
                    break;
            }

            EditorGUI.EndProperty();
        }




    }

}
