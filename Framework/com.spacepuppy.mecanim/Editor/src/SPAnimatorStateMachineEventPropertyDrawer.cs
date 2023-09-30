using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Mecanim;
using com.spacepuppy.Utils;
using com.spacepuppyeditor;

using com.spacepuppyeditor.Internal;

namespace com.spacepuppyeditor.Mecanim
{

    [CustomPropertyDrawer(typeof(SPAnimatorStateMachineEvent), true)]
    public class SPAnimatorStateMachineEventPropertyDrawer : PropertyDrawer
    {

        public const string PROP_ANIMTARGETS = "_animatorTargets";
        private const string PROP_ACTION = "_action";
        private const string PROP_ID = "_id";
        private const string PROP_VALUE = "_value";
        private const string PROP_OBJREF = "_objectRef";


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
            float h;
            if (EditorHelper.AssertMultiObjectEditingNotSupportedHeight(property, label, out h)) return h;

            this.Init(property, label);

            if (property.isExpanded)
            {
                return _targetList?.GetHeight() ?? 0f;
            }
            else
            {
                return EditorGUIUtility.singleLineHeight;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (EditorHelper.AssertMultiObjectEditingNotSupported(position, property, label)) return;

            this.Init(property, label);

            property.isExpanded = EditorGUI.Foldout(new Rect(position.xMin, position.yMin, position.width, EditorGUIUtility.singleLineHeight), property.isExpanded, GUIContent.none, true);
            if (property.isExpanded)
            {
                _targetList.DoList(position);
            }
            else
            {
                EditorGUI.BeginProperty(position, label, property);
                ReorderableListHelper.DrawRetractedHeader(position, label);
                EditorGUI.EndProperty();
            }
            _controller = null;
        }

        private Rect DrawTargets(Rect position, SerializedProperty property)
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

            return position;
        }



        #endregion

        #region ReorderableList Handlers

        private void _targetList_DrawHeader(Rect area)
        {
            EditorGUI.LabelField(area, _currentLabel);
        }

        private void _targetList_DrawElement(Rect area, int index, bool isActive, bool isFocused)
        {
            if (index < 0 || index >= _targetList.serializedProperty.arraySize) return;

            const float PAD = 4f;

            var elprop = _targetList.serializedProperty.GetArrayElementAtIndex(index);
            var actprop = elprop.FindPropertyRelative(PROP_ACTION);
            var idprop = elprop.FindPropertyRelative(PROP_ID);
            var valprop = elprop.FindPropertyRelative(PROP_VALUE);
            var overprop = elprop.FindPropertyRelative(PROP_OBJREF);

            var r0 = new Rect(area.xMin, area.yMin, area.width * 0.35f, area.height);

            SPEditorGUI.PropertyField(r0, actprop, GUIContent.none);

            switch (actprop.GetEnumValue<SPAnimatorStateMachineEvent.AnimatorTriggerAction>())
            {
                case SPAnimatorStateMachineEvent.AnimatorTriggerAction.SetTrigger:
                case SPAnimatorStateMachineEvent.AnimatorTriggerAction.ResetTrigger:
                    {
                        valprop.floatValue = 0f;
                        overprop.objectReferenceValue = null;

                        var r1 = new Rect(r0.xMax + PAD, area.yMin, Mathf.Max(0f, area.width - r0.width - PAD), area.height);
                        IDPropField(r1, idprop, GUIContent.none, AnimatorControllerParameterType.Trigger);
                    }
                    break;
                case SPAnimatorStateMachineEvent.AnimatorTriggerAction.SetBool:
                    {
                        overprop.objectReferenceValue = null;

                        const float TOGGLE_WIDTH = 14f;
                        var r1 = new Rect(r0.xMax + PAD, area.yMin, Mathf.Max(0f, area.width - r0.width - TOGGLE_WIDTH - PAD - PAD), area.height);
                        var r2 = new Rect(r1.xMax + PAD, area.yMin, Mathf.Max(0f, area.xMax - r1.xMax - PAD), area.height);
                        IDPropField(r1, idprop, GUIContent.none, AnimatorControllerParameterType.Bool);
                        valprop.floatValue = EditorGUI.Toggle(r2, valprop.floatValue != 0f) ? 1f : 0f;
                    }
                    break;
                case SPAnimatorStateMachineEvent.AnimatorTriggerAction.SetInt:
                    {
                        overprop.objectReferenceValue = null;

                        var tw = Mathf.Max(0f, area.width - r0.width - PAD - PAD);
                        var r1 = new Rect(r0.xMax + PAD, area.yMin, tw * 0.66f, area.height);
                        var r2 = new Rect(r1.xMax + PAD, area.yMin, tw * 0.33f, area.height);
                        IDPropField(r1, idprop, GUIContent.none, AnimatorControllerParameterType.Int);
                        valprop.floatValue = EditorGUI.IntField(r2, Mathf.RoundToInt(valprop.floatValue));
                    }
                    break;
                case SPAnimatorStateMachineEvent.AnimatorTriggerAction.SetFloat:
                    {
                        overprop.objectReferenceValue = null;

                        var tw = Mathf.Max(0f, area.width - r0.width - PAD - PAD);
                        var r1 = new Rect(r0.xMax + PAD, area.yMin, tw * 0.66f, area.height);
                        var r2 = new Rect(r1.xMax + PAD, area.yMin, tw * 0.33f, area.height);
                        IDPropField(r1, idprop, GUIContent.none, AnimatorControllerParameterType.Float);
                        SPEditorGUI.PropertyField(r2, valprop, GUIContent.none);
                    }
                    break;
                case SPAnimatorStateMachineEvent.AnimatorTriggerAction.OverrideAnimatorController:
                    {
                        valprop.floatValue = 0f;

                        var tw = Mathf.Max(0f, area.width - r0.width - PAD - PAD);
                        var r1 = new Rect(r0.xMax + PAD, area.yMin, Mathf.Min(28f, tw * 0.45f), area.height);
                        var r2 = new Rect(r1.xMax + PAD, area.yMin, tw * 0.5f - r1.width, area.height);
                        var r3 = new Rect(r2.xMax + PAD, area.yMin, tw * 0.5f, area.height);

                        EditorGUI.LabelField(r1, "Key:");
                        idprop.stringValue = EditorGUI.TextField(r2, idprop.stringValue);
                        overprop.objectReferenceValue = SPEditorGUI.ObjectFieldX(r3, 
                                                                                 overprop.objectReferenceValue, 
                                                                                 (o) => ObjUtil.IsType(o, typeof(AnimatorOverrideController), true) || ObjUtil.IsType(o, typeof(IAnimatorOverrideSource), true), 
                                                                                 false);
                    }
                    break;
                case SPAnimatorStateMachineEvent.AnimatorTriggerAction.PurgeAnimatorOverride:
                    {
                        valprop.floatValue = 0f;
                        overprop.objectReferenceValue = null;

                        var tw = Mathf.Max(0f, area.width - r0.width - PAD - PAD);
                        var r1 = new Rect(r0.xMax + PAD, area.yMin, Mathf.Min(28f, tw * 0.95f), area.height);
                        var r2 = new Rect(r1.xMax + PAD, area.yMin, Mathf.Max(0f, tw - r1.width), area.height);
                        EditorGUI.LabelField(r1, "Key:");
                        idprop.stringValue = EditorGUI.TextField(r2, idprop.stringValue);
                    }
                    break;
                case SPAnimatorStateMachineEvent.AnimatorTriggerAction.TriggerAllOnTarget:
                    {
                        idprop.stringValue = string.Empty;
                        valprop.floatValue = 0f;

                        var r1 = new Rect(r0.xMax + PAD, area.yMin, Mathf.Max(0f, area.width - r0.width - PAD), area.height);
                        overprop.objectReferenceValue = EditorGUI.ObjectField(r1, overprop.objectReferenceValue, typeof(UnityEngine.Object), false);
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
