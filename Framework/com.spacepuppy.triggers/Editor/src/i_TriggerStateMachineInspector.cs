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

            if (Application.isPlaying)
            {
                if (GUILayout.Button("Next State") && this.target is i_TriggerStateMachine machine)
                {
                    machine.GoToNextState(i_TriggerStateMachine.WrapMode.Loop);
                }
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
