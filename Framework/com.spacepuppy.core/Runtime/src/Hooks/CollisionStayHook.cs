using UnityEngine;
using System.Collections.Generic;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Hooks
{

    public interface IOnCollisionStaySubscriber : IOnCollisionSubscriber
    {
        void OnCollisionStay(GameObject sender, Collision collision);
    }

    public class CollisionStayHooks : CollisionHooks, Messaging.ISubscribableMessageHook<IOnCollisionStaySubscriber>
    {

        public event OnCollisionCallback OnStay;

        protected override bool PreserveOnUnsubscribe() => this.OnStay != null || base.PreserveOnUnsubscribe();

        protected virtual void OnCollisionStay(Collision collision)
        {
            if (!this.isActiveAndEnabled) return;

            this.OnStay?.Invoke(this.gameObject, collision);
            if (this.SubscriberCount > 0) this.Signal((this.gameObject, collision), (o, a) => o.OnCollisionStay(a.gameObject, a.collision));
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            this.OnStay = null;
        }

        #region ISubscribableMessageHook Interface

        public bool Subscribe(IOnCollisionStaySubscriber observer)
        {
            return base.Subscribe((IOnCollisionSubscriber)observer);
        }

        public bool Unsubscribe(IOnCollisionStaySubscriber observer)
        {
            return base.Unsubscribe((IOnCollisionSubscriber)observer);
        }

        public void Signal(System.Action<IOnCollisionStaySubscriber> functor)
        {
            if (functor == null) return;
            var e = this.GetSubscriberEnumerator();
            while (e.MoveNext())
            {
                if (e.Current is IOnCollisionStaySubscriber h) functor(h);
            }
        }

        public void Signal<TArg>(TArg arg, System.Action<IOnCollisionStaySubscriber, TArg> functor)
        {
            if (functor == null) return;
            var e = this.GetSubscriberEnumerator();
            while (e.MoveNext())
            {
                if (e.Current is IOnCollisionStaySubscriber h) functor(h, arg);
            }
        }

        #endregion

    }

}
