#pragma warning disable 0649 // variable declared but not used.

using UnityEngine;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Events
{
    public class i_TriggerRandomDelay : AutoTriggerable, IObservableTrigger
    {
        #region Fields

        [SerializeField()]
        [SPEvent.Config("daisy chained arg (object)")]
        private SPEvent _trigger = new SPEvent("Trigger");

        [SerializeField()]
        private bool _passAlongTriggerArg;

        [SerializeField()]
        private SPTimePeriod _minDelay = 0f;

        [SerializeField()]
        private SPTimePeriod _maxDelay = 0f;

        [SerializeField]
        private RandomRef _rng = new RandomRef();

        #endregion

        #region Properties

        public SPEvent TriggerEvent
        {
            get { return _trigger; }
        }

        public bool PassAlongTriggerArg
        {
            get { return _passAlongTriggerArg; }
            set { _passAlongTriggerArg = value; }
        }

        public SPTimePeriod MinDelay
        {
            get { return _minDelay; }
            set { _minDelay = value; }
        }

        public SPTimePeriod MaxDelay
        {
            get { return _maxDelay; }
            set { _maxDelay = value; }
        }

        public IRandom RNG
        {
            get => _rng.Value;
            set => _rng.Value = value;
        }

        #endregion

        #region Methods

        private void DoTriggerNext(object arg)
        {
            if (_passAlongTriggerArg)
                _trigger.ActivateTrigger(this, arg);
            else
                _trigger.ActivateTrigger(this, null);
        }

        #endregion

        #region ITriggerableMechanism Interface

        public override bool Trigger(object sender, object arg)
        {
            if (!CanTrigger) return false;

            float randomDelay = _rng.ValueOrDefault.Range(_maxDelay.Seconds, _minDelay.Seconds);

            if (randomDelay > 0f)
            {
                this.InvokeGuaranteed(() =>
                {
                    DoTriggerNext(arg);
                }, randomDelay);
            }
            else
            {
                DoTriggerNext(arg);
            }

            return true;
        }

        #endregion

        #region IObservableTrigger Interface

        BaseSPEvent[] IObservableTrigger.GetEvents()
        {
            return new BaseSPEvent[] { _trigger };
        }

        #endregion
    }
}
