using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Events;
using com.spacepuppy.Utils;
using com.spacepuppyeditor.Internal;

namespace com.spacepuppyeditor.Events
{

    [CustomEditor(typeof(i_TriggerStateMachine), true)]
    public class i_TriggerStateMachineInspector : SPEditor
    {

        protected override void OnSPInspectorGUI()
        {
            base.OnSPInspectorGUI();

            if (!Application.isPlaying || this.serializedObject.isEditingMultipleObjects) return;

            if (this.target is i_TriggerStateMachine machine)
            {
                if (GUILayout.Button("Previous State"))
                {
                    machine.GoToPreviousState(i_TriggerStateMachine.WrapMode.Loop);
                }
                if (GUILayout.Button("Next State"))
                {
                    machine.GoToNextState(i_TriggerStateMachine.WrapMode.Loop);
                }
            }
        }

    }

    [CustomEditor(typeof(i_TriggerStateMachineWithHistory))]
    public class i_TriggerStateMachineWithHistoryInspector : SPEditor
    {

        private bool _historyFoldout;

        protected override void OnSPInspectorGUI()
        {
            base.OnSPInspectorGUI();

            if (!Application.isPlaying || this.serializedObject.isEditingMultipleObjects) return;

            if (this.target is i_TriggerStateMachine machine)
            {
                if (GUILayout.Button("Previous State"))
                {
                    machine.GoToPreviousState(i_TriggerStateMachine.WrapMode.Loop);
                }
                if (GUILayout.Button("Next State"))
                {
                    machine.GoToNextState(i_TriggerStateMachine.WrapMode.Loop);
                }
            }
            this.DrawDefaultInspectorFooters();

            if (this.target is i_TriggerStateMachineWithHistory historymachine)
            {
                EditorGUILayout.BeginVertical("box");
                var style = new GUIStyle(EditorStyles.boldLabel);
                style.alignment = TextAnchor.MiddleCenter;

                var r = EditorGUILayout.GetControlRect();
                GUI.Label(r, "History", style);
                _historyFoldout = EditorGUI.Foldout(r, _historyFoldout, GUIContent.none, true);

                if (_historyFoldout)
                {
                    const int MAX_CNT = 5;
                    int cnt = 0;
                    foreach (var i in historymachine.History)
                    {
                        if (i < 0 || i >= historymachine.States.Count) continue;

                        cnt++;
                        if (cnt > MAX_CNT) break;

                        string sid = historymachine.States[i].Id;
                        EditorGUILayout.LabelField($"{sid} [{i}]");
                    }
                }

                EditorGUILayout.EndVertical();
            }

        }

    }

    [CustomPropertyDrawer(typeof(i_TriggerStateMachine.StateCollection))]
    public class i_TriggerStateMachine_StateCollectionPropertyDrawer : PropertyDrawer
    {

        private const string PROP_STATES = "_states";

        private CachedReorderableList _drawer;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float h;
            if (EditorHelper.AssertMultiObjectEditingNotSupportedHeight(property, label, out h)) return h;

            _drawer = com.spacepuppyeditor.Internal.CachedReorderableList.GetListDrawer(property.FindPropertyRelative(PROP_STATES), _lst_DrawHeader, _lst_DrawElement);
            _drawer.elementHeight = EditorGUIUtility.singleLineHeight;
            return _drawer.GetHeight();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (EditorHelper.AssertMultiObjectEditingNotSupported(position, property, label)) return;

            _drawer = com.spacepuppyeditor.Internal.CachedReorderableList.GetListDrawer(property.FindPropertyRelative(PROP_STATES), _lst_DrawHeader, _lst_DrawElement);
            _drawer.elementHeight = EditorGUIUtility.singleLineHeight;
            _drawer.DoList(position);

        }

        private void _lst_DrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, EditorHelper.TempContent("States", "Left field behaves as an Id that can be used to reference the state."));
        }

        private void _lst_DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (_drawer == null || _drawer.serializedProperty.arraySize == 0) return;

            var property = _drawer.serializedProperty.GetArrayElementAtIndex(index);
            EditorHelper.SuppressIndentLevel();

            try
            {
                const float MARGIN_HALF = 1f;
                var r0 = new Rect(rect.xMin, rect.yMin, Mathf.Min(25f, EditorGUIUtility.labelWidth), EditorGUIUtility.singleLineHeight);
                var r1 = new Rect(r0.xMax, rect.yMin, Mathf.Max(0f, EditorGUIUtility.labelWidth - r0.width) - MARGIN_HALF, EditorGUIUtility.singleLineHeight);
                var r2 = new Rect(rect.xMin + EditorGUIUtility.labelWidth + MARGIN_HALF, rect.yMin, rect.width - EditorGUIUtility.labelWidth - MARGIN_HALF, EditorGUIUtility.singleLineHeight);

                EditorGUI.LabelField(r0, string.Format("{0:00}:", index));
                EditorGUI.PropertyField(r1, property.FindPropertyRelative("_id"), GUIContent.none);
                EditorGUI.PropertyField(r2, property.FindPropertyRelative("_target"), GUIContent.none);
            }
            finally
            {
                EditorHelper.ResumeIndentLevel();
            }
        }

    }

}
