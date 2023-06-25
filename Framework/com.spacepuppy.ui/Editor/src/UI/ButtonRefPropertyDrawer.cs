using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.UI;
using com.spacepuppy.Utils;

namespace com.spacepuppyeditor.UI
{

    [CustomPropertyDrawer(typeof(ButtonRef))]
    public class ButtonRefPropertyDrawer : PropertyDrawer, EditorHelper.ISerializedWrapperHelper
    {

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SPEditorGUI.PropertyField(position, property.FindPropertyRelative("_value"), label);
        }

        #region EditorHelper.ISerializedWrapperHelper Interface

        object EditorHelper.ISerializedWrapperHelper.GetValue(SerializedProperty property)
        {
            return property.FindPropertyRelative("_value").objectReferenceValue;
        }

        bool EditorHelper.ISerializedWrapperHelper.SetValue(SerializedProperty property, object value)
        {
            if (ObjUtil.GetAsFromSource<SPUIButton>(value, out SPUIButton spui))
            {
                property.FindPropertyRelative("_value").objectReferenceValue = spui;
            }
            else if (ObjUtil.GetAsFromSource<UnityEngine.UI.Button>(value, out UnityEngine.UI.Button ub))
            {
                property.FindPropertyRelative("_value").objectReferenceValue = ub;
            }
            else
            {
                property.FindPropertyRelative("_value").objectReferenceValue = null;
            }
            return true;
        }

        System.Type EditorHelper.ISerializedWrapperHelper.GetValueType(SerializedProperty property)
        {
            var child = property.FindPropertyRelative("_value");
            if (child.objectReferenceValue is SPUIButton) return typeof(SPUIButton);
            else if (child.objectReferenceValue is UnityEngine.UI.Button) return typeof(UnityEngine.UI.Button);
            else return typeof(UnityEngine.UI.Selectable);
        }

        #endregion

    }

}
