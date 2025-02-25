using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

using com.spacepuppy.Utils;
using com.spacepuppy.Collections;

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

            no.AddOrGetComponent<OnNetworkSpawnReceiverHook>().RegisterReceiver(c);
        }

        private class OnNetworkSpawnReceiverHook : NetworkBehaviour
        {

            private HashSet<IMOnNetworkSpawnReceiver> receivers = new();
            internal void RegisterReceiver(IMOnNetworkSpawnReceiver r)
            {
                if (this.IsSpawned) return;
                receivers.Add(r);
            }

            public override void OnNetworkSpawn()
            {
                base.OnNetworkSpawn();

                var e = this.receivers.GetEnumerator();
                while (e.MoveNext())
                {
                    e.Current.OnNetworkSpawn();
                }
                receivers.Clear();

                //DO NOT DESTROY, it messes with netcode for gameobject's logic
            }
        }

    }

}
