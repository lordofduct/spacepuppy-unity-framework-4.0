using UnityEngine;

using com.spacepuppy.Utils;

namespace com.spacepuppy.Tween.Curves
{

    public class Vector4MemberCurve : MemberCurve<Vector4>
    {

        #region Fields

        private Vector4 _start;
        private Vector4 _end;

        #endregion

        #region CONSTRUCTOR

        protected internal Vector4MemberCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor) : base(accessor)
        {

        }

        public Vector4MemberCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor, float dur, Vector4 start, Vector4 end)
             : base(accessor, null, dur)
        {
            _start = start;
            _end = end;
        }

        public Vector4MemberCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor, Ease ease, float dur, Vector4 start, Vector4 end)
             : base(accessor, ease, dur)
        {
            _start = start;
            _end = end;
        }

        protected internal override void Configure(Ease ease, float dur, Vector4 start, Vector4 end, int option = 0)
        {
            this.Ease = ease;
            this.Duration = dur;
            _start = start;
            _end = end;
        }

        protected internal override void ConfigureAsRedirectTo(Ease ease, float totalDur, Vector4 c, Vector4 s, Vector4 e, int option = 0)
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
                this.Duration = totalDur * Vector4.Dot(c, s.normalized) / Vector4.Dot(s, c.normalized);
            }
        }

        #endregion

        #region Properties

        public Vector4 Start
        {
            get { return _start; }
            set { _start = value; }
        }

        public Vector4 End
        {
            get { return _end; }
            set { _end = value; }
        }

        #endregion

        #region MemberCurve Interface

        protected override Vector4 GetValueAt(float dt, float t)
        {
            if (this.Duration == 0f) return _end;
            t = this.Ease(t, 0f, 1f, this.Duration);
            return Vector4.LerpUnclamped(_start, _end, t);
        }

        #endregion

    }
}
