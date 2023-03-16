using UnityEngine;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Hooks
{

    public delegate void OnCollisionCallback(GameObject sender, Collision collision);

    public interface IOnCollisionSubscriber //subscriber denotes that this message is only invoked by a SubscribableMessageHook
    {
        void OnCollisionEnter(GameObject sender, Collision collision);
        void OnCollisionExit(GameObject sender, Collision collision);
    }

    public class CollisionHooks : Messaging.SubscribableMessageHook<IOnCollisionSubscriber>
    {
        public event OnCollisionCallback OnEnter;
        public event OnCollisionCallback OnExit;

        public bool Preserve { get; set; }
        protected override bool PreserveOnUnsubscribe() => this.Preserve || this.OnEnter != null || this.OnExit != null;

        protected virtual void OnCollisionEnter(Collision collision)
        {
            if (!this.isActiveAndEnabled) return;

            this.OnEnter?.Invoke(this.gameObject, collision);
            if (this.SubscriberCount > 0) this.Signal((this.gameObject, collision), (o, a) => o.OnCollisionEnter(a.gameObject, a.collision));
        }

        protected virtual void OnCollisionExit(Collision collision)
        {
            if (!this.isActiveAndEnabled) return;

            this.OnExit?.Invoke(this.gameObject, collision);
            if (this.SubscriberCount > 0) this.Signal((this.gameObject, collision), (o, a) => o.OnCollisionExit(a.gameObject, a.collision));
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            this.OnEnter = null;
            this.OnExit = null;
        }

    }

}
