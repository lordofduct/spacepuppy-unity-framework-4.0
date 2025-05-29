using UnityEngine;
using System.Collections.Generic;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Hooks
{

    public interface IOnTriggerStaySubscriber : IOnTriggerSubscriber
    {
        void OnTriggerStay(GameObject sender, Collider otherCollider);
    }

    //NOTE - due to how OnXXXStay works if you unregister then reregister mid 'stay', that collision will stop firing until you exit and re-enter it.
    public class TriggerStayHooks : TriggerHooks, Messaging.ISubscribableMessageHook<IOnTriggerStaySubscriber>
    {

        public event OnTriggerCallback OnStay;

        protected override bool PreserveOnUnsubscribe() => this.OnStay != null || base.PreserveOnUnsubscribe();

        protected virtual void OnTriggerStay(Collider otherCollider)
        {
            if (!this.isActiveAndEnabled) return;

            this.OnStay?.Invoke(this.gameObject, otherCollider);
            if (this.SubscriberCount > 0) this.Signal((this.gameObject, otherCollider), (o, a) => o.OnTriggerStay(a.gameObject, a.otherCollider));
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            this.OnStay = null;
        }

        #region ISubscribableMessageHook Interface

        public bool Subscribe(IOnTriggerStaySubscriber observer)
        {
            return base.Subscribe((IOnTriggerSubscriber)observer);
        }

        public bool Unsubscribe(IOnTriggerStaySubscriber observer)
        {
            return base.Unsubscribe((IOnTriggerSubscriber)observer);
        }

        public void Signal(System.Action<IOnTriggerStaySubscriber> functor)
        {
            if (functor == null) return;
            try
            {
                this.LockObservers();
                var e = this.GetSubscriberEnumerator();
                while (e.MoveNext())
                {
                    if (e.Current is IOnTriggerStaySubscriber h) functor(h);
                }
            }
            finally
            {
                this.UnlockObservers();
                this.ValidateContinueUpdateLoop();
            }
        }

        public void Signal<TArg>(TArg arg, System.Action<IOnTriggerStaySubscriber, TArg> functor)
        {
            if (functor == null) return;
            try
            {
                this.LockObservers();
                var e = this.GetSubscriberEnumerator();
                while (e.MoveNext())
                {
                    if (e.Current is IOnTriggerStaySubscriber h) functor(h, arg);
                }
            }
            finally
            {
                this.UnlockObservers();
                this.ValidateContinueUpdateLoop();
            }
        }

        #endregion

    }

}
