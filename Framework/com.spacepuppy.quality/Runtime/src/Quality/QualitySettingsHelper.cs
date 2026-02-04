using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

using com.spacepuppy.Utils;

namespace com.spacepuppy.Quality
{

    public struct QualitySettingAccessor
    {
        private PropertyInfo _property;
        internal QualitySettingAccessor(PropertyInfo prop)
        {
            _property = prop;
        }

        public string Name => _property?.Name ?? string.Empty;

        public object Value
        {
            get => _property?.GetValue(null);
            set => _property?.SetValue(null, ConvertUtil.Coerce(value, _property.PropertyType));
        }

        public System.Type ValueType => _property?.PropertyType;

    }

    public static class QualitySettingsHelper
    {

        public const string SETTING_SHADOWSETTINGS = nameof(QualitySettingsInterpreterWrapper.shadowSettings);

        private static Dictionary<string, QualitySettingAccessor> _allPropertyAccessors = new(System.StringComparer.CurrentCultureIgnoreCase);
        private static Dictionary<string, QualitySettingAccessor> _preferredPropertyAccessors = new(System.StringComparer.CurrentCultureIgnoreCase);
        static QualitySettingsHelper()
        {
            foreach (var info in typeof(QualitySettings).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
            {
                if (info.DeclaringType != typeof(QualitySettings)) continue;
                if (!info.CanRead || !info.CanWrite) continue;
                if (info.IsObsolete()) continue;

                var accessor = new QualitySettingAccessor(info);
                switch (info.Name)
                {
                    case nameof(QualitySettings.antiAliasing):
                        accessor = new QualitySettingAccessor(typeof(QualitySettingsInterpreterWrapper).GetProperty(nameof(QualitySettingsInterpreterWrapper.antiAliasing), BindingFlags.Public | BindingFlags.Static));
                        break;
                    case nameof(QualitySettings.shadowCascades):
                        accessor = new QualitySettingAccessor(typeof(QualitySettingsInterpreterWrapper).GetProperty(nameof(QualitySettingsInterpreterWrapper.shadowCascades), BindingFlags.Public | BindingFlags.Static));
                        break;
                }

                _allPropertyAccessors[info.Name] = accessor;
                if (!info.Name.StartsWith("shadow"))
                {
                    _preferredPropertyAccessors[info.Name] = new QualitySettingAccessor(info);
                }
            }
            _preferredPropertyAccessors[SETTING_SHADOWSETTINGS] = ShadowSettingsAccessor;
        }


#if UNITY_2022_2_OR_NEWER
        public static int ProfileCount => QualitySettings.count;
#else
        private static int _profileCount = -1;
        public static int ProfileCount
        {
            get
            {
                if (_profileCount < 0) _profileCount = QualitySettings.names.Length;
                return _profileCount;
            }
        }
#endif


        //exists to make above dicts simpler to setup
        static class QualitySettingsInterpreterWrapper
        {
            public static AntiAliasingLevel antiAliasing
            {
                get => (AntiAliasingLevel)QualitySettings.antiAliasing;
                set => QualitySettings.antiAliasing = (int)value;
            }
            public static ShadowCascades shadowCascades
            {
                get => (ShadowCascades)QualitySettings.shadowCascades;
                set => QualitySettings.shadowCascades = (int)value;
            }
            public static ShadowSettings shadowSettings
            {
                get => ShadowSettings.ReadFromCurrentQualitySettings();
                set
                {
                    value.WriteToCurrentQualitySettings();
                }
            }
        }
        public static readonly QualitySettingAccessor ShadowSettingsAccessor = new QualitySettingAccessor(typeof(QualitySettingsInterpreterWrapper).GetProperty(nameof(QualitySettingsInterpreterWrapper.shadowSettings), BindingFlags.Public | BindingFlags.Static));

        public static bool TryGetSettingAccessor(string name, out QualitySettingAccessor accessor)
        {
            if (_allPropertyAccessors.TryGetValue(name, out accessor))
            {
                return true;
            }
            else if (_preferredPropertyAccessors.TryGetValue(name, out accessor))
            {
                return true;
            }
            return false;
        }

        public static IEnumerable<QualitySettingAccessor> GetAllSettingAccessors(bool flattenShadowProperties = false)
        {
            if (flattenShadowProperties)
            {
                return _allPropertyAccessors.Values;
            }
            else
            {
                return _preferredPropertyAccessors.Values;
            }
        }

        public static IEnumerable<string> GetAllSettingNames(bool flattenShadowProperties = false)
        {
            if (flattenShadowProperties)
            {
                return _allPropertyAccessors.Keys;
            }
            else
            {
                return _preferredPropertyAccessors.Keys;
            }
        }

        public static bool SetSetting(string name, object value)
        {
            switch (name)
            {
                case string s when string.Equals(s, SETTING_SHADOWSETTINGS, System.StringComparison.OrdinalIgnoreCase):
                    if (value is ShadowSettings shadowsettings)
                    {
                        shadowsettings.WriteToCurrentQualitySettings();
                        return true;
                    }
                    return false;
                default:
                    if (_allPropertyAccessors.TryGetValue(name, out QualitySettingAccessor accessor))
                    {
                        accessor.Value = value;
                        return true;
                    }
                    else
                    {
                        Debug.LogWarning($"Attempted to set unknown QualitySetting with name '{name}'.");
                    }
                    return false;
            }
        }

        public static bool TryGetSetting(string name, out object value)
        {
            switch (name)
            {
                case string s when string.Equals(s, SETTING_SHADOWSETTINGS, System.StringComparison.OrdinalIgnoreCase):
                    value = ShadowSettings.ReadFromCurrentQualitySettings();
                    return true;
                default:
                    if (_allPropertyAccessors.TryGetValue(name, out QualitySettingAccessor accessor))
                    {
                        value = accessor.Value;
                        return true;
                    }
                    value = null;
                    return false;
            }
        }

    }

}
