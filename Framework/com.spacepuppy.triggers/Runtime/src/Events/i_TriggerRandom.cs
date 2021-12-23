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
        private bool _selectOnlyActiveTargets;

        [SerializeField()]
        private bool _passAlongTriggerArg;

        [SerializeField()]
        private SPTimePeriod _delay = 0f;

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

        public IRandom RNG
        {
            get { return _rng.Value; }
            set { _rng.Value = value; }
        }

        public SPEvent Targets => _targets;

        #endregion
        
        #region ITriggerableMechanism Interface

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            if (_delay.Seconds > 0f)
            {
                this.InvokeGuaranteed(() =>
                {
                    if (this._passAlongTriggerArg)
                        _targets.ActivateRandomTrigger(this, arg, true, _selectOnlyActiveTargets, _rng.Value);
                    else
                        _targets.ActivateRandomTrigger(this, null, true, _selectOnlyActiveTargets, _rng.Value);
                }, _delay.Seconds, _delay.TimeSupplier);
            }
            else
            {
                if (this._passAlongTriggerArg)
                    _targets.ActivateRandomTrigger(this, arg, true, _selectOnlyActiveTargets, _rng.Value);
                else
                    _targets.ActivateRandomTrigger(this, null, true, _selectOnlyActiveTargets, _rng.Value);
            }

            return true;
        }

        #endregion

        #region IObservableTrigger Interface

        BaseSPEvent[] IObservableTrigger.GetEvents()
        {
            return new BaseSPEvent[] { _targets };
        }

        #endregion

    }

}
