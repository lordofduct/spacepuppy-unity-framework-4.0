#pragma warning disable 0649 // variable declared but not used.

using UnityEngine;

using com.spacepuppy.Geom;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Events
{

    public sealed class t_IntervalRandom : SPComponent, IObservableTrigger
    {

        #region Fields

        [SerializeField()]
        private ActivateEvent _activateOn = ActivateEvent.OnStartOrEnable;

        [SerializeField]
        private RandomRef _rng;

        [SerializeField]
        private SPTime _timeSupplier;
        [SerializeField()]
        private Interval _interval = com.spacepuppy.Geom.Interval.MinMax(0f, 1f);

        [SerializeField()]
        [Tooltip("Negative values will repeat forever like infinity.")]
        [DiscreteFloat.NonNegative()]
        private DiscreteFloat _repeatCount = DiscreteFloat.PositiveInfinity;

        [SerializeField()]
        [TimeUnitsSelector()]
        [Tooltip("Wait a duration before starting interval, waits using the same timesupplier as interval.")]
        private float _delay;

        [SerializeField]
        [UnityEngine.Serialization.FormerlySerializedAs("_trigger")]
        private SPEvent _onInterval = new SPEvent("OnInterval");

        [SerializeField]
        private SPEvent _onComplete = new SPEvent("OnComplete");


        [System.NonSerialized()]
        private RadicalCoroutine _routine;

        #endregion

        #region CONSTRUCTOR

        protected override void Awake()
        {
            base.Awake();

            if ((_activateOn & ActivateEvent.Awake) != 0)
            {
                this.OnTriggerActivate();
            }
        }

        protected override void Start()
        {
            base.Start();

            if ((_activateOn & ActivateEvent.OnStart) != 0 || (_activateOn & ActivateEvent.OnEnable) != 0)
            {
                this.OnTriggerActivate();
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (!this.started) return;

            if ((_activateOn & ActivateEvent.OnEnable) != 0)
            {
                this.OnTriggerActivate();
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (_routine != null)
            {
                RadicalCoroutine.Release(ref _routine);
            }
        }

        #endregion

        #region Properties

        public IRandom RNG
        {
            get { return _rng.Value; }
            set { _rng.Value = value; }
        }

        public SPTime TimeSupplier
        {
            get { return _timeSupplier; }
            set { _timeSupplier = value; }
        }

        public Interval Interval
        {
            get { return _interval; }
            set { _interval = value; }
        }

        public float Delay
        {
            get { return _delay; }
        }

        public DiscreteFloat RepeatCount
        {
            get { return _repeatCount; }
            set { _repeatCount = (value < 0f) ? DiscreteFloat.PositiveInfinity : value; }
        }

        public SPEvent OnInterval => _onInterval;

        public SPEvent OnComplete => _onComplete;

        #endregion

        #region Methods

        public void StartTimer()
        {
            if (_routine != null) return;

            _routine = this.StartRadicalCoroutine(this.TickerCallback(_delay, _interval, _timeSupplier.TimeSupplier ?? SPTime.Normal, _repeatCount), RadicalCoroutineDisableMode.CancelOnDisable);
        }

        public void RestartTimer()
        {
            if (_routine != null)
            {
                RadicalCoroutine.Release(ref _routine);
            }

            _routine = this.StartRadicalCoroutine(this.TickerCallback(_delay, _interval, _timeSupplier.TimeSupplier ?? SPTime.Normal, _repeatCount), RadicalCoroutineDisableMode.CancelOnDisable);
        }

        private void OnTriggerActivate()
        {
            this.RestartTimer();
        }

        private System.Collections.IEnumerator TickerCallback(float delay, Interval interval, ITimeSupplier timeSupplier, DiscreteFloat repeatCount)
        {
            if (delay > 0f) yield return WaitForDuration.Seconds(delay, timeSupplier);

            int cnt = 0;

            while (cnt <= repeatCount)
            {
                yield return WaitForDuration.Seconds(_rng.Value.SelfOrDefault().Range(interval.Max, interval.Min), timeSupplier);

                cnt++;
                if (_onInterval.HasReceivers) _onInterval.ActivateTrigger(this, null);
            }

            if (_onComplete.HasReceivers) _onComplete.ActivateTrigger(this, null);
        }

        #endregion

        #region IObservableTrigger Interface

        BaseSPEvent[] IObservableTrigger.GetEvents()
        {
            return new BaseSPEvent[] { _onInterval, _onComplete };
        }

        #endregion

    }

}
