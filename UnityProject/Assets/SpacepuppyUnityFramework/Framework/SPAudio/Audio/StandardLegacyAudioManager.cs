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

        [System.NonSerialized]
        private AudioSource _backgroundAmbientAudioSource;

        #endregion

        #region CONSTRUCTOR

        protected override void OnValidAwake()
        {
            base.OnValidAwake();

            _masterVolume = AudioListener.volume;
            _fadeVolume = 1f;
            _backgroundAmbientAudioSource = this.AddOrGetComponent<AudioSource>();
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

        public AudioSource BackgroundAmbientAudioSource
        {
            get { return _backgroundAmbientAudioSource; }
        }

        #endregion

    }

}