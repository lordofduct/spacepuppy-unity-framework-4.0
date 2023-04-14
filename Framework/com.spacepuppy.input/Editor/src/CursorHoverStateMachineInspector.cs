using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

using com.spacepuppyeditor.Internal;

using com.spacepuppy;
using com.spacepuppy.Utils;
using com.spacepuppy.SPInput;
using UnityEditorInternal;

namespace com.spacepuppyeditor.SPInput
{

    [CustomEditor(typeof(CursorHoverStateMachine))]
    public class CursorHoverStateMachineInspector : SPEditor
    {

        private ReorderableList _lstDrawer;

        protected override void OnEnable()
        {
            base.OnEnable();

            _lstDrawer = new SPReorderableList(this.serializedObject, this.serializedObject.FindProperty(CursorHoverStateMachine.PROP_ACTIVESTATES))
            {
                draggable = true,
                elementHeight = EditorGUIUtility.singleLineHeight,
                drawHeaderCallback = _lstDrawer_DrawHeader,
                drawElementCallback = _lstDrawer_DrawElement
            };
        }

        protected override void OnSPInspectorGUI()
        {
            this.serializedObject.UpdateIfRequiredOrScript();

            this.DrawPropertyField(EditorHelper.PROP_SCRIPT);
            _lstDrawer.DoLayoutList();

            this.DrawDefaultInspectorExcept(EditorHelper.PROP_SCRIPT, CursorHoverStateMachine.PROP_ACTIVESTATES);

            this.serializedObject.ApplyModifiedProperties();
        }

        #region List Drawer Methods

        private void _lstDrawer_DrawHeader(Rect area)
        {
            EditorGUI.LabelField(area, "Hover Active States");
        }

        private void _lstDrawer_DrawElement(Rect area, int index, bool isActive, bool isFocused)
        {
            var prop = _lstDrawer.serializedProperty.GetArrayElementAtIndex(index);
            var tokenprop = prop.FindPropertyRelative("Token");
            var stateprop = prop.FindPropertyRelative("State");

            const float LBL_TOKEN_WIDTH = 45f;
            const float LBL_STATE_WIDTH = 38f;
            var r0lbl = new Rect(area.xMin, area.yMin, LBL_TOKEN_WIDTH, area.height);
            var r0 = new Rect(r0lbl.xMax, area.yMin, (area.width * 0.45f) - LBL_TOKEN_WIDTH, area.height);
            var r1lbl = new Rect(r0.xMax + 2f, area.yMin, LBL_STATE_WIDTH, area.height);
            var r1 = new Rect(r1lbl.xMax, area.yMin, area.xMax - r1lbl.xMax, area.height);

            EditorGUI.LabelField(r0lbl, EditorHelper.TempContent("Token:"));
            SPEditorGUI.PropertyField(r0, tokenprop, GUIContent.none);
            EditorGUI.LabelField(r1lbl, EditorHelper.TempContent("State:"));
            SPEditorGUI.PropertyField(r1, stateprop, GUIContent.none);
        }

        #endregion

    }
}
