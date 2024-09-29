using UnityEngine;
using UnityEditor;
using System.Reflection;

using com.spacepuppy;
using com.spacepuppy.Utils;

namespace com.spacepuppyeditor.Core
{

    [CustomPropertyDrawer(typeof(DiscreteFloat))]
    public class DiscreteFloatPropertyDrawer : PropertyDrawer
    {


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            const float WIDTH_INFTOGGLE = 55f;

            var valueProp = property.FindPropertyRelative("_value");
            float value;

            position = SPEditorGUI.SafePrefixLabel(position, label);
            if (this.fieldInfo.GetCustomAttribute<DiscreteFloat.HideInfinityCheckbox>() == null)
            {
                var r_inftoggle = new Rect(position.xMin, position.yMin, WIDTH_INFTOGGLE, EditorGUIUtility.singleLineHeight);
                position = new Rect(r_inftoggle.xMax, r_inftoggle.yMin, Mathf.Max(0f, position.width - r_inftoggle.width), EditorGUIUtility.singleLineHeight);

                bool isinf = float.IsPositiveInfinity(valueProp.floatValue);
                EditorGUI.BeginChangeCheck();
                isinf = EditorGUI.ToggleLeft(r_inftoggle, "Inf", isinf);
                if (EditorGUI.EndChangeCheck())
                {
                    value = isinf ? float.PositiveInfinity : 0f;
                    value = NormalizeBasedOnAttribs(value);
                    valueProp.floatValue = value;
                }
            }

            EditorGUI.BeginChangeCheck();
            value = EditorGUI.FloatField(position, valueProp.floatValue);

            if (EditorGUI.EndChangeCheck())
            {
                //if the value increased ever so much, ceil the value, good for the mouse scroll
                value = NormalizeValue(valueProp.floatValue, value);

                if (this.fieldInfo != null)
                {
                    value = NormalizeBasedOnAttribs(value);

                    //if the value increased ever so much, ceil the value, good for the mouse scroll
                    value = NormalizeValue(valueProp.floatValue, value);
                }

                valueProp.floatValue = value;
            }
        }

        private float NormalizeBasedOnAttribs(float value)
        {
            var attribs = this.fieldInfo.GetCustomAttributes(typeof(DiscreteFloat.ConfigAttribute), false) as DiscreteFloat.ConfigAttribute[];
            foreach (var attrib in attribs)
            {
                value = attrib.Normalize(value);
            }
            return value;
        }


        public static float NormalizeValue(float oldValue, float newValue)
        {
            return (newValue != oldValue && MathUtil.Shear(newValue) == oldValue) ? Mathf.Ceil(newValue) : Mathf.Floor(newValue);
        }

        public static float GetValue(SerializedProperty prop)
        {
            if (prop != null)
            {
                var propValue = prop.FindPropertyRelative("_value");
                return propValue != null && propValue.propertyType == SerializedPropertyType.Float ? propValue.floatValue : float.NaN;
            }

            return float.NaN;
        }

        public static float SetValue(SerializedProperty prop, float value)
        {
            if (prop != null)
            {
                var propValue = prop.FindPropertyRelative("_value");
                if (propValue != null && propValue.propertyType == SerializedPropertyType.Float)
                {
                    propValue.floatValue = Mathf.Round(value);
                }
            }

            return float.NaN;
        }

    }

    [CustomPropertyDrawer(typeof(NullableFloat))]
    public class NullableFloatPropertyDrawer : PropertyDrawer
    {

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            const float WIDTH_INFTOGGLE = 62f;

            string shortlabel, longlabel;
            GetLabels(out shortlabel, out longlabel);

            position = SPEditorGUI.SafePrefixLabel(position, label);
            var r0 = new Rect(position.xMin, position.yMin, WIDTH_INFTOGGLE, EditorGUIUtility.singleLineHeight);
            var r1 = new Rect(r0.xMax, r0.yMin, Mathf.Max(0f, position.width - r0.width), EditorGUIUtility.singleLineHeight);

            var prop_hasvalue = property.FindPropertyRelative("_hasValue");
            var prop_value = property.FindPropertyRelative("_value");

            bool hasvalue = prop_hasvalue.boolValue;
            EditorGUI.BeginChangeCheck();
            hasvalue = !EditorGUI.ToggleLeft(r0, shortlabel, !hasvalue);
            if (EditorGUI.EndChangeCheck())
            {
                prop_hasvalue.boolValue = hasvalue;
                prop_value.floatValue = hasvalue ? NormalizeBasedOnAttribs(0f) : 0f;
            }

            if (hasvalue)
            {
                EditorGUI.BeginChangeCheck();
                float value = EditorGUI.FloatField(r1, prop_value.floatValue);
                if (EditorGUI.EndChangeCheck())
                {
                    value = NormalizeBasedOnAttribs(value);
                    prop_value.floatValue = value;
                }
            }
            else
            {
                prop_value.floatValue = 0f;
                EditorGUI.BeginChangeCheck();
                string soutput = EditorGUI.DelayedTextField(r1, longlabel, GUI.skin.textField);
                if (EditorGUI.EndChangeCheck())
                {
                    if (float.TryParse(soutput, out float output))
                    {
                        prop_hasvalue.boolValue = true;
                        prop_value.floatValue = NormalizeBasedOnAttribs(output);
                    }
                }
            }
        }

        void GetLabels(out string shortlabel, out string longlabel)
        {
            var attrib = this.fieldInfo?.GetCustomAttribute<NullableFloat.LabelAttribute>();
            shortlabel = attrib?.ShortLabel ?? "null";
            longlabel = attrib?.LongLabel ?? "NULL";
        }

        private float NormalizeBasedOnAttribs(float value)
        {
            var attribs = this.fieldInfo?.GetCustomAttributes(typeof(NullableFloat.ConfigAttribute), false) as NullableFloat.ConfigAttribute[];
            if (attribs == null) return value;

            foreach (var attrib in attribs)
            {
                value = attrib.Normalize(value);
            }
            return value;
        }

    }

}
