using UnityEngine;
using System.Collections.Generic;

namespace com.spacepuppy.Events
{

    public sealed class t_OnMove : SPComponent, IObservableTrigger
    {

        #region Fields

        [SerializeField]
        private float _delta = 1f;

        [SerializeField]
        private SPEvent _onMoved = new SPEvent("OnMoved");

        [System.NonSerialized]
        private Vector3 _lastPosition;
        [System.NonSerialized]
        private float _deltaSquared;

        #endregion

        #region CONSTRUCTOR

        protected override void OnEnable()
        {
            base.OnEnable();

            _lastPosition = this.transform.position;
            _deltaSquared = _delta * _delta;
        }

        #endregion

        #region Properties

        public float Delta
        {
            get => _delta;
            set
            {
                _delta = value;
                _deltaSquared = value * value;
            }
        }

        public SPEvent OnMoved => _onMoved;

        #endregion

        #region Methods

        void Update()
        {
            var pos = this.transform.position;
            if ((pos - _lastPosition).sqrMagnitude >= _deltaSquared)
            {
                _lastPosition = pos;
                _onMoved.ActivateTrigger(this, null);
            }
        }

        #endregion

        #region IObservableTrigger Interface

        BaseSPEvent[] IObservableTrigger.GetEvents() => new[] { _onMoved }; 

        #endregion

    }

}
