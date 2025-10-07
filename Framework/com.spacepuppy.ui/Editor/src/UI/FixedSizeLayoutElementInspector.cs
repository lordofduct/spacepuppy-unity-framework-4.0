using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

using com.spacepuppy.UI;

namespace com.spacepuppyeditor.UI
{

    [CustomEditor(typeof(FixedSizeLayoutElement)), CanEditMultipleObjects]
    public class FixedSizeLayoutElementInspector : SPEditor
    {

        const string PROP_WIDTH = "_width";
        const string PROP_HEIGHT = "_height";
        const string PROP_PRIORITY = "_layoutPriority";

        protected override void OnSPInspectorGUI()
        {
            this.serializedObject.UpdateIfRequiredOrScript();

            this.DrawDefaultInspectorExcept(PROP_WIDTH, PROP_HEIGHT, PROP_PRIORITY);

            this.DrawSizeField(this.serializedObject.FindProperty(PROP_WIDTH), true);
            this.DrawSizeField(this.serializedObject.FindProperty(PROP_HEIGHT), false);

            this.DrawPropertyField(PROP_PRIORITY);

            this.serializedObject.ApplyModifiedProperties();
        }

        void DrawSizeField(SerializedProperty property, bool iswidth)
        {
            var label = EditorHelper.GetLabelContent(property);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);

            EditorGUI.BeginChangeCheck();
            bool check = EditorGUILayout.Toggle(property.floatValue >= 0f, GUILayout.Width(18f));
            if (EditorGUI.EndChangeCheck())
            {
                if (check)
                {
                    property.floatValue = iswidth ? (this.serializedObject.targetObject as FixedSizeLayoutElement).transform.sizeDelta.x : (this.serializedObject.targetObject as FixedSizeLayoutElement).transform.sizeDelta.y;
                }
                else
                {
                    property.floatValue = -1f;
                }
            }

            if (check)
            {
                property.floatValue = Mathf.Max(0f, EditorGUILayout.DelayedFloatField(property.floatValue));
            }

            EditorGUILayout.EndHorizontal();
        }

    }

}
