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

        [SerializeField()]
        [SPEvent.Config("daisy chained arg (object)")]
        private SPEvent _trigger = new SPEvent("Trigger");

        [SerializeField]
        private ProxyMediator _mediator;

        [SerializeField]
        private bool _useProxyMediatorAsDaisyChainedArg;

        #region CONSTRUCTOR

        protected override void OnEnable()
        {
            base.OnEnable();

            if (_mediator != null)
            {
                _mediator.OnTriggered -= this.OnMediatorTriggeredCallback;
                _mediator.OnTriggered += this.OnMediatorTriggeredCallback;
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (!object.ReferenceEquals(_mediator, null))
            {
                _mediator.OnTriggered -= this.OnMediatorTriggeredCallback;
            }
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

                if (Application.isPlaying && this.enabled)
                {
                    if (!object.ReferenceEquals(_mediator, null)) _mediator.OnTriggered -= this.OnMediatorTriggeredCallback;
                    _mediator = value;
                    if (_mediator != null)
                    {
                        _mediator.OnTriggered -= this.OnMediatorTriggeredCallback;
                        _mediator.OnTriggered += this.OnMediatorTriggeredCallback;
                    }
                }
                else
                {
                    _mediator = value;
                }
            }
        }

        #endregion

        #region Methods

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
