using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

using com.spacepuppy;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Netcode
{

    [Infobox("The 'active/enabled' state of this GameObject will be synced from server to client.\r\n\r\nNote that the component has to be enabled on start to initiate itself and receive messages.")]
    public sealed class NetworkEnabledGameObject : SPNetworkComponent
    {

        #region Fields

        [SerializeField]
        private bool _disableOnStart;

        #endregion

        #region CONSTRUCTOR

        protected override void OnStartOrNetworkSpawn()
        {
            base.OnStartOrNetworkSpawn();

            if (this.IsServer && this.enabled)
            {
                this.SyncStateClientRpc(!_disableOnStart && this.gameObject.activeSelf);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (!this.started || !this.IsSpawned) return;

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
        private void SyncStateClientRpc(bool state)
        {
            this.gameObject.SetActive(state);
        }

        #endregion

    }

}
