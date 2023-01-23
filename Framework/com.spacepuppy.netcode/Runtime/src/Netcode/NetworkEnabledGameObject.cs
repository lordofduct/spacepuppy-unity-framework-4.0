using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

using com.spacepuppy;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Netcode
{

    [Infobox("The 'active/enabled' state of this GameObject will be synced from server to client.")]
    public sealed class NetworkEnabledGameObject : SPNetworkComponent, IMStartOrEnableReceiver
    {

        #region Fields



        #endregion

        #region CONSTRUCTOR

        void IMStartOrEnableReceiver.OnStartOrEnable()
        {
            if (this.IsServer && this.enabled)
            {
                this.SyncStateClientRpc(this.gameObject.activeSelf);
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (this.IsServer && !this.IsDestroyed() && this.enabled)
            {
                this.SyncStateClientRpc(this.gameObject.activeSelf);
            }
        }

        #endregion

        #region Methods

        [ClientRpc]
        private void SyncStateClientRpc(bool active)
        {
            if (this.IsServer) return;

            this.gameObject.SetActive(active);
        }

        #endregion

    }

}
