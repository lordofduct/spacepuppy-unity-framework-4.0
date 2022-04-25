using UnityEngine;

using com.spacepuppy.Utils;

namespace com.spacepuppy.Tween.Curves
{

    public class Vector3LerpMemberCurve : MemberCurve<Vector3>
    {

        #region Fields

        private Vector3 _start;
        private Vector3 _end;

        #endregion

        #region CONSTRUCTOR

        protected internal Vector3LerpMemberCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor) : base(accessor)
        {

        }

        public Vector3LerpMemberCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor, float dur, Vector3 start, Vector3 end) : base(accessor, null, dur)
        {
            _start = start;
            _end = end;
        }

        public Vector3LerpMemberCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor, Ease ease, float dur, Vector3 start, Vector3 end) : base(accessor, ease, dur)
        {
            _start = start;
            _end = end;
        }

        protected internal override void Configure(Ease ease, float dur, Vector3 start, Vector3 end, int option = 0)
        {
            this.Ease = ease;
            this.Duration = dur;
            _start = start;
            _end = end;
        }

        protected internal override void ConfigureAsRedirectTo(Ease ease, float totalDur, Vector3 c, Vector3 s, Vector3 e, int option = 0)
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
                this.Duration = totalDur * Vector3.Dot(c, s.normalized) / Vector3.Dot(s, c.normalized);
            }
        }

        #endregion

        #region Properties

        public Vector3 Start { get { return _start; } }

        public Vector3 End { get { return _end; } }

        #endregion

        #region Methods

        protected override Vector3 GetValueAt(float dt, float t)
        {
            if (this.Duration == 0f) return _end;
            t = this.Ease(t, 0f, 1f, this.Duration);
            return Vector3.LerpUnclamped(_start, _end, t);
        }

        #endregion

    }

    public class Vector3SlerpMemberCurve : MemberCurve<Vector3>
    {

        #region Fields

        private Vector3 _start;
        private Vector3 _end;

        #endregion

        #region CONSTRUCTOR

        protected internal Vector3SlerpMemberCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor) : base(accessor)
        {

        }

        public Vector3SlerpMemberCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor, float dur, Vector3 start, Vector3 end) : base(accessor, null, dur)
        {
            _start = start;
            _end = end;
        }

        public Vector3SlerpMemberCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor, Ease ease, float dur, Vector3 start, Vector3 end) : base(accessor, ease, dur)
        {
            _start = start;
            _end = end;
        }

        protected internal override void Configure(Ease ease, float dur, Vector3 start, Vector3 end, int option = 0)
        {
            this.Ease = ease;
            this.Duration = dur;
            _start = start;
            _end = end;
        }

        protected internal override void ConfigureAsRedirectTo(Ease ease, float totalDur, Vector3 c, Vector3 s, Vector3 e, int option = 0)
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
                this.Duration = totalDur * Vector3.Dot(c, s.normalized) / Vector3.Dot(s, c.normalized);
            }
        }

        #endregion

        #region Properties

        public Vector3 Start { get { return _start; } }

        public Vector3 End { get { return _end; } }

        #endregion

        #region Methods

        protected override Vector3 GetValueAt(float dt, float t)
        {
            if (this.Duration == 0f) return _end;
            t = this.Ease(t, 0f, 1f, this.Duration);
            return Vector3.SlerpUnclamped(_start, _end, t);
        }

        #endregion

    }

    public class Vector3ScaleMemberCurve : MemberCurve<Vector3>
    {

        #region Fields

        private Vector3 _start;
        private Vector3 _end;

        #endregion

        #region CONSTRUCTOR

        protected internal Vector3ScaleMemberCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor) : base(accessor)
        {

        }

        public Vector3ScaleMemberCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor, float dur, Vector3 start, Vector3 end) : base(accessor, null, dur)
        {
            _start = start;
            _end = end;
        }

        public Vector3ScaleMemberCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor, Ease ease, float dur, Vector3 start, Vector3 end) : base(accessor, ease, dur)
        {
            _start = start;
            _end = end;
        }

        protected internal override void Configure(Ease ease, float dur, Vector3 start, Vector3 end, int option = 0)
        {
            this.Ease = ease;
            this.Duration = dur;
            _start = start;
            _end = end;
        }

        protected internal override void ConfigureAsRedirectTo(Ease ease, float totalDur, Vector3 c, Vector3 s, Vector3 e, int option = 0)
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
                this.Duration = totalDur * Vector3.Dot(c, s.normalized) / Vector3.Dot(s, c.normalized);
            }
        }

        #endregion

        #region Properties

        public Vector3 Start { get { return _start; } }

        public Vector3 End { get { return _end; } }

        #endregion

        #region Methods

        protected override Vector3 GetValueAt(float dt, float t)
        {
            if (this.Duration == 0f) return _end;
            t = this.Ease(t, 0f, 1f, this.Duration);

            if (t < this.Duration)
                return _start * t + (_end - _start) * t;
            else
                return _end;
        }

        #endregion

    }

}
