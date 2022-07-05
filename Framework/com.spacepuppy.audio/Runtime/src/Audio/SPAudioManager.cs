using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Utils;
using UnityEngine.Audio;

namespace com.spacepuppy.Audio
{

    public class SPAudioManager : ServiceComponent<IAudioManager>, IAudioManager
    {

        #region Fields

        [SerializeField]
        private AudioMixer _audioMixer;

        [SerializeField]
        private string _masterVolumeLabel = string.Empty;

        [System.NonSerialized]
        private float _masterVolume;
        [System.NonSerialized]
        private float _fadeVolume;

        #endregion

        #region CONSTRUCTOR

        protected override void OnValidAwake()
        {
            base.OnValidAwake();

            _masterVolume = _audioMixer.GetVolume01(_masterVolumeLabel);
            _fadeVolume = 1f;
        }

        #endregion

        #region Properties

        public AudioMixer AudioMixer => _audioMixer;

        public bool UseAudioListenerInterfaceAsMasterVolume => string.IsNullOrEmpty(_masterVolumeLabel);

        #endregion

        #region IAudioManager Interface

        public float MasterVolume
        {
            get { return _masterVolume; }
            set
            {
                _masterVolume = Mathf.Clamp01(value);
                if (this.UseAudioListenerInterfaceAsMasterVolume)
                {
                    AudioListener.volume = _masterVolume * _fadeVolume;
                }
                else if (_audioMixer)
                {
                    _audioMixer.SetVolume01(_masterVolumeLabel, _masterVolume * _fadeVolume);
                }
            }
        }

        public float FadeVolume
        {
            get { return _fadeVolume; }
            set
            {
                _fadeVolume = Mathf.Clamp01(value);
                if (this.UseAudioListenerInterfaceAsMasterVolume)
                {
                    AudioListener.volume = _masterVolume * _fadeVolume;
                }
                else if (_audioMixer)
                {
                    _audioMixer.SetVolume01(_masterVolumeLabel, _masterVolume * _fadeVolume);
                }
            }
        }

        public bool SetVolume(string label, float value)
        {
            return _audioMixer.SetVolume01(label, value);
        }

        public bool GetVolume(string label, out float value)
        {
            return _audioMixer.GetVolume01(label, out value);
        }

        #endregion


    }

}
