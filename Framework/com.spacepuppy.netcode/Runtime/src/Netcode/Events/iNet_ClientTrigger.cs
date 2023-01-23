using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

using com.spacepuppy.Events;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Netcode.Events
{

    [Infobox("This triggers the targets on all clients connected to the server.")]
    public class iNet_ClientTrigger : NetworkAutoTriggerable
    {


        #region Fields

        [SerializeField()]
        [SPEvent.Config("daisy chained arg (object)")]
        private SPEvent _trigger = new SPEvent("Trigger");

        [SerializeField()]
        private SPTimePeriod _delay = 0f;

        #endregion

        #region Properties

        public SPEvent TriggerEvent
        {
            get
            {
                return _trigger;
            }
        }

        public SPTimePeriod Delay
        {
            get { return _delay; }
            set { _delay = value; }
        }

        #endregion

        #region Methods

        [ClientRpc]
        private void DoTriggerClientRpc()
        {
            this.DoTrigger();
        }

        private void DoTrigger()
        {
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

        #region ITriggerable Interface

        public override bool CanTrigger => base.CanTrigger && this.IsServerOrOffline();

        public override bool Trigger(object sender, object arg)
        {
            if (!base.CanTrigger) return false;

            switch (this.GetNetworkRelationship())
            {
                case NetworkRelationship.Offline:
                    this.DoTrigger();
                    return true;
                case NetworkRelationship.Server:
                case NetworkRelationship.Host:
                    this.DoTriggerClientRpc();
                    return true;
                default:
                    return false;
            }
        }

        #endregion


    }

}
