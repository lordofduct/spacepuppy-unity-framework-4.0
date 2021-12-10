using System;
using com.spacepuppy.Collections;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Tween
{

    public delegate void TweenerUpdateCallback(Tweener tween, float dt, float t);

    /// <summary>
    /// A tweener that calls an update function tick by tick.
    /// 
    /// When AutoKilling Id must be set.
    /// </summary>
    public class CallbackTweener : Tweener
    {

        #region Fields

        private TweenerUpdateCallback _callback;
        private Ease _ease;
        private float _dur;
        
        #endregion
        
        #region CONSTRUCTOR

        public CallbackTweener(TweenerUpdateCallback callback, float dur)
        {
            _callback = callback;
            _ease = EaseMethods.LinearEaseNone;
            _dur = dur;
        }

        public CallbackTweener(TweenerUpdateCallback callback, Ease ease, float dur)
        {
            _callback = callback;
            _ease = ease ?? EaseMethods.LinearEaseNone;
            _dur = dur;
        }

        #endregion

        #region Properties

        public override object Id { get; set; }

        public Ease Ease
        {
            get { return _ease; }
            set
            {
                _ease = value ?? EaseMethods.LinearEaseNone;
            }
        }

        #endregion

        #region Tweener Interface

        protected internal override bool GetTargetIsDestroyed()
        {
            return (_callback.Target is UnityEngine.Object) && _callback.Target.IsNullOrDestroyed();
        }

        protected internal override float GetPlayHeadLength()
        {
 	        return _dur;
        }

        protected internal override void DoUpdate(float dt, float t)
        {
            if (_callback == null) return;
            _callback(this, dt, _ease(t, 0, _dur, _dur));
        }

        #endregion
        
    }

    public class CallbackTweenerHash : ITweenHash
    {

        #region Fields

        private object _id;
        private TweenerUpdateCallback _callback;
        private Ease _ease;
        private float _dur;
        private float _delay;
        private UpdateSequence _updateType;
        private ITimeSupplier _timeSupplier;
        private TweenWrapMode _wrap;
        private int _wrapCount;
        private bool _reverse;
        private float _speedScale = 1.0f;
        private System.EventHandler _onStep;
        private System.EventHandler _onWrap;
        private System.EventHandler _onFinish;
        private System.EventHandler _onStopped;

        #endregion

        #region Constructor

        private CallbackTweenerHash()
        {
            //used for pooling
        }

        public CallbackTweenerHash(TweenerUpdateCallback callback, float dur)
        {
            if (callback == null) throw new System.ArgumentNullException(nameof(callback));
            _callback = callback;
            _dur = dur;
        }

        #endregion

        #region ITweenHash Interface

        public ITweenHash SetId(object id)
        {
            _id = id;
            return this;
        }

        public ITweenHash Ease(Ease ease)
        {
            _ease = ease;
            return this;
        }

        public ITweenHash Delay(float delay)
        {
            _delay = delay;
            return this;
        }

        public ITweenHash Use(UpdateSequence type)
        {
            _updateType = type;
            return this;
        }

        public ITweenHash Use(ITimeSupplier time)
        {
            _timeSupplier = time;
            return this;
        }

        public ITweenHash Wrap(TweenWrapMode wrap, int count)
        {
            _wrap = wrap;
            _wrapCount = count;
            return this;
        }

        public ITweenHash Reverse(bool reverse)
        {
            _reverse = reverse;
            return this;
        }

        public ITweenHash SpeedScale(float scale)
        {
            _speedScale = scale;
            return this;
        }

        public ITweenHash OnStep(EventHandler d)
        {
            if (d == null) return this;
            _onStep += d;
            return this;
        }

        public ITweenHash OnWrap(EventHandler d)
        {
            if (d == null) return this;
            _onWrap += d;
            return this;
        }

        public ITweenHash OnFinish(EventHandler d)
        {
            if (d == null) return this;
            _onFinish += d;
            return this;
        }

        public ITweenHash OnStopped(EventHandler d)
        {
            if (d == null) return this;
            _onStopped += d;
            return this;
        }

        public Tweener Create()
        {
            var tweener = new CallbackTweener(_callback, _dur)
            {
                Id = _id,
                Ease = _ease ?? EaseMethods.LinearEaseNone,
                Delay = _delay,
                UpdateType = _updateType,
                TimeSupplier = _timeSupplier ?? SPTime.Normal,
                WrapMode = _wrap,
                WrapCount = _wrapCount,
                Reverse = _reverse,
                SpeedScale = _speedScale,
            };
            if (_onStep != null) tweener.OnStep += _onStep;
            if (_onWrap != null) tweener.OnWrap += _onWrap;
            if (_onFinish != null) tweener.OnFinish += _onFinish;
            if (_onStopped != null) tweener.OnStep += _onStopped;
            return tweener;
        }

        #endregion

        #region ICloneable INterface

        public CallbackTweenerHash Clone()
        {
            var hash = new CallbackTweenerHash(_callback, _dur);
            hash._id = _id;
            hash._ease = _ease;
            hash._delay = _delay;
            hash._updateType = _updateType;
            hash._timeSupplier = _timeSupplier;
            hash._wrap = _wrap;
            hash._wrapCount = _wrapCount;
            hash._reverse = _reverse;
            hash._speedScale = _speedScale;
            hash._onStep = _onStep;
            hash._onWrap = _onWrap;
            hash._onFinish = _onFinish;
            hash._onStopped = _onStopped;
            return hash;
        }

        object System.ICloneable.Clone()
        {
            return this.Clone();
        }

        #endregion

        #region IDisposable Interface

        public void Dispose()
        {
            if (_pool.Release(this))
            {
                _id = null;
                _callback = null;
                _ease = EaseMethods.LinearEaseNone;
                _dur = 0f;
                _delay = 0f;
                _updateType = UpdateSequence.Update;
                _timeSupplier = null;
                _wrap = TweenWrapMode.Once;
                _wrapCount = 0;
                _reverse = false;
                _speedScale = 1.0f;
                _onStep = null;
                _onWrap = null;
                _onFinish = null;
                _onStopped = null;
            }
        }

        #endregion

        #region Static Factory

        private const int CACHESIZE = 4;
        private static ObjectCachePool<CallbackTweenerHash> _pool = new ObjectCachePool<CallbackTweenerHash>(CACHESIZE, () => new CallbackTweenerHash());

        public static CallbackTweenerHash GetTweenHash(TweenerUpdateCallback callback, float dur)
        {
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            CallbackTweenerHash result;
            if (_pool.TryGetInstance(out result))
            {
                result._callback = callback;
                result._dur = dur;
            }
            else
            {
                result = new CallbackTweenerHash(callback, dur);
            }
            return result;
        }

        #endregion

    }

}
