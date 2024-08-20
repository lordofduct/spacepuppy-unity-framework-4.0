using UnityEngine;
using Unity.Netcode;

using com.spacepuppy.Utils;

namespace com.spacepuppy.Netcode
{

    [MOnNetworkSpawnReceiver]
    public interface IMOnNetworkSpawnReceiver : IAutoMixinDecorator, IComponent
    {

        void OnNetworkSpawn();

    }
    internal class MOnNetworkSpawnReceiverAttribute : StatelessAutoMixinConfigAttribute
    {
        protected override void OnAutoCreated(object obj, System.Type mixinType)
        {
            var c = obj as IMOnNetworkSpawnReceiver;
            if (c == null || c.gameObject == null) return;

            var no = c.gameObject.GetComponentInParent<NetworkObject>();
            if (no == null || no.IsSpawned) return;

            no.AddOrGetComponent<OnNetworkSpawnReceiverHook>();
        }

        private class OnNetworkSpawnReceiverHook : NetworkBehaviour
        {
            public override void OnNetworkSpawn()
            {
                base.OnNetworkSpawn();

                this.gameObject.Broadcast<IMOnNetworkSpawnReceiver>(o => o.OnNetworkSpawn());
                //DO NOT DESTROY, it messes with netcode for gameobject's logic
            }
        }

    }

}
