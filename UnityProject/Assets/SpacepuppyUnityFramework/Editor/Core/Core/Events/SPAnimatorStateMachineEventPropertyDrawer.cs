using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Events;
using com.spacepuppy.Utils;
using com.spacepuppyeditor;

using com.spacepuppyeditor.Events;
using com.spacepuppyeditor.Internal;

namespace com.spacepuppyeditor.Events
{

    [CustomPropertyDrawer(typeof(SPAnimatorStateMachineEvent), true)]
    public class SPAnimatorStateMachineEventPropertyDrawer : SPEventPropertyDrawer
    {

        public const string PROP_ANIMTARGETS = "_animatorTargets";
        private const string PROP_ACTION = "Action";
        private const string PROP_ID = "Id";
        private const string PROP_VALUE = "Value";

        private GUIContent _currentLabel;
        private AnimatorController _controller;
        private AnimatorControllerParameter[] _knownParameters;

        private UnityEditorInternal.ReorderableList _targetList;

        #region Methods



        private void Init(SerializedProperty property, GUIContent label)
        {
            _currentLabel = label;

            var targprop = property.FindPropertyRelative(PROP_ANIMTARGETS);
            if (targprop != null)
            {
                _targetList = CachedReorderableList.GetListDrawer(targprop, _targetList_DrawHeader, _targetList_DrawElement);
                _targetList.elementHeight = EditorGUIUtility.singleLineHeight;
            }

            _controller = null;
            _knownParameters = ArrayUtil.Empty<AnimatorControllerParameter>();
            var behaviour = property.serializedObject.targetObject as StateMachineBehaviour;
            if (behaviour != null)
            {
                _controller = AnimatorController.FindStateMachineBehaviourContext(behaviour)?.FirstOrDefault()?.animatorController;
                _knownParameters = _controller?.parameters ?? ArrayUtil.Empty<AnimatorControllerParameter>();
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            this.Init(property, label);

            float h = base.GetPropertyHeight(property, label);
            h += _targetList?.GetHeight() ?? 0f;
            return h;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            this.Init(property, label);

            base.OnGUI(position, property, EditorHelper.TempContent(" "));
            _controller = null;
        }

        protected override Rect DrawTargets(Rect position, SerializedProperty property)
        {
            var targprop = property.FindPropertyRelative(PROP_ANIMTARGETS);
            if (targprop != null)
            {
                _targetList = CachedReorderableList.GetListDrawer(targprop, _targetList_DrawHeader, _targetList_DrawElement);
                _targetList.elementHeight = EditorGUIUtility.singleLineHeight;

                var listRect = new Rect(position.xMin, position.yMin, position.width, _targetList.GetHeight());

                EditorGUI.BeginChangeCheck();
                _targetList.DoList(listRect);
                if (EditorGUI.EndChangeCheck())
                    property.serializedObject.ApplyModifiedProperties();
                if (_targetList.index >= _targetList.count) _targetList.index = -1;

                position = new Rect(position.xMin, listRect.yMax, position.width, position.height - listRect.height);
                _targetList = null;
            }

            position = this.DrawList(position, property);
            position = this.DrawAdvancedTargetSettings(position, property);
            return position;
        }



        #endregion

        #region ReorderableList Handlers

        private void _targetList_DrawHeader(Rect area)
        {
            EditorGUI.LabelField(area, _currentLabel, EditorHelper.TempContent("Animator Targets"));
        }

        private void _targetList_DrawElement(Rect area, int index, bool isActive, bool isFocused)
        {
            if (index < 0 || index >= _targetList.serializedProperty.arraySize) return;

            const float PAD = 4f;

            var elprop = _targetList.serializedProperty.GetArrayElementAtIndex(index);
            var actprop = elprop.FindPropertyRelative(PROP_ACTION);
            var idprop = elprop.FindPropertyRelative(PROP_ID);
            var valprop = elprop.FindPropertyRelative(PROP_VALUE);

            var r0 = new Rect(area.xMin, area.yMin, area.width * 0.35f, area.height);

            SPEditorGUI.PropertyField(r0, actprop, GUIContent.none);

            switch (actprop.GetEnumValue<SPAnimatorStateMachineEvent.AnimatorTriggerAction>())
            {
                case SPAnimatorStateMachineEvent.AnimatorTriggerAction.SetTrigger:
                case SPAnimatorStateMachineEvent.AnimatorTriggerAction.ResetTrigger:
                    {
                        var r1 = new Rect(r0.xMax + PAD, area.yMin, Mathf.Max(0f, area.width - r0.width - PAD), area.height);
                        IDPropField(r1, idprop, GUIContent.none, AnimatorControllerParameterType.Trigger);
                    }
                    break;
                case SPAnimatorStateMachineEvent.AnimatorTriggerAction.SetBool:
                    {
                        const float TOGGLE_WIDTH = 14f;
                        var r1 = new Rect(r0.xMax + PAD, area.yMin, Mathf.Max(0f, area.width - r0.width - TOGGLE_WIDTH - PAD - PAD), area.height);
                        var r2 = new Rect(r1.xMax + PAD, area.yMin, Mathf.Max(0f, area.xMax - r1.xMax - PAD), area.height);
                        IDPropField(r1, idprop, GUIContent.none, AnimatorControllerParameterType.Bool);
                        valprop.floatValue = EditorGUI.Toggle(r2, valprop.floatValue != 0f) ? 1f : 0f;
                    }
                    break;
                case SPAnimatorStateMachineEvent.AnimatorTriggerAction.SetInt:
                    {
                        var tw = Mathf.Max(0f, area.width - r0.width - PAD - PAD);
                        var r1 = new Rect(r0.xMax + PAD, area.yMin, tw * 0.66f, area.height);
                        var r2 = new Rect(r1.xMax + PAD, area.yMin, tw * 0.33f, area.height);
                        IDPropField(r1, idprop, GUIContent.none, AnimatorControllerParameterType.Int);
                        valprop.floatValue = EditorGUI.IntField(r2, Mathf.RoundToInt(valprop.floatValue));
                    }
                    break;
                case SPAnimatorStateMachineEvent.AnimatorTriggerAction.SetFloat:
                    {
                        var tw = Mathf.Max(0f, area.width - r0.width - PAD - PAD);
                        var r1 = new Rect(r0.xMax + PAD, area.yMin, tw * 0.66f, area.height);
                        var r2 = new Rect(r1.xMax + PAD, area.yMin, tw * 0.33f, area.height);
                        IDPropField(r1, idprop, GUIContent.none, AnimatorControllerParameterType.Float);
                        SPEditorGUI.PropertyField(r2, valprop, GUIContent.none);
                    }
                    break;
            }

        }

        private void IDPropField(Rect rect, SerializedProperty idprop, GUIContent label, AnimatorControllerParameterType paramtype)
        {
            if (_knownParameters?.Length > 0)
            {
                idprop.stringValue = SPEditorGUI.OptionPopupWithCustom(rect, label, idprop.stringValue, _knownParameters.Where(o => o.type == paramtype).Select(o => o.name).ToArray());
            }
            else
            {
                SPEditorGUI.PropertyField(rect, idprop, label);
            }
        }

        #endregion

    }

}
