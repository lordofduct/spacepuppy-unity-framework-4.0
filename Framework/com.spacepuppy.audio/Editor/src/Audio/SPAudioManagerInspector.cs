using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

using com.spacepuppy.Audio;

namespace com.spacepuppyeditor.Audio
{

    [CustomEditor(typeof(SPAudioManager), true)]
    public class SPAudioManagerInspector : SPEditor
    {

        public const string PROP_AUDIOMIXER = "_audioMixer";
        public const string PROP_MASTERVOLUMELABEL = "_masterVolumeLabel";

        private bool _useGlobalMasterVolume;

        protected override void OnEnable()
        {
            base.OnEnable();

            _useGlobalMasterVolume = string.IsNullOrEmpty(this.serializedObject.FindProperty(PROP_MASTERVOLUMELABEL).stringValue);
        }

        protected override void OnSPInspectorGUI()
        {
            this.serializedObject.UpdateIfRequiredOrScript();

            this.DrawPropertyField(EditorHelper.PROP_SCRIPT);
            this.DrawPropertyField(EditorHelper.PROP_SERVICEREGISTRATIONOPTS);
            this.DrawPropertyField(PROP_AUDIOMIXER);

            this.DrawMasterVolumeField();

            this.DrawDefaultInspectorExcept(EditorHelper.PROP_SCRIPT, EditorHelper.PROP_SERVICEREGISTRATIONOPTS, PROP_AUDIOMIXER, PROP_MASTERVOLUMELABEL);

            this.serializedObject.ApplyModifiedProperties();
        }

        protected void DrawMasterVolumeField()
        {
            const float WIDTH_TOGGLE = 60f;
            const string LBL_SHORT = "Global";
            const string LBL_LONG = "Use Global Master Volume";

            var masterVolProp = this.serializedObject.FindProperty(PROP_MASTERVOLUMELABEL);

            var position = EditorGUILayout.GetControlRect();
            position = SPEditorGUI.SafePrefixLabel(position, EditorHelper.TempContent(masterVolProp.displayName, masterVolProp.tooltip));
            var r0 = new Rect(position.xMax - WIDTH_TOGGLE, position.yMin, WIDTH_TOGGLE, EditorGUIUtility.singleLineHeight);
            var r1 = new Rect(position.xMin, position.yMin, Mathf.Max(0f, position.width - r0.width - 1f), EditorGUIUtility.singleLineHeight);

            _useGlobalMasterVolume = EditorGUI.ToggleLeft(r0, LBL_SHORT, _useGlobalMasterVolume);
            if (_useGlobalMasterVolume)
            {
                masterVolProp.stringValue = string.Empty;
                EditorGUI.BeginChangeCheck();
                string soutput = EditorGUI.DelayedTextField(r1, LBL_LONG, GUI.skin.textField);
                if (EditorGUI.EndChangeCheck())
                {
                    masterVolProp.stringValue = soutput;
                    _useGlobalMasterVolume = string.IsNullOrEmpty(soutput);
                }
            }
            else
            {
                masterVolProp.stringValue = EditorGUI.TextField(r1, masterVolProp.stringValue);
            }
        }

    }

}
