using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using com.spacepuppy.UI;

namespace com.spacepuppyeditor.UI
{

    [CustomPropertyDrawer(typeof(TextFieldTarget))]
    public class TextFieldTargetPropertyDrawer : PropertyDrawer
    {

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var targprop = property.FindPropertyRelative("_target");
            return targprop != null ? SPEditorGUI.GetPropertyHeight(targprop, label) : EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var targprop = property.FindPropertyRelative("_target");
            if (targprop == null)
            {
                EditorGUI.LabelField(position, label);
                return;
            }

            SPEditorGUI.PropertyField(position, targprop, label);
        }

    }

}
