using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Mecanim.Events;
using com.spacepuppy.Utils;

using com.spacepuppyeditor.Core.Events;

namespace com.spacepuppyeditor.Mecanim
{

    [CustomEditor(typeof(i_OverrideAnimator), true)]
    public class i_OverrideAnimatorInspector : SPEditor
    {

        public const string PROP_TARGET = "_target";
        public const string PROP_OVERRIDES = "_overrides";
        public const string PROP_TREATUNCONFIG = "_treatUnconfiguredEntriesAsValidEntries";

        protected override void OnSPInspectorGUI()
        {
            this.serializedObject.Update();

            this.DrawPropertyField(EditorHelper.PROP_SCRIPT);
            this.DrawPropertyField(EditorHelper.PROP_ORDER);
            this.DrawPropertyField(EditorHelper.PROP_ACTIVATEON);

            this.DrawPropertyField(PROP_TARGET);

            var overridesProp = this.serializedObject.FindProperty(PROP_OVERRIDES);
            SPEditorGUILayout.ObjectFieldX(overridesProp, (o) => ObjUtil.IsType(o, typeof(AnimatorOverrideController), true) || ObjUtil.IsType(o, typeof(IAnimatorOverrideSource), true), EditorHelper.TempContent(overridesProp.displayName), true);

            var treatProp = this.serializedObject.FindProperty(PROP_TREATUNCONFIG);
            if(ObjUtil.IsType(overridesProp.objectReferenceValue, typeof(AnimatorOverrideController), true))
            {
                SPEditorGUILayout.PropertyField(treatProp);
            }
            else
            {
                treatProp.boolValue = false;
            }

            this.DrawDefaultInspectorExcept(EditorHelper.PROP_SCRIPT, EditorHelper.PROP_ORDER, EditorHelper.PROP_ACTIVATEON, PROP_TARGET, PROP_OVERRIDES, PROP_TREATUNCONFIG);

            this.serializedObject.ApplyModifiedProperties();
        }

    }

}