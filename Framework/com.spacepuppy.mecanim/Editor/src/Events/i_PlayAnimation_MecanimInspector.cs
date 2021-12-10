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

    [CustomEditor(typeof(i_PlayAnimation_Mecanim))]
    public class i_PlayAnimation_MecanimInspector : SPEditor
    {

        public const string PROP_TARGET = "_targetAnimator";
        public const string PROP_OVERRIDES = "_animatorOverrides";
        public const string PROP_CONFIG = "_config";
        public const string PROP_TOKEN = "_token";
        public const string PROP_PURGE = "_purgeTokenOnExit";

        protected override void OnSPInspectorGUI()
        {
            this.serializedObject.Update();

            this.DrawPropertyField(EditorHelper.PROP_SCRIPT);
            this.DrawPropertyField(EditorHelper.PROP_ORDER);
            this.DrawPropertyField(EditorHelper.PROP_ACTIVATEON);

            this.DrawPropertyField(PROP_TARGET);
            this.DrawPropertyField(PROP_OVERRIDES);
            this.DrawPropertyField(PROP_CONFIG);

            var tokenProp = this.serializedObject.FindProperty(PROP_TOKEN);
            var purgeProp = this.serializedObject.FindProperty(PROP_PURGE);
            SPEditorGUILayout.PropertyField(tokenProp);
            if (!string.IsNullOrEmpty(tokenProp.stringValue))
            {
                SPEditorGUILayout.PropertyField(purgeProp);
            }

            this.DrawDefaultInspectorExcept(EditorHelper.PROP_SCRIPT, EditorHelper.PROP_ORDER, EditorHelper.PROP_ACTIVATEON, PROP_TARGET, PROP_OVERRIDES, PROP_CONFIG, PROP_TOKEN, PROP_PURGE);

            this.serializedObject.ApplyModifiedProperties();
        }

    }

}