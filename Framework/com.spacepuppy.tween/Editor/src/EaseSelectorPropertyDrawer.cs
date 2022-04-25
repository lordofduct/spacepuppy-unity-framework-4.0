using UnityEngine;
using UnityEditor;
using System.Linq;

using com.spacepuppy.Tween;
using com.spacepuppy.Utils;
using Codice.CM.SEIDInfo;
using System;

namespace com.spacepuppyeditor.Tween
{

    [CustomPropertyDrawer(typeof(EaseSelector))]
    public class EaseSelectorPropertyDrawer : PropertyDrawer
    {

        private struct EaseTuple
        {
            public EaseStyle? Value;
            public System.Type SelectorType;
            public GUIContent Label;

            public EaseSelector CreateInstance()
            {
                switch (SelectorType)
                {
                    case System.Type _ when SelectorType == typeof(EaseStyleSelector):
                        return new EaseStyleSelector()
                        {
                            Style = Value ?? EaseStyle.Linear
                        };
                    case System.Type _ when SelectorType == typeof(EaseAnimationCurveSelector):
                        return new EaseAnimationCurveSelector()
                        {
                            Curve = AnimationCurve.Linear(0f, 0f, 1f, 1f)
                        };
                    default:
                        return new EaseStyleSelector()
                        {
                            Style = Value ?? EaseStyle.Linear
                        };
                }
            }

        }
        //private static readonly int[] _easeStyles;
        private static readonly EaseTuple[] _easeStyles;
        private static readonly GUIContent[] _easeStyleLabels;
        static EaseSelectorPropertyDrawer()
        {
            _easeStyles = EaseStylePropertyDrawer.GetCascadingPopupEntries()
                .Select(o => new EaseTuple()
                {
                    Value = o.Value,
                    SelectorType = typeof(EaseStyleSelector),
                    Label = o.Label,
                })
                .Append(new EaseTuple()
                {
                    Value = null,
                    SelectorType = typeof(EaseAnimationCurveSelector),
                    Label = new GUIContent("Anim Curve"),
                })
                .ToArray();


            _easeStyleLabels = _easeStyles.Select(o => o.Label).ToArray();
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position = EditorGUI.PrefixLabel(position, label);

            var tp = property.GetManagedReferenceType();
            EaseStyle? style = tp == typeof(EaseStyleSelector) ? (EaseStyle?)property.FindPropertyRelative(EaseStyleSelector.PROP_STYLE).GetEnumValue<EaseStyle>() : null;
            int index = _easeStyles.IndexOf(o => o.Value == style && o.SelectorType == tp);
            var tuple = index >= 0 ? _easeStyles[index] : default(EaseTuple);

            if(tuple.SelectorType != tp || tuple.SelectorType == null)
            {
                property.managedReferenceValue = tuple.CreateInstance();
                GUI.changed = true;
                return;
            }

            switch (tuple.SelectorType)
            {
                case System.Type _ when tuple.SelectorType == typeof(EaseStyleSelector):
                    {
                        EditorGUI.BeginChangeCheck();
                        index = EditorGUI.Popup(position, index, _easeStyleLabels);
                        if (EditorGUI.EndChangeCheck() && _easeStyles.InBounds(index))
                        {
                            if (_easeStyles[index].SelectorType == typeof(EaseStyleSelector))
                            {
                                property.FindPropertyRelative(EaseStyleSelector.PROP_STYLE).SetEnumValue(_easeStyles[index].Value ?? EaseStyle.Linear);
                            }
                            else
                            {
                                property.managedReferenceValue = _easeStyles[index].CreateInstance();
                            }
                        }
                    }
                    break;
                case System.Type _ when tuple.SelectorType == typeof(EaseAnimationCurveSelector):
                    {
                        var r0 = new Rect(position.xMin, position.yMin, Mathf.Min(position.width * 0.5f, 100f), position.height);
                        var r1 = new Rect(r0.xMax, position.yMin, Mathf.Max(position.width - r0.width, 0f), position.height);

                        EditorGUI.BeginChangeCheck();
                        index = EditorGUI.Popup(r0, index, _easeStyleLabels);
                        if (EditorGUI.EndChangeCheck() && _easeStyles.InBounds(index) && _easeStyles[index].SelectorType != typeof(EaseAnimationCurveSelector))
                        {
                            property.managedReferenceValue = _easeStyles[index].CreateInstance();
                            return;
                        }

                        EditorGUI.PropertyField(r1, property.FindPropertyRelative(EaseAnimationCurveSelector.PROP_CURVE), GUIContent.none);
                    }
                    break;
            }
        }
    }

}
