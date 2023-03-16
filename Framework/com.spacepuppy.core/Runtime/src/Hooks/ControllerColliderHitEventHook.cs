using com.spacepuppy.Utils;
using UnityEngine;

namespace com.spacepuppy.Hooks
{

    public delegate void OnControllerColliderHitCallback(GameObject sender, ControllerColliderHit hit);

    public interface IOnControllerColliderHitSubscriber //subscriber denotes that this message is only invoked by a SubscribableMessageHook
    {
        void OnControllerColliderHit(GameObject sender, ControllerColliderHit hit);
    }

    public sealed class ControllerColliderHitEventHook : Messaging.SubscribableMessageHook<IOnControllerColliderHitSubscriber>
    {

        public event OnControllerColliderHitCallback ControllerColliderHit;

        public bool Preserve { get; set; }
        protected override bool PreserveOnUnsubscribe() => this.Preserve || this.ControllerColliderHit != null;

        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (!this.isActiveAndEnabled) return;

            this.ControllerColliderHit?.Invoke(this.gameObject, hit);
            if (this.SubscriberCount > 0) this.Signal((this.gameObject, hit), (o, a) => o.OnControllerColliderHit(a.gameObject, a.hit));
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            this.ControllerColliderHit = null;
        }

    }
}
