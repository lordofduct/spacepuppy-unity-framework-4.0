using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Quality;

namespace com.spacepuppy
{

    public interface IQualitySettings : IService
    {
        event System.EventHandler<QualitySettingsChangedTempEventArgs> QualityChanged;
        EventHandlerRef<QualitySettingsChangedTempEventArgs> QualityChanged_ref() => EventHandlerRef<QualitySettingsChangedTempEventArgs>.Create(this, (o, h) => o.QualityChanged += h, (o, h) => o.QualityChanged -= h);

        IEnumerable<string> TrackedSettings { get; }
        int ProfileCount { get; }
        IEnumerable<QualitySettingsProfile> Profiles { get; }

        object GetSetting(string name);
        bool SetSetting(string name, object value);

        QualitySettingsProfile GetProfile(string name);
        QualitySettingsProfile GetProfile(int index);

        int SetProfile(string name) => this.SetProfile(this.GetProfile(name));
        int SetProfile(int index) => this.SetProfile(this.GetProfile(index));
        int SetProfile(QualitySettingsProfile profile);

    }

}
