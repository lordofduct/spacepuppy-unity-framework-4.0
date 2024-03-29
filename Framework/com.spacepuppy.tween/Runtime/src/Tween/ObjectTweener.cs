﻿
using System;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Tween
{
    public class ObjectTweener : Tweener
    {

        #region Fields

        private object _target;
        private TweenCurve _curve;
        private object _id;

        #endregion
        
        #region CONSTRUCTOR

        public ObjectTweener(object target, TweenCurve curve)
        {
            if (target == null) throw new System.ArgumentNullException(nameof(target));
            if (curve == null) throw new System.ArgumentNullException(nameof(curve));
            if (curve.Tween != null) throw new System.ArgumentException("Tweener can only be created with an unregistered Curve.", "curve");

            _target = target;
            _curve = curve;
            _curve.Init(this);
        }

        #endregion

        #region Properties

        public override object Id
        {
            get
            {
                return _id ?? _target;
            }
            set
            {
                _id = value;
            }
        }

        public object Target { get { return _target; } }

        public TweenCurve Curve { get { return _curve; } }

        #endregion

        #region Tweener Interface

        protected internal override bool GetTargetIsDestroyed()
        {
            return _target.IsNullOrDestroyed();
        }

        protected internal override float GetPlayHeadLength()
        {
            return _curve.TotalTime;
        }

        protected internal override void DoUpdate(float dt, float t)
        {
            if (_target.IsNullOrDestroyed())
            {
                this.Stop();
                return;
            }
            _curve.Update(_target, dt, t);
        }

        #endregion

    }

}
