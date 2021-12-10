using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy.Mecanim;
using com.spacepuppy.Utils;

namespace com.spacepuppyeditor.Anim
{

    [CustomPropertyDrawer(typeof(AnimatorParameterNameAttribute))]
    public class AnimatorParameterNamePropertyDrawer : PropertyDrawer
    {

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label, EditorHelper.TempContent("AnimatorParameterName applied to field that is not a string."));
                return;
            }

            var attrib = this.attribute as AnimatorParameterNameAttribute;
            var behaviour = property.serializedObject.targetObject as StateMachineBehaviour;
            if (attrib == null || behaviour == null)
            {
                EditorGUI.LabelField(position, label, EditorHelper.TempContent("AnimatorParameterName not configured correctly."));
                return;
            }

            var knownParams = GetKnownParameters(behaviour, attrib).Select(o => o.name).ToArray();
            property.stringValue = SPEditorGUI.OptionPopupWithCustom(position, label, property.stringValue, knownParams);
        }

        private IEnumerable<AnimatorControllerParameter> GetKnownParameters(StateMachineBehaviour behaviour, AnimatorParameterNameAttribute attrib)
        {
            if (behaviour == null) yield break;

            var controller = AnimatorController.FindStateMachineBehaviourContext(behaviour)?.FirstOrDefault()?.animatorController;
            if (controller == null) yield break;

            var arr = controller.parameters;
            if (arr == null || arr.Length == 0) yield break;

            foreach(var p in arr)
            {
                if(p.type.FitsMask(attrib.ParameterType))
                {
                    yield return p;
                }
            }
        }

    }

}
