using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

using com.spacepuppy.Mecanim;
using com.spacepuppy.Utils;

namespace com.spacepuppyeditor.Mecanim
{

    [CustomPropertyDrawer(typeof(AnimatorOverrideSourceRef), true)]
    public class AnimatorOverrideSourceRefPropertyDrawer : PropertyDrawer
    {

        private const string PROP_OBJ = "_obj";
        private const string PROP_TREATNULLS = "_treatUnconfiguredEntriesAsValidEntries";

        #region Methods

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float h;
            if (EditorHelper.AssertMultiObjectEditingNotSupportedHeight(property, label, out h)) return h;

            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (EditorHelper.AssertMultiObjectEditingNotSupported(position, property, label)) return;

            var objProp = property.FindPropertyRelative(PROP_OBJ);
            var treatNullsProp = property.FindPropertyRelative(PROP_TREATNULLS);
            if (objProp.objectReferenceValue is AnimatorOverrideController)
            {
                position = EditorGUI.PrefixLabel(position, label);
                EditorHelper.SuppressIndentLevel();

                try
                {
                    var nulllbl = EditorHelper.TempContent("Respect Nulls");
                    const float TREATNULLS_PAD = 5f;
                    const float TREATNULLS_WIDTH = 100f;
                    var r0 = new Rect(position.xMin, position.yMin, Mathf.Max(position.width * 0.66f, position.width - TREATNULLS_WIDTH - TREATNULLS_PAD), position.height);
                    var r1 = new Rect(r0.xMax + TREATNULLS_PAD, r0.yMin, Mathf.Max(0, position.width - r0.width - TREATNULLS_PAD), r0.height);

                    objProp.objectReferenceValue = SPEditorGUI.ObjectFieldX(r0, objProp.objectReferenceValue, ObjFilter, true);
                    treatNullsProp.boolValue = EditorGUI.ToggleLeft(r1, nulllbl, treatNullsProp.boolValue);
                }
                finally
                {
                    EditorHelper.ResumeIndentLevel();
                }
            }
            else
            {
                treatNullsProp.boolValue = false;
                objProp.objectReferenceValue = SPEditorGUI.ObjectFieldX(position, label, objProp.objectReferenceValue, ObjFilter, true);
            }
        }

        private static bool ObjFilter(UnityEngine.Object o)
        {
            var b = ObjUtil.IsType(o, typeof(AnimatorOverrideController), true) || ObjUtil.IsType(o, typeof(IAnimatorOverrideSource), true);
            return b;
        }

        #endregion

    }

}