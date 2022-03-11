using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

using com.spacepuppy.Events;
using com.spacepuppy.Utils;

namespace com.spacepuppyeditor.Events
{

    [CustomEditor(typeof(t_CurveInterval))]
    public class t_CurveIntervalInspector : SPEditor
    {

        public const string PROP_DURATION = "_duration";
        public const string PROP_RATEOVERTIME = "_rateOverTime";
        public const string PROP_TIMEPROVIDER = "_timerProvider";
        public const string PROP_WRAPMODE = "_wrapMode";
        public const string PROP_REPEATCOUNT = "_repeatCount";

        protected override void OnSPInspectorGUI()
        {
            this.serializedObject.UpdateIfRequiredOrScript();

            this.DrawPropertyField(EditorHelper.PROP_SCRIPT);
            this.DrawPropertyField(PROP_DURATION);
            this.DrawPropertyField(PROP_RATEOVERTIME);
            this.DrawPropertyField(PROP_TIMEPROVIDER);

            var wrapModeProp = this.serializedObject.FindProperty(PROP_WRAPMODE);
            SPEditorGUILayout.PropertyField(wrapModeProp);
            if(wrapModeProp.GetEnumValue<MathUtil.WrapMode>() > MathUtil.WrapMode.Clamp)
            {
                this.DrawPropertyField(PROP_REPEATCOUNT);
            }

            this.DrawDefaultInspectorExcept(EditorHelper.PROP_SCRIPT, PROP_DURATION, PROP_RATEOVERTIME, PROP_TIMEPROVIDER, PROP_WRAPMODE, PROP_REPEATCOUNT);

            this.serializedObject.ApplyModifiedProperties();
        }

    }
}
