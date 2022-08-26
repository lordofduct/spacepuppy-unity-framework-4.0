using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Geom;
using com.spacepuppy.Utils;
using com.spacepuppy.Dynamic;

namespace com.spacepuppyeditor.Core.Geom
{

    [CustomPropertyDrawer(typeof(Interval))]
    public class IntervalInspector : PropertyDrawer
    {
        private const string PROP_MIN = "_min";
        private const string PROP_MAX = "_max";

        #region Fields

        private GUIContent[] _defaultLabels = new GUIContent[] { new GUIContent("Min"), new GUIContent("Max") };
        private GUIContent[] _reverseLabels = new GUIContent[] { new GUIContent("Max"), new GUIContent("Min") };
        private float[] _values = new float[2];

        #endregion

        #region CONSTRUCTOR

        public IntervalInspector() { }

        public IntervalInspector(System.Reflection.FieldInfo field)
        {
            this.SetValue("m_FieldInfo", field);
        }

        #endregion

        public double ValueScale { get; set; } = 1f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var prop_min = property.FindPropertyRelative(PROP_MIN);
            var prop_max = property.FindPropertyRelative(PROP_MAX);

            var attrib = this.fieldInfo?.GetCustomAttributes(typeof(Interval.ConfigAttribute), false).Cast<Interval.ConfigAttribute>().FirstOrDefault();
            bool reverse = (attrib != null) ? attrib.Reverse : false;
            float minValue = (attrib != null) ? attrib.MinValue : float.NegativeInfinity;
            float maxValue = (attrib != null) ? attrib.MaxValue : float.PositiveInfinity;
            bool discreteValuesOnly = (attrib != null) ? attrib.DiscreteValuesOnly : false;


            GUIContent[] subLabels;
            float labelWidth;

            if (reverse)
            {
                subLabels = (attrib != null) ? new GUIContent[] { EditorHelper.TempContent(attrib.MaxLabel), EditorHelper.TempContent(attrib.MinLabel) } : _defaultLabels;
                labelWidth = (attrib != null) ? attrib.LabelWidth : 30f;
                _values[1] = (float)(prop_min.floatValue * this.ValueScale);
                _values[0] = (float)(prop_max.floatValue * this.ValueScale);
            }
            else
            {
                subLabels = (attrib != null) ? new GUIContent[] { EditorHelper.TempContent(attrib.MinLabel), EditorHelper.TempContent(attrib.MaxLabel) } : _defaultLabels;
                labelWidth = (attrib != null) ? attrib.LabelWidth : 30f;
                _values[0] = (float)(prop_min.floatValue * this.ValueScale);
                _values[1] = (float)(prop_max.floatValue * this.ValueScale);
            }

            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            SPEditorGUI.DelayedMultiFloatField(position, label, subLabels, _values, labelWidth);
            if (EditorGUI.EndChangeCheck())
            {
                _values[0] = Mathf.Clamp(_values[0], minValue, maxValue);
                _values[1] = Mathf.Clamp(_values[1], minValue, maxValue);
                if (discreteValuesOnly) _values[0] = Mathf.Round(_values[0]);
                if (discreteValuesOnly) _values[1] = Mathf.Round(_values[1]);

                float min = (float)((reverse ? _values[1] : _values[0]) / this.ValueScale);
                float max = (float)((reverse ? _values[0] : _values[1]) / this.ValueScale);

                if (!MathUtil.FuzzyEqual(min, prop_min.floatValue))
                {
                    //changed min
                    if (max < min) max = min;
                }
                else
                {
                    //changed max
                    if (min > max) min = max;
                }

                prop_min.floatValue = min;
                prop_max.floatValue = max;
            }
            EditorGUI.EndProperty();
        }

    }

}
