using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Utils;

namespace com.spacepuppy.Audio
{

    [RequireComponent(typeof(AudioSource))]
    public class StandardLegacyAudioManager : ServiceComponent<IAudioManager>, IAudioManager
    {

        #region Fields

        [System.NonSerialized]
        private float _masterVolume;
        [System.NonSerialized]
        private float _fadeVolume;

        #endregion

        #region CONSTRUCTOR

        protected override void OnValidAwake()
        {
            base.OnValidAwake();

            _masterVolume = AudioListener.volume;
            _fadeVolume = 1f;
        }

        #endregion

        #region IAudioManager Interface

        public float MasterVolume
        {
            get { return _masterVolume; }
            set
            {
                _masterVolume = Mathf.Clamp01(value);
                AudioListener.volume = _masterVolume * _fadeVolume;
            }
        }

        public float FadeVolume
        {
            get { return _fadeVolume; }
            set
            {
                _fadeVolume = Mathf.Clamp01(value);
                AudioListener.volume = _masterVolume * _fadeVolume;
            }
        }

        bool IAudioManager.SetVolume(string label, float value)
        {
            return false;
        }

        bool IAudioManager.GetVolume(string label, out float value)
        {
            value = 0f;
            return false;
        }

        #endregion

    }

}