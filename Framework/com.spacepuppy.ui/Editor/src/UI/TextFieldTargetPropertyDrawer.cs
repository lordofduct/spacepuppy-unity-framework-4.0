using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

using com.spacepuppy.UI;

namespace com.spacepuppyeditor.UI
{

    [CustomPropertyDrawer(typeof(TextFieldTarget))]
    [CustomPropertyDrawer(typeof(TextInputFieldTarget))]
    public sealed class TextFieldTargetPropertyDrawer : PropertyDrawer
    {

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUIUtility.singleLineHeight;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var prop_target = property.FindPropertyRelative("_target");
            EditorGUI.PropertyField(position, prop_target, label);
        }

    }

}
