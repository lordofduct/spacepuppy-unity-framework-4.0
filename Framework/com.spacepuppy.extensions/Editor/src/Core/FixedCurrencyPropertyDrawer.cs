using UnityEngine;
using UnityEditor;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Utils;

namespace com.spacepuppyeditor.Core
{

    [CustomPropertyDrawer(typeof(FixedCurrency))]
    public class FixedCurrencyPropertyDrawer : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attrib = this.fieldInfo.GetCustomAttributes(typeof(FixedCurrency.ConfigAttribute), false).FirstOrDefault() as FixedCurrency.ConfigAttribute;
            decimal min = FixedCurrency.MIN_VALUE;
            decimal max = FixedCurrency.MAX_VALUE;
            bool useSlider = false;
            if (attrib != null)
            {
                useSlider = attrib.displayAsRange;
                if (attrib.min > min && attrib.min < FixedCurrency.MAX_VALUE) min = (decimal)attrib.min;
                if (attrib.max < max && attrib.max > FixedCurrency.MIN_VALUE) max = (decimal)attrib.max;
            }

            position = EditorGUI.PrefixLabel(position, label);

            var valueProp = property.FindPropertyRelative("_value");
            FixedCurrency value = FixedCurrency.FromRawValue(valueProp.longValue);

            EditorGUI.BeginChangeCheck();
            if (useSlider)
            {
                value = EditorGUI.Slider(position, (float)value.Value, (float)min, (float)max);
            }
            else
            {
                string sval = EditorGUI.DelayedTextField(position, value.ToString("0.######"));
                //value = ConvertUtil.ToDecimal(sval);
                FixedCurrency.TryParse(sval, out value);
            }

            if (EditorGUI.EndChangeCheck())
            {
                valueProp.longValue = value.RawValue;
            }

        }

    }

}
