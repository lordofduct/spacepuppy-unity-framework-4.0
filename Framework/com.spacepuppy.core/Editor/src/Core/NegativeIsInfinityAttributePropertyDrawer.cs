using UnityEngine;
using UnityEditor;

using com.spacepuppy;
using UnityEditor.Graphs;

namespace com.spacepuppyeditor.Core
{

    [CustomPropertyDrawer(typeof(NegativeIsInfinityAttribute))]
    public class NegativeIsInfinityAttributePropertyDrawer : PropertyDrawer
    {

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            const float WIDTH_INFTOGGLE = 50f;

            position = SPEditorGUI.SafePrefixLabel(position, label);
            var r0 = new Rect(position.xMin, position.yMin, WIDTH_INFTOGGLE, EditorGUIUtility.singleLineHeight);
            var r1 = new Rect(r0.xMax, r0.yMin, Mathf.Max(0f, position.width - r0.width), EditorGUIUtility.singleLineHeight);

            switch(property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    {
                        bool isinf = property.intValue < 0;
                        EditorGUI.BeginChangeCheck();
                        isinf = EditorGUI.ToggleLeft(r0, "Inf", isinf);
                        if(EditorGUI.EndChangeCheck())
                        {
                            property.intValue = isinf ? -1 : 0;
                        }
                        
                        if(isinf)
                        {
                            property.intValue = -1;
                            EditorGUI.SelectableLabel(r1, "Infinity", GUI.skin.textField);
                        }
                        else
                        {
                            property.intValue = EditorGUI.IntField(r1, property.intValue);
                        }
                    }
                    break;
                case SerializedPropertyType.Float:
                    {
                        bool isinf = property.floatValue < 0;
                        EditorGUI.BeginChangeCheck();
                        isinf = EditorGUI.ToggleLeft(r0, "Inf", isinf);
                        if (EditorGUI.EndChangeCheck())
                        {
                            property.floatValue = isinf ? -1 : 0;
                        }

                        if (isinf)
                        {
                            property.floatValue = -1;
                            EditorGUI.SelectableLabel(r1, "Infinity", GUI.skin.textField);
                        }
                        else
                        {
                            property.floatValue = EditorGUI.FloatField(r1, property.floatValue);
                        }
                    }
                    break;
                default:
                    EditorGUI.LabelField(position, EditorHelper.TempContent("Used NegativeIsInfinityAttribute on a field of the wrong type."));
                    break;
            }
        }

    }

}
