using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Events;
using com.spacepuppy.Utils;
using System;

namespace com.spacepuppy.Mecanim.Events
{

    public sealed class i_PlayAnimation_Mecanim : AutoTriggerable, IObservableTrigger
    {

        #region Fields

        [SerializeField]
        [TriggerableTargetObject.Config(typeof(Animator))]
        private TriggerableTargetObject _targetAnimator;

        [SerializeField]
        private AnimatorOverrideSourceRef _animatorOverrides;

        [SerializeField]
        private PlayStateConfiguration _config;

        [SerializeField]
        [Tooltip("The token used to identify the layer to purge. Leave blank to ignore.")]
        private string _token;

        [SerializeField]
        [Tooltip("Purge the override by token on exit. If left blank the overrides will always be purged on exit.")]
        private bool _purgeTokenOnExit = true;

        [SerializeField]
        [SPEvent.Config("animator (Animator)")]
        private SPEvent _onStateExit = new SPEvent("OnStateExit");

        #endregion

        #region Properties

        public TriggerableTargetObject TargetAnimator => _targetAnimator;

        public AnimatorOverrideSourceRef AnimatorOverrides => _animatorOverrides;

        /// <summary>
        /// The token used to id the override animations. If left blank than PurgeTokenOnExit is ignored and the override animations will be purged no matter what.
        /// </summary>
        public string Token { get { return _token; } set { _token = value; } }

        public bool PurgeTokenOnExit { get { return _purgeTokenOnExit; } set { _purgeTokenOnExit = value; } }

        public PlayStateConfiguration Config => _config;

        public SPEvent OnStateExit => _onStateExit;

        #endregion

        #region Triggerable Interface

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            var targ = _targetAnimator.GetTarget<Animator>(arg);
            if (targ == null) return false;

            object token = string.IsNullOrEmpty(_token) ? (object)this : _token;
            bool purge = string.IsNullOrEmpty(_token) || _purgeTokenOnExit;
            targ.StackOverride(_animatorOverrides, token);

            _config.Play(targ);
            if (purge || _onStateExit.HasReceivers)
            {
                GameLoop.Hook.StartPooledRadicalCoroutine(this.DoWait(targ, _config.StateName, _config.Layer, token, purge, _config.FinalState, _config.FinalStateTimeout));
            }
            return true;
        }

        private System.Collections.IEnumerator DoWait(Animator animator, string stateName, int layerIndex, object token, bool purge, string finalState, SPTimePeriod timeout)
        {
            yield return WaitForAnimState.WaitForStateExit_PostPlay(animator, stateName, layerIndex);
            if(!string.IsNullOrEmpty(finalState))
            {
                yield return WaitForAnimState.WaitForStateEnter(animator, finalState, layerIndex, timeout);
                yield return WaitForAnimState.WaitForStateExit(animator, finalState, layerIndex);
            }

            if (purge) animator.RemoveOverride(token);

            _onStateExit.ActivateTrigger(this, animator);
        }

        #endregion

        #region IObserverableTrigger Interface

        BaseSPEvent[] IObservableTrigger.GetEvents()
        {
            return new BaseSPEvent[] { _onStateExit };
        }

        #endregion

    }

}
