using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy.Collections;
using com.spacepuppy.Dynamic.Accessors;
using com.spacepuppy.Utils;
using com.spacepuppy.Tween.Curves;

namespace com.spacepuppy.Tween
{

    public class TweenHash : ITweenHash
    {

        public enum AnimMode
        {
            To = 0,
            From = 1,
            By = 2,
            FromTo = 3,
            RedirectTo = 4
        }

        #region Fields

        private TweenCurveFactory _curveFactory;
        private TweenHash _prevNode;

        private object _targ;

        private object _id;
        private List<CurveGeneratorToken> _curves = new List<CurveGeneratorToken>();
        private Ease _defaultEase = EaseMethods.Linear;
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

        #region CONSTRUCTOR

        protected TweenHash()
        {
            //used for pooling constructor
            _curveFactory = SPTween.CurveFactory;
        }

        public TweenHash(object target, object id = null, TweenCurveFactory curveFactory = null)
        {
            if (target == null) throw new System.ArgumentNullException(nameof(target));
            _targ = target;
            _id = id;
            _curveFactory = curveFactory ?? SPTween.CurveFactory;
        }

        #endregion

        #region Properties

        public object Target { get { return _targ; } }

        #endregion

        #region Config Methods

        public float EstimateDuration()
        {
            float dur = _delay;
            switch (_curves.Count)
            {
                case 0:
                    break;
                case 1:
                    dur += float.IsFinite(_curves[0].EstimatedDuration) ? _curves[0].EstimatedDuration : 0f;
                    break;
                default:
                    {
                        float max = float.IsFinite(_curves[0].EstimatedDuration) ? _curves[0].EstimatedDuration : 0f;
                        for (int i = 1; i < _curves.Count; i++)
                        {
                            if (float.IsFinite(_curves[i].EstimatedDuration) && _curves[i].EstimatedDuration > max)
                            {
                                max = _curves[i].EstimatedDuration;
                            }
                        }
                        dur += max;
                    }
                    break;
            }
            if (_prevNode != null)
            {
                dur += _prevNode.EstimateDuration();
            }
            return dur;
        }

        /// <summary>
        /// Sets the id for the tween, if a FollowOn sequence, the entire sequence id is updated.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TweenHash SetId(object id)
        {
            _id = id;
            if (_prevNode != null) _prevNode.SetId(id);
            return this;
        }

        public TweenHash Ease(Ease ease)
        {
            _defaultEase = ease ?? EaseMethods.Linear;
            return this;
        }

        public TweenHash Delay(float delay)
        {
            _delay = delay;
            return this;
        }

        public TweenHash Use(UpdateSequence type)
        {
            _updateType = type;
            return this;
        }

        public TweenHash Use(ITimeSupplier time)
        {
            _timeSupplier = time ?? SPTime.Normal;
            return this;
        }

        public TweenHash Wrap(TweenWrapMode wrap, int count = -1)
        {
            _wrap = wrap;
            _wrapCount = count;
            return this;
        }

        public TweenHash Reverse(bool reverse)
        {
            _reverse = reverse;
            return this;
        }

        public TweenHash SpeedScale(float scale)
        {
            _speedScale = scale;
            return this;
        }

        public TweenHash OnStep(System.EventHandler d)
        {
            if (d == null) return this;
            _onStep += d;
            return this;
        }

        public TweenHash OnWrap(System.EventHandler d)
        {
            if (d == null) return this;
            _onWrap += d;
            return this;
        }

        public TweenHash OnFinish(System.EventHandler d)
        {
            if (d == null) return this;
            _onFinish += d;
            return this;
        }

        public TweenHash OnStopped(System.EventHandler d)
        {
            if (d == null) return this;
            _onStopped += d;
            return this;
        }

        public Tweener Create(bool preserve = false)
        {
            try
            {
                if (_targ == null) return null;

                if (_prevNode != null)
                {
                    var seq = new FollowOnTweenSequence(_id);
                    seq.UpdateType = _updateType;
                    seq.TimeSupplier = _timeSupplier;

                    var n = this;
                    while (n != null)
                    {
                        seq.Prepend(n.ShallowClone());
                        n = n._prevNode;
                    }

                    return seq;
                }
                else
                {
                    return this.CreateOnlySelfTweener();
                }

                /*
                //set curves
                Tweener tween = null;
                if (_curves.Count > 1)
                {
                    var grp = new TweenCurveGroup();
                    for (int i = 0; i < _curves.Count; i++)
                    {
                        var curve = _curves[i].Callback?.Invoke();
                        if (curve == null)
                            Debug.LogWarning("Failed to create tween for property '" + _curves[i].Accessor?.GetMemberName() ?? "UNKNOWN" + "' on target.", _targ as Object);
                        else
                            grp.Curves.Add(curve);
                    }
                    tween = new ObjectTweener(_targ, grp);
                }
                else if (_curves.Count == 1)
                {
                    var curve = _curves[0].Callback?.Invoke();
                    if (curve == null)
                    {
                        Debug.LogWarning("Failed to create tween for property '" + _curves[0].Accessor?.GetMemberName() ?? "UNKNOWN" + "' on target.", _targ as UnityEngine.Object);
                        return new ObjectTweener(_targ, TweenCurve.Null);
                    }
                    else
                        tween = new ObjectTweener(_targ, curve);
                }
                else
                {
                    tween = new ObjectTweener(_targ, TweenCurve.Null);
                }

                //set props
                if (_id != null) tween.Id = _id;
                tween.UpdateType = _updateType;
                tween.TimeSupplier = _timeSupplier;
                tween.SpeedScale = _speedScale;
                tween.WrapMode = _wrap;
                tween.WrapCount = _wrapCount;
                tween.Reverse = _reverse;
                tween.Delay = _delay;

                if (_prevNode != null)
                {
                    var seq = new TweenSequence(tween.Id);
                    seq.UpdateType = _updateType;
                    seq.TimeSupplier = _timeSupplier;
                    seq.Tweens.Add(tween);

                    var node = _prevNode;
                    while (node != null)
                    {
                        seq.Tweens.Insert(0, node.Create());
                        node = node._prevNode;
                    }

                    tween = seq;
                }

                if (_onStep != null) tween.OnStep += _onStep;
                if (_onWrap != null) tween.OnWrap += _onWrap;
                if (_onFinish != null) tween.OnFinish += _onFinish;
                if (_onStopped != null) tween.OnStopped += _onStopped;

                return tween;
                */
            }
            finally
            {
                if (!preserve) this.Dispose();
            }
        }
        Tweener CreateOnlySelfTweener()
        {
            //set curves
            Tweener tween = null;
            if (_curves.Count > 1)
            {
                var grp = new TweenCurveGroup();
                for (int i = 0; i < _curves.Count; i++)
                {
                    var curve = _curves[i].Callback?.Invoke(this);
                    if (curve == null)
                        Debug.LogWarning("Failed to create tween for property '" + _curves[i].Accessor?.GetMemberName() ?? "UNKNOWN" + "' on target.", _targ as Object);
                    else
                        grp.Curves.Add(curve);
                }
                tween = new ObjectTweener(_targ, grp);
            }
            else if (_curves.Count == 1)
            {
                var curve = _curves[0].Callback?.Invoke(this);
                if (curve == null)
                {
                    Debug.LogWarning("Failed to create tween for property '" + _curves[0].Accessor?.GetMemberName() ?? "UNKNOWN" + "' on target.", _targ as UnityEngine.Object);
                    return new ObjectTweener(_targ, TweenCurve.Null);
                }
                else
                    tween = new ObjectTweener(_targ, curve);
            }
            else
            {
                tween = new ObjectTweener(_targ, TweenCurve.Null);
            }

            //set props
            if (_id != null) tween.Id = _id;
            tween.UpdateType = _updateType;
            tween.TimeSupplier = _timeSupplier;
            tween.SpeedScale = _speedScale;
            tween.WrapMode = _wrap;
            tween.WrapCount = _wrapCount;
            tween.Reverse = _reverse;
            tween.Delay = _delay;

            if (_onStep != null) tween.OnStep += _onStep;
            if (_onWrap != null) tween.OnWrap += _onWrap;
            if (_onFinish != null) tween.OnFinish += _onFinish;
            if (_onStopped != null) tween.OnStopped += _onStopped;

            return tween;
        }

        #endregion

        #region Curve Methods

        public CurveGenerator Prop(string memberName)
        {
            return new CurveGenerator(this, _curveFactory.GetAccessor(_targ, memberName));
        }

        public CurveGenerator<TProp> Prop<T, TProp>(MemberGetter<T, TProp> getter, MemberSetter<T, TProp> setter) where T : class
        {
            return new CurveGenerator<TProp>(this, new GetterSetterMemberAccessor<T, TProp>(getter, setter));
        }

        public CurveGenerator<TProp> Prop<TProp>(IMemberAccessor<TProp> accessor)
        {
            if (accessor == null) throw new System.ArgumentNullException(nameof(accessor));
            return new CurveGenerator<TProp>(this, accessor);
        }

        /// <summary>
        /// Follow this tween with another tween.
        /// 
        /// Inherits updateType and timeSupplier, unless otherwise changed.
        /// </summary>
        /// <returns></returns>
        public TweenHash FollowOn()
        {
            var hash = GetTweenHash(_targ, _id);
            hash._prevNode = this;
            hash._updateType = _updateType;
            hash._timeSupplier = _timeSupplier;
            return hash;
        }

        public TweenHash Apply(TweenConfigCallback callback, Ease ease, float dur)
        {
            if (callback != null) callback(this, ease, dur);
            return this;
        }

        //#########################
        // CURVES
        //

        public TweenHash UseCurve(System.Func<TweenHash, TweenCurve> callback, float estimatedDuration, IMemberAccessor accessor = null)
        {
            if (callback == null) throw new System.ArgumentNullException(nameof(callback));
            _curves.Add(new CurveGeneratorToken()
            {
                Callback = callback,
                EstimatedDuration = estimatedDuration,
                Accessor = accessor,
            });
            return this;
        }

        public TweenHash UseCurve(TweenCurve curve, IMemberAccessor accessor = null)
        {
            if (curve == null) throw new System.ArgumentNullException(nameof(curve));
            _curves.Add(new CurveGeneratorToken()
            {
                Callback = (hash) => curve,
                EstimatedDuration = curve?.TotalTime ?? 0f,
                Accessor = accessor,
            });
            return this;
        }

        public TweenHash ByAnimMode(AnimMode mode, string memberName, Ease ease, float dur, object value, object end, int option = 0)
        {
            switch (mode)
            {
                case AnimMode.To:
                    return this.Prop(memberName).To(ease, dur, value, option);
                case AnimMode.From:
                    return this.Prop(memberName).From(ease, dur, value, option);
                case AnimMode.By:
                    return this.Prop(memberName).By(ease, dur, value, option);
                case AnimMode.FromTo:
                    return this.Prop(memberName).FromTo(ease, dur, value, end, option);
                case AnimMode.RedirectTo:
                    return this.Prop(memberName).RedirectTo(ease, dur, value, end, option);
                default:
                    return this;
            }
        }

        public TweenHash CallbackCurve(Ease ease, float dur, TweenerUpdateCallback callback)
        {
            if (ease == null) ease = _defaultEase;
            return this.UseCurve((hash) => new CallbackCurve(ease, dur, callback), dur);
        }

        #endregion

        #region ITweenHash Interface

        ITweenHash ITweenHash.SetId(object id)
        {
            return this.SetId(id);
        }

        ITweenHash ITweenHash.Ease(Ease ease)
        {
            return this.Ease(ease);
        }

        ITweenHash ITweenHash.Delay(float delay)
        {
            return this.Delay(delay);
        }

        ITweenHash ITweenHash.Use(UpdateSequence type)
        {
            return this.Use(type);
        }

        ITweenHash ITweenHash.Use(ITimeSupplier time)
        {
            return this.Use(time);
        }

        ITweenHash ITweenHash.Wrap(TweenWrapMode wrap, int count)
        {
            return this.Wrap(wrap, count);
        }

        ITweenHash ITweenHash.Reverse(bool reverse)
        {
            return this.Reverse(reverse);
        }

        ITweenHash ITweenHash.SpeedScale(float scale)
        {
            return this.SpeedScale(scale);
        }

        ITweenHash ITweenHash.OnStep(System.EventHandler d)
        {
            return this.OnStep(d);
        }

        ITweenHash ITweenHash.OnWrap(System.EventHandler d)
        {
            return this.OnWrap(d);
        }

        ITweenHash ITweenHash.OnFinish(System.EventHandler d)
        {
            return this.OnFinish(d);
        }

        ITweenHash ITweenHash.OnStopped(System.EventHandler d)
        {
            return this.OnStopped(d);
        }

        Tweener ITweenHash.Create(bool preserve)
        {
            return this.Create(preserve);
        }

        #endregion

        #region ICloneable Interface

        public TweenHash Clone()
        {
            var hash = GetTweenHash(_targ, _id);
            hash._curveFactory = _curveFactory;
            hash._curves.Clear();
            hash._curves.AddRange(_curves);
            hash._defaultEase = _defaultEase;
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
            hash._prevNode = _prevNode?.Clone();
            return hash;
        }

        /// <summary>
        /// Returns a copy of this node with no followon history.
        /// </summary>
        /// <returns></returns>
        public TweenHash ShallowClone()
        {
            var hash = GetTweenHash(_targ, _id);
            hash._curveFactory = _curveFactory;
            hash._curves.Clear();
            hash._curves.AddRange(_curves);
            hash._defaultEase = _defaultEase;
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
            hash._prevNode = null;
            return hash;
        }

        object System.ICloneable.Clone()
        {
            return this.Clone();
        }

        #endregion

        #region IDisposable Interface

        public virtual void Dispose()
        {
            _prevNode?.Dispose();

            if (this.GetType() == typeof(TweenHash) && _pool.Release(this))
            {
                _curveFactory = null;
                _id = null;
                _targ = null;
                _curves.Clear();
                _defaultEase = EaseMethods.Linear;
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

        #region Special Types

        private struct CurveGeneratorToken
        {
            public IMemberAccessor Accessor;
            public float EstimatedDuration;
            public System.Func<TweenHash, TweenCurve> Callback;
        }

        public struct CurveGenerator
        {

            private TweenHash _hash;
            private IMemberAccessor _accessor;

            internal CurveGenerator(TweenHash hash, IMemberAccessor acc)
            {
                _hash = hash;
                _accessor = acc;
            }

            public TweenHash FromTo(float dur, object start, object end, int option = 0)
            {
                return FromTo(null, dur, start, end, option);
            }
            public TweenHash FromTo(Ease ease, float dur, object start, object end, int option = 0)
            {
                var acc = _accessor;
                return _hash?.UseCurve((hash) => hash._curveFactory.CreateFromTo(hash._targ, acc, ease ?? hash?._defaultEase, dur, start, end, option), dur, acc);
            }

            public TweenHash To(float dur, object end, int option = 0)
            {
                return To(null, dur, end, option);
            }
            public TweenHash To(Ease ease, float dur, object end, int option = 0)
            {
                var acc = _accessor;
                return _hash?.UseCurve((hash) => hash._curveFactory.CreateTo(hash._targ, acc, ease ?? hash?._defaultEase, dur, end, option), dur, acc);
            }

            public TweenHash From(float dur, object start, int option = 0)
            {
                return From(null, dur, start, option);
            }
            public TweenHash From(Ease ease, float dur, object start, int option = 0)
            {
                var acc = _accessor;
                return _hash?.UseCurve((hash) => hash._curveFactory.CreateFrom(hash._targ, acc, ease ?? hash?._defaultEase, dur, start, option), dur, acc);
            }

            public TweenHash By(float dur, object amt, int option = 0)
            {
                return By(null, dur, amt, option);
            }
            public TweenHash By(Ease ease, float dur, object amt, int option = 0)
            {
                var acc = _accessor;
                return _hash?.UseCurve((hash) => hash._curveFactory.CreateBy(hash._targ, acc, ease ?? hash?._defaultEase, dur, amt, option), dur, acc);
            }

            public TweenHash RedirectTo(float dur, object start, object end, int option = 0)
            {
                return RedirectTo(null, dur, start, end, option);
            }
            public TweenHash RedirectTo(Ease ease, float dur, object start, object end, int option = 0)
            {
                var acc = _accessor;
                return _hash?.UseCurve((hash) => hash._curveFactory.CreateRedirectTo(hash._targ, acc, ease ?? hash?._defaultEase, dur, start, end, option), dur, acc);
            }

            public TweenHash ByAnimMode(AnimMode mode, Ease ease, float dur, object value, object end, int option = 0)
            {
                switch (mode)
                {
                    case AnimMode.To:
                        return this.To(ease, dur, value, option);
                    case AnimMode.From:
                        return this.From(ease, dur, value, option);
                    case AnimMode.By:
                        return this.By(ease, dur, value, option);
                    case AnimMode.FromTo:
                        return this.FromTo(ease, dur, value, end, option);
                    case AnimMode.RedirectTo:
                        return this.RedirectTo(ease, dur, value, end, option);
                    default:
                        return _hash;
                }
            }

        }

        public struct CurveGenerator<TProp>
        {

            private TweenHash _hash;
            private IMemberAccessor<TProp> _accessor;

            internal CurveGenerator(TweenHash hash, IMemberAccessor<TProp> acc)
            {
                _hash = hash;
                _accessor = acc;
            }

            public TweenHash FromTo(float dur, TProp start, TProp end, int option = 0)
            {
                return FromTo(null, dur, start, end, option);
            }
            public TweenHash FromTo(Ease ease, float dur, TProp start, TProp end, int option = 0)
            {
                var acc = _accessor;
                return _hash?.UseCurve((hash) => hash._curveFactory.CreateFromTo<TProp>(hash._targ, acc, ease ?? hash?._defaultEase, dur, start, end, option), dur, acc);
            }

            public TweenHash To(float dur, TProp end, int option = 0)
            {
                return To(null, dur, end, option);
            }
            public TweenHash To(Ease ease, float dur, TProp end, int option = 0)
            {
                var acc = _accessor;
                return _hash?.UseCurve((hash) => hash._curveFactory.CreateTo<TProp>(hash._targ, acc, ease ?? hash?._defaultEase, dur, end, option), dur, acc);
            }

            public TweenHash From(float dur, TProp start, int option = 0)
            {
                return From(null, dur, start, option);
            }
            public TweenHash From(Ease ease, float dur, TProp start, int option = 0)
            {
                var acc = _accessor;
                return _hash?.UseCurve((hash) => hash._curveFactory.CreateFrom<TProp>(hash._targ, acc, ease ?? hash?._defaultEase, dur, start, option), dur, acc);
            }

            public TweenHash By(float dur, TProp amt, int option = 0)
            {
                return By(null, dur, amt, option);
            }
            public TweenHash By(Ease ease, float dur, TProp amt, int option = 0)
            {
                var acc = _accessor;
                return _hash?.UseCurve((hash) => hash._curveFactory.CreateBy<TProp>(hash._targ, acc, ease ?? hash?._defaultEase, dur, amt, option), dur, acc);
            }

            public TweenHash RedirectTo(float dur, TProp start, TProp end, int option = 0)
            {
                return RedirectTo(null, dur, start, end, option);
            }
            public TweenHash RedirectTo(Ease ease, float dur, TProp start, TProp end, int option = 0)
            {
                var acc = _accessor;
                return _hash?.UseCurve((hash) => hash._curveFactory.CreateRedirectTo<TProp>(hash._targ, acc, ease ?? hash?._defaultEase, dur, start, end, option), dur, acc);
            }

            public TweenHash UseCurve(AnimationCurve curve, int option = 0)
            {
                var acc = _accessor;
                float dur = (curve.keys.Length > 0) ? curve.keys.Last().time : 0f;
                return _hash?.UseCurve((hash) => hash._curveFactory.CreateFromTo<TProp>(hash._targ, acc, EaseMethods.FromAnimationCurve(curve), dur, default(TProp), default(TProp), option), dur, acc);
            }

            public TweenHash UseCurve(AnimationCurve curve, float dur, int option = 0)
            {
                var acc = _accessor;
                return _hash?.UseCurve((hash) => hash._curveFactory.CreateFromTo<TProp>(hash._targ, acc, EaseMethods.FromAnimationCurve(curve), dur, default(TProp), default(TProp), option), dur, acc);
            }

            public TweenHash ByAnimMode(AnimMode mode, Ease ease, float dur, TProp value, TProp end, int option = 0)
            {
                switch (mode)
                {
                    case AnimMode.To:
                        return this.To(ease, dur, value, option);
                    case AnimMode.From:
                        return this.From(ease, dur, value, option);
                    case AnimMode.By:
                        return this.By(ease, dur, value, option);
                    case AnimMode.FromTo:
                        return this.FromTo(ease, dur, value, end, option);
                    case AnimMode.RedirectTo:
                        return this.RedirectTo(ease, dur, value, end, option);
                    default:
                        return _hash;
                }
            }

        }

        #endregion

        #region Static Factory

        private static ObjectCachePool<TweenHash> _pool = new ObjectCachePool<TweenHash>(-1, () => new TweenHash());

        public static TweenHash GetTweenHash(object target, object id = null, TweenCurveFactory curveFactory = null)
        {
            if (object.ReferenceEquals(target, null)) throw new System.ArgumentNullException(nameof(target));

            TweenHash result;
            if (_pool.TryGetInstance(out result))
            {
                result._targ = target;
                result._id = id;
                result._curveFactory = curveFactory ?? SPTween.CurveFactory;
            }
            else
            {
                result = new TweenHash(target, id, curveFactory);
            }
            return result;
        }

        #endregion

    }

    public static class TweenHashExtensions
    {

        public static TweenHash To(this TweenHash hash, string memberName, EaseStyle ease, float dur, object end, int option = 0)
        {
            return hash.Prop(memberName).To(EaseMethods.GetEase(ease), dur, end, option);
        }

        public static TweenHash To(this TweenHash hash, string memberName, Ease ease, float dur, object end, int option = 0)
        {
            return hash.Prop(memberName).To(ease, dur, end, option);
        }

        public static TweenHash To(this TweenHash hash, string memberName, float dur, object end, int option = 0)
        {
            return hash.Prop(memberName).To(null, dur, end, option);
        }

        public static TweenHash From(this TweenHash hash, string memberName, EaseStyle ease, float dur, object start, int option = 0)
        {
            return hash.Prop(memberName).From(EaseMethods.GetEase(ease), dur, start, option);
        }

        public static TweenHash From(this TweenHash hash, string memberName, Ease ease, float dur, object start, int option = 0)
        {
            return hash.Prop(memberName).From(ease, dur, start, option);
        }

        public static TweenHash From(this TweenHash hash, string memberName, float dur, object start, int option = 0)
        {
            return hash.Prop(memberName).From(null, dur, start, option);
        }

        public static TweenHash By(this TweenHash hash, string memberName, EaseStyle ease, float dur, object amt, int option = 0)
        {
            return hash.Prop(memberName).By(EaseMethods.GetEase(ease), dur, amt, option);
        }

        public static TweenHash By(this TweenHash hash, string memberName, Ease ease, float dur, object amt, int option = 0)
        {
            return hash.Prop(memberName).By(ease, dur, amt, option);
        }

        public static TweenHash By(this TweenHash hash, string memberName, float dur, object amt, int option = 0)
        {
            return hash.Prop(memberName).By(null, dur, amt, option);
        }

        public static TweenHash FromTo(this TweenHash hash, string memberName, EaseStyle ease, float dur, object start, object end, int option = 0)
        {
            return hash.Prop(memberName).FromTo(EaseMethods.GetEase(ease), dur, start, end, option);
        }

        public static TweenHash FromTo(this TweenHash hash, string memberName, Ease ease, float dur, object start, object end, int option = 0)
        {
            return hash.Prop(memberName).FromTo(ease, dur, start, end, option);
        }

        public static TweenHash FromTo(this TweenHash hash, string memberName, float dur, object start, object end, int option = 0)
        {
            return hash.Prop(memberName).FromTo(null, dur, start, end, option);
        }

        public static TweenHash RedirectTo(this TweenHash hash, string memberName, EaseStyle ease, float dur, object start, object end, int option = 0)
        {
            return hash.Prop(memberName).RedirectTo(EaseMethods.GetEase(ease), dur, start, end, option);
        }

        /// <summary>
        /// Creates a curve that will animate from the current value to the end value, but will rescale the duration from how long it should have 
        /// taken from start to end, but already animated up to current.
        /// </summary>
        /// <param name="memberName"></param>
        /// <param name="ease"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="dur"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public static TweenHash RedirectTo(this TweenHash hash, string memberName, Ease ease, float dur, object start, object end, int option = 0)
        {
            return hash.Prop(memberName).RedirectTo(ease, dur, start, end, option);
        }

        /// <summary>
        /// Creates a curve that will animate from the current value to the end value, but will rescale the duration from how long it should have 
        /// taken from start to end, but already animated up to current.
        /// </summary>
        /// <param name="memberName"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="dur"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public static TweenHash RedirectTo(this TweenHash hash, string memberName, float dur, object start, object end, int option = 0)
        {
            return hash.Prop(memberName).RedirectTo(null, dur, start, end, option);
        }

    }

}
