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

    [CustomPropertyDrawer(typeof(PlayStateConfiguration), true)]
    public class i_PlayAnimationMecanimInspector : PropertyDrawer
    {

        public const string PROP_STATENAME = "_stateName";
        public const string PROP_LAYER = "_layer";
        public const string PROP_CROSSFADEDUR = "_crossFadeDur";
        public const string PROP_STARTOFFSET = "_startOffset";
        public const string PROP_USEFIXEDTIME = "_useFixedTime";
        public const string PROP_FINALSTATE = "_finalState";
        public const string PROP_FINALSTATETIMEOUT = "_finalStateTimeout";

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float h;
            if (EditorHelper.AssertMultiObjectEditingNotSupportedHeight(property, label, out h)) return h;

            if (property.isExpanded)
            {
                h = EditorGUIUtility.singleLineHeight * 6f;
                if (!string.IsNullOrEmpty(property.FindPropertyRelative(PROP_FINALSTATE)?.stringValue)) h += EditorGUIUtility.singleLineHeight;
                return h;
            }
            else
            {
                return EditorGUIUtility.singleLineHeight;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (EditorHelper.AssertMultiObjectEditingNotSupported(position, property, label)) return;

            var rline = new Rect(position.xMin, position.yMin, position.width, EditorGUIUtility.singleLineHeight);
            Rect r, r0, r1;

            property.isExpanded = EditorGUI.Foldout(rline, property.isExpanded, GUIContent.none);
            SPEditorGUI.PropertyField(rline, property.FindPropertyRelative(PROP_STATENAME), EditorHelper.TempContent("State To Play"));

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                try
                {
                    //layer
                    rline = new Rect(position.xMin, rline.yMax, position.width, EditorGUIUtility.singleLineHeight);
                    var layerProp = property.FindPropertyRelative(PROP_LAYER);
                    r = EditorGUI.PrefixLabel(rline, EditorHelper.TempContent("Layer"));
                    r0 = new Rect(r.xMin, r.yMin, 20f, r.height);
                    r1 = new Rect(r.xMin + 20f, r.yMin, r.width - 20f, r.height);
                    EditorHelper.SuppressIndentLevel();
                    if (layerProp.intValue >= 0)
                    {
                        if (!EditorGUI.Toggle(r0, true)) layerProp.intValue = -1;
                        layerProp.intValue = EditorGUI.IntField(r1, layerProp.intValue);
                    }
                    else
                    {
                        layerProp.intValue = -1;
                        if (EditorGUI.Toggle(r0, false)) layerProp.intValue = 0;
                        EditorGUI.LabelField(r1, "Toggle checkbox to target specific layer by index.");
                    }
                    EditorHelper.ResumeIndentLevel();

                    bool useFixedTime = property.FindPropertyRelative(PROP_USEFIXEDTIME).boolValue;

                    //crossfade dur
                    rline = new Rect(position.xMin, rline.yMax, position.width, EditorGUIUtility.singleLineHeight);
                    var crossfadeProp = property.FindPropertyRelative(PROP_CROSSFADEDUR);
                    r = EditorGUI.PrefixLabel(rline, EditorHelper.TempContent("Cross Fade Duration"));
                    r0 = new Rect(r.xMin, r.yMin, 20f, r.height);
                    r1 = new Rect(r.xMin + 20f, r.yMin, r.width - 20f, r.height);
                    EditorHelper.SuppressIndentLevel();
                    if (crossfadeProp.floatValue < 0f)
                    {
                        crossfadeProp.floatValue = EditorGUI.Toggle(r0, false) ? 0f : float.NegativeInfinity;
                        EditorGUI.LabelField(r1, "Toggle checkbox to set cross fade dur.");
                    }
                    else
                    {
                        if (EditorGUI.Toggle(r0, true) == false)
                        {
                            if (useFixedTime)
                            {
                                EditorGUI.FloatField(r1, crossfadeProp.floatValue);
                            }
                            else
                            {
                                EditorGUI.Slider(r1, crossfadeProp.floatValue, 0f, 1f);
                            }
                            crossfadeProp.floatValue = float.NegativeInfinity;
                        }
                        else
                        {
                            if (useFixedTime)
                            {
                                crossfadeProp.floatValue = Mathf.Abs(EditorGUI.FloatField(r1, crossfadeProp.floatValue));
                            }
                            else
                            {
                                crossfadeProp.floatValue = EditorGUI.Slider(r1, crossfadeProp.floatValue, 0f, 1f);
                            }
                        }
                    }
                    EditorHelper.ResumeIndentLevel();

                    //offset time
                    rline = new Rect(position.xMin, rline.yMax, position.width, EditorGUIUtility.singleLineHeight);
                    var offsetProp = property.FindPropertyRelative(PROP_STARTOFFSET);
                    r = EditorGUI.PrefixLabel(rline, EditorHelper.TempContent("Start Offset"));
                    r0 = new Rect(r.xMin, r.yMin, 20f, r.height);
                    r1 = new Rect(r.xMin + 20f, r.yMin, r.width - 20f, r.height);
                    EditorHelper.SuppressIndentLevel();
                    if (offsetProp.floatValue == float.NegativeInfinity)
                    {
                        offsetProp.floatValue = EditorGUI.Toggle(r0, false) ? 0f : float.NegativeInfinity;
                        EditorGUI.LabelField(r1, "Toggle checkbox to set start offset time.");
                    }
                    else
                    {
                        if (EditorGUI.Toggle(r0, true) == false)
                        {
                            if (useFixedTime)
                            {
                                EditorGUI.FloatField(r1, offsetProp.floatValue);
                            }
                            else
                            {
                                EditorGUI.Slider(r1, offsetProp.floatValue, 0f, 1f);
                            }
                            offsetProp.floatValue = float.NegativeInfinity;
                        }
                        else
                        {
                            if (useFixedTime)
                            {
                                offsetProp.floatValue = Mathf.Abs(EditorGUI.FloatField(r1, offsetProp.floatValue));
                            }
                            else
                            {
                                offsetProp.floatValue = EditorGUI.Slider(r1, offsetProp.floatValue, 0f, 1f);
                            }
                        }
                    }
                    EditorHelper.ResumeIndentLevel();

                    //UseFixedTime
                    rline = new Rect(position.xMin, rline.yMax, position.width, EditorGUIUtility.singleLineHeight);
                    SPEditorGUI.PropertyField(rline, property.FindPropertyRelative(PROP_USEFIXEDTIME));

                    //FinalState
                    rline = new Rect(position.xMin, rline.yMax, position.width, EditorGUIUtility.singleLineHeight);
                    var finalStateProp = property.FindPropertyRelative(PROP_FINALSTATE);
                    SPEditorGUI.PropertyField(rline, finalStateProp);
                    if(!string.IsNullOrEmpty(finalStateProp.stringValue))
                    {
                        rline = new Rect(position.xMin, rline.yMax, position.width, EditorGUIUtility.singleLineHeight);
                        SPEditorGUI.PropertyField(rline, property.FindPropertyRelative(PROP_FINALSTATETIMEOUT));
                    }
                }
                finally
                {
                    EditorGUI.indentLevel--;
                }
            }
        }

    }

}
