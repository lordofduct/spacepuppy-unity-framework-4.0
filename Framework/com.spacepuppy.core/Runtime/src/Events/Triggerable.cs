using UnityEngine;

using com.spacepuppy.Utils;

namespace com.spacepuppy.Events
{

    public abstract class Triggerable : SPComponent, ITriggerable
    {

        #region Fields

        [SerializeField()]
        private int _order;

        #endregion

        #region ITriggerable Interface

        public int Order
        {
            get { return _order; }
            set { _order = value; }
        }

        public virtual bool CanTrigger => this.IsActiveAndEnabled_OrderAgnostic();

        public void Trigger()
        {
            this.Trigger(null, null);
        }

        public abstract bool Trigger(object sender, object arg);

        #endregion

    }

    public abstract class AutoTriggerable : Triggerable, IMActivateOnReceiver
    {

        #region Fields

        [SerializeField]
        private ActivateEvent _activateOn = ActivateEvent.None;

        #endregion

        #region CONSTRUCTOR

        public AutoTriggerable() { }
        public AutoTriggerable(ActivateEvent defaultActiveOn)
        {
            _activateOn = defaultActiveOn;
        }

        #endregion

        #region Properties

        public ActivateEvent ActivateOn
        {
            get { return _activateOn; }
            set { _activateOn = value; }
        }

        #endregion

        #region IMActivateOnReceiver Interface

        void IMActivateOnReceiver.Activate() => this.Trigger(this, null);

        #endregion

    }

}
