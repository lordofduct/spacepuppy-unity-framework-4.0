using com.spacepuppy.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.spacepuppy.Events
{
    public sealed class t_CurveInterval : MonoBehaviour, IUpdateable
    {

        #region Fields

        [SerializeField]
        [TimeUnitsSelector]
        private float _duration = 1f;

        [SerializeField]
        private ParticleSystem.MinMaxCurve _rateOverTime = new ParticleSystem.MinMaxCurve(10f, AnimationCurve.Linear(0f, 0f, 1f, 1f));

        [SerializeField]
        private SPTime _timerProvider;

        [SerializeField]
        [EnumPopupExcluding((int)MathUtil.WrapMode.Oblivion)]
        private MathUtil.WrapMode _wrapMode = MathUtil.WrapMode.Clamp;

        [SerializeField]
        [NegativeIsInfinity]
        [Tooltip("How many times should it repeat? Negative values mean infinitely.")]
        private int _repeatCount = -1;

        [SerializeField]
        private bool _resetOnEnable;

        [SerializeField]
        private SPEvent _onTick = new SPEvent("OnTick");

        [SerializeField]
        private SPEvent _onElapsed = new SPEvent("OnElapsed");

        [SerializeField]
        private SPEvent _onComplete = new SPEvent("OnComplete");


        [System.NonSerialized]
        private float _t = 0f;
        [System.NonSerialized]
        private float _tickTimer = 0f;
        [System.NonSerialized]
        private bool _complete;

        #endregion

        #region Properties

        public float Duration
        {
            get => _duration;
            set => _duration = value;
        }

        public ParticleSystem.MinMaxCurve RateOverTime
        {
            get => _rateOverTime;
            set => _rateOverTime = value;
        }

        public SPTime TimeProvider
        {
            get => _timerProvider;
            set => _timerProvider = value;
        }

        public MathUtil.WrapMode WrapMode
        {
            get => _wrapMode;
            set => _wrapMode = value;
        }

        public bool ResetOnEnable
        {
            get => _resetOnEnable;
            set => _resetOnEnable = value;
        }

        public SPEvent OnTick => _onTick;

        public SPEvent OnElapsed => _onElapsed;

        public SPEvent OnComplete => _onComplete;

        #endregion

        #region Methods

        private void OnEnable()
        {
            if (_resetOnEnable) this.ResetTimer();

            if (!_complete) GameLoop.UpdatePump.Add(this);
        }

        private void OnDisable()
        {
            GameLoop.UpdatePump.Remove(this);
        }

        void IUpdateable.Update()
        {
            float freq = 1f / _rateOverTime.Evaluate(_duration > 0f ? MathUtil.Wrap(_wrapMode, _t, _duration) / _duration : 1f);
            if(_tickTimer > freq)
            {
                _tickTimer -= freq;
                _onTick.ActivateTrigger(this, null);
            }

            float delta = _timerProvider.Delta;
            float oldt = _t;
            _t += delta;
            _tickTimer += delta;

            switch (_wrapMode)
            {
                case MathUtil.WrapMode.Oblivion: //oblivion currently doesn't behave differently than Clamp and therefore doesn't even show up in the editor
                case MathUtil.WrapMode.Clamp:
                    if (oldt <= _duration && _t > _duration)
                    {
                        _complete = true;
                        GameLoop.UpdatePump.Remove(this);
                        _onElapsed.ActivateTrigger(this, null);
                        _onComplete.ActivateTrigger(this, null);
                    }
                    break;
                case MathUtil.WrapMode.Loop:
                case MathUtil.WrapMode.PingPong:
                    if(_duration > 0f)
                    {
                        int a = Mathf.FloorToInt(oldt / _duration);
                        int b = Mathf.FloorToInt(_t / _duration);
                        if(a != b && b > a)
                        {
                            if(_repeatCount >= 0 && b > _repeatCount)
                            {
                                _complete = true;
                                GameLoop.UpdatePump.Remove(this);
                                _onElapsed.ActivateTrigger(this, null);
                                _onComplete.ActivateTrigger(this, null);
                            }
                            else
                            {
                                _onElapsed.ActivateTrigger(this, null);
                            }
                        }
                    }
                    break;
            }
        }

        public void ResetTimer()
        {
            _t = 0f;
            _tickTimer = 0f;
            _complete = false;
        }

        #endregion

    }
}
