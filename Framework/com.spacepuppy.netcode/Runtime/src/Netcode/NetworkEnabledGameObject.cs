using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

using com.spacepuppy;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Netcode
{

    [Infobox("The 'active/enabled' state of this GameObject will be synced from server to client.\r\n\r\nNote that the component has to be enabled on start to initiate itself and receive messages.")]
    public sealed class NetworkEnabledGameObject : SPNetworkComponent, IMStartOrEnableReceiver
    {

        #region Fields



        #endregion

        #region CONSTRUCTOR

        void IMStartOrEnableReceiver.OnStartOrEnable()
        {
            if (this.IsServer && this.enabled)
            {
                this.SyncState(this.gameObject.activeSelf);
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (this.IsServer && !this.IsDestroyed() && this.enabled)
            {
                this.SyncState(this.gameObject.activeSelf);
            }
        }

        #endregion

        #region Methods

        private void SyncState(bool state)
        {
            if (state)
                this.SyncActiveClientRpc();
            else
                this.SyncInactiveClientRpc();
        }

        [ClientRpc]
        private void SyncActiveClientRpc()
        {
            if (this.IsServer) return;

            this.gameObject.SetActive(true);
        }

        [ClientRpc]
        private void SyncInactiveClientRpc()
        {
            if (this.IsServer) return;

            this.gameObject.SetActive(false);
        }

        #endregion

    }

}
