using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

using com.spacepuppyeditor.Internal;

using com.spacepuppy.Events;

namespace com.spacepuppyeditor.Events
{

    [CustomEditor(typeof(t_PhaseEvaluator), true)]
    public class t_PhaseEvaluatorInspector : SPEditor
    {

        public const string PROP_CURRENTVALUE = "_currentValue";
        public const string PROP_PHASES = "_phases";
        public const string PROP_DIRECTION = "Direction";
        public const string PROP_VALUE = "Value";
        public const string PROP_TARGET = "Target";

        private ReorderableList _lstDrawer;
        protected ReorderableList ListDrawer
        {
            get
            {
                if (_lstDrawer == null)
                {
                    _lstDrawer = new SPReorderableList(this.serializedObject, this.serializedObject.FindProperty(PROP_PHASES))
                    {
                        draggable = true,
                        elementHeight = EditorGUIUtility.singleLineHeight,
                        drawHeaderCallback = _lstDrawer_DrawHeader,
                        drawElementCallback = _lstDrawer_DrawElement
                    };
                }
                return _lstDrawer;
            }
        }

        private float _lastValue;

        protected override void OnBeforeSPInspectorGUI()
        {

            base.OnBeforeSPInspectorGUI();

            _lastValue = this.serializedObject.FindProperty(PROP_CURRENTVALUE).floatValue;
        }

        protected override void OnSPInspectorGUI()
        {
            this.serializedObject.UpdateIfRequiredOrScript();

            this.DrawDefaultInspectorExcept(PROP_PHASES);

            this.ListDrawer.DoLayoutList();

            this.serializedObject.ApplyModifiedProperties();
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            if(Application.isPlaying)
            {
                float newvalue = this.serializedObject.FindProperty(PROP_CURRENTVALUE).floatValue;
                if(newvalue != _lastValue)
                {
                    t_PhaseEvaluator.EditorHelper.EvaluateTransitions(this.serializedObject.targetObject as t_PhaseEvaluator, _lastValue, newvalue);
                }
            }
        }

        #region List Drawer Methods

        protected virtual void _lstDrawer_DrawHeader(Rect area)
        {
            EditorGUI.LabelField(area, "Phases");
        }

        protected virtual void _lstDrawer_DrawElement(Rect area, int index, bool isActive, bool isFocused)
        {
            const float DIR_WIDTH = 56f;
            
            var prop = _lstDrawer.serializedProperty.GetArrayElementAtIndex(index);
            var r0 = new Rect(area.xMin, area.yMin, DIR_WIDTH, EditorGUIUtility.singleLineHeight);
            var r1 = new Rect(r0.xMax, area.yMin, EditorGUIUtility.labelWidth - DIR_WIDTH, EditorGUIUtility.singleLineHeight);
            var r2 = new Rect(r1.xMax, area.yMin, area.xMax - r1.xMax, EditorGUIUtility.singleLineHeight);

            SPEditorGUI.PropertyField(r0, prop.FindPropertyRelative(PROP_DIRECTION), GUIContent.none);
            SPEditorGUI.PropertyField(r1, prop.FindPropertyRelative(PROP_VALUE), GUIContent.none);
            SPEditorGUI.PropertyField(r2, prop.FindPropertyRelative(PROP_TARGET), GUIContent.none);
        }

        #endregion

    }

    [CustomEditor(typeof(t_PhaseEvaluatorStateMachine), true)]
    public class t_PhaseEvaluatorStateMachineInspector : t_PhaseEvaluatorInspector
    {

        protected override void _lstDrawer_DrawElement(Rect area, int index, bool isActive, bool isFocused)
        {
            const float DIR_WIDTH = 32f;

            var prop = this.ListDrawer.serializedProperty.GetArrayElementAtIndex(index);
            var r0 = new Rect(area.xMin, area.yMin, DIR_WIDTH, EditorGUIUtility.singleLineHeight);
            var r1 = new Rect(r0.xMax, area.yMin, EditorGUIUtility.labelWidth - DIR_WIDTH - 1f, EditorGUIUtility.singleLineHeight);
            var r2 = new Rect(r1.xMax + 2f, area.yMin, area.xMax - r1.xMax - 2f, EditorGUIUtility.singleLineHeight);

            //SPEditorGUI.PropertyField(r0, prop.FindPropertyRelative(PROP_DIRECTION), GUIContent.none);
            var dirprop = prop.FindPropertyRelative(PROP_DIRECTION);
            int idir = Mathf.Clamp((int)dirprop.GetEnumValue<t_PhaseEvaluator.Direction>(), 0, 1);
            idir = EditorGUI.Popup(r0, idir, new GUIContent[] { EditorHelper.TempContent(">"), EditorHelper.TempContent("<") });
            dirprop.SetEnumValue((t_PhaseEvaluator.Direction)idir);

            SPEditorGUI.PropertyField(r1, prop.FindPropertyRelative(PROP_VALUE), GUIContent.none);
            SPEditorGUI.PropertyField(r2, prop.FindPropertyRelative(PROP_TARGET), GUIContent.none);
        }

    }

}
