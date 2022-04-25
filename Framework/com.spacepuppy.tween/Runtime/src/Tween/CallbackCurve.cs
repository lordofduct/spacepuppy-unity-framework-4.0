using UnityEngine;

using com.spacepuppy.Utils;

namespace com.spacepuppy.Tween
{

    public delegate void TweenerUpdateCallback(Tweener tween, float dt, float t);

    public class CallbackCurve : TweenCurve
    {

        #region Fields

        private TweenerUpdateCallback _callback;
        private Ease _ease;
        private float _dur;

        #endregion

        #region CONSTRUCTOR

        public CallbackCurve() { }

        public CallbackCurve(Ease ease, float dur, TweenerUpdateCallback callback)
        {
            _ease = ease ?? EaseMethods.Linear;
            _dur = Mathf.Max(dur, 0f);
            _callback = callback;
        }

        #endregion

        #region Properties

        public TweenerUpdateCallback Callback
        {
            get => _callback;
            set => _callback = value;
        }

        public Ease Ease
        {
            get { return _ease; }
            set
            {
                _ease = value ?? EaseMethods.Linear;
            }
        }

        public float Duration
        {
            get => _dur;
            set => _dur = Mathf.Max(value, 0f);
        }

        #endregion

        #region TweenCurve Interface

        public override float TotalTime => _dur;

        public override void Update(object targ, float dt, float t)
        {
            _callback?.Invoke(this.Tween, dt, this.Ease(t, 0f, _dur, _dur));
        }

        #endregion

    }

}
