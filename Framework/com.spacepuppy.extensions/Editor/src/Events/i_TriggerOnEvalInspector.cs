using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Events;
using com.spacepuppy.Utils;

using com.spacepuppyeditor.Core;

namespace com.spacepuppyeditor.Events
{

    [CustomEditor(typeof(i_TriggerOnEval), true)]
    public class i_TriggerOnEvalInspector : SPEditor
    {

        private VariantReferencePropertyDrawer _variantDrawer = new VariantReferencePropertyDrawer();

        protected override void OnSPInspectorGUI()
        {
            this.serializedObject.Update();

            this.DrawPropertyField(EditorHelper.PROP_SCRIPT);
            this.DrawPropertyField(EditorHelper.PROP_ORDER);
            this.DrawPropertyField(EditorHelper.PROP_ACTIVATEON);

            this.DrawPropertyField("_input");

            EditorGUILayout.BeginVertical("Box");
            var conditionsArrayProp = this.serializedObject.FindProperty("_conditions");

            for (int i = 0; i < conditionsArrayProp.arraySize; i++)
            {
                EditorGUILayout.LabelField(string.Format(i == 0 ? "IF Condition {0}" : "ELSE IF Condition {0}", i), EditorStyles.boldLabel);
                var conditionBlockProp = conditionsArrayProp.GetArrayElementAtIndex(i);
                var operatorProp = conditionBlockProp.FindPropertyRelative("_operator");
                var valueProp = conditionBlockProp.FindPropertyRelative("_value");

                var r = EditorGUILayout.GetControlRect(false, _variantDrawer.GetPropertyHeight(operatorProp, GUIContent.none));
                var r0 = new Rect(r.xMin, r.yMin, r.width / 2f - 1f, r.height);
                var r1 = new Rect(r0.xMax + 1, r.yMin, r.width / 2f - 1f, r.height);
                SPEditorGUI.PropertyField(r0, operatorProp, GUIContent.none);
                SPEditorGUI.PropertyField(r1, valueProp, GUIContent.none);

                var triggerProp = conditionBlockProp.FindPropertyRelative("_trigger");
                SPEditorGUILayout.PropertyField(triggerProp);
                EditorGUILayout.Space(5f);
            }

            //draw else
            EditorGUILayout.LabelField("ELSE");
            SPEditorGUILayout.PropertyField(this.serializedObject.FindProperty("_elseCondition"));

            var fullRect = EditorGUILayout.GetControlRect();
            var leftRect = new Rect(fullRect.xMin, fullRect.yMin, fullRect.width / 2f, fullRect.height);
            var rightRect = new Rect(fullRect.xMin + leftRect.width, fullRect.yMin, fullRect.width / 2f, fullRect.height);
            if (GUI.Button(leftRect, "Add Condition"))
            {
                conditionsArrayProp.arraySize++;
            }
            if (GUI.Button(rightRect, "Remove Condition"))
            {
                conditionsArrayProp.arraySize--;
            }

            EditorGUILayout.EndVertical();

            this.DrawDefaultInspectorExcept(EditorHelper.PROP_SCRIPT, EditorHelper.PROP_ORDER, EditorHelper.PROP_ACTIVATEON, "_conditions");


            this.serializedObject.ApplyModifiedProperties();
        }

    }

}
