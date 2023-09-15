using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace com.spacepuppy.SPInput
{

    /// <summary>
    /// Creates a button signature that repeats the 'Down' signal as it is held.
    /// 
    /// Note, only works for Update and not FixedUpdate.
    /// </summary>
    public class RepeatingButtonInputSignature : BaseInputSignature, IButtonInputSignature
    {

        #region Fields

        private ButtonDelegate _button;
        private float _firstRepeatDelay;
        private float _repeatDelay;
        private float _repeatLerp;
        private float _maxRepeat;

        private ButtonState _current;
        private ButtonState _currentFixed;
        private double _lastDown = double.NegativeInfinity;
        private double _lastUp = double.NegativeInfinity;

        private float _delay;
        private int _repeatCount;

        #endregion

        #region CONSTRUCTOR

        public RepeatingButtonInputSignature(string id)
            : base(id)
        {
        }

        public RepeatingButtonInputSignature(string id, ButtonDelegate btn, float repeatDelay)
            : base(id)
        {
            _button = btn;
            this.FirstRepeatDelay = repeatDelay;
            this.RepeatDelay = repeatDelay;
            this.RepeatLerp = 0f;
            this.MaxRepeat = repeatDelay;
        }

        #endregion

        #region Properties

        public ButtonDelegate ButtonDelegate
        {
            get { return _button; }
            set { _button = value; }
        }

        /// <summary>
        /// How long to wait before the first repeat should occur.
        /// </summary>
        public float FirstRepeatDelay
        {
            get { return _firstRepeatDelay; }
            set { _firstRepeatDelay = Mathf.Max(0f, value); }
        }

        /// <summary>
        /// How long to wait between each repeat after the first.
        /// </summary>
        public float RepeatDelay
        {
            get { return _repeatDelay; }
            set { _repeatDelay = Mathf.Max(0f, value); }
        }

        /// <summary>
        /// Set to a value &gt 0 to have the RepeatRate scale over time.
        /// </summary>
        public float RepeatLerp
        {
            get { return _repeatLerp; }
            set { _repeatLerp = Mathf.Clamp01(value); }
        }

        /// <summary>
        /// The value that RepeatRate scales towards over time if RepeatLerp is &gt 0.
        /// </summary>
        public float MaxRepeat
        {
            get { return _maxRepeat; }
            set { _maxRepeat = Mathf.Max(0f, value); }
        }

        /// <summary>
        /// Number of times the Down signal has repeated since last official down.
        /// </summary>
        public int CurrentRepeatCount
        {
            get { return _repeatCount; }
        }

        #endregion

        #region IButtonInputSignature Interface

        public ButtonState CurrentState
        {
            get
            {
                if (GameLoop.CurrentSequence == UpdateSequence.FixedUpdate)
                {
                    return _currentFixed;
                }
                else
                {
                    return _current;
                }
            }
        }

        public ButtonState GetCurrentState(bool getFixedState)
        {
            return (getFixedState) ? _currentFixed : _current;
        }

        public void Consume()
        {
            if (GameLoop.CurrentSequence == UpdateSequence.FixedUpdate)
            {
                _currentFixed = InputUtil.ConsumeButtonState(_currentFixed);
            }
            else
            {
                _current = InputUtil.ConsumeButtonState(_current);
            }
        }

        public double LastDownTime => _lastDown;

        public double LastReleaseTime => _lastUp;

        #endregion

        #region IInputSignature Interfacce

        public override void Update()
        {
            //determine based on history
            _current = InputUtil.GetNextButtonState(_current, _button != null ? _button() : false);
            if (_current == ButtonState.Down)
            {
                _lastDown = Time.unscaledTimeAsDouble;
                _delay = _firstRepeatDelay;
                _repeatCount = 0;
            }
            else if (_current == ButtonState.Held && Time.realtimeSinceStartup - _lastDown > _repeatDelay)
            {
                _current = ButtonState.Down;
                _lastDown = Time.unscaledTimeAsDouble;
                if (_repeatCount == 0)
                    _delay = _repeatDelay;
                else
                    _delay = Mathf.Lerp(_delay, _maxRepeat, _repeatLerp);
                _repeatCount++;
            }
            else if (_current == ButtonState.Released)
            {
                _lastUp = Time.unscaledTimeAsDouble;
            }
        }

        public override void FixedUpdate()
        {
            //determine based on history
            _currentFixed = InputUtil.GetNextButtonState(_current, _button != null ? _button() : false);
        }

        public override void Reset()
        {
            _current = ButtonState.None;
            _currentFixed = ButtonState.None;
            _lastDown = double.NegativeInfinity;
            _lastUp = double.NegativeInfinity;
        }

        #endregion

    }

    /// <summary>
    /// A button that needs to be tapped multiple times before activating as 'down'.
    /// 
    /// Think like pressing A twice and holding to activate a 'turbo boost'.
    /// </summary>
    public class MultiTapButton : BaseInputSignature, IButtonInputSignature
    {

        #region Fields

        private ButtonDelegate _delegate;
        private int _taps;
        private float _delay;

        private ButtonState _current;
        private ButtonState _currentFixed;
        private double _lastDown = double.NegativeInfinity;
        private double _lastUp = double.NegativeInfinity;

        private ButtonState _realState;
        private int _count;
        private double _lastRealDown;

        #endregion

        #region CONSTRUCTOR

        public MultiTapButton(string id, ButtonDelegate del, int taps, float delay) : base(id)
        {
            _delegate = del;
            _taps = Mathf.Max(1, taps);
            _delay = delay;

            _realState = ButtonState.None;
            _count = 0;
        }

        #endregion

        #region Properties

        public ButtonDelegate Delegate
        {
            get { return _delegate; }
            set { _delegate = value; }
        }

        public int Taps
        {
            get { return _taps; }
            set { _taps = Mathf.Max(1, value); }
        }

        public float Delay
        {
            get { return _delay; }
            set { _delay = value; }
        }

        #endregion

        #region IButtonInputSignature Interface

        public ButtonState CurrentState
        {
            get
            {
                if (GameLoop.CurrentSequence == UpdateSequence.FixedUpdate)
                {
                    return _currentFixed;
                }
                else
                {
                    return _current;
                }
            }
        }

        public double LastDownTime => _lastDown;

        public double LastReleaseTime => _lastUp;

        public void Consume()
        {
            if (GameLoop.CurrentSequence == UpdateSequence.FixedUpdate)
            {
                _currentFixed = InputUtil.ConsumeButtonState(_currentFixed);
            }
            else
            {
                _current = InputUtil.ConsumeButtonState(_current);
            }
        }

        public ButtonState GetCurrentState(bool getFixedState)
        {
            return (getFixedState) ? _currentFixed : _current;
        }

        #endregion

        #region IInputSignature Interfacce

        public override void Update()
        {
            _realState = InputUtil.GetNextButtonState(_realState, _delegate != null ? _delegate() : false);
            if (_count < _taps)
            {
                switch (_realState)
                {
                    case ButtonState.Down:
                        {
                            _count++;
                            _lastRealDown = Time.unscaledTimeAsDouble;

                            if (_count == _taps)
                            {
                                _current = ButtonState.Down;
                                _lastDown = Time.unscaledTimeAsDouble;
                            }
                        }
                        break;
                    default:
                        _current = ButtonState.None;
                        if (Time.unscaledTimeAsDouble - _lastRealDown > _delay)
                        {
                            _count = 0;
                        }
                        break;
                }
            }
            else if (_current > ButtonState.None)
            {
                switch (_realState)
                {
                    case ButtonState.Released:
                    case ButtonState.None:
                        _current = ButtonState.Released;
                        _lastUp = Time.unscaledTimeAsDouble;
                        _count = 0;
                        break;
                    default:
                        _current = ButtonState.Held;
                        break;
                }
            }
            else
            {
                _current = ButtonState.None;
                _count = 0;
            }
        }

        public override void FixedUpdate()
        {
            _currentFixed = InputUtil.GetNextFixedButtonStateFromCurrent(_currentFixed, _current);
        }

        public override void Reset()
        {
            _current = ButtonState.None;
            _currentFixed = ButtonState.None;
            _lastDown = double.NegativeInfinity;
            _lastUp = double.NegativeInfinity;

            _realState = ButtonState.None;
            _lastRealDown = 0d;
            _count = 0;
        }

        #endregion

    }

    /// <summary>
    /// Similar to MultiTapButton, but it validates a cursor position to register a valid repeat press of the button. 
    /// Used to simulate something like a 'double-click' on the same spot.
    /// </summary>
    public class MultiClickButton : BaseInputSignature, IButtonInputSignature
    {

        #region Fields

        private ButtonDelegate _delegate;
        private int _taps;
        private float _delay;
        private System.Func<Vector2> _cursorPosDel;
        private float _cursorPosTolerance;

        private ButtonState _current;
        private ButtonState _currentFixed;
        private double _lastDown = double.NegativeInfinity;
        private double _lastUp = double.NegativeInfinity;
        private Vector2 _lastPressPos;

        private ButtonState _realState;
        private int _count;
        private double _lastRealDown;

        #endregion

        #region CONSTRUCTOR

        public MultiClickButton(string id, ButtonDelegate del, int taps, float delay, System.Func<Vector2> cursorPosDel, float cursorPosTolerance) : base(id)
        {
            _delegate = del;
            _taps = Mathf.Max(1, taps);
            _delay = delay;
            _cursorPosDel = cursorPosDel;
            _cursorPosTolerance = cursorPosTolerance;

            _realState = ButtonState.None;
            _count = 0;
        }

        #endregion

        #region Properties

        public ButtonDelegate Delegate
        {
            get { return _delegate; }
            set { _delegate = value; }
        }

        public int Taps
        {
            get { return _taps; }
            set { _taps = Mathf.Max(1, value); }
        }

        public float Delay
        {
            get { return _delay; }
            set { _delay = value; }
        }

        public System.Func<Vector2> CursorPosDel
        {
            get => _cursorPosDel;
            set => _cursorPosDel = value;
        }

        public float CursorPosTolerance
        {
            get => _cursorPosTolerance;
            set => _cursorPosTolerance = value;
        }

        #endregion

        #region IButtonInputSignature Interface

        public ButtonState CurrentState
        {
            get
            {
                if (GameLoop.CurrentSequence == UpdateSequence.FixedUpdate)
                {
                    return _currentFixed;
                }
                else
                {
                    return _current;
                }
            }
        }

        public double LastDownTime => _lastDown;

        public double LastReleaseTime => _lastUp;

        public void Consume()
        {
            if (GameLoop.CurrentSequence == UpdateSequence.FixedUpdate)
            {
                _currentFixed = InputUtil.ConsumeButtonState(_currentFixed);
            }
            else
            {
                _current = InputUtil.ConsumeButtonState(_current);
            }
        }

        public ButtonState GetCurrentState(bool getFixedState)
        {
            return (getFixedState) ? _currentFixed : _current;
        }

        #endregion

        #region IInputSignature Interfacce

        public override void Update()
        {
            _realState = InputUtil.GetNextButtonState(_realState, _delegate != null ? _delegate() : false);
            if (_count < _taps)
            {
                switch (_realState)
                {
                    case ButtonState.Down:
                        if (_count == 0)
                        {
                            _lastPressPos = _cursorPosDel?.Invoke() ?? default;
                            _count = 1;
                            _lastRealDown = Time.unscaledTimeAsDouble;

                            if (_count == _taps)
                            {
                                _current = ButtonState.Down;
                                _lastDown = Time.unscaledTimeAsDouble;
                            }
                        }
                        else
                        {
                            var pos = _cursorPosDel?.Invoke() ?? default;
                            float dist = (pos - _lastPressPos).sqrMagnitude;
                            _lastPressPos = pos;
                            if (dist > _cursorPosTolerance * _cursorPosTolerance)
                            {
                                _count = 1;
                            }
                            else
                            {
                                _count++;
                            }

                            _lastRealDown = Time.unscaledTimeAsDouble;

                            if (_count == _taps)
                            {
                                _current = ButtonState.Down;
                                _lastDown = Time.unscaledTimeAsDouble;
                            }
                        }
                        break;
                    default:
                        _current = ButtonState.None;
                        if (Time.unscaledTimeAsDouble - _lastRealDown > _delay)
                        {
                            _count = 0;
                        }
                        break;
                }
            }
            else if (_current > ButtonState.None)
            {
                switch (_realState)
                {
                    case ButtonState.Released:
                    case ButtonState.None:
                        _current = ButtonState.Released;
                        _lastUp = Time.unscaledTimeAsDouble;
                        _count = 0;
                        break;
                    default:
                        _current = ButtonState.Held;
                        break;
                }
            }
            else
            {
                _current = ButtonState.None;
                _count = 0;
            }
        }

        public override void FixedUpdate()
        {
            _currentFixed = InputUtil.GetNextFixedButtonStateFromCurrent(_currentFixed, _current);
        }

        public override void Reset()
        {
            _current = ButtonState.None;
            _currentFixed = ButtonState.None;
            _lastDown = double.NegativeInfinity;
            _lastUp = double.NegativeInfinity;

            _realState = ButtonState.None;
            _lastRealDown = 0d;
            _count = 0;
        }

        #endregion

    }

}
