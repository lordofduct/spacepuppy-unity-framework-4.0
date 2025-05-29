using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

using com.spacepuppy.Events;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Netcode.Events
{

    public abstract class NetworkTriggerable : SPNetworkComponent, ITriggerable
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

    public abstract class NetworkAutoTriggerable : NetworkTriggerable, IMActivateOnReceiver
    {

        #region Fields

        [SerializeField()]
        private ActivateEvent _activateOn = ActivateEvent.None;

        #endregion

        #region Properties

        public ActivateEvent ActivateOn
        {
            get { return _activateOn; }
            set { _activateOn = value; }
        }

        #endregion

        #region IMActivateOnReceiver Interface

        void IMActivateOnReceiver.OnInitMixin()
        {
            NetworkOnActivateReceiverMixinLogic.NetworkOnActivateMixinLogic.Initialize(this);
        }

        void IMActivateOnReceiver.Activate()
        {
            this.Trigger(this, null);
        }

        #endregion

    }

}
