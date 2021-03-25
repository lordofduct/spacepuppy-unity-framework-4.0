using UnityEngine;

using com.spacepuppy.Utils;

namespace com.spacepuppy.Tween.Curves
{

    public class RectMemberCurve : MemberCurve<Rect>
    {

        #region Fields

        private Rect _start;
        private Rect _end;

        #endregion

        #region CONSTRUCTOR

        protected internal RectMemberCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor)
            : base(accessor)
        {

        }

        public RectMemberCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor, float dur, Rect start, Rect end)
            : base(accessor, null, dur)
        {
            _start = start;
            _end = end;
        }

        public RectMemberCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor, float dur, Rect start, Rect end, bool slerp)
            : base(accessor, null, dur)
        {
            _start = start;
            _end = end;
        }

        public RectMemberCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor, Ease ease, float dur, Rect start, Rect end)
            : base(accessor, ease, dur)
        {
            _start = start;
            _end = end;
        }

        protected internal override void Configure(Ease ease, float dur, Rect start, Rect end, int option = 0)
        {
            this.Ease = ease;
            this.Duration = dur;
            _start = start;
            _end = end;
        }

        protected internal override void ConfigureAsRedirectTo(Ease ease, float totalDur, Rect c, Rect s, Rect e, int option = 0)
        {
            this.Ease = ease;
            _start = c;
            _end = e;
            this.Duration = totalDur;
        }

        #endregion

        #region Properties

        public Rect Start
        {
            get { return _start; }
            set { _start = value; }
        }

        public Rect End
        {
            get { return _end; }
            set { _end = value; }
        }

        #endregion

        #region MemberCurve Interface

        protected override Rect GetValueAt(float dt, float t)
        {
            if (this.Duration == 0f) return _end;
            t = this.Ease(t, 0f, 1f, this.Duration);

            return new Rect(Mathf.LerpUnclamped(_start.xMin, _end.xMin, t),
                             Mathf.LerpUnclamped(_start.yMin, _end.yMin, t),
                             Mathf.LerpUnclamped(_start.width, _end.width, t),
                             Mathf.LerpUnclamped(_start.height, _end.height, t));
        }

        #endregion

    }
}
