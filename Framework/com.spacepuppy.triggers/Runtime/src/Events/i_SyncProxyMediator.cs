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

        public enum Modes : sbyte
        {
            Clear = -1,
            Set = 0,
            Push = 1,
            Pop = 2,
        }

        #region Fields

        [SerializeField]
        private Modes _mode;

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

        public Modes Mode
        {
            get => _mode;
            set => _mode = value;
        }

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
                switch (_mode)
                {
                    case Modes.Clear:
                        _mediator.SetProxyTarget(null);
                        break;
                    case Modes.Set:
                        _mediator.SetProxyTarget(IProxyExtensions.ReduceIfProxy(_target.GetTarget(typeof(object), arg)));
                        break;
                    case Modes.Push:
                        {
                            var targ = IProxyExtensions.ReduceIfProxy(_target.GetTarget(typeof(object), arg));
                            if (targ != null) _mediator.PushProxyTarget(targ);
                        }
                        break;
                    case Modes.Pop:
                        _mediator.PopProxyTarget();
                        break;

                }
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
