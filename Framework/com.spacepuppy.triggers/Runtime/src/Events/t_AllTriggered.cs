#pragma warning disable 0649 // variable declared but not used.

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace com.spacepuppy.Events
{

    public sealed class t_AllTriggered : SPComponent, IObservableTrigger, IMStartOrEnableReceiver
    {

        #region Fields

        [SerializeField()]
        [ReorderableArray()]
        [DisableOnPlay()]
        private List<ObservableTargetData> _observedTargets;

        [SerializeField()]
        [OnChangedInEditor("ResetOnTriggeredChanged", OnlyAtRuntime = true)]
        [Tooltip("After the obvserved targets all signal and this signals in turn, should it reset and start listening again.")]
        private bool _resetOnTriggered;

        [SerializeField()]
        private SPEvent _trigger = new SPEvent();

        [System.NonSerialized()]
        private HashSet<IObservableTrigger> _activatedTriggers = new HashSet<IObservableTrigger>();
        [System.NonSerialized()]
        private bool _triggered;

        #endregion

        #region CONSTRUCTOR

        void IMStartOrEnableReceiver.OnStartOrEnable()
        {
            _activatedTriggers.Clear();
            this.RegisterListeners();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            this.UnRegisterListeners();
            _activatedTriggers.Clear();
        }

        #endregion

        #region Properties

        public bool ResetOnTriggered
        {
            get { return _resetOnTriggered; }
            set
            {
                if (_resetOnTriggered == value) return;

                _resetOnTriggered = value;
                this.ResetOnTriggeredChanged();
            }
        }

        #endregion

        #region Methods

        private void ResetOnTriggeredChanged()
        {
            if (_resetOnTriggered && _triggered && this.isActiveAndEnabled)
            {
                _triggered = false;
                this.RegisterListeners();
            }
        }

        private void RegisterListeners()
        {
            if (_triggered) return;

            ObservableTargetData targ;
            var d = new System.EventHandler<TempEventArgs>(this.OnTriggerActivated);
            for (int i = 0; i < _observedTargets.Count; i++)
            {
                targ = _observedTargets[i];
                if (targ != null)
                {
                    targ.Init();
                    targ.TriggerActivated += d;
                }
            }
        }

        private void UnRegisterListeners()
        {
            ObservableTargetData targ;
            var d = new System.EventHandler<TempEventArgs>(this.OnTriggerActivated);
            for (int i = 0; i < _observedTargets.Count; i++)
            {
                targ = _observedTargets[i];
                if (targ != null)
                {
                    targ.TriggerActivated -= d;
                    targ.DeInit();
                }
            }
        }

        private void OnTriggerActivated(object sender, TempEventArgs e)
        {
            if (_triggered) return;

            var targ = sender as IObservableTrigger;
            if (targ != null) _activatedTriggers.Add(targ);

            bool all = true;
            foreach (var o in _observedTargets)
            {
                if (!_activatedTriggers.Contains(o.Target))
                {
                    all = false;
                    return;
                }
            }

            if (all)
            {
                _activatedTriggers.Clear();
                if (this._resetOnTriggered)
                {
                    _triggered = false;
                }
                else
                {
                    _triggered = true;
                    this.UnRegisterListeners();
                }
                _trigger.ActivateTrigger(this, null);
            }
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
