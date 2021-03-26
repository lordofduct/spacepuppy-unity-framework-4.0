using UnityEngine;

using com.spacepuppy.Tween.Accessors;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Tween.Curves
{

    public sealed class FollowTargetPositionCurve : MemberCurve<Vector3>
    {

        #region Fields

        private Vector3 _start;
        private System.Func<Vector3> _target;

        #endregion

        #region CONSTRUCTOR

        internal FollowTargetPositionCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor)
            : base(accessor)
        {

        }

        public FollowTargetPositionCurve(FollowTargetPositionAccessor accessor, float dur, Vector3 start, Transform target)
            : base(accessor, null, dur)
        {
            _start = start;
            _target = () => target.position;
        }

        public FollowTargetPositionCurve(FollowTargetPositionAccessor accessor, float dur, Transform start, Transform target)
            : base(accessor, null, dur)
        {
            _start = start.position;
            _target = () => target.position;
        }

        public FollowTargetPositionCurve(FollowTargetPositionAccessor accessor, Ease ease, float dur, Vector3 start, Transform target)
            : base(accessor, ease, dur)
        {
            _start = start;
            _target = () => target.position;
        }

        public FollowTargetPositionCurve(FollowTargetPositionAccessor accessor, Ease ease, float dur, Transform start, Transform target)
            : base(accessor, ease, dur)
        {
            _start = start.position;
            _target = () => target.position;
        }

        protected internal override void Configure(Ease ease, float dur, Vector3 start, Vector3 end, int option = 0)
        {
            this.Ease = ease;
            this.Duration = dur;
            _start = start;
            _target = () => end;
        }

        protected internal override void ConfigureAsRedirectTo(Ease ease, float totalDur, Vector3 c, Vector3 s, Vector3 e, int option = 0)
        {
            this.Ease = ease;
            _start = c;
            _target = () => e;

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

        protected override void ConfigureBoxed(Ease ease, float dur, object start, object end, int option = 0)
        {
            this.Ease = ease;
            this.Duration = dur;

            var trans = GameObjectUtil.GetTransformFromSource(start);
            if (trans != null)
                _start = trans.position;
            else
                _start = ConvertUtil.ToVector3(start);

            var targ = GameObjectUtil.GetTransformFromSource(end);
            _target = () => targ.position;
        }

        protected override void ConfigureAsRedirectToBoxed(Ease ease, float dur, object current, object start, object end, int option = 0)
        {
            this.Ease = ease;
            this.Duration = dur;

            var trans = GameObjectUtil.GetTransformFromSource(start);
            if (trans != null)
                _start = trans.position;
            else
                _start = ConvertUtil.ToVector3(start);

            var targ = GameObjectUtil.GetTransformFromSource(end);
            _target = () => targ.position;
        }

        #endregion

        #region TweenCurve Interface

        protected override Vector3 GetValueAt(float dt, float t)
        {
            return VectorUtil.Lerp(_start, _target(), this.Ease(t, 0f, 1f, this.Duration));
        }

        #endregion
    }

}
