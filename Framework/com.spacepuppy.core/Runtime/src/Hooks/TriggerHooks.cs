using com.spacepuppy.Utils;
using UnityEngine;

namespace com.spacepuppy.Hooks
{

    public delegate void OnTriggerCallback(GameObject sender, Collider otherCollider);

    public interface IOnTriggerSubscriber //subscriber denotes that this message is only invoked by a SubscribableMessageHook
    {
        void OnTriggerEnter(GameObject sender, Collider otherCollider);
        void OnTriggerExit(GameObject sender, Collider otherCollider);
    }

    public class TriggerHooks : Messaging.SubscribableMessageHook<IOnTriggerSubscriber>
    {
        public event OnTriggerCallback OnEnter;
        public event OnTriggerCallback OnExit;

        public virtual bool Preserve { get; set; }
        protected override bool PreserveOnUnsubscribe() => this.Preserve || this.OnEnter != null || this.OnExit != null;

        protected virtual void OnTriggerEnter(Collider otherCollider)
        {
            if (!this.isActiveAndEnabled) return;

            this.OnEnter?.Invoke(this.gameObject, otherCollider);
            if (this.SubscriberCount > 0) this.Signal((this.gameObject, otherCollider), (o, a) => o.OnTriggerEnter(a.gameObject, a.otherCollider));
        }

        protected virtual void OnTriggerExit(Collider otherCollider)
        {
            if (!this.isActiveAndEnabled) return;

            this.OnExit?.Invoke(this.gameObject, otherCollider);
            if (this.SubscriberCount > 0) this.Signal((this.gameObject, otherCollider), (o, a) => o.OnTriggerExit(a.gameObject, a.otherCollider));
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            this.OnEnter = null;
            this.OnExit = null;
        }

    }

}
