using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy.Collections;
using com.spacepuppy.Tween.Accessors;
using com.spacepuppy.Tween.Curves;
using com.spacepuppy.Utils;
using com.spacepuppy.Dynamic.Accessors;

namespace com.spacepuppy.Tween
{
    
    public interface ISupportBoxedConfigurableTweenCurve
    {
        void Configure(Ease ease, float dur, object start, object end, int option = 0);
        void ConfigureAsRedirectTo(Ease ease, float dur, object current, object start, object end, int option = 0);
    }

    public abstract class MemberCurve<TProp> : TweenCurve, ISupportBoxedConfigurableTweenCurve
    {

        #region Fields

        IMemberAccessor _accessor;
        private System.Action<object, TProp> _setter;
        private Ease _ease;
        private float _dur;

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// This MUST exist for reflective creation.
        /// </summary>
        protected MemberCurve(IMemberAccessor accessor)
            : this(accessor, null, 0f)
        {
        }

        protected MemberCurve(IMemberAccessor accessor, Ease ease, float dur)
        {
            if (accessor == null) throw new System.ArgumentNullException(nameof(accessor));

            _accessor = accessor;
            if (accessor is IMemberAccessor<TProp> acc)
            {
                _setter = acc.Set;
            }
            else
            {
                var tp = accessor.GetMemberType();
                if (tp == typeof(TProp))
                    _setter = (t, v) => accessor.Set(t, v);
                else
                    _setter = (t, v) => accessor.Set(t, ConvertUtil.Coerce(v, tp));
            }
            _ease = ease ?? EaseMethods.Linear;
            _dur = dur;
        }

        protected internal abstract void Configure(Ease ease, float dur, TProp start, TProp end, int option = 0);

        protected internal abstract void ConfigureAsRedirectTo(Ease ease, float totalDur, TProp current, TProp start, TProp end, int option = 0);

        #endregion

        #region Properties

        public IMemberAccessor Accessor
        {
            get { return _accessor; }
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
            get { return _dur; }
            protected set { _dur = value; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns the appropriate value of the member on the curve at t, where t is a scalar (t would be 1.0, not 100, for 100%).
        /// </summary>
        /// <param name="t">The percentage of completion across the curve that the member is at.</param>
        /// <returns></returns>
        protected abstract TProp GetValueAt(float dt, float t);

        #endregion

        #region ICurve Interface

        public override float TotalTime
        {
            get { return _dur; }
        }

        public sealed override void Update(object targ, float dt, float t)
        {
            if (t > _dur) t = _dur;
            var value = GetValueAt(dt, t);
            _setter(targ, value);
        }

        #endregion

        #region ISupportBoxedConfigurableTweenCurve Interface

        void ISupportBoxedConfigurableTweenCurve.Configure(Ease ease, float dur, object start, object end, int option = 0)
        {
            this.ConfigureBoxed(ease, dur, start, end, option);
        }

        protected virtual void ConfigureBoxed(Ease ease, float dur, object start, object end, int option = 0)
        {
            this.Configure(ease, dur, ConvertUtil.Coerce<TProp>(start), ConvertUtil.Coerce<TProp>(end), option);
        }

        void ISupportBoxedConfigurableTweenCurve.ConfigureAsRedirectTo(Ease ease, float dur, object current, object start, object end, int option = 0)
        {
            this.ConfigureAsRedirectToBoxed(ease, dur, current, start, end, option);
        }

        protected virtual void ConfigureAsRedirectToBoxed(Ease ease, float dur, object current, object start, object end, int option = 0)
        {
            this.ConfigureAsRedirectTo(ease, dur, ConvertUtil.Coerce<TProp>(current), ConvertUtil.Coerce<TProp>(start), ConvertUtil.Coerce<TProp>(end), option);
        }

        #endregion

    }

}
