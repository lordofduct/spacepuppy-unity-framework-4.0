using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Utils;
using com.spacepuppy.Events;

namespace com.spacepuppy
{

    [Infobox("Assigns 'target' as the proxy target of a ProxyMediator. Anything referencing ProxyMediator as a source will receive this target.")]
    public sealed class i_SyncProxyMediator : AutoTriggerable
    {

        #region Fields

        [SerializeField]
        [DisableOnPlay]
        [UnityEngine.Serialization.FormerlySerializedAs("_proxy")]
        private ProxyMediator _mediator;

        [SerializeField]
        private VariantReference _target = new VariantReference();

        [SerializeField]
        private bool _triggerMediatorOnSync;

        #endregion

        #region Properties

        public VariantReference Target
        {
            get => _target;
            set => _target = value;
        }

        public ProxyMediator Mediator
        {
            get => _mediator;
            set => _mediator = value;
        }

        public bool TriggerMediatorOnSync
        {
            get => _triggerMediatorOnSync;
            set => _triggerMediatorOnSync = value;
        }

        #endregion

        #region Methods

        public void Sync()
        {
            if (_mediator)
            {
                _mediator.SetProxyTarget(IProxyExtensions.ReduceIfProxy(_target.Value));
                if (_triggerMediatorOnSync) _mediator.Trigger(this, null);
            }
        }

        #endregion

        #region ITriggerable Interface

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            this.Sync();
            return true;
        }

        #endregion

    }
}
