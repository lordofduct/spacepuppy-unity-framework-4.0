using UnityEngine;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Hooks
{

    [Messaging.SubscribableMessageConfig(Precedence = -1000)]
    public class FilteredTriggerStayHooks : TriggerStayHooks
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

        protected override void OnTriggerEnter(Collider otherCollider)
        {
            if (!this.isActiveAndEnabled || !(_mask.Value?.Intersects(otherCollider) ?? true)) return;

            base.OnTriggerEnter(otherCollider);
        }

        protected override void OnTriggerStay(Collider otherCollider)
        {
            if (!this.isActiveAndEnabled || !(_mask.Value?.Intersects(otherCollider) ?? true)) return;

            base.OnTriggerStay(otherCollider);
        }

        protected override void OnTriggerExit(Collider otherCollider)
        {
            if (!this.isActiveAndEnabled || !(_mask.Value?.Intersects(otherCollider) ?? true)) return;

            base.OnTriggerExit(otherCollider);
        }

        #endregion

    }
}
