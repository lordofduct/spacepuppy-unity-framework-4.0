using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;
using com.spacepuppy.Events;

namespace com.spacepuppy.events
{

    public class i_TransitionToAudioMixerSnapshot : AutoTriggerable
    {

        #region Fields

        [SerializeField]
        private AudioMixerSnapshot _snapshot;

        [SerializeField]
        private float _duration;

        #endregion

        #region Properties

        public AudioMixerSnapshot Snapshot { get => _snapshot; set => _snapshot = value; }

        public float Duration { get => _duration; set => _duration = value; }

        #endregion

        #region Triggerable Interface

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;
            if (!_snapshot) return false;

            _snapshot.TransitionTo(_duration);
            return true;
        }

        #endregion

    }

}
