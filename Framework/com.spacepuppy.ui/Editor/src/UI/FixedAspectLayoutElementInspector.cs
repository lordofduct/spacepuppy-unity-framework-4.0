using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

using com.spacepuppy.UI;

namespace com.spacepuppyeditor.UI
{

    [CustomEditor(typeof(FixedAspectLayoutElement)), CanEditMultipleObjects]
    public class FixedAspectLayoutElementInspector : SPEditor
    {

        const string PROP_MINWIDTH = "_minWidth";
        const string PROP_PREFWIDTH = "_preferredWidth";
        const string PROP_FLEXWIDTH = "_flexibleWidth";
        const string PROP_PRIORITY = "_layoutPriority";

        protected override void OnSPInspectorGUI()
        {
            this.serializedObject.UpdateIfRequiredOrScript();

            this.DrawDefaultInspectorExcept(PROP_MINWIDTH, PROP_PREFWIDTH, PROP_FLEXWIDTH, PROP_PRIORITY);

            this.DrawXWidthField(this.serializedObject.FindProperty(PROP_MINWIDTH));
            this.DrawXWidthField(this.serializedObject.FindProperty(PROP_PREFWIDTH));
            this.DrawXWidthField(this.serializedObject.FindProperty(PROP_FLEXWIDTH));

            this.DrawPropertyField(PROP_PRIORITY);

            this.serializedObject.ApplyModifiedProperties();
        }

        void DrawXWidthField(SerializedProperty property)
        {
            var label = EditorHelper.GetLabelContent(property);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);

            EditorGUI.BeginChangeCheck();
            bool check = EditorGUILayout.Toggle(property.floatValue >= 0f, GUILayout.Width(18f));
            if (EditorGUI.EndChangeCheck())
            {
                property.floatValue = check ? (this.serializedObject.targetObject as FixedAspectLayoutElement).transform.sizeDelta.x : -1f;
            }

            if (check)
            {
                property.floatValue = Mathf.Max(0f, EditorGUILayout.DelayedFloatField(property.floatValue));
            }

            EditorGUILayout.EndHorizontal();
        }

    }

}
