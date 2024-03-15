#pragma warning disable 0649 // variable declared but not used.

using UnityEngine;

using com.spacepuppy.Utils;

namespace com.spacepuppy.Events
{

    public class i_TriggerRandom : AutoTriggerable, IObservableTrigger
    {

        #region Fields

        [SerializeField]
        private RandomRef _rng = new RandomRef();

        [SerializeField()]
        [SPEvent.Config(Weighted = true)]
        private SPEvent _targets = new SPEvent("Targets");

        [SerializeField]
        [DisplayIf("SelectOnlyActiveTargets")]
        [Tooltip("If 'Select Only Active Targets' is true and no target is found when attempting to trigger, this is called in its place.")]
        private SPEvent _failOver = new SPEvent("FailOver");

        [SerializeField]
        private bool _selectOnlyActiveTargets;

        [SerializeField()]
        private bool _passAlongTriggerArg;

        [SerializeField()]
        private SPTimePeriod _delay = 0f;

        [SerializeField]
        private bool _invokeGuaranteed;

        #endregion

        #region Properties

        public bool SelectOnlyActiveTargets
        {
            get { return _selectOnlyActiveTargets; }
            set { _selectOnlyActiveTargets = value; }
        }

        public bool PassAlongTriggerArg
        {
            get { return _passAlongTriggerArg; }
            set { _passAlongTriggerArg = value; }
        }

        public SPTimePeriod Delay
        {
            get { return _delay; }
            set { _delay = value; }
        }

        public bool InvokeGuaranteed
        {
            get => _invokeGuaranteed;
            set => _invokeGuaranteed = value;
        }

        public IRandom RNG
        {
            get { return _rng.Value; }
            set { _rng.Value = value; }
        }

        public SPEvent Targets => _targets;

        public SPEvent FailOver => _failOver;

        #endregion
        
        #region ITriggerableMechanism Interface

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            if (!_passAlongTriggerArg) arg = null;

            if (_delay.Seconds > 0f)
            {
                if (_invokeGuaranteed)
                {
                    this.InvokeGuaranteed(() =>
                    {
                        if (!_targets.ActivateRandomTrigger(this, arg, true, _selectOnlyActiveTargets, _rng.Value) && _selectOnlyActiveTargets)
                        {
                            _failOver.ActivateTrigger(this, arg);
                        }
                    }, _delay.Seconds, _delay.TimeSupplier);
                }
                else
                {
                    this.Invoke(() =>
                    {
                        if (!_targets.ActivateRandomTrigger(this, arg, true, _selectOnlyActiveTargets, _rng.Value) && _selectOnlyActiveTargets)
                        {
                            _failOver.ActivateTrigger(this, arg);
                        }
                    }, _delay.Seconds, _delay.TimeSupplier, RadicalCoroutineDisableMode.CancelOnDisable);
                }
            }
            else
            {
                if (!_targets.ActivateRandomTrigger(this, arg, true, _selectOnlyActiveTargets, _rng.Value) && _selectOnlyActiveTargets)
                {
                    _failOver.ActivateTrigger(this, arg);
                }
            }

            return true;
        }

        #endregion

        #region IObservableTrigger Interface

        BaseSPEvent[] IObservableTrigger.GetEvents()
        {
            return new BaseSPEvent[] { _targets, _failOver };
        }

        #endregion

    }

}
