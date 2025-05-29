using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

using com.spacepuppy.Utils;
using com.spacepuppy.Events;

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
                if (this.started && (no == null || no.IsSpawned))
                {
                    this.OnStartOrEnableOrNetworkSpawn();
                }
            };
        }

        void OnStartOrEnableOrNetworkSpawn();

    }

    public class NetworkOnActivateReceiverMixinLogic : OnActivateReceiverMixinLogic
    {

        public static readonly NetworkOnActivateReceiverMixinLogic NetworkOnActivateMixinLogic = new NetworkOnActivateReceiverMixinLogic();

        public override void Initialize(IMActivateOnReceiver receiver)
        {
            base.Initialize(receiver);

            if (receiver is SPNetworkComponent spnb)
            {
                if (!spnb.IsSpawned)
                {
                    spnb.OnNetworkSpawned += Target_OnSpawned;
                }
            }
            else if (receiver.gameObject.GetComponentInParent(out NetworkObject nobj))
            {
                if (!nobj.IsSpawned && !receiver.started)
                {
                    nobj.AddOrGetComponent<IMOnNetworkSpawnReceiver.OnNetworkSpawnReceiverHook>().OnSpawned += () => this.Target_OnSpawned(receiver, System.EventArgs.Empty);
                }
            }
        }

        protected override void Target_OnEnabled(object sender, System.EventArgs e)
        {
            var targ = sender as IMActivateOnReceiver;
            if (targ == null || !targ.started) return;

            if ((targ.ActivateOn & ActivateEvent.OnEnable) != 0)
            {
                if (targ.started && this.TestIsSpawned(targ))
                {
                    if (GameLoop.LateUpdateWasCalled)
                    {
                        targ.Activate();
                    }
                    else
                    {
                        GameLoop.LateUpdateHandle.BeginInvoke(targ.Activate);
                    }
                }
            }
        }

        protected override void Target_OnStarted(object sender, System.EventArgs e)
        {
            var targ = sender as IMActivateOnReceiver;
            if (targ == null) return;

            if (this.TestIsSpawned(targ))
            {
                var aoe = targ.ActivateOn;
                if ((aoe & ActivateEvent.OnLateStart) != 0 && !GameLoop.LateUpdateWasCalled)
                {
                    GameLoop.LateUpdateHandle.BeginInvoke(() => targ.Activate());
                }
                else if ((aoe & ActivateEvent.OnStart) != 0 || (aoe & ActivateEvent.OnEnable) != 0)
                {
                    targ.Activate();
                }
            }
        }

        protected virtual void Target_OnSpawned(object sender, System.EventArgs e)
        {
            var targ = sender as IMActivateOnReceiver;
            if (targ == null) return;

            if (targ.started)
            {
                var aoe = targ.ActivateOn;
                if ((aoe & ActivateEvent.OnLateStart) != 0 && !GameLoop.LateUpdateWasCalled)
                {
                    GameLoop.LateUpdateHandle.BeginInvoke(() => targ.Activate());
                }
                else if ((aoe & ActivateEvent.OnStart) != 0 || (aoe & ActivateEvent.OnEnable) != 0)
                {
                    targ.Activate();
                }
            }
        }

        protected bool TestIsSpawned(IMActivateOnReceiver receiver)
        {
            if (receiver is NetworkBehaviour nb)
            {
                return nb.IsSpawned;
            }
            else
            {
                return !receiver.gameObject.GetComponentInParent(out NetworkObject nobj) || nobj.IsSpawned;
            }
        }

    }

}
