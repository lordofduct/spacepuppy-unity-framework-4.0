using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Utils;

namespace com.spacepuppyeditor.Core
{

    [CustomPropertyDrawer(typeof(MaxRangeAttribute))]
    public class MaxRangePropertyDrawer : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attrib = this.attribute as MaxRangeAttribute;

            switch (attrib != null ? property.propertyType : SerializedPropertyType.Generic)
            {
                case SerializedPropertyType.Float:
                    EditorGUI.BeginProperty(position, label, property);
                    EditorGUI.BeginChangeCheck();
                    var fval = Mathf.Min(EditorGUI.FloatField(position, label, property.floatValue), attrib.Max);
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.floatValue = fval;
                    }
                    EditorGUI.EndProperty();
                    break;
                case SerializedPropertyType.Integer:
                    EditorGUI.BeginProperty(position, label, property);
                    EditorGUI.BeginChangeCheck();
                    var ival = (int)Mathf.Min(EditorGUI.IntField(position, label, property.intValue), attrib.Max);
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.intValue = ival;
                    }
                    EditorGUI.EndProperty();
                    break;
                default:
                    SPEditorGUI.DefaultPropertyField(position, property, label);
                    break;
            }
        }

    }

}
