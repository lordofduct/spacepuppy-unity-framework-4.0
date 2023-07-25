using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;
using System.Collections.Generic;

using com.spacepuppy.UI;
using com.spacepuppy.Utils;

namespace com.spacepuppyeditor.UI
{

    [CustomPropertyDrawer(typeof(TextFieldTarget))]
    [CustomPropertyDrawer(typeof(TextInputFieldTarget))]
    public sealed class TextFieldTargetPropertyDrawer : PropertyDrawer, EditorHelper.ISerializedWrapperHelper
    {

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUIUtility.singleLineHeight;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var prop_target = property.FindPropertyRelative("_target");
            EditorGUI.PropertyField(position, prop_target, label);
        }

        #region EditorHelper.ISerializedWrapperHelper Interface

        object EditorHelper.ISerializedWrapperHelper.GetValue(SerializedProperty property)
        {
            return property.FindPropertyRelative("_target").objectReferenceValue;
        }

        System.Type EditorHelper.ISerializedWrapperHelper.GetValueType(SerializedProperty property)
        {
            var child = property.FindPropertyRelative("_target");
            if (child.objectReferenceValue is UIBehaviour) return child.objectReferenceValue.GetType();
            return typeof(UIBehaviour);
        }

        bool EditorHelper.ISerializedWrapperHelper.SetValue(SerializedProperty property, object value)
        {
            if (ObjUtil.GetAsFromSource(value, out UnityEngine.UI.Text txt))
            {
                property.FindPropertyRelative("_target").objectReferenceValue = txt;
            }
            else if (ObjUtil.GetAsFromSource(value, out UnityEngine.UI.InputField ifld))
            {
                property.FindPropertyRelative("_target").objectReferenceValue = ifld;
            }
            else if (ObjUtil.GetAsFromSource(value, out TMPro.TMP_Text tmp_txt))
            {
                property.FindPropertyRelative("_target").objectReferenceValue = tmp_txt;
            }
            else if (ObjUtil.GetAsFromSource(value, out TMPro.TMP_InputField tmp_ifld))
            {
                property.FindPropertyRelative("_target").objectReferenceValue = tmp_ifld;
            }
            else
            {
                property.FindPropertyRelative("_target").objectReferenceValue = null;
            }
            return true;
        }

        #endregion

    }

}
