using UnityEngine;
using com.spacepuppy.Collections;

namespace com.spacepuppy.Utils
{

    /// <summary>
    /// Custom 'PlayOneShot' with interruption modes and callbacks. Modes include:
    /// StopIfPlaying - stops the currently playing audio before playing this
    /// DoNotPlayIfPlaying - does not play if currently playing
    /// PlayOverExisting - plays the audio along side the existing (since the callback relies on 'isPlaying' turning false, the callback is not reliable here)
    /// QueueOnce - if the audio isPlaying, it'll attempt to play this track once complete. Only one clip can be queued at a time, so subsequent calls will throw this clip out.
    /// </summary>
    /// <remarks>
    /// OnComplete Callback behaviour will fire when the AudioSource is no longer signaling that it 'isPlaying'. 
    /// Subsequent AudioClips being played over the clip you called to play can cause the callback to fire in specific ways based on how it is called. 
    /// 
    /// StopIfPlaying - called just before the new clip is queued up.
    /// DoNotPlayIfPlaying - no impact since it is ignored
    /// PlayOverExisting - if the clip is longer than what is left, the isPlaying will extend until the 2nd clip completes
    /// QueueOnce - callback will not fire until the queued clip completes, this can keep rolling if more clips are queued.
    /// </remarks>
    public static class AudioUtil
    {

        public static void PlayOneShot(this AudioSource src, AudioClip clip, AudioInterruptMode mode) => PlayOneShot(src, clip, 1f, mode, null);

        public static void PlayOneShot(this AudioSource src, AudioClip clip, AudioInterruptMode mode, System.Action callback) => PlayOneShot(src, clip, 1f, mode, callback);

        public static void PlayOneShot(this AudioSource src, AudioClip clip, float volumeScale, AudioInterruptMode mode) => PlayOneShot(src, clip, volumeScale, mode, null);

        public static void PlayOneShot(this AudioSource src, AudioClip clip, float volumeScale, AudioInterruptMode mode, System.Action callback)
        {
            if (src == null) throw new System.ArgumentNullException(nameof(src));
            if (clip == null) throw new System.ArgumentNullException(nameof(clip));

            var hook = AudioSourceHook.FindHook(src);
            if (hook || callback != null || mode == AudioInterruptMode.QueueOnce)
            {
                if (!hook) hook = AudioSourceHook.CreateHook(src);
                hook.PlayOneShot(clip, volumeScale, mode, callback);
            }
            else
            {
                switch (mode)
                {
                    case AudioInterruptMode.StopIfPlaying:
                        {
                            if (src.isPlaying) src.Stop();
                            src.PlayOneShot(clip, volumeScale);
                        }
                        break;
                    case AudioInterruptMode.DoNotPlayIfPlaying:
                        {
                            if (src.isPlaying) return;
                            src.PlayOneShot(clip, volumeScale);
                        }
                        break;
                    case AudioInterruptMode.PlayOverExisting:
                        {
                            src.PlayOneShot(clip, volumeScale);
                        }
                        break;
                }
            }

        }

        #region Special Types

        private sealed class AudioSourceHook : MonoBehaviour
        {

            #region Fields

            [System.NonSerialized]
            private AudioSource _source;

            [System.NonSerialized]
            private System.Action _callback;
            [System.NonSerialized]
            private AudioClip _queuedClip;
            [System.NonSerialized]
            private float _queuedVolumeScale;

            #endregion

            #region CONSTRUCTOR

            private void OnDestroy()
            {
                var d = _callback;
                _callback = null;
                d?.Invoke();
            }

            #endregion

            #region Methods

            public void Stop()
            {
                if (_source) _source.Stop();
                _queuedClip = null;
                _queuedVolumeScale = 1f;

                _callback?.Invoke();
                _callback = null;
            }

            public void PlayOneShot(AudioClip clip, float volumeScale, AudioInterruptMode mode, System.Action callback)
            {
                switch (mode)
                {
                    case AudioInterruptMode.StopIfPlaying:
                        {
                            if (_source.isPlaying) this.Stop();
                            _source.PlayOneShot(clip, volumeScale);
                            this.TryRegisterCallback(callback);
                        }
                        break;
                    case AudioInterruptMode.DoNotPlayIfPlaying:
                        {
                            if (_source.isPlaying) return;
                            _source.PlayOneShot(clip, volumeScale);
                            this.TryRegisterCallback(callback);
                        }
                        break;
                    case AudioInterruptMode.PlayOverExisting:
                        {
                            _source.PlayOneShot(clip, volumeScale);
                            this.TryRegisterCallback(callback);
                        }
                        break;
                    case AudioInterruptMode.QueueOnce:
                        {
                            if (_source.isPlaying)
                            {
                                _queuedClip = clip;
                                _queuedVolumeScale = volumeScale;
                                this.TryRegisterCallback(callback);
                                this.enabled = true;
                            }
                            else
                            {
                                _source.PlayOneShot(clip, volumeScale);
                                this.TryRegisterCallback(callback);
                            }
                        }
                        break;
                }
            }

            private void TryRegisterCallback(System.Action action)
            {
                if (action == null) return;

                if (_callback != null)
                {
                    _callback += action;
                }
                else
                {
                    _callback = action;
                }
                this.enabled = true;
            }

            private void Update()
            {
                if (!_source || !_source.isPlaying)
                {
                    if (_queuedClip != null)
                    {
                        var clip = _queuedClip;
                        var volsc = _queuedVolumeScale;
                        _queuedClip = null;
                        _queuedVolumeScale = 1f;
                        _source.PlayOneShot(clip, volsc);
                    }
                    else
                    {
                        var d = _callback;
                        _callback = null;
                        d?.Invoke();
                        this.enabled = false;
                    }
                }
            }

            #endregion

            #region Static Hooks

            public static AudioSourceHook FindHook(AudioSource source)
            {
                if (source == null) return null;

                using (var lst = TempCollection.GetList<AudioSourceHook>())
                {
                    source.gameObject.GetComponents<AudioSourceHook>(lst);
                    foreach (var hook in lst)
                    {
                        if (hook._source == null || hook._source == source) return hook;
                    }
                }

                return null;
            }

            public static AudioSourceHook CreateHook(AudioSource source)
            {
                var hook = source.AddComponent<AudioSourceHook>();
                hook.enabled = false;
                hook._source = source;
                return hook;
            }

            #endregion

        }

        #endregion

    }
}
