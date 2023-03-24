using UnityEngine;
using UnityEditor;

using com.spacepuppy;

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
            const float WIDTH_INFTOGGLE = 55f;

            var shortlabel = (this.attribute as NegativeIsInfinityAttribute)?.ShortInfinityLabel;
            var longlabel = (this.attribute as NegativeIsInfinityAttribute)?.InfinityLabel;
            bool includeZero = (this.attribute as NegativeIsInfinityAttribute)?.ZeroIsAlsoInfinity ?? false;
            const int DEFAULT_INF_VALUE = -1;
            int MAX_INF_VALUE = includeZero ? 0 : -1;
            int MIN_REAL_VALUE = includeZero ? 1 : 0;

            if (string.IsNullOrEmpty(shortlabel)) shortlabel = "Inf";
            if (string.IsNullOrEmpty(longlabel)) longlabel = "Infinity";

            position = SPEditorGUI.SafePrefixLabel(position, label);
            var r0 = new Rect(position.xMin, position.yMin, WIDTH_INFTOGGLE, EditorGUIUtility.singleLineHeight);
            var r1 = new Rect(r0.xMax, r0.yMin, Mathf.Max(0f, position.width - r0.width), EditorGUIUtility.singleLineHeight);

            switch(property.propertyType)
            {
                case SerializedPropertyType.Integer when property.type == "long":
                    {
                        bool isinf = property.longValue <= MAX_INF_VALUE;
                        EditorGUI.BeginChangeCheck();
                        isinf = EditorGUI.ToggleLeft(r0, shortlabel, isinf);
                        if (EditorGUI.EndChangeCheck())
                        {
                            property.longValue = isinf ? DEFAULT_INF_VALUE : MIN_REAL_VALUE;
                        }

                        if (isinf)
                        {
                            property.longValue = DEFAULT_INF_VALUE;
                            EditorGUI.BeginChangeCheck();
                            string soutput = EditorGUI.DelayedTextField(r1, longlabel, GUI.skin.textField);
                            if (EditorGUI.EndChangeCheck())
                            {
                                long output;
                                if(long.TryParse(soutput, out output) && output > MAX_INF_VALUE)
                                {
                                    property.longValue = output;
                                }
                            }
                        }
                        else
                        {
                            property.longValue = EditorGUI.LongField(r1, property.longValue);
                        }
                    }
                    break;
                case SerializedPropertyType.Integer:
                    {
                        bool isinf = property.intValue <= MAX_INF_VALUE;
                        EditorGUI.BeginChangeCheck();
                        isinf = EditorGUI.ToggleLeft(r0, shortlabel, isinf);
                        if(EditorGUI.EndChangeCheck())
                        {
                            property.intValue = isinf ? DEFAULT_INF_VALUE : MIN_REAL_VALUE;
                        }
                        
                        if(isinf)
                        {
                            property.intValue = DEFAULT_INF_VALUE;
                            EditorGUI.BeginChangeCheck();
                            string soutput = EditorGUI.DelayedTextField(r1, longlabel, GUI.skin.textField);
                            if (EditorGUI.EndChangeCheck())
                            {
                                int output;
                                if (int.TryParse(soutput, out output) && output > MAX_INF_VALUE)
                                {
                                    property.intValue = output;
                                }
                            }
                        }
                        else
                        {
                            property.intValue = EditorGUI.IntField(r1, property.intValue);
                        }
                    }
                    break;
                case SerializedPropertyType.Float when property.type == "double":
                    {
                        bool isinf = property.doubleValue <= MAX_INF_VALUE;
                        EditorGUI.BeginChangeCheck();
                        isinf = EditorGUI.ToggleLeft(r0, shortlabel, isinf);
                        if (EditorGUI.EndChangeCheck())
                        {
                            property.doubleValue = isinf ? DEFAULT_INF_VALUE : MIN_REAL_VALUE;
                        }

                        if (isinf)
                        {
                            property.doubleValue = DEFAULT_INF_VALUE;
                            EditorGUI.BeginChangeCheck();
                            string soutput = EditorGUI.DelayedTextField(r1, longlabel, GUI.skin.textField);
                            if (EditorGUI.EndChangeCheck())
                            {
                                double output;
                                if (double.TryParse(soutput, out output) && output > MAX_INF_VALUE)
                                {
                                    property.doubleValue = output;
                                }
                            }
                        }
                        else
                        {
                            property.doubleValue = EditorGUI.DoubleField(r1, property.doubleValue);
                        }
                    }
                    break;
                case SerializedPropertyType.Float:
                    {
                        bool isinf = property.floatValue <= MAX_INF_VALUE;
                        EditorGUI.BeginChangeCheck();
                        isinf = EditorGUI.ToggleLeft(r0, shortlabel, isinf);
                        if (EditorGUI.EndChangeCheck())
                        {
                            property.floatValue = isinf ? DEFAULT_INF_VALUE : MIN_REAL_VALUE;
                        }

                        if (isinf)
                        {
                            property.floatValue = DEFAULT_INF_VALUE;
                            EditorGUI.BeginChangeCheck();
                            string soutput = EditorGUI.DelayedTextField(r1, longlabel, GUI.skin.textField);
                            if (EditorGUI.EndChangeCheck())
                            {
                                float output;
                                if (float.TryParse(soutput, out output) && output > MAX_INF_VALUE)
                                {
                                    property.floatValue = output;
                                }
                            }
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
