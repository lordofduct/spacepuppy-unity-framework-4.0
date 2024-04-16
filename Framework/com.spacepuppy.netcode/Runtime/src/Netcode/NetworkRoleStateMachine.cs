using UnityEngine;
using System.Collections.Generic;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Netcode
{

    [Infobox("Server and Host aren't necessariliy the same thing depending your setup. If your setup has server and host being the same role, assign the same state object to both.")]
    public sealed class NetworkRoleStateMachine : SPNetworkComponent, IMStartOrEnableReceiver
    {

        #region Fields

        [SerializeField]
        private GameObject _offlineState;
        [SerializeField]
        private GameObject _serverState;
        [SerializeField]
        private GameObject _hostState;
        [SerializeField]
        private GameObject _clientState;

        #endregion

        #region CONSTRUCTOR

        void IMStartOrEnableReceiver.OnStartOrEnable()
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

        public GameObject OfflineState
        {
            get => _offlineState;
            set => _offlineState = value;
        }

        public GameObject ServerState
        {
            get => _serverState;
            set => _serverState = value;
        }

        public GameObject HostState
        {
            get => _hostState;
            set => _hostState = value;
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
            switch (this.GetNetworkRelationship())
            {
                case NetworkRelationship.Offline:
                    this.ToggleState(_offlineState);
                    break;
                case NetworkRelationship.Server:
                    this.ToggleState(_serverState);
                    break;
                case NetworkRelationship.Host:
                    this.ToggleState(_hostState);
                    break;
                case NetworkRelationship.Client:
                case NetworkRelationship.ConnectedClient:
                    this.ToggleState(_clientState);
                    break;
            }
        }

        void ToggleState(GameObject current)
        {
            if (_offlineState != current) _offlineState.TrySetActive(false);
            if (_serverState != current) _serverState.TrySetActive(false);
            if (_hostState != current) _hostState.TrySetActive(false);
            if (_clientState != current) _clientState.TrySetActive(false);
            current.TrySetActive(true);
        }

        #endregion

    }

}
