using UnityEngine;
using System.Collections.Generic;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Netcode
{

    public sealed class NetworkOwnerStateMachine : SPNetworkComponent
    {

        #region Fields

        [SerializeField]
        private GameObject _localState;
        [SerializeField]
        private GameObject _remoteState;

        #endregion

        #region CONSTRUCTOR

        protected override void OnStartOrEnableOrNetworkSpawn()
        {
            this.SyncState();
        }

        #endregion

        #region Properties

        public GameObject LocalState
        {
            get => _localState;
            set => _localState = value;
        }

        public GameObject RemoteState
        {
            get => _remoteState;
            set => _remoteState = value;
        }

        #endregion

        #region Methods

        public void SyncState()
        {
            bool islocal = (this.NetworkObject.OwnerClientId == this.NetworkObject.NetworkManager.LocalClientId || this.NetworkObject.IsOffline());
            if (islocal)
            {
                _remoteState.TrySetActive(false);
                _localState.TrySetActive(true);
            }
            else
            {
                _localState.TrySetActive(false);
                _remoteState.TrySetActive(true);
            }
        }

        #endregion

    }

}
