using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy.Utils;
using com.spacepuppy.Collections;

namespace com.spacepuppy.Quality
{

#if UNITY_2022_2_OR_NEWER
    public class SPQualitySettings : ServiceComponent<IQualitySettings>, IQualitySettings
    {

        public event System.EventHandler<QualitySettingsChangedTempEventArgs> QualityChanged;
        public EventHandlerRef<QualitySettingsChangedTempEventArgs> QualityChanged_ref() => EventHandlerRef<QualitySettingsChangedTempEventArgs>.Create(this, (o, h) => o.QualityChanged += h, (o, h) => o.QualityChanged -= h);

        #region Fields

        [SerializeField, ReorderableArray, QualitySettingsPropertyName]
        private string[] _trackedSettings;

        [System.NonSerialized]
        private int _initialQualityLevel;
        [System.NonSerialized]
        private int _currentQualityLevel;
        [System.NonSerialized]
        private int _lastQualityLevel;

        [System.NonSerialized]
        private QualitySettingsProfile[] _profiles = ArrayUtil.Empty<QualitySettingsProfile>();

        [System.NonSerialized]
        private bool _ignoreChangeEvent;

        #endregion

        #region CONSTRUCTOR

        protected override void OnValidAwake()
        {
            base.OnValidAwake();

            _initialQualityLevel = QualitySettings.GetQualityLevel();
            _currentQualityLevel = _initialQualityLevel;
            _lastQualityLevel = _currentQualityLevel;
            QualitySettings.activeQualityLevelChanged -= QualitySettings_activeQualityLevelChanged;
            QualitySettings.activeQualityLevelChanged += QualitySettings_activeQualityLevelChanged;

            this.ConfigureTrackedSettings(_trackedSettings);
        }

        protected override void OnDestroy()
        {
            QualitySettings.activeQualityLevelChanged -= QualitySettings_activeQualityLevelChanged;
            base.OnDestroy();
        }

        #endregion

        #region Properties

        public int InitialQualityLevel => _initialQualityLevel;
        public int LastQualityLevel => _lastQualityLevel;
        public int CurrentQualityLevel => _currentQualityLevel;

        public IEnumerable<string> TrackedSettings => _trackedSettings;

        public int ProfileCount => _profiles.Length;
        public IEnumerable<QualitySettingsProfile> Profiles => _profiles;



        public AnisotropicFiltering AnisotropicFiltering
        {
            get => QualitySettings.anisotropicFiltering;
            set
            {
                QualitySettings.anisotropicFiltering = value;
                this.OnQualitySettingChanged(nameof(QualitySettings.anisotropicFiltering));
            }
        }

        public AntiAliasingLevel AntiAliasing
        {
            get => (AntiAliasingLevel)QualitySettings.antiAliasing;
            set
            {
                QualitySettings.antiAliasing = (int)value;
                this.OnQualitySettingChanged(nameof(QualitySettings.antiAliasing));
            }
        }

        public int PixelLightCount
        {
            get => QualitySettings.pixelLightCount;
            set
            {
                QualitySettings.pixelLightCount = value;
                this.OnQualitySettingChanged(nameof(QualitySettings.pixelLightCount));
            }
        }

        #endregion

        #region Methods

        public virtual void ConfigureTrackedSettings(IEnumerable<string> settingNames)
        {
            using (var lst = TempCollection.GetList<string>())
            {
                foreach (var s in settingNames)
                {
                    if (QualitySettingsHelper.TryGetSettingAccessor(s, out _))
                    {
                        lst.Add(s);
                    }
                }
                _trackedSettings = lst.ToArray();
            }

            _profiles = QualitySettingsProfile.CreateAllProfilesFromQualitySettings();
        }

        public ShadowSettings GetShadowSettings() => ShadowSettings.ReadFromCurrentQualitySettings();
        public void SetShadowSettings(ShadowSettings settings)
        {
            settings.WriteToCurrentQualitySettings();
            OnQualitySettingChanged(QualitySettingsHelper.SETTING_SHADOWSETTINGS);
        }

        public virtual object GetSetting(string name)
        {
            if (QualitySettingsHelper.TryGetSetting(name, out object value))
            {
                return value;
            }
            return null;
        }

        public virtual bool SetSetting(string name, object value)
        {
            if (QualitySettingsHelper.SetSetting(name, value))
            {

                return true;
            }
            return false;
        }

        public virtual QualitySettingsProfile GetProfile(string name)
        {
            if (_profiles == null) return null;

            for (int i = 0; i < _profiles.Length; i++)
            {
                if (_profiles[i].Name == name) return _profiles[i];
            }
            return null;
        }

        public virtual QualitySettingsProfile GetProfile(int index)
        {
            if (_profiles == null) return null;
            return index >= 0 && index < _profiles.Length ? _profiles[index] : null;
        }

        public virtual int SetProfile(string name) => this.SetProfile(this.GetProfile(name));
        public virtual int SetProfile(int index) => this.SetProfile(this.GetProfile(index));
        public virtual int SetProfile(QualitySettingsProfile profile)
        {
            if (profile == null) return 0;

            bool ignore = _ignoreChangeEvent;
            _ignoreChangeEvent = true;
            try
            {
                if (profile.ProfileIndex >= 0 && profile.ProfileIndex < QualitySettings.count)
                {
                    QualitySettings.SetQualityLevel(profile.ProfileIndex);
                }

                int cnt = 0;
                foreach (var nm in profile.TrackedSettings)
                {
                    if (QualitySettingsHelper.SetSetting(nm, profile.GetValue(nm)))
                    {
                        cnt++;
                    }
                }

                if (!ignore)
                {
                    using (var ev = QualitySettingsChangedTempEventArgs.Create(this, profile))
                    {
                        this.QualityChanged?.Invoke(this, ev);
                        Messaging.Broadcast<IOnQualitySettingsChangedGlobalHandler, QualitySettingsChangedTempEventArgs>(ev, (o, a) => o.OnQualitySettingsChanged(a));
                    }
                }

                return cnt;
            }
            finally
            {
                _ignoreChangeEvent = ignore;
            }
        }




        protected void SuppressQualityChangedEvent() => _ignoreChangeEvent = true;
        protected void ResumeQualityChangedEvent() => _ignoreChangeEvent = false;

        private void OnQualitySettingChanged(string name)
        {
            if (_ignoreChangeEvent) return;

            using (var ev = QualitySettingsChangedTempEventArgs.Create(this, name))
            {
                this.QualityChanged?.Invoke(this, ev);
                Messaging.Broadcast<IOnQualitySettingsChangedGlobalHandler, QualitySettingsChangedTempEventArgs>(ev, (o, a) => o.OnQualitySettingsChanged(a));
            }
        }

        private void QualitySettings_activeQualityLevelChanged(int prev, int curr)
        {
            _lastQualityLevel = prev;
            _currentQualityLevel = curr;
            if (_ignoreChangeEvent) return;

            var profile = _currentQualityLevel >= 0 && _currentQualityLevel < _profiles.Length ? _profiles[_currentQualityLevel] : null;
            using (var ev = QualitySettingsChangedTempEventArgs.Create(this, profile))
            {
                this.QualityChanged?.Invoke(this, ev);
                Messaging.Broadcast<IOnQualitySettingsChangedGlobalHandler, QualitySettingsChangedTempEventArgs>(ev, (o, a) => o.OnQualitySettingsChanged(a));
            }
        }

        #endregion

#if UNITY_EDITOR
        [ShowNonSerializedProperty("Quality Level Index", ShowOutsideRuntimeValuesFoldout = true)]
        private int EditorOnly_CurrentQualityLevelIndex => QualitySettings.GetQualityLevel();
        [ShowNonSerializedProperty("Quality Level", ShowOutsideRuntimeValuesFoldout = true)]
        private string EditorOnly_CurrentQualityLevel
        {
            get
            {
                int i = QualitySettings.GetQualityLevel();
                return i >= 0 && i < _profiles.Length ? _profiles[i]?.Name ?? "UNKNOWN" : "UNKNOWN";
            }
        }
#endif

    }

#else
    /// <summary>
    /// SPQualitySettings does not support all features in Unity 2022.2 or earlier since Unity does not make the required API calls available.
    /// </summary>
    public class SPQualitySettings : ServiceComponent<IQualitySettings>, IQualitySettings
    {

        public event System.EventHandler<QualitySettingsChangedTempEventArgs> QualityChanged;
        public EventHandlerRef<QualitySettingsChangedTempEventArgs> QualityChanged_ref() => EventHandlerRef<QualitySettingsChangedTempEventArgs>.Create(this, (o, h) => o.QualityChanged += h, (o, h) => o.QualityChanged -= h);

        #region Fields

        [SerializeField, ReorderableArray, QualitySettingsPropertyName]
        private string[] _trackedSettings;

        [System.NonSerialized]
        private int _initialQualityLevel;
        [System.NonSerialized]
        private int _currentQualityLevel;
        [System.NonSerialized]
        private int _lastQualityLevel;

        [System.NonSerialized]
        private bool _ignoreChangeEvent;

        #endregion

        #region CONSTRUCTOR

        protected override void OnValidAwake()
        {
            base.OnValidAwake();

            _initialQualityLevel = QualitySettings.GetQualityLevel();
            _currentQualityLevel = _initialQualityLevel;
            _lastQualityLevel = _currentQualityLevel;

            this.ConfigureTrackedSettings(_trackedSettings);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        #endregion

        #region Properties

        public int InitialQualityLevel => _initialQualityLevel;
        public int LastQualityLevel => _lastQualityLevel;
        public int CurrentQualityLevel => _currentQualityLevel;

        public IEnumerable<string> TrackedSettings => _trackedSettings;

        public int ProfileCount => QualitySettingsHelper.ProfileCount;



        public AnisotropicFiltering AnisotropicFiltering
        {
            get => QualitySettings.anisotropicFiltering;
            set
            {
                QualitySettings.anisotropicFiltering = value;
                this.OnQualitySettingChanged(nameof(QualitySettings.anisotropicFiltering));
            }
        }

        public AntiAliasingLevel AntiAliasing
        {
            get => (AntiAliasingLevel)QualitySettings.antiAliasing;
            set
            {
                QualitySettings.antiAliasing = (int)value;
                this.OnQualitySettingChanged(nameof(QualitySettings.antiAliasing));
            }
        }

        public int PixelLightCount
        {
            get => QualitySettings.pixelLightCount;
            set
            {
                QualitySettings.pixelLightCount = value;
                this.OnQualitySettingChanged(nameof(QualitySettings.pixelLightCount));
            }
        }

        #endregion

        #region Methods

        public virtual void ConfigureTrackedSettings(IEnumerable<string> settingNames)
        {
            using (var lst = TempCollection.GetList<string>())
            {
                foreach (var s in settingNames)
                {
                    if (QualitySettingsHelper.TryGetSettingAccessor(s, out _))
                    {
                        lst.Add(s);
                    }
                }
                _trackedSettings = lst.ToArray();
            }
        }

        public ShadowSettings GetShadowSettings() => ShadowSettings.ReadFromCurrentQualitySettings();
        public void SetShadowSettings(ShadowSettings settings)
        {
            settings.WriteToCurrentQualitySettings();
            OnQualitySettingChanged(QualitySettingsHelper.SETTING_SHADOWSETTINGS);
        }

        public virtual object GetSetting(string name)
        {
            if (QualitySettingsHelper.TryGetSetting(name, out object value))
            {
                return value;
            }
            return null;
        }

        public virtual bool SetSetting(string name, object value)
        {
            if (QualitySettingsHelper.SetSetting(name, value))
            {

                return true;
            }
            return false;
        }

        QualitySettingsProfile IQualitySettings.GetProfile(string name)
        {
            throw new System.NotSupportedException("IQualitySettings.GetProfile is not supported in Unity 2022.2 or earlier.");
        }

        QualitySettingsProfile IQualitySettings.GetProfile(int index)
        {
            throw new System.NotSupportedException("IQualitySettings.GetProfile is not supported in Unity 2022.2 or earlier.");
        }

        public virtual int SetProfile(string name)
        {
            int index = QualitySettings.names.IndexOf(name);
            if (index < 0 || index >= QualitySettingsHelper.ProfileCount) return 0;

            _lastQualityLevel = _currentQualityLevel;
            _currentQualityLevel = index;
            QualitySettings.SetQualityLevel(index);
            if (!_ignoreChangeEvent)
            {
                using (var ev = QualitySettingsChangedTempEventArgs.Create(this, (QualitySettingsProfile)null))
                {
                    this.QualityChanged?.Invoke(this, ev);
                    Messaging.Broadcast<IOnQualitySettingsChangedGlobalHandler, QualitySettingsChangedTempEventArgs>(ev, (o, a) => o.OnQualitySettingsChanged(a));
                }
            }
            return 0;
        }
        public virtual int SetProfile(int index)
        {
            if (index < 0 || index >= QualitySettingsHelper.ProfileCount) return 0;

            _lastQualityLevel = _currentQualityLevel;
            _currentQualityLevel = index;
            QualitySettings.SetQualityLevel(index);
            if (!_ignoreChangeEvent)
            {
                using (var ev = QualitySettingsChangedTempEventArgs.Create(this, (QualitySettingsProfile)null))
                {
                    this.QualityChanged?.Invoke(this, ev);
                    Messaging.Broadcast<IOnQualitySettingsChangedGlobalHandler, QualitySettingsChangedTempEventArgs>(ev, (o, a) => o.OnQualitySettingsChanged(a));
                }
            }
            return 0;
        }
        public virtual int SetProfile(QualitySettingsProfile profile)
        {
            if (profile == null) return 0;

            bool ignore = _ignoreChangeEvent;
            _ignoreChangeEvent = true;
            try
            {
                if (profile.ProfileIndex >= 0 && profile.ProfileIndex < QualitySettingsHelper.ProfileCount)
                {
                    QualitySettings.SetQualityLevel(profile.ProfileIndex);
                }

                int cnt = 0;
                foreach (var nm in profile.TrackedSettings)
                {
                    if (QualitySettingsHelper.SetSetting(nm, profile.GetValue(nm)))
                    {
                        cnt++;
                    }
                }

                if (!ignore)
                {
                    using (var ev = QualitySettingsChangedTempEventArgs.Create(this, profile))
                    {
                        this.QualityChanged?.Invoke(this, ev);
                        Messaging.Broadcast<IOnQualitySettingsChangedGlobalHandler, QualitySettingsChangedTempEventArgs>(ev, (o, a) => o.OnQualitySettingsChanged(a));
                    }
                }

                return cnt;
            }
            finally
            {
                _ignoreChangeEvent = ignore;
            }
        }




        protected void SuppressQualityChangedEvent() => _ignoreChangeEvent = true;
        protected void ResumeQualityChangedEvent() => _ignoreChangeEvent = false;

        private void OnQualitySettingChanged(string name)
        {
            if (_ignoreChangeEvent) return;

            using (var ev = QualitySettingsChangedTempEventArgs.Create(this, name))
            {
                this.QualityChanged?.Invoke(this, ev);
                Messaging.Broadcast<IOnQualitySettingsChangedGlobalHandler, QualitySettingsChangedTempEventArgs>(ev, (o, a) => o.OnQualitySettingsChanged(a));
            }
        }

        #endregion

#if UNITY_EDITOR
        [ShowNonSerializedProperty("Quality Level Index", ShowOutsideRuntimeValuesFoldout = true)]
        private int EditorOnly_CurrentQualityLevelIndex => QualitySettings.GetQualityLevel();

        public IEnumerable<QualitySettingsProfile> Profiles => throw new System.NotImplementedException();
#endif

    }

#endif
}
