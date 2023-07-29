#pragma warning disable 0649 // variable declared but not used.

using UnityEngine;

namespace com.spacepuppy.Events
{

    public class i_StopAudioSource : AutoTriggerable
    {

        #region Fields

        [SerializeField]
        [TriggerableTargetObject.Config(typeof(AudioSource))]
        private TriggerableTargetObject _target = new TriggerableTargetObject();

        [SerializeField]
        private SPTimePeriod _fadeOutDur;

        [SerializeField]
        private DisableMode _disableAudioSource;

        #endregion

        #region Properties

        public TriggerableTargetObject Target
        {
            get { return _target; }
        }

        public SPTimePeriod FadeOutDur
        {
            get { return _fadeOutDur; }
            set { _fadeOutDur = value; }
        }

        public DisableMode DisableAudioSource
        {
            get { return _disableAudioSource; }
            set { _disableAudioSource = value; }
        }

        #endregion

        #region Triggerable Interface

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            var targ = _target.GetTarget<AudioSource>(arg);
            if (targ == null) return false;
            if (!targ.isPlaying) return false;

            if (_fadeOutDur.Seconds > 0f)
            {
                GameLoop.Hook.StartCoroutine(TweenOutVolume(targ, _fadeOutDur, _disableAudioSource));
            }
            else
            {
                targ.Stop();
                switch (_disableAudioSource)
                {
                    case DisableMode.DisableComponent:
                        targ.enabled = false;
                        break;
                    case DisableMode.DisableGameObject:
                        targ.gameObject.SetActive(false);
                        break;
                }
            }

            return true;
        }

        private static System.Collections.IEnumerator TweenOutVolume(AudioSource targ, SPTimePeriod duration, DisableMode disableMode)
        {
            float t = 0f;
            float cache = targ.volume;
            while (t < duration.Seconds && targ)
            {
                targ.volume = Mathf.Lerp(cache, 0f, t / duration.Seconds);
                yield return null;
                t += duration.TimeSupplierOrDefault.Delta;
            }

            if (targ)
            {
                targ.Stop();
                targ.volume = cache;
                switch (disableMode)
                {
                    case DisableMode.DisableComponent:
                        targ.enabled = false;
                        break;
                    case DisableMode.DisableGameObject:
                        targ.gameObject.SetActive(false);
                        break;
                }
            }
        }

        #endregion

    }

}
