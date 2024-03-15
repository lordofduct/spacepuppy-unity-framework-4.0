#pragma warning disable 0168, 0649 // variable declared but not used.

using UnityEngine;

using com.spacepuppy.Utils;

namespace com.spacepuppy.Events
{

    public class t_OnCollisionEnter : SPComponent, IObservableTrigger
    {

        #region Fields

        [SerializeField]
        private EventActivatorMaskRef _mask;
        [SerializeField]
        private float _cooldownInterval = 1.0f;
        [SerializeField]
        private bool _includeColliderAsTriggerArg = true;

        [SerializeField()]
        private SPEvent _trigger = new SPEvent();

        [System.NonSerialized()]
        private bool _coolingDown;

        #endregion

        #region CONSTRUCTOR

        protected override void OnDisable()
        {
            base.OnDisable();

            _coolingDown = false;
        }

        #endregion

        #region Properties

        public IEventActivatorMask Mask
        {
            get { return _mask.Value; }
            set { _mask.Value = value; }
        }

        public float CooldownInterval
        {
            get { return _cooldownInterval; }
            set { _cooldownInterval = value; }
        }

        public bool IncludeCollidersAsTriggerArg
        {
            get { return _includeColliderAsTriggerArg; }
            set { _includeColliderAsTriggerArg = value; }
        }

        public SPEvent Trigger => _trigger;

        #endregion

        #region Methods

        private void OnCollisionEnter(Collision c)
        {
            if (!this.isActiveAndEnabled || _coolingDown) return;

            if (_mask.Value == null || _mask.Value.Intersects(c.collider))
            {
                if (_includeColliderAsTriggerArg)
                {
                    _trigger.ActivateTrigger(this, c.collider);
                }
                else
                {
                    _trigger.ActivateTrigger(this, null);
                }

                _coolingDown = true;
                //use global incase this gets disabled
                this.Invoke(() =>
                {
                    _coolingDown = false;
                }, _cooldownInterval);
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
