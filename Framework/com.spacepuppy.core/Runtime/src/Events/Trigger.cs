using UnityEngine;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Events
{

    public abstract class TriggerComponent : SPComponent, IObservableTrigger
    {

        #region Fields

        [SerializeField()]
        private SPEvent _trigger = new SPEvent();

        #endregion

        #region CONSTRUCTOR

        public TriggerComponent()
        {
            _trigger.ObservableTriggerId = this.GetType().Name;
        }

        #endregion

        #region Properties

        public SPEvent Trigger
        {
            get
            {
                return _trigger;
            }
        }

        #endregion

        #region Methods

        public virtual void ActivateTrigger()
        {
            _trigger.ActivateTrigger(this, null);
        }

        public virtual void ActivateTrigger(object arg)
        {
            _trigger.ActivateTrigger(this, arg);
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
