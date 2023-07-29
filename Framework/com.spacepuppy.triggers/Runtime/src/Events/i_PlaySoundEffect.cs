#pragma warning disable 0649 // variable declared but not used.

using UnityEngine;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Events
{

    [Infobox("Interrupt modes other than 'StopIfPlaying' can cause unpredictable 'OnAudioComplete' event callbacks or even doubling up of them. Avoid using the 'OnAudioComplete' if you're using any other mode than 'StopIfPlaying'.")]
    public class i_PlaySoundEffect : AutoTriggerable
    {

        #region Fields

        [SerializeField()]
        [TriggerableTargetObject.Config(typeof(AudioSource))]
        private TriggerableTargetObject _targetAudioSource = new TriggerableTargetObject();

        [SerializeField()]
        [WeightedValueCollection("Weight", "_clip", ElementLabelFormatString = "Clip {0:00}")]
        [Tooltip("One or Many, if many they will be randomly selected by the weights supplied.")]
        private AudioClipEntry[] _clips = ArrayUtil.Empty<AudioClipEntry>();

        [SerializeField()]
        private AudioInterruptMode _interrupt = AudioInterruptMode.StopIfPlaying;

        [SerializeField()]
        private SPTimePeriod _delay;

        [SerializeField]
        private RandomRef _rng = new RandomRef();

        [Tooltip("Trigger something at the end of the sound effect. This is NOT perfectly accurate and really just starts a timer for the duration of the sound being played.")]
        [SerializeField()]
        private SPEvent _onAudioComplete = new SPEvent("OnAudioComplete");

        #endregion

        #region Properties

        public TriggerableTargetObject TargetAudioSource
        {
            get { return _targetAudioSource; }
        }

        public AudioClipEntry[] Clips
        {
            get { return _clips; }
            set { _clips = value ?? ArrayUtil.Empty<AudioClipEntry>(); }
        }

        public AudioInterruptMode Interrupt
        {
            get { return _interrupt; }
            set { _interrupt = value; }
        }

        public SPTimePeriod Delay
        {
            get { return _delay; }
            set { _delay = value; }
        }

        public IRandom RNG
        {
            get => _rng.Value;
            set => _rng.Value = value;
        }

        public SPEvent OnAudioComplete
        {
            get { return _onAudioComplete; }
        }

        #endregion

        #region ITriggerableMechanism Interface

        public override bool CanTrigger
        {
            get
            {
                return base.CanTrigger && _clips != null && _clips.Length > 0;
            }
        }

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            var src = _targetAudioSource.GetTarget<AudioSource>(arg);
            if (src == null)
            {
                Debug.LogWarning("Failed to play audio due to a lack of AudioSource on the target.", this);
                return false;
            }

            AudioClip clip;
            if (_clips.Length == 0)
            {
                return false;
            }
            else if (_clips.Length == 1)
            {
                clip = ObjUtil.GetAsFromSource<AudioClip>(_clips[0].Clip, true);
            }
            else
            {
                clip = ObjUtil.GetAsFromSource<AudioClip>(_clips.PickRandom((e) => e.Weight, _rng.Value).Clip, true);
            }
            if (!clip) return false;

            if (_delay.Seconds > 0)
            {
                this.InvokeGuaranteed(() =>
                {
                    if (src != null)
                    {
                        if (this && this.isActiveAndEnabled && _onAudioComplete.HasReceivers)
                        {
                            src.PlayOneShot(clip, _interrupt, () =>
                            {
                                if (this && this.isActiveAndEnabled) _onAudioComplete.ActivateTrigger(this, null);
                            });
                        }
                        else
                        {
                            src.PlayOneShot(clip, _interrupt);
                        }
                    }
                }, _delay.Seconds, _delay.TimeSupplier);
                return true;
            }
            else
            {
                if (_onAudioComplete.HasReceivers)
                {
                    src.PlayOneShot(clip, _interrupt, () =>
                    {
                        if (this && this.isActiveAndEnabled) _onAudioComplete.ActivateTrigger(this, null);
                    });
                }
                else
                {
                    src.PlayOneShot(clip, _interrupt);
                }
                return true;
            }
        }

        #endregion

        #region Special Types

        [System.Serializable]
        public struct AudioClipEntry
        {
            public float Weight;
            [UnityEngine.Serialization.FormerlySerializedAs("Clip")]
            [SerializeField]
            [TypeRestriction(typeof(AudioClip), AllowProxy = true)]
            private UnityEngine.Object _clip;

            public UnityEngine.Object Clip
            {
                get => _clip;
                set
                {
                    _clip = IProxyExtensions.FilterAsProxyOrType<AudioClip>(value) as UnityEngine.Object;
                }
            }

        }

        #endregion

    }

}
