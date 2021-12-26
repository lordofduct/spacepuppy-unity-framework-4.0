using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Events;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Mecanim.Events
{

    public sealed class i_GoToAnimatorState : AutoTriggerable, IObservableTrigger
    {

        #region Fields

        [SerializeField]
        [TriggerableTargetObject.Config(typeof(Animator))]
        private TriggerableTargetObject _targetAnimator;

        [SerializeField]
        private PlayStateConfiguration _config;

        [SerializeField]
        private SPEvent _onStateExit = new SPEvent("OnStateExit");

        #endregion

        #region Properties

        public TriggerableTargetObject TargetAnimator => _targetAnimator;

        public PlayStateConfiguration Config => _config;

        public SPEvent OnStateExit => _onStateExit;

        #endregion

        #region Triggerable Interface

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            var targ = _targetAnimator.GetTarget<Animator>(arg);
            if (targ == null) return false;

            _config.Play(targ);
            if (_onStateExit.HasReceivers)
            {
                GameLoop.Hook.StartPooledRadicalCoroutine(this.DoWait(targ, _config.StateName, _config.Layer, _config.FinalState, _config.FinalStateTimeout));
            }
            return true;
        }

        private System.Collections.IEnumerator DoWait(Animator animator, string stateName, int layerIndex, string finalState, SPTimePeriod timeout)
        {
            yield return WaitForAnimState.WaitForStateExit_PostPlay(animator, stateName, layerIndex);
            if (!string.IsNullOrEmpty(finalState))
            {
                yield return WaitForAnimState.WaitForStateEnter(animator, finalState, layerIndex, timeout);
                yield return WaitForAnimState.WaitForStateExit(animator, finalState, layerIndex);
            }
            _onStateExit.ActivateTrigger(this, null);
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
