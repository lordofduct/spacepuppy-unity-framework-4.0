using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

using com.spacepuppy.Netcode;
using com.spacepuppyeditor;
using UnityEngine.UIElements;

namespace com.ardenteditor.Netcode
{

    [CustomPropertyDrawer(typeof(SerializedNetworkVariableAttribute))]
    public class SerializedNetworkVariablePropertyDrawer : PropertyDrawer
    {

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (this.TryValidateSerializedProperty(property, out _))
            {
                return EditorGUIUtility.singleLineHeight;
            }
            else
            {
                return EditorGUIUtility.singleLineHeight;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty innerprop;
            if (!this.TryValidateSerializedProperty(property, out innerprop))
            {
                this.DrawMalformed(position, label);
                return;
            }

            SPEditorGUI.PropertyField(position, innerprop, label);
        }

        private bool TryValidateSerializedProperty(SerializedProperty property, out SerializedProperty innerprop)
        {
            innerprop = null;
            if (!property.type.StartsWith("NetworkVariable")) return false;

            innerprop = property.FindPropertyRelative("m_InternalValue");
            if (innerprop == null) return false;

            switch (innerprop.propertyType)
            {
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.Boolean:
                case SerializedPropertyType.Float:
                case SerializedPropertyType.String:
                case SerializedPropertyType.Color:
                case SerializedPropertyType.LayerMask:
                case SerializedPropertyType.Enum:
                case SerializedPropertyType.Character:
                    return true;
                default:
                    innerprop = null;
                    return false;
            }
        }

        private void DrawMalformed(Rect position, GUIContent label)
        {
            EditorGUI.LabelField(position, EditorHelper.TempContent("Malformed NetworkVariable - only supports NetworkVariable of simple data types."));
        }

    }

}
