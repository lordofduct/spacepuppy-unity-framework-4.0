using System;

using com.spacepuppy.Utils;

namespace com.spacepuppy.Tween.Curves
{

    public class NumericMemberCurve<T> : MemberCurve<T> where T : IConvertible
    {

        #region Fields

        private T _unconvertedStart;
        private T _uncovertedEnd;
        private float _start;
        private float _end;

        #endregion

        #region CONSTRUCTOR

        protected internal NumericMemberCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor)
            : base(accessor)
        {

        }

        public NumericMemberCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor, float dur, T start, T end)
            : base(accessor, null, dur)
        {
            _unconvertedStart = start;
            _uncovertedEnd = end;
            _start = ConvertUtil.ToSingle(start);
            _end = ConvertUtil.ToSingle(end);
        }

        public NumericMemberCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor, Ease ease, float dur, T start, T end)
            : base(accessor, ease, dur)
        {
            _unconvertedStart = start;
            _uncovertedEnd = end;
            _start = ConvertUtil.ToSingle(start);
            _end = ConvertUtil.ToSingle(end);
        }

        protected internal override void Configure(Ease ease, float dur, T start, T end, int option = 0)
        {
            this.Ease = ease;
            this.Duration = dur;
            _start = ConvertUtil.ToSingle(start);
            _end = ConvertUtil.ToSingle(end);
            this.Duration = dur;
        }

        protected internal override void ConfigureAsRedirectTo(Ease ease, float totalDur, T current, T start, T end, int option = 0)
        {
            var c = ConvertUtil.ToSingle(current);
            var s = ConvertUtil.ToSingle(start);
            var e = ConvertUtil.ToSingle(end);

            this.Ease = ease;
            _unconvertedStart = start;
            _uncovertedEnd = end;
            _start = c;
            _end = e;
            c -= e;
            s -= e;
            this.Duration = (float)(System.Math.Abs(s) < MathUtil.DBL_EPSILON ? 0f : totalDur * c / s);
        }

        #endregion

        #region Properties

        public T Start
        {
            get { return _unconvertedStart; }
            set
            {
                _unconvertedStart = value;
                _start = ConvertUtil.ToSingle(value);
            }
        }

        public T End
        {
            get { return _uncovertedEnd; }
            set
            {
                _uncovertedEnd = value;
                _end = ConvertUtil.ToSingle(value);
            }
        }

        #endregion

        #region MemberCurve Interface

        protected override T GetValueAt(float dt, float t)
        {
            if (this.Duration == 0) return _uncovertedEnd;
            var v = this.Ease(t, (float)_start, (float)_end - (float)_start, this.Duration);
            return ConvertUtil.ToPrim<T>(v);
        }

        #endregion

    }

    /*

    public class NumericMemberCurve : MemberCurve<double>
    {

        #region Fields

        private double _start;
        private double _end;

        #endregion

        #region CONSTRUCTOR

        protected internal NumericMemberCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor)
            : base(accessor)
        {

        }

        public NumericMemberCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor, float dur, double start, double end)
            : base(accessor, null, dur)
        {
            _start = start;
            _end = end;
        }

        public NumericMemberCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor, Ease ease, float dur, double start, double end)
            : base(accessor, ease, dur)
        {
            _start = start;
            _end = end;
        }

        protected internal override void Configure(Ease ease, float dur, double start, double end, int option = 0)
        {
            _start = start;
            _end = end;
            this.Duration = dur;
        }

        protected internal override void ConfigureAsRedirectTo(Ease ease, float totalDur, double c, double s, double e, int option = 0)
        {
            _start = c;
            _end = e;

            c -= e;
            s -= e;
            this.Duration = (float)(System.Math.Abs(s) < MathUtil.DBL_EPSILON ? 0f : totalDur * c / s);
        }

        #endregion

        #region Properties

        public double Start
        {
            get { return _start; }
            set { _start = value; }
        }

        public double End
        {
            get { return _end; }
            set { _end = value; }
        }

        #endregion

        #region MemberCurve Interface

        protected override double GetValueAt(float dt, float t)
        {
            if (this.Duration == 0) return _end;
            return this.Ease(t, (float)_start, (float)_end - (float)_start, this.Duration);
        }

        #endregion

    }

    */
}
