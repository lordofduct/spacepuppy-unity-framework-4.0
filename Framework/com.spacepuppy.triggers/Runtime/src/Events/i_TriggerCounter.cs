using UnityEngine;

using com.spacepuppy.Utils;

namespace com.spacepuppy.Events
{

    public class i_TriggerCounter : AutoTriggerable, IObservableTrigger
    {

        #region Fields

        [SerializeField]
        [Min(0)]
        private int _totalCount;

        [SerializeField()]
        private MathUtil.WrapMode _wrapMode;

        [SerializeField]
        private bool _resetOnEnable;

        [SerializeField()]
        private bool _passAlongTriggerArg;

        [SerializeField()]
        [SPEvent.Config("daisy chained arg (object)")]
        private SPEvent _trigger = new SPEvent("Trigger");

        [System.NonSerialized]
        private int _currentCount;

        #endregion

        #region CONSTRUCTOR

        protected override void OnEnable()
        {
            base.OnEnable();

            if (_resetOnEnable) this.ResetCounter();
        }

        #endregion

        #region Properties

        public int TotalCount
        {
            get => _totalCount;
            set => _totalCount = Mathf.Max(0, value);
        }

        public MathUtil.WrapMode Wrap
        {
            get => _wrapMode;
            set => _wrapMode = value;
        }

        public bool ResetOnEnable
        {
            get => _resetOnEnable;
            set => _resetOnEnable = value;
        }

        public bool PassAlongTriggerArg
        {
            get { return _passAlongTriggerArg; }
            set { _passAlongTriggerArg = value; }
        }

        public SPEvent TriggerEvent => _trigger;

        [ShowNonSerializedProperty("Current Count")]
        public int CurrentCount
        {
            get => _currentCount;
            set => _currentCount = value;
        }

        [ShowNonSerializedProperty("Current Count Normalized")]
        public int CurrentCountNormalized
        {
            get
            {
                switch (_wrapMode)
                {
                    case MathUtil.WrapMode.Clamp:
                        return _totalCount > 0 ? Mathf.Clamp(_currentCount, 0, _totalCount) : 0;
                    case MathUtil.WrapMode.Loop:
                        return _totalCount > 0 ? MathUtil.Wrap(_currentCount, _totalCount + 1, 1) : 0;
                    case MathUtil.WrapMode.PingPong:
                        return _totalCount > 0 ? (int)Mathf.PingPong(_currentCount, _totalCount) : 0;
                    default:
                        return _currentCount;
                }
            }
        }

        #endregion

        #region Methods

        public void ResetCounter()
        {
            _currentCount = 0;
        }

        private void DoTriggerNext(object arg)
        {
            if (_passAlongTriggerArg)
                _trigger.ActivateTrigger(this, arg);
            else
                _trigger.ActivateTrigger(this, null);
        }

        #endregion

        #region ITriggerableMechanism Interface

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            _currentCount++;
            if (this.CurrentCountNormalized == _totalCount)
            {
                this.DoTriggerNext(arg);
            }

            return true;
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
