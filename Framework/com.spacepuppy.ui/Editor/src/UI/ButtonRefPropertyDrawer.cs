using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.UI;
using com.spacepuppy.Utils;

namespace com.spacepuppyeditor.UI
{

    [CustomPropertyDrawer(typeof(ButtonRef))]
    public class ButtonRefPropertyDrawer : PropertyDrawer
    {

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SPEditorGUI.PropertyField(position, property.FindPropertyRelative("_value"), label);
        }

    }

}
