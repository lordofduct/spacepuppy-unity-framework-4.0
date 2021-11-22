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

    [CustomEditor(typeof(i_GoToAnimatorState), true)]
    public class i_GoToAnimatorStateInspector : SPEditor
    {

        public const string PROP_TARGET = "_target";
        public const string PROP_STATENAME = "_stateName";
        public const string PROP_LAYER = "_layer";
        public const string PROP_NORMALIZEDTIME = "_normalizedTime";

        protected override void OnSPInspectorGUI()
        {
            this.serializedObject.Update();

            this.DrawPropertyField(EditorHelper.PROP_SCRIPT);
            this.DrawPropertyField(EditorHelper.PROP_ORDER);
            this.DrawPropertyField(EditorHelper.PROP_ACTIVATEON);

            this.DrawPropertyField(PROP_TARGET);
            this.DrawPropertyField(PROP_STATENAME);

            //layer
            var layerProp = this.serializedObject.FindProperty(PROP_LAYER);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(EditorHelper.TempContent("Layer"));
            if(layerProp.intValue >= 0)
            {
                if (!EditorGUILayout.Toggle(true, GUILayout.Width(20f))) layerProp.intValue = -1;
                layerProp.intValue = EditorGUILayout.IntField(layerProp.intValue);
            }
            else
            {
                layerProp.intValue = -1;
                if (EditorGUILayout.Toggle(false, GUILayout.Width(20f))) layerProp.intValue = 0;
                EditorGUILayout.LabelField("Toggle checkbox to target specific layer by index.");
            }
            EditorGUILayout.EndHorizontal();

            //normalized time
            var ntimeProp = this.serializedObject.FindProperty(PROP_NORMALIZEDTIME);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(EditorHelper.TempContent("Normalized Time"));
            if(ntimeProp.floatValue == float.NegativeInfinity)
            {
                ntimeProp.floatValue = EditorGUILayout.Toggle(false, GUILayout.Width(20f)) ? 0f : float.NegativeInfinity;
                EditorGUILayout.LabelField("Toggle checkbox to set normalized time.");
            }
            else
            {
                if (EditorGUILayout.Toggle(true, GUILayout.Width(20f)) == false)
                {
                    EditorGUILayout.Slider(ntimeProp.floatValue, 0f, 1f);
                    ntimeProp.floatValue = float.NegativeInfinity;
                }
                else
                {
                    ntimeProp.floatValue = EditorGUILayout.Slider(ntimeProp.floatValue, 0f, 1f);
                }
            }
            EditorGUILayout.EndHorizontal();

            this.DrawDefaultInspectorExcept(EditorHelper.PROP_SCRIPT, EditorHelper.PROP_ORDER, EditorHelper.PROP_ACTIVATEON, PROP_TARGET, PROP_STATENAME, PROP_LAYER, PROP_NORMALIZEDTIME);

            this.serializedObject.ApplyModifiedProperties();
        }

    }

}