#pragma warning disable 0649 // variable declared but not used.

using UnityEngine;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Events
{

    /// <summary>
    /// Perform an action on some interval.
    /// </summary>
    public sealed class t_Interval : SPComponent, IMActivateOnReceiver, IObservableTrigger
    {

        #region Fields

        [SerializeField()]
        private ActivateEvent _activateOn = ActivateEvent.OnStartOrEnable;

        [SerializeField()]
        private SPTimePeriod _interval = new SPTimePeriod(1f);

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
        [System.NonSerialized]
        private double _startTime = double.NaN;

        #endregion

        #region CONSTRUCTOR

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

        public ActivateEvent ActivateOn
        {
            get { return _activateOn; }
            set { _activateOn = value; }
        }

        public SPTimePeriod Interval
        {
            get { return _interval; }
            set { _interval = value; }
        }

        public float IntervalSeconds
        {
            get => _interval.Seconds;
            set => _interval.Seconds = Mathf.Max(value, 0f);
        }

        public float Duration
        {
            get { return _interval.Seconds; }
            set { _interval.Seconds = value; }
        }

        public float Delay
        {
            get { return _delay; }
        }

        public DiscreteFloat RepeatCount
        {
            get { return _repeatCount; }
            set { _repeatCount = (value < 0f) ? DiscreteFloat.Zero : value; }
        }

        public SPEvent OnInterval => _onInterval;

        public SPEvent OnComplete => _onComplete;

        public double ElapsedSecondsPrecise => double.IsNaN(_startTime) ? 0d : System.Math.Clamp(_interval.TimeSupplierOrDefault.TotalPrecise - _startTime, 0d, _interval.Seconds);

        [ShowNonSerializedProperty("Elapsed Seconds", ShowAtEditorTime = true)]
        public float ElapsedSeconds => (float)this.ElapsedSecondsPrecise;

        [ShowNonSerializedProperty("Elapsed Time", ShowAtEditorTime = true)]
        public System.TimeSpan ElapsedTime => System.TimeSpan.FromSeconds(this.ElapsedSecondsPrecise);

        public double RemainingSecondsPrecise => double.IsNaN(_startTime) ? (double)_interval.Seconds : (double)_interval.Seconds - System.Math.Clamp(_interval.TimeSupplierOrDefault.TotalPrecise - _startTime, 0d, _interval.Seconds);

        [ShowNonSerializedProperty("Remaining Seconds", ShowAtEditorTime = true)]
        public float RemainingSeconds => (float)this.RemainingSecondsPrecise;

        [ShowNonSerializedProperty("Remaining Time", ShowAtEditorTime = true)]
        public System.TimeSpan RemainingTime => System.TimeSpan.FromSeconds(this.RemainingSecondsPrecise);

        #endregion

        #region Methods

        public void StartTimer()
        {
            if (_routine != null) return;

            _routine = this.StartRadicalCoroutine(this.TickerCallback(), RadicalCoroutineDisableMode.CancelOnDisable);
        }

        public void RestartTimer()
        {
            if (_routine != null)
            {
                RadicalCoroutine.Release(ref _routine);
            }

            _routine = this.StartRadicalCoroutine(this.TickerCallback(), RadicalCoroutineDisableMode.CancelOnDisable);
        }

        public void Reset()
        {
            if (_routine != null)
            {
                RadicalCoroutine.Release(ref _routine);
            }

            _startTime = double.NaN;
        }

        public void AddTimeToInterval(float seconds)
        {
            _interval.Seconds = Mathf.Max(_interval.Seconds + seconds, 0f);
        }

        private System.Collections.IEnumerator TickerCallback()
        {
            _startTime = double.NaN;

            if (_delay > 0f) yield return WaitForDuration.Seconds(_delay, _interval.TimeSupplierOrDefault);

            int cnt = 0;
            while (cnt <= _repeatCount)
            {
                _startTime = _interval.TimeSupplierOrDefault.TotalPrecise;
                yield return null;
                while ((_interval.TimeSupplierOrDefault.TotalPrecise - _startTime) < _interval.Seconds)
                {
                    yield return null;
                }
                cnt++;

                if (_onInterval.HasReceivers) _onInterval.ActivateTrigger(this, null);
            }

            if (_onComplete.HasReceivers) _onComplete.ActivateTrigger(this, null);

            if (_routine != null)
            {
                RadicalCoroutine.Release(ref _routine);
            }
        }

        #endregion

        #region IMActivateOnReceiver Interface

        void IMActivateOnReceiver.Activate() => this.RestartTimer();

        #endregion

        #region IObservableTrigger Interface

        BaseSPEvent[] IObservableTrigger.GetEvents()
        {
            return new BaseSPEvent[] { _onInterval, _onComplete };
        }

        #endregion

    }

}
