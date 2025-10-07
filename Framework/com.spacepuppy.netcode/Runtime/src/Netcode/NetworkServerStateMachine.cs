using UnityEngine;
using System.Collections.Generic;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Netcode
{

    public class NetworkServerStateMachine : SPNetworkComponent
    {

        #region Fields

        [SerializeField, Tooltip("Is considered server if 'offline'.")]
        private GameObject _serverState;
        [SerializeField]
        private GameObject _clientState;

        #endregion

        #region CONSTRUCTOR

        protected override void OnStartOrEnableOrNetworkSpawn()
        {
            this.SyncState();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            this.SyncState();
        }

        #endregion

        #region Properties

        public GameObject ServerState
        {
            get => _serverState;
            set => _serverState = value;
        }

        public GameObject ClientState
        {
            get => _clientState;
            set => _clientState = value;
        }

        #endregion

        #region Methods

        public void SyncState()
        {
            bool isserver = this.IsServerOrOffline();
            if (isserver)
            {
                _clientState.TrySetActive(false);
                _serverState.TrySetActive(true);
            }
            else
            {
                _serverState.TrySetActive(false);
                _clientState.TrySetActive(true);
            }
        }

        #endregion

    }

}
