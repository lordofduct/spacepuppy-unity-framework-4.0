using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

using com.spacepuppy.Quality;
using System.Linq;

namespace com.spacepuppyeditor.Quality
{

    [CustomPropertyDrawer(typeof(QualitySettingsPropertyNameAttribute))]
    public class QualitySettingsPropertyNamePropertyDrawer : PropertyDrawer
    {

        static readonly QualitySettingAccessor[] _accessors = QualitySettingsHelper.GetAllSettingAccessors(true).Append(QualitySettingsHelper.ShadowSettingsAccessor).OrderBy(o =>
        {
            if (o.Name.StartsWith("shadow") && o.Name != QualitySettingsHelper.SETTING_SHADOWSETTINGS)
            {
                return $"shadowSettings/{o.Name}";
            }
            else
            {
                return o.Name;
            }
        }).ToArray();
        static readonly string[] _options = _accessors.Select(o => o.Name).ToArray();
        static readonly GUIContent[] _optionsGUIC = _accessors.Select(o =>
        {
            if (o.Name.StartsWith("shadow") && o.Name != QualitySettingsHelper.SETTING_SHADOWSETTINGS)
            {
                return new GUIContent($"shadowSettings/{o.Name} [{o.ValueType?.Name}]");
            }
            else
            {
                return new GUIContent($"{o.Name} [{o.ValueType?.Name}]");
            }
        }).ToArray();
        static readonly GUIContent[] _optionsWithCustomGUIC = _optionsGUIC.Append(new GUIContent("Custom...")).ToArray();


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attrib = this.attribute as QualitySettingsPropertyNameAttribute;
            if (attrib?.allowCustom ?? false)
            {
                property.stringValue = SPEditorGUI.OptionPopupWithCustom(position, label, property.stringValue, _options, _optionsWithCustomGUIC);
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                int oi = System.Array.IndexOf(_options, property.stringValue);
                int ni = EditorGUI.Popup(position, label, oi, _optionsGUIC);
                if (EditorGUI.EndChangeCheck())
                {
                    property.stringValue = ni >= 0 && ni < _options.Length ? _options[ni] : string.Empty;
                }
            }
        }

    }

}
