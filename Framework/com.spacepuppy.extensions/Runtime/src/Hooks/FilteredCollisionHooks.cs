using UnityEngine;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Hooks
{

    [Messaging.SubscribableMessageConfig(Precedence = -1000)]
    public class FilteredCollisionHooks : CollisionHooks
    {

        #region Fields

        [HideInInspector]
        [SerializeField]
        private bool _donotPreserve = false;
        [SerializeField]
        private EventActivatorMaskRef _mask = new EventActivatorMaskRef();

        #endregion

        #region Properties

        public bool Preserve
        {
            get => !_donotPreserve;
            set => _donotPreserve = !value;
        }

        public IEventActivatorMask Mask
        {
            get => _mask.Value;
            set => _mask.Value = value;
        }

        #endregion

        #region Methods

        protected override bool PreserveOnUnsubscribe() => !_donotPreserve || base.PreserveOnUnsubscribe();

        protected override void OnCollisionEnter(Collision collision)
        {
            if (!this.isActiveAndEnabled || !(_mask.Value?.Intersects(collision.collider) ?? true)) return;

            base.OnCollisionEnter(collision);
        }

        protected override void OnCollisionExit(Collision collision)
        {
            if (!this.isActiveAndEnabled || !(_mask.Value?.Intersects(collision.collider) ?? true)) return;

            base.OnCollisionExit(collision);
        }

        #endregion

    }
}
