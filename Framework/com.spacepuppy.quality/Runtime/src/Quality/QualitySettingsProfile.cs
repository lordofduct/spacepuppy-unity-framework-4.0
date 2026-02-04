using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Utils;

namespace com.spacepuppy.Quality
{

    public class QualitySettingsProfile
    {

        #region Fields

        private Dictionary<string, object> _values = new();
        private int _profileIndex = -1;
        private bool _allowStoringInvalidSettingNames;

        #endregion

        #region Properties

        public int ProfileIndex
        {
            get => _profileIndex;
            set => _profileIndex = value;
        }
        public string Name { get; set; }
        public int ValueCount => _values.Count;
        public IEnumerable<string> TrackedSettings => _values.Keys;

        /// <summary>
        /// Allows storing settings that aren't known settings in QualitySettings.
        /// </summary>
        public bool AllowStoringInvalidSettingNames
        {
            get => _allowStoringInvalidSettingNames;
            set => _allowStoringInvalidSettingNames = value;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Stores a value as the setting if it exists as a valid setting in QualitySettings.
        /// </summary>
        /// <param name="settingName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetValue(string settingName, object value)
        {
            if (string.IsNullOrEmpty(settingName)) return false;

            switch (settingName)
            {
                case QualitySettingsHelper.SETTING_SHADOWSETTINGS:
                case nameof(ShadowSettings):
                    if (value is ShadowSettings)
                    {
                        _values[QualitySettingsHelper.SETTING_SHADOWSETTINGS] = value;
                    }
                    return true;
                default:
                    if (QualitySettingsHelper.TryGetSettingAccessor(settingName, out QualitySettingAccessor accessor))
                    {
                        _values[settingName] = ConvertUtil.Coerce(value, accessor.ValueType);
                        return true;
                    }
                    else if (_allowStoringInvalidSettingNames)
                    {
                        _values[settingName] = value;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
            }
        }

        public object GetValue(string settingName)
        {
            if (settingName != null && _values.TryGetValue(settingName, out object value))
            {
                return value;
            }
            return null;
        }

        public T GetValue<T>(string settingName)
        {
            if (settingName != null && _values.TryGetValue(settingName, out object value))
            {
                return ConvertUtil.Coerce<T>(value);
            }
            return default(T);
        }

        public bool TryGetValue(string settingName, out object value)
        {
            if (settingName != null && _values.TryGetValue(settingName, out value))
            {
                return true;
            }
            value = null;
            return false;
        }

        public bool HasValue(string settingName) => settingName != null && _values.ContainsKey(settingName);

        public bool Remove(string settingName) => settingName != null && _values.Remove(settingName);

        /// <summary>
        /// Write a named value to current QualitySettings if it exists.
        /// </summary>
        /// <param name="settingName"></param>
        /// <returns>Returns true if the write was successful.</returns>
        public bool WriteValueToCurrentQualitySettings(string settingName)
        {
            if (settingName != null && _values.TryGetValue(settingName, out object value))
            {
                if (Services.Get(out IQualitySettings settings))
                {
                    return settings.SetSetting(settingName, value);
                }
                else
                {
                    return QualitySettingsHelper.SetSetting(settingName, value);
                }
            }
            return false;
        }

        /// <summary>
        /// Writes all stored values to QualitySettings.
        /// </summary>
        /// <returns>Returns number of values written.</returns>
        public int WriteValuesToCurrentQualitySettings()
        {
            if (Services.Get(out IQualitySettings settings))
            {
                return settings.SetProfile(this);
            }
            else
            {
                if (_profileIndex >= 0 && _profileIndex < QualitySettingsHelper.ProfileCount)
                {
                    QualitySettings.SetQualityLevel(_profileIndex);
                }

                int cnt = 0;
                foreach (var pair in _values)
                {
                    if (QualitySettingsHelper.SetSetting(pair.Key, pair.Value))
                    {
                        cnt++;
                    }
                }
                return cnt;
            }
        }

        /// <summary>
        /// Sets token to the state of the current QualitySettings. 
        /// Note - all shadow settings are stored as a ShadowSettings struct with the name 'shadowSettings'.
        /// </summary>
        public void ReadAllValuesFromCurrentQualitySettings(bool flattenShadowProperties = false)
        {
            _values.Clear();
            _values[QualitySettingsHelper.SETTING_SHADOWSETTINGS] = ShadowSettings.ReadFromCurrentQualitySettings();
            foreach (var accessor in QualitySettingsHelper.GetAllSettingAccessors(flattenShadowProperties))
            {
                _values[accessor.Name] = accessor.Value;
            }
            _profileIndex = QualitySettings.GetQualityLevel();
        }

        #endregion

        #region Static Utils

#if UNITY_2022_2_OR_NEWER
        public static QualitySettingsProfile[] CreateAllProfilesFromQualitySettings() => CreateAllProfilesFromQualitySettings(null);
        public static QualitySettingsProfile[] CreateAllProfilesFromQualitySettings(string[] trackedSettings)
        {
            var names = QualitySettings.names;
            var profiles = new QualitySettingsProfile[names.Length];
            QualitySettings.ForEach(() =>
            {
                var index = QualitySettings.GetQualityLevel();
                if (index < 0 || index >= profiles.Length) return;

                var token = new QualitySettingsProfile()
                {
                    _profileIndex = index,
                    Name = names[index],
                };
                if (trackedSettings == null)
                {
                    token.ReadAllValuesFromCurrentQualitySettings();
                }
                else
                {
                    foreach (var nm in trackedSettings)
                    {
                        if (QualitySettingsHelper.TryGetSetting(nm, out object value))
                        {
                            token.SetValue(nm, value);
                        }
                    }
                }
                profiles[index] = token;
            });
            return profiles;
        }
#endif

        #endregion

    }

}
