using UnityEngine;

using com.spacepuppy.Utils;

namespace com.spacepuppy.Tween.Curves
{

    public class ColorMemberCurve : MemberCurve<Color>
    {

        #region Fields

        private Color _start;
        private Color _end;
        private bool _useSlerp;

        #endregion

        #region CONSTRUCTOR

        protected internal ColorMemberCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor)
            : base(accessor)
        {

        }

        public ColorMemberCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor, float dur, Color start, Color end)
            : base(accessor, null, dur)
        {
            _start = start;
            _end = end;
            _useSlerp = false;
        }

        public ColorMemberCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor, Ease ease, float dur, Color start, Color end)
            : base(accessor, ease, dur)
        {
            _start = start;
            _end = end;
            _useSlerp = false;
        }

        public ColorMemberCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor, float dur, Color start, Color end, VectorTweenOptions option)
            : base(accessor, null, dur)
        {
            _start = start;
            _end = end;
            _useSlerp = option == VectorTweenOptions.Slerp;
        }

        public ColorMemberCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor, Ease ease, float dur, Color start, Color end, VectorTweenOptions option)
            : base(accessor, ease, dur)
        {
            _start = start;
            _end = end;
            _useSlerp = option == VectorTweenOptions.Slerp;
        }

        protected internal override void Configure(Ease ease, float dur, Color start, Color end, int option = 0)
        {
            this.Ease = ease;
            this.Duration = dur;
            _start = start;
            _end = end;
            _useSlerp = (VectorTweenOptions)option == VectorTweenOptions.Slerp;
        }

        protected internal override void ConfigureAsRedirectTo(Ease ease, float totalDur, Color current, Color start, Color end, int option = 0)
        {
            this.Ease = ease;
            _start = current;
            _end = end;
            _useSlerp = (VectorTweenOptions)option == VectorTweenOptions.Slerp;

            if (_useSlerp)
            {
                var c = (ColorHSV)_start;
                var s = (ColorHSV)start;
                var e = (ColorHSV)_end;

                var t = ColorHSV.InverseSlerp(c, s, e);
                if (float.IsNaN(t))
                    this.Duration = totalDur;
                else
                    this.Duration = (1f - t) * totalDur;
            }
            else
            {
                var c = ConvertUtil.ToVector4(_start);
                var s = ConvertUtil.ToVector4(start);
                var e = ConvertUtil.ToVector4(_end);

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
        }

        #endregion

        #region Properties

        public Color Start
        {
            get { return _start; }
            set { _start = value; }
        }

        public Color End
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

        protected override Color GetValueAt(float dt, float t)
        {
            if (this.Duration == 0f) return _end;
            t = this.Ease(t, 0f, 1f, this.Duration);
            return _useSlerp ? Slerp(_start, _end, t) : Color.LerpUnclamped(_start, _end, t);
        }

        private static Color Slerp(Color a, Color b, float t)
        {
            return (Color)ColorHSV.Slerp((ColorHSV)a, (ColorHSV)b, t);
        }

        #endregion

    }

}
