
using com.spacepuppy.Utils;

namespace com.spacepuppy.Tween.Curves
{

    /// <summary>
    /// The BoolMemberCurve favors 'true'.
    /// </summary>
    public class BoolMemberCurve : MemberCurve<bool>
    {

        #region Fields

        private bool _start;
        private bool _end;

        #endregion

        #region CONSTRUCTOR

        protected internal BoolMemberCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor) : base(accessor)
        {

        }

        public BoolMemberCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor, float dur, bool start, bool end) : base(accessor, null, dur)
        {
            _start = start;
            _end = end;
        }

        public BoolMemberCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor, Ease ease, float dur, bool start, bool end) : base(accessor, ease, dur)
        {
            _start = start;
            _end = end;
        }

        protected internal override void Configure(Ease ease, float dur, bool start, bool end, int option = 0)
        {
            this.Ease = ease;
            this.Duration = dur;
            _start = start;
            _end = end;
        }

        protected internal override void ConfigureAsRedirectTo(Ease ease, float totalDur, bool c, bool s, bool e, int option = 0)
        {
            this.Ease = ease;
            _start = c;
            _end = e;
            this.Duration = (c == e) ? 0f : totalDur;
        }

        #endregion

        #region Properties

        public bool Start
        {
            get { return _start; }
            set { _start = value; }
        }

        public bool End
        {
            get { return _end; }
            set { _end = value; }
        }

        #endregion

        #region MemberCurve Interface

        protected override bool GetValueAt(float dt, float t)
        {
            if (this.Duration == 0f) return _end;
            //return this.Ease(t, _start, _end - _start, this.Duration);

            if (_end)
                return t > 0f;
            else
                return t < this.Duration;
        }

        #endregion

    }
}
