using UnityEngine;

using com.spacepuppy.Utils;

namespace com.spacepuppy.Tween.Curves
{

    public class Vector2LerpMemberCurve : MemberCurve<Vector2>
    {

        #region Fields

        private Vector2 _start;
        private Vector2 _end;

        #endregion

        #region CONSTRUCTOR

        protected internal Vector2LerpMemberCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor) : base(accessor)
        {

        }

        public Vector2LerpMemberCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor<Vector2> accessor, float dur, Vector2 start, Vector2 end) : base(accessor)
        {
            this.Configure(EaseMethods.LinearEaseNone, dur, start, end);
        }

        public Vector2LerpMemberCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor<Vector2> accessor, Ease ease, float dur, Vector2 start, Vector2 end) : base(accessor)
        {
            this.Configure(ease, dur, start, end);
        }

        protected internal override void Configure(Ease ease, float dur, Vector2 start, Vector2 end, int option = 0)
        {
            this.Ease = ease;
            this.Duration = dur;
            _start = start;
            _end = end;
        }

        protected internal override void ConfigureAsRedirectTo(Ease ease, float totalDur, Vector2 c, Vector2 s, Vector2 e, int option = 0)
        {
            this.Ease = ease;
            _start = c;
            _end = e;

            c -= e;
            s -= e;
            if (VectorUtil.NearZeroVector(s))
            {
                this.Duration = 0f;
            }
            else
            {
                this.Duration = totalDur * Vector2.Dot(c, s.normalized) / Vector2.Dot(s, c.normalized);
            }
        }

        #endregion

        #region Properties

        public Vector2 Start { get { return _start; } }

        public Vector2 End { get { return _end; } }

        #endregion

        #region Methods

        protected override Vector2 GetValueAt(float dt, float t)
        {
            if (this.Duration == 0f) return _end;
            t = this.Ease(t, 0f, 1f, this.Duration);
            return Vector2.LerpUnclamped(_start, _end, t);
        }

        #endregion

    }

    public class Vector2SlerpMemberCurve : MemberCurve<Vector2>
    {

        #region Fields

        private Vector2 _start;
        private Vector2 _end;

        #endregion

        #region CONSTRUCTOR

        protected internal Vector2SlerpMemberCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor) : base(accessor)
        {

        }

        public Vector2SlerpMemberCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor, float dur, Vector2 start, Vector2 end) : base(accessor, null, dur)
        {
            _start = start;
            _end = end;
        }

        public Vector2SlerpMemberCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor, Ease ease, float dur, Vector2 start, Vector2 end) : base(accessor, ease, dur)
        {
            _start = start;
            _end = end;
        }

        protected internal override void Configure(Ease ease, float dur, Vector2 start, Vector2 end, int option = 0)
        {
            this.Ease = ease;
            this.Duration = dur;
            _start = start;
            _end = end;
        }

        protected internal override void ConfigureAsRedirectTo(Ease ease, float totalDur, Vector2 c, Vector2 s, Vector2 e, int option = 0)
        {
            this.Ease = ease;
            _start = c;
            _end = e;

            var at = Vector2.Angle(s, e);
            if ((System.Math.Abs(at) < MathUtil.EPSILON))
            {
                this.Duration = 0f;
            }
            else
            {
                var ap = Vector2.Angle(c, e);
                this.Duration = totalDur * ap / at;
            }
        }

        #endregion

        #region Properties

        public Vector2 Start { get { return _start; } }

        public Vector2 End { get { return _end; } }

        #endregion

        #region Methods

        protected override Vector2 GetValueAt(float dt, float t)
        {
            if (this.Duration == 0f) return _end;
            t = this.Ease(t, 0f, 1f, this.Duration);
            return VectorUtil.Slerp(_start, _end, t);
        }

        #endregion

    }

}
