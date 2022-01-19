#pragma warning disable 0649 // variable declared but not used.

using UnityEngine;

using com.spacepuppy.Utils;

namespace com.spacepuppy.Events
{

    public sealed class i_TriggerSequence : AutoTriggerable, IMStartOrEnableReceiver, IObservableTrigger
    {

        public enum WrapMode
        {
            Oblivion = 0,
            Clamp = 1,
            Loop = 2,
            PingPong = 3
        }

        public enum SignalMode
        {
            Manual,
            Auto
        }


        #region Fields

        [SerializeField()]
        private WrapMode _wrapMode;

        [SerializeField]
        private SignalMode _signal;

        [SerializeField()]
        private SPEvent _trigger = new SPEvent("Trigger");

        [SerializeField()]
        private bool _passAlongTriggerArg;

        [SerializeField]
        [MinRange(0)]
        private int _currentIndex = 0;


        [System.NonSerialized()]
        private RadicalCoroutine _routine;

        #endregion

        #region CONSTRUCTOR

        void IMStartOrEnableReceiver.OnStartOrEnable()
        {
            if (this.ActivateOn == ActivateEvent.None)
                this.AttemptAutoStart();
        }

        #endregion

        #region Properties

        public WrapMode Wrap
        {
            get { return _wrapMode; }
            set { _wrapMode = value; }
        }

        public SignalMode Signal
        {
            get { return _signal; }
            set { _signal = value; }
        }

        public SPEvent TriggerSequence
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

        public int CurrentIndex
        {
            get { return _currentIndex; }
            set { _currentIndex = value; }
        }

        public int CurrentIndexNormalized
        {
            get => CalculateWrap(_wrapMode, _currentIndex, _trigger.TargetCount);
        }

        #endregion

        #region Methods

        public void Reset()
        {
            if (_routine != null)
            {
                RadicalCoroutine.Release(ref _routine);
            }

            _currentIndex = 0;

            if (Application.isPlaying && this.enabled)
            {
                this.AttemptAutoStart();
            }
        }

        public void AttemptAutoStart()
        {
            int i = this.CurrentIndexNormalized;
            if (i < 0 || i >= _trigger.Targets.Count) return;
            
            if (_signal == SignalMode.Auto)
            {
                IAutoSequenceSignal signal;
                var targ = GameObjectUtil.GetGameObjectFromSource(_trigger.Targets[i].Target, true);
                if (targ != null && targ.GetComponentInChildren<IAutoSequenceSignal>(out signal))
                    if (signal != null)
                    {
                        _routine = this.StartRadicalCoroutine(this.DoAutoSequence(signal), RadicalCoroutineDisableMode.Pauses);
                    }
            }
        }

        private System.Collections.IEnumerator DoAutoSequence(IAutoSequenceSignal signal)
        {
            if (signal != null)
            {
                yield return signal.Wait();
                _currentIndex++;
            }

            while (true)
            {
                int i = this.CurrentIndexNormalized;
                if (i < 0 || i >= _trigger.Targets.Count) yield break;
                
                var go = GameObjectUtil.GetGameObjectFromSource(_trigger.Targets[i].Target, true);
                if (go != null && go.GetComponentInChildren<IAutoSequenceSignal>(out signal))
                {
                    var handle = signal.Wait();
                    _trigger.ActivateTriggerAt(i, this, null);
                    yield return handle;
                }
                else
                {
                    _trigger.ActivateTriggerAt(i, this, null);
                    yield return null;
                }

                _currentIndex++;
            }
        }

        #endregion

        #region ITriggerableMechanism Interface

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;


            switch (_signal)
            {
                case SignalMode.Manual:
                    {
                        _trigger.ActivateTriggerAt(this.CurrentIndexNormalized, this, _passAlongTriggerArg ? arg : null);
                        _currentIndex++;
                    }
                    break;
                case SignalMode.Auto:
                    {
                        if (_routine != null)
                        {
                            RadicalCoroutine.Release(ref _routine);
                        }
                        _routine = this.StartRadicalCoroutine(this.DoAutoSequence(null), RadicalCoroutineDisableMode.Pauses);
                    }
                    break;
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

        public static int CalculateWrap(WrapMode mode, int index, int count)
        {
            switch (mode)
            {
                case WrapMode.Oblivion:
                    return index;
                case WrapMode.Clamp:
                    return Mathf.Clamp(index, 0, count - 1);
                case WrapMode.Loop:
                    return MathUtil.Wrap(index, count);
                case WrapMode.PingPong:
                    return (int)Mathf.PingPong(index, count - 1);
                default:
                    return index;
            }
        }

    }

}
