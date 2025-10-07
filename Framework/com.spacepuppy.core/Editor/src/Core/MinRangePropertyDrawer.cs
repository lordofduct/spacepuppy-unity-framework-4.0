using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Utils;

namespace com.spacepuppyeditor.Core
{

    [CustomPropertyDrawer(typeof(MinRangeAttribute))]
    public class MinRangePropertyDrawer : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attrib = this.attribute as MinRangeAttribute;

            switch(attrib != null ? property.propertyType : SerializedPropertyType.Generic)
            {
                case SerializedPropertyType.Float:
                    EditorGUI.BeginChangeCheck();
                    var fval = Mathf.Max(EditorGUI.FloatField(position, label, property.floatValue), attrib.Min);
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.floatValue = fval;
                    }
                    break;
                case SerializedPropertyType.Integer:
                    EditorGUI.BeginChangeCheck();
                    var ival = (int)Mathf.Max(EditorGUI.IntField(position, label, property.intValue), attrib.Min);
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.intValue = ival;
                    }
                    break;
                default:
                    SPEditorGUI.DefaultPropertyField(position, property, label);
                    break;
            }
        }

    }
}
