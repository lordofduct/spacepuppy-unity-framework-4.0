#pragma warning disable 0649 // variable declared but not used.

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace com.spacepuppy.Events
{

    public class i_TriggerGated : AutoTriggerable, IObservableTrigger
    {

        #region Fields

        [SerializeField()]
        private SPEvent _trigger;

        [SerializeField()]
        private bool _passAlongTriggerArg;

        [SerializeField()]
        private SPTimePeriod _gateDelay = 0f;

        [SerializeField]
        [DisableOnPlay]
        [OneOrMany()]
        private List<ProxyMediator> _activateGateSilentlyMediator;

        [System.NonSerialized]
        private MediatorCollection _mediatorColl;
        [System.NonSerialized]
        private double? _lastTimeGated = null;

        #endregion

        #region CONSTRUCTOR

        protected override void Awake()
        {
            base.Awake();

            if(_activateGateSilentlyMediator != null && _activateGateSilentlyMediator.Count > 0)
            {
                _mediatorColl = new MediatorCollection(this);
            }
        }

        #endregion

        #region Properties

        public SPEvent TriggerEvent
        {
            get
            {
                return _trigger;
            }
        }

        public bool PassAlongTriggerArg
        {
            get { return _passAlongTriggerArg; }
            set { _passAlongTriggerArg = value; }
        }

        /// <summary>
        /// The delay after being gated before allowing to be triggered again.
        /// Setting this while gated reopens the gate.
        /// </summary>
        public SPTimePeriod GateDelay
        {
            get { return _gateDelay; }
            set
            {
                if(this.IsGated)
                {
                    _lastTimeGated = null;
                }
                _gateDelay = value;
            }
        }

        public ICollection<ProxyMediator> ActivateGateSilentlyMediators
        {
            get
            {
                return _mediatorColl ?? (_mediatorColl = new MediatorCollection(this));
            }
        }

        public bool IsGated
        {
            get
            {
                return _lastTimeGated.HasValue && !_gateDelay.Elapsed(_lastTimeGated.Value);
            }
        }

        #endregion

        #region Methods

        public void ActivateGate()
        {
            _lastTimeGated = _gateDelay.TimeSupplier?.TotalPrecise;
        }

        #endregion

        #region ITriggerableMechanism Interface

        public override bool CanTrigger => base.CanTrigger && !this.IsGated;

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            this.ActivateGate();
            if (this._passAlongTriggerArg)
                _trigger.ActivateTrigger(this, arg);
            else
                _trigger.ActivateTrigger(this, null);

            return true;
        }

        #endregion

        #region IObservableTrigger Interface

        BaseSPEvent[] IObservableTrigger.GetEvents()
        {
            return new BaseSPEvent[] { _trigger };
        }

        #endregion

        #region Special Types

        private class MediatorCollection : ICollection<ProxyMediator>
        {

            private i_TriggerGated _owner;

            public MediatorCollection(i_TriggerGated owner)
            {
                _owner = owner;
                this.RegisterListeners();
            }

            public void RegisterListeners()
            {
                if (_owner._activateGateSilentlyMediator == null || _owner._activateGateSilentlyMediator.Count == 0) return;

                var e = _owner._activateGateSilentlyMediator.GetEnumerator();
                while(e.MoveNext())
                {
                    e.Current.OnTriggered -= OnMediatorTriggered;
                    e.Current.OnTriggered += OnMediatorTriggered;
                }
            }

            private void OnMediatorTriggered(object sender, System.EventArgs e)
            {
                _owner.ActivateGate();
            }

            #region ICollection Interface

            public int Count => _owner._activateGateSilentlyMediator?.Count ?? 0;

            bool ICollection<ProxyMediator>.IsReadOnly => false;

            public void Add(ProxyMediator item)
            {
                if (_owner._activateGateSilentlyMediator.Contains(item)) return;

                _owner._activateGateSilentlyMediator.Add(item);
                item.OnTriggered += OnMediatorTriggered;
            }

            public void Clear()
            {
                if (_owner._activateGateSilentlyMediator.Count == 0) return;

                var e = _owner._activateGateSilentlyMediator.GetEnumerator();
                while (e.MoveNext())
                {
                    e.Current.OnTriggered -= OnMediatorTriggered;
                }
                _owner._activateGateSilentlyMediator.Clear();
            }

            public bool Contains(ProxyMediator item)
            {
                return _owner._activateGateSilentlyMediator.Contains(item);
            }

            public void CopyTo(ProxyMediator[] array, int arrayIndex)
            {
                _owner._activateGateSilentlyMediator.CopyTo(array, arrayIndex);
            }

            public bool Remove(ProxyMediator item)
            {
                if(_owner._activateGateSilentlyMediator.Remove(item))
                {
                    item.OnTriggered -= OnMediatorTriggered;
                    return true;
                }

                return false;
            }

            public IEnumerator<ProxyMediator> GetEnumerator()
            {
                return _owner._activateGateSilentlyMediator.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return _owner._activateGateSilentlyMediator.GetEnumerator();
            }

            #endregion

        }

        #endregion

    }

}
