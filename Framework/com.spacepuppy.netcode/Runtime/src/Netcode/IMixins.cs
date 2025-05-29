using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

using com.spacepuppy.Utils;

namespace com.spacepuppy.Netcode
{

    [AutoInitMixin]
    public interface IMOnNetworkSpawnReceiver : IMixin, IComponent
    {

        sealed void OnInitMixin()
        {
            if (this.gameObject == null) return;

            var no = this.gameObject.GetComponentInParent<NetworkObject>();
            if (no == null || no.IsSpawned) return;

            no.AddOrGetComponent<OnNetworkSpawnReceiverHook>().OnSpawned += this.OnNetworkSpawn;
        }

        void OnNetworkSpawn();


        internal class OnNetworkSpawnReceiverHook : NetworkBehaviour
        {

            public System.Action OnSpawned;

            public override void OnNetworkSpawn()
            {
                base.OnNetworkSpawn();

                this.OnSpawned?.Invoke();
                this.OnSpawned = null;

                //DO NOT DESTROY, it messes with netcode for gameobject's logic
            }
        }

    }

    [AutoInitMixin]
    public interface IMStartOrEnableOrNetworkSpawnReceiver : IMixin, IEventfulComponent
    {

        sealed void OnInitMixin()
        {
            if (this.gameObject == null) return;

            var no = this.gameObject.GetComponentInParent<NetworkObject>();
            if (no && !no.IsSpawned)
            {
                no.AddOrGetComponent<IMOnNetworkSpawnReceiver.OnNetworkSpawnReceiverHook>().OnSpawned += () =>
                {
                    if (this.started)
                    {
                        this.OnStartOrEnableOrNetworkSpawn();
                    }
                };
            }

            this.OnStarted += (s, e) =>
            {
                if (no == null || no.IsSpawned)
                {
                    this.OnStartOrEnableOrNetworkSpawn();
                }
            };
            this.OnEnabled += (s, e) =>
            {
                if (this.started)
                {
                    this.OnStartOrEnableOrNetworkSpawn();
                }
            };
        }

        void OnStartOrEnableOrNetworkSpawn();

    }

}
