using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Utils;
using com.spacepuppy.Events;

namespace com.spacepuppy
{

    [Infobox("Assigns 'target' as the proxy target of a ProxyMediator. Anything referencing ProxyMediator as a source will receive this target.\r\nIf 'TriggerMediatorOnSync' is true, the trigger arg is daisy chained.")]
    public sealed class i_SyncProxyMediator : AutoTriggerable
    {

        #region Fields

        [SerializeField]
        [DisableOnPlay]
        [UnityEngine.Serialization.FormerlySerializedAs("_proxy")]
        private ProxyMediator _mediator;

        [SerializeField]
        private TriggerableTargetObject _target = new TriggerableTargetObject();

        [SerializeField]
        private bool _triggerMediatorOnSync;

        #endregion

        #region Properties

        public TriggerableTargetObject Target
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

        public void Sync() => this.Sync(null);
        public void Sync(object arg)
        {
            if (_mediator)
            {
                _mediator.SetProxyTarget(IProxyExtensions.ReduceIfProxy(_target.GetTarget(typeof(object), arg)));
                if (_triggerMediatorOnSync) _mediator.Trigger(this, arg);
            }
        }

        #endregion

        #region ITriggerable Interface

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            this.Sync(arg);
            return true;
        }

        #endregion

    }
}
