
using com.spacepuppy.Utils;

namespace com.spacepuppy.Tween.Curves
{

    public class FloatMemberCurve : MemberCurve<float>
    {

        #region Fields

        private float _start;
        private float _end;

        #endregion

        #region CONSTRUCTOR

        protected internal FloatMemberCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor)
            : base(accessor)
        {

        }

        public FloatMemberCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor, float dur, float start, float end)
            : base(accessor, null, dur)
        {
            _start = start;
            _end = end;
        }

        public FloatMemberCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor, Ease ease, float dur, float start, float end)
            : base(accessor, ease, dur)
        {
            _start = start;
            _end = end;
        }

        protected internal override void Configure(Ease ease, float dur, float start, float end, int option = 0)
        {
            this.Ease = ease;
            this.Duration = dur;
            _start = start;
            _end = end;
        }

        protected internal override void ConfigureAsRedirectTo(Ease ease, float totalDur, float c, float s, float e, int option = 0)
        {
            this.Ease = ease;
            _start = c;
            _end = e;

            c -= e;
            s -= e;
            this.Duration = System.Math.Abs(s) < MathUtil.EPSILON ? 0f : totalDur * c / s;
        }

        #endregion

        #region Properties

        public float Start
        {
            get { return _start; }
            set { _start = value; }
        }

        public float End
        {
            get { return _end; }
            set { _end = value; }
        }

        #endregion

        #region MemberCurve Interface

        protected override float GetValueAt(float dt, float t)
        {
            if (this.Duration == 0f) return _end;
            return this.Ease(t, _start, _end - _start, this.Duration);
        }

        #endregion

    }
}
