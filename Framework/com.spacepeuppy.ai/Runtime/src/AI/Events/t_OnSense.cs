using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy.Collections;
using com.spacepuppy.Events;
using com.spacepuppy.Sensors;
using com.spacepuppy.Utils;

namespace com.spacepuppy.AI.Events
{

    [Infobox("Setting 'UseProximityTrigger' false will make this tick constantly, multiple t_OnSense configured this way can be very expensive.", MessageType =InfoBoxMessageType.Warning)]
    public class t_OnSense : SPComponent, IObservableTrigger
    {

        #region Fields

        [SerializeField]
        [DefaultFromSelf]
        private Sensor _sensor;
        [SerializeField]
        private float _interval = 1f;
        [SerializeField]
        private bool _useProximityTrigger = true;

        [SerializeField()]
        [UnityEngine.Serialization.FormerlySerializedAs("_trigger")]
        private SPEvent _onSense = new SPEvent("OnSense");


        [System.NonSerialized]
        private RadicalCoroutine _routine;
        [System.NonSerialized]
        private HashSet<Collider> _nearColliders;

        #endregion

        #region CONSTRUCTOR

        protected override void Start()
        {
            base.Start();

            this.TryStartRoutine();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            _nearColliders.Clear();
            _routine?.Cancel();
            _routine = null;
        }

        #endregion

        #region Properties

        public Sensor Sensor
        {
            get { return _sensor; }
            set { _sensor = value; }
        }

        public float Interval
        {
            get { return _interval; }
            set { _interval = value; }
        }

        public bool UseProximityTrigger
        {
            get { return _useProximityTrigger; }
            set { _useProximityTrigger = value; }
        }

        public SPEvent OnSense => _onSense;

        #endregion

        #region Methods

        protected void OnTriggerEnter(Collider other)
        {
            if (!this.isActiveAndEnabled) return;
            if (!_useProximityTrigger) return;
            if (_sensor == null) return;
            if (!_sensor.ConcernedWith(other.gameObject)) return;

            if (_nearColliders == null) _nearColliders = new HashSet<Collider>();
            _nearColliders.Add(other);

            this.TryStartRoutine();
        }

        protected void OnTriggerExit(Collider other)
        {
            if (!this.isActiveAndEnabled) return;
            if (!_useProximityTrigger || _nearColliders == null) return;
            if (_sensor == null) return;

            _nearColliders.Remove(other);

            //clean up any potentially lost colliders since Unity doesn't signal OnTriggerExit if a collider is destroyed/disabled.
            if (_nearColliders.Count > 0)
            {
                _nearColliders.RemoveWhere(o => !ObjUtil.IsValidObject(o) || !o.IsActiveAndEnabled());
            }

            if (_nearColliders.Count == 0 && (_routine?.Active ?? false))
            {
                _routine.Stop();
            }
        }




        private void TryStartRoutine()
        {
            if (!this.isActiveAndEnabled) return;

            if (!_useProximityTrigger || (_nearColliders != null && _nearColliders.Count > 0))
            {
                if (_routine == null || _routine.Finished)
                {
                    _routine = this.StartRadicalCoroutine(this.SenseRoutine(), RadicalCoroutineDisableMode.Pauses);
                }
                else if (!_routine.Active)
                {
                    _routine.Start(this, RadicalCoroutineDisableMode.Pauses);
                }
            }
        }

        private System.Collections.IEnumerator SenseRoutine()
        {
            while (true)
            {
                if (_sensor == null) yield break;

                if (_sensor.SenseAny())
                {
                    _onSense.ActivateTrigger(this, null);
                }

                yield return WaitForDuration.Seconds(_interval);
            }
        }

        #endregion

        #region IObservableTrigger Interface

        BaseSPEvent[] IObservableTrigger.GetEvents()
        {
            return new BaseSPEvent[] { _onSense };
        }

        #endregion

    }
}
