using UnityEngine;
using System.Collections.Generic;

namespace com.spacepuppy.Quality
{

    /// <summary>
    /// Represents a change event of IQualitySettings. 
    /// These are temporary and are disposed after completion of event. 
    /// </summary>
    public class QualitySettingsChangedTempEventArgs : System.EventArgs, System.IDisposable
    {

        #region Fields

        private IQualitySettings _settings;
        private string _settingName;
        private QualitySettingsProfile _profile;

        #endregion

        #region CONSTRUCTOR

        public QualitySettingsChangedTempEventArgs() { }
        public QualitySettingsChangedTempEventArgs(IQualitySettings settings, string settingName)
        {
            this.Reconfigure(settings, settingName, null);
        }
        public QualitySettingsChangedTempEventArgs(IQualitySettings settings, QualitySettingsProfile profile)
        {
            this.Reconfigure(settings, null, profile);
        }

        #endregion

        #region Properties

        public IQualitySettings Settings => _settings;
        public string SettingName => _settingName;
        public QualitySettingsProfile Profile => _profile;

        public bool ChangedProfile => _profile != null && string.IsNullOrEmpty(_settingName);
        public bool ChangedIndividualSetting => !string.IsNullOrEmpty(_settingName);

        #endregion

        #region Methods

        private void Reconfigure(IQualitySettings settings, string settingName, QualitySettingsProfile profile)
        {
            _settings = settings;
            _settingName = settingName;
            _profile = profile;
        }

        public void Dispose()
        {
            Release(this);
        }

        #endregion

        #region Multiton Reference

        private static int _cacheLimit = 100;
        public static int CacheLimit
        {
            get { return _cacheLimit; }
            set { _cacheLimit = value; }
        }

        private static Stack<QualitySettingsChangedTempEventArgs> _args = new Stack<QualitySettingsChangedTempEventArgs>();

        public static QualitySettingsChangedTempEventArgs Create(IQualitySettings settings, string propName) => CreateInternal(settings, propName, null);
        public static QualitySettingsChangedTempEventArgs Create(IQualitySettings settings, QualitySettingsProfile profile) => CreateInternal(settings, null, profile);
        private static QualitySettingsChangedTempEventArgs CreateInternal(IQualitySettings settings, string propName, QualitySettingsProfile profile)
        {
            QualitySettingsChangedTempEventArgs e;
            lock (_args)
            {
                if (_args.Count > 0)
                {
                    e = _args.Pop();
                }
                else
                {
                    e = new();
                }
            }

            e.Reconfigure(settings, propName, profile);
            return e;
        }

        public static void Release(QualitySettingsChangedTempEventArgs e)
        {
            lock (_args)
            {
                if (_args.Count < _cacheLimit)
                {
                    e.Reconfigure(null, null, null);
                    _args.Push(e);
                }
            }
        }

        #endregion

    }

    public interface IOnQualitySettingsChangedGlobalHandler
    {
        void OnQualitySettingsChanged(QualitySettingsChangedTempEventArgs args);
    }

}
