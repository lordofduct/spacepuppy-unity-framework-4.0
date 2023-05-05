using UnityEngine;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Events
{

    [DefaultExecutionOrder(t_OnStart.DEFAULT_EXECUTION_ORDER)]
    public class t_OnStart : SPComponent, IObservableTrigger
    {
        public const int DEFAULT_EXECUTION_ORDER = 31900;

        #region Fields

        [SerializeField()]
        private SPTimePeriod _delay;

        [SerializeField()]
        private SPEvent _trigger = new SPEvent();

        #endregion

        #region Properties

        public SPTimePeriod Delay
        {
            get { return _delay; }
            set { _delay = value; }
        }

        public SPEvent Trigger => _trigger;

        #endregion

        #region Messages

        protected override void Start()
        {
            base.Start();

            if (_delay.Seconds > 0f)
            {
                this.InvokeGuaranteed(() =>
                {
                    _trigger.ActivateTrigger(this, null);
                }, _delay.Seconds, _delay.TimeSupplier);
            }
            else
            {
                _trigger.ActivateTrigger(this, null);
            }
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
