using System;
using com.spacepuppy.Collections;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Tween
{

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

        public CallbackTweener(object id, TweenerUpdateCallback callback, float dur)
        {
            this.Id = id;
            _callback = callback;
            _ease = EaseMethods.Linear;
            _dur = dur;
        }

        public CallbackTweener(object id, TweenerUpdateCallback callback, Ease ease, float dur)
        {
            this.Id = id;
            _callback = callback;
            _ease = ease ?? EaseMethods.Linear;
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
                _ease = value ?? EaseMethods.Linear;
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

    public sealed class CallbackTweenerHash : ITweenHash
    {

        #region Fields

        private TweenerUpdateCallback _callback;

        private object _id;
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

        public CallbackTweenerHash(object id, TweenerUpdateCallback callback, float dur)
        {
            if (callback == null) throw new System.ArgumentNullException(nameof(callback));
            _id = id;
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
            var tweener = new CallbackTweener(_id, _callback, _dur)
            {
                Ease = _ease ?? EaseMethods.Linear,
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
            return new CallbackTweenerHash() {
                _callback = _callback,
                _dur = _dur,
                _id = _id,
                _ease = _ease,
                _delay = _delay,
                _updateType = _updateType,
                _timeSupplier = _timeSupplier,
                _wrap = _wrap,
                _wrapCount = _wrapCount,
                _reverse = _reverse,
                _speedScale = _speedScale,
                _onStep = _onStep,
                _onWrap = _onWrap,
                _onFinish = _onFinish,
                _onStopped = _onStopped,
            };
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
                _ease = EaseMethods.Linear;
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

        public static CallbackTweenerHash GetTweenHash(object id, TweenerUpdateCallback callback, float dur)
        {
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            CallbackTweenerHash result;
            if (_pool.TryGetInstance(out result))
            {
                result._id = id;
                result._callback = callback;
                result._dur = dur;
            }
            else
            {
                result = new CallbackTweenerHash(id, callback, dur);
            }
            return result;
        }

        #endregion

    }

}
