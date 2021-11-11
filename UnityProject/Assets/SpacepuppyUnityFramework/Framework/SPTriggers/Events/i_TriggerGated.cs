#pragma warning disable 0649 // variable declared but not used.

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Utils;

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
        [OneOrMany]
        private ProxyMediator[] _activateGateSilentlyMediator;

        [System.NonSerialized]
        private MediatorCollection _mediatorColl;
        [System.NonSerialized]
        private double? _lastTimeGated = null;

        #endregion

        #region CONSTRUCTOR

        protected override void Awake()
        {
            base.Awake();

            if (_activateGateSilentlyMediator != null && _activateGateSilentlyMediator.Length > 0)
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

        [ShowNonSerializedProperty("Runtime Activate Gate Silently Mediators", Readonly = true)]
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
            private HashSet<ProxyMediator> _mediators;

            public MediatorCollection(i_TriggerGated owner)
            {
                _owner = owner;
                _mediators = new HashSet<ProxyMediator>();
                if (owner._activateGateSilentlyMediator != null)
                {
                    foreach (var m in owner._activateGateSilentlyMediator)
                    {
                        if (m != null) this.Add(m);
                    }
                    owner._activateGateSilentlyMediator = null;
                }
            }

            private void OnMediatorTriggered(object sender, System.EventArgs e)
            {
                _owner.ActivateGate();
            }

            #region ICollection Interface

            public int Count => _mediators.Count;

            bool ICollection<ProxyMediator>.IsReadOnly => false;

            public void Add(ProxyMediator item)
            {
                if (item == null) return;
                if (_mediators.Contains(item)) return;

                if(_mediators.Add(item))
                {
                    item.OnTriggered += OnMediatorTriggered;
                }
            }

            public void Clear()
            {
                if (_mediators.Count == 0) return;

                var e = _mediators.GetEnumerator();
                while (e.MoveNext())
                {
                    if (e.Current != null) e.Current.OnTriggered -= OnMediatorTriggered;
                }
                _mediators.Clear();
            }

            public bool Contains(ProxyMediator item)
            {
                if (object.ReferenceEquals(item, null)) return false;

                return _mediators.Contains(item);
            }

            public void CopyTo(ProxyMediator[] array, int arrayIndex)
            {
                _mediators.CopyTo(array, arrayIndex);
            }

            public bool Remove(ProxyMediator item)
            {
                if (object.ReferenceEquals(item, null)) return false;

                if(_mediators.Remove(item))
                {
                    item.OnTriggered -= OnMediatorTriggered;
                    return true;
                }

                return false;
            }

            public IEnumerator<ProxyMediator> GetEnumerator()
            {
                return _mediators.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return _mediators.GetEnumerator();
            }

            #endregion

        }

        #endregion

    }

}
