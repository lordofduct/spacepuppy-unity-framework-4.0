using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy.Dynamic;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Events
{

    /// <summary>
    /// Trigger that responds to the ProxyMediator.OnTriggered event.
    /// 
    /// Associate a ProxyMediator to facilitate communication between transient assets like prefab instances.
    /// </summary>
    public class t_OnProxyMediatorTriggered : SPComponent, IObservableTrigger
    {

        #region Fields

        [SerializeField()]
        [SPEvent.Config("daisy chained arg (object)")]
        private SPEvent _trigger = new SPEvent("Trigger");

        [SerializeField]
        private ProxyMediator _mediator;

        [SerializeField]
        private bool _useProxyMediatorAsDaisyChainedArg;

        [System.NonSerialized]
        private TrackedEventListenerToken<TempEventArgs> _onTriggerHook;

        #endregion

        #region CONSTRUCTOR

        protected override void OnEnable()
        {
            base.OnEnable();

            _onTriggerHook.Dispose();
            if (_mediator != null) _onTriggerHook = _mediator.AddTrackedOnTriggeredListener(this.OnMediatorTriggeredCallback);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            _onTriggerHook.Dispose();
        }

        #endregion

        #region Properties

        public SPEvent Trigger => _trigger;

        public ProxyMediator Mediator
        {
            get { return _mediator; }
            set
            {
                if (_mediator == value) return;

#if UNITY_EDITOR
                if (Application.isPlaying && this.isActiveAndEnabled)
#else
                if (this.isActiveAndEnabled)
#endif
                {
                    _onTriggerHook.Dispose();
                    _mediator = value;
                    if (_mediator != null) _onTriggerHook = _mediator.AddTrackedOnTriggeredListener(this.OnMediatorTriggeredCallback);
                }
                else
                {
                    _mediator = value;
                }
            }
        }

        public bool UseProxyMediatorAsDaisyChainedArg
        {
            get => _useProxyMediatorAsDaisyChainedArg;
            set => _useProxyMediatorAsDaisyChainedArg = value;
        }

        #endregion

        #region Methods

        public void Signal(object tempEventArgValue = null)
        {
            _trigger.ActivateTrigger(this, _useProxyMediatorAsDaisyChainedArg ? _mediator : tempEventArgValue);
        }

        private System.EventHandler<TempEventArgs> _onMediatorTriggeredCallback;
        private System.EventHandler<TempEventArgs> OnMediatorTriggeredCallback
        {
            get
            {
                if (_onMediatorTriggeredCallback == null) _onMediatorTriggeredCallback = this.ReceivedMediatorTriggeredMessage;
                return _onMediatorTriggeredCallback;
            }
        }
        protected virtual void ReceivedMediatorTriggeredMessage(object sender, TempEventArgs e)
        {
            _trigger.ActivateTrigger(this, _useProxyMediatorAsDaisyChainedArg ? _mediator : e?.Value);
        }

        #endregion

        #region IObservableTrigger Interface

        BaseSPEvent[] IObservableTrigger.GetEvents()
        {
            return new BaseSPEvent[] { _trigger };
        }

        #endregion

    }

}
