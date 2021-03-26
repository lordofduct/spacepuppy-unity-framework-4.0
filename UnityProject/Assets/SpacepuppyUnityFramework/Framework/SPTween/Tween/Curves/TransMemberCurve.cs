
using com.spacepuppy.Geom;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Tween.Curves
{

    public class TransMemberCurve : MemberCurve<Trans>
    {

        #region Fields

        private Trans _start;
        private Trans _end;
        private bool _useSlerp;

        #endregion

        #region CONSTRUCTOR

        protected internal TransMemberCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor)
            : base(accessor)
        {

        }

        public TransMemberCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor, float dur, Trans start, Trans end)
            : base(accessor, null, dur)
        {
            _start = start;
            _end = end;
            _useSlerp = false;
        }

        public TransMemberCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor, float dur, Trans start, Trans end, VectorTweenOptions option)
            : base(accessor, null, dur)
        {
            _start = start;
            _end = end;
            _useSlerp = option == VectorTweenOptions.Slerp;
        }

        public TransMemberCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor, Ease ease, float dur, Trans start, Trans end)
            : base(accessor, ease, dur)
        {
            _start = start;
            _end = end;
            _useSlerp = false;
        }

        public TransMemberCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor, Ease ease, float dur, Trans start, Trans end, VectorTweenOptions option)
            : base(accessor, ease, dur)
        {
            _start = start;
            _end = end;
            _useSlerp = option == VectorTweenOptions.Slerp;
        }

        protected internal override void Configure(Ease ease, float dur, Trans start, Trans end, int option = 0)
        {
            this.Ease = ease;
            this.Duration = dur;
            _start = start;
            _end = end;
            _useSlerp = (VectorTweenOptions)option == VectorTweenOptions.Slerp;
        }

        protected internal override void ConfigureAsRedirectTo(Ease ease, float totalDur, Trans c, Trans s, Trans e, int option = 0)
        {
            //redirectto isn't really supported
            this.Ease = ease;
            _start = c;
            _end = e;
            _useSlerp = (VectorTweenOptions)option == VectorTweenOptions.Slerp;
            this.Duration = totalDur;
        }

        protected override void ConfigureBoxed(Ease ease, float dur, object start, object end, int option = 0)
        {
            this.Configure(ease, dur, Trans.Massage(start), Trans.Massage(end), option);
        }

        protected override void ConfigureAsRedirectToBoxed(Ease ease, float dur, object current, object start, object end, int option = 0)
        {
            this.ConfigureAsRedirectTo(ease, dur, Trans.Massage(current), Trans.Massage(start), Trans.Massage(end), option);
        }

        #endregion

        #region Properties

        public Trans Start
        {
            get { return _start; }
            set { _start = value; }
        }

        public Trans End
        {
            get { return _end; }
            set { _end = value; }
        }

        public bool UseSlerp
        {
            get { return _useSlerp; }
            set { _useSlerp = value; }
        }

        #endregion

        #region MemberCurve Interface

        protected override Trans GetValueAt(float dt, float t)
        {
            if (this.Duration == 0f) return _end;
            t = this.Ease(t, 0f, 1f, this.Duration);
            return (_useSlerp) ? Trans.Slerp(_start, _end, t) : Trans.Lerp(_start, _end, t);
        }

        #endregion

    }
}
