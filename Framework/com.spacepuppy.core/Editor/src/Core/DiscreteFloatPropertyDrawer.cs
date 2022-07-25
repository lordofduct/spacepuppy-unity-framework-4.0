using UnityEngine;
using UnityEditor;

using com.spacepuppy;
using com.spacepuppy.Utils;
using System.Reflection;

namespace com.spacepuppyeditor.Core
{

    [CustomPropertyDrawer(typeof(DiscreteFloat))]
    public class DiscreteFloatPropertyDrawer : PropertyDrawer
    {


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            const float WIDTH_INFTOGGLE = 50f;

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
}
