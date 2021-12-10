using UnityEngine;

using com.spacepuppy.Utils;

namespace com.spacepuppy.Tween.Curves
{

    public class Color32MemberCurve : MemberCurve<Color32>
    {

        #region Fields

        private Color32 _start;
        private Color32 _end;

        #endregion

        #region CONSTRUCTOR

        protected internal Color32MemberCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor)
            : base(accessor)
        {

        }

        public Color32MemberCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor, float dur, Color32 start, Color32 end)
            : base(accessor, null, dur)
        {
            _start = start;
            _end = end;
        }

        public Color32MemberCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor, Ease ease, float dur, Color32 start, Color32 end)
            : base(accessor, ease, dur)
        {
            _start = start;
            _end = end;
        }

        protected internal override void Configure(Ease ease, float dur, Color32 start, Color32 end, int option = 0)
        {
            this.Ease = ease;
            this.Duration = dur;
            _start = start;
            _end = end;
        }

        protected internal override void ConfigureAsRedirectTo(Ease ease, float totalDur, Color32 current, Color32 start, Color32 end, int option = 0)
        {
            this.Ease = ease;
            _start = current;
            _end = end;

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

        #endregion

        #region Properties

        public Color32 Start
        {
            get { return _start; }
            set { _start = value; }
        }

        public Color32 End
        {
            get { return _end; }
            set { _end = value; }
        }

        #endregion

        #region MemberCurve Interface

        protected override Color32 GetValueAt(float dt, float t)
        {
            if (this.Duration == 0f) return _end;
            t = this.Ease(t, 0f, 1f, this.Duration);
            return Color32.LerpUnclamped(_start, _end, t);
        }

        #endregion

    }
}
