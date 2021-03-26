using UnityEngine;

using com.spacepuppy.Utils;
using com.spacepuppy.Dynamic.Accessors;

namespace com.spacepuppy.Tween.Curves
{

    public sealed class QuaternionMemberCurveGenerator : ITweenCurveGenerator
    {
        public TweenCurve CreateCurve(IMemberAccessor accessor, int option)
        {
            return TweenCurveFactory.CreateUninitializedQuaternionMemberCurve(accessor, option);
        }

        public System.Type GetExpectedMemberType(int option)
        {
            switch ((QuaternionTweenOptions)option)
            {
                case QuaternionTweenOptions.Long:
                    return typeof(UnityEngine.Vector3);
                default:
                    return typeof(UnityEngine.Quaternion);
            }
        }

        public System.Type GetOptionEnumType()
        {
            return typeof(QuaternionTweenOptions);
        }
    }

    public class QuaternionMemberCurve : MemberCurve<Quaternion>
    {

        #region Fields

        private Quaternion _start;
        private Quaternion _end;
        private Vector3 _startLong;
        private Vector3 _endLong;
        private QuaternionTweenOptions _option;

        #endregion

        #region CONSTRUCTOR

        protected internal QuaternionMemberCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor)
            : base(accessor)
        {

        }

        public QuaternionMemberCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor, float dur, Quaternion start, Quaternion end)
            : base(accessor, null, dur)
        {
            _start = start;
            _end = end;
            _startLong = start.eulerAngles;
            _endLong = end.eulerAngles;
            _option = QuaternionTweenOptions.Spherical;
        }

        public QuaternionMemberCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor, float dur, Quaternion start, Quaternion end, QuaternionTweenOptions mode)
            : base(accessor, null, dur)
        {
            _start = start;
            _end = end;
            _startLong = start.eulerAngles;
            _endLong = end.eulerAngles;
            _option = mode == QuaternionTweenOptions.Long ? QuaternionTweenOptions.Spherical : mode;
        }

        public QuaternionMemberCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor, Ease ease, float dur, Quaternion start, Quaternion end)
            : base(accessor, ease, dur)
        {
            _start = start;
            _end = end;
            _startLong = start.eulerAngles;
            _endLong = end.eulerAngles;
            _option = QuaternionTweenOptions.Spherical;
        }

        public QuaternionMemberCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor, Ease ease, float dur, Quaternion start, Quaternion end, QuaternionTweenOptions mode)
            : base(accessor, ease, dur)
        {
            _start = start;
            _end = end;
            _startLong = start.eulerAngles;
            _endLong = end.eulerAngles;
            _option = mode == QuaternionTweenOptions.Long ? QuaternionTweenOptions.Spherical : mode;
        }

        public QuaternionMemberCurve(com.spacepuppy.Dynamic.Accessors.IMemberAccessor accessor, Ease ease, float dur, Vector3 eulerStart, Vector3 eulerEnd, QuaternionTweenOptions mode)
            : base(accessor, ease, dur)
        {
            _option = mode;
            _start = Quaternion.Euler(eulerStart);
            _end = Quaternion.Euler(eulerEnd);
            _startLong = eulerStart;
            _endLong = eulerEnd;
        }

        protected internal override void Configure(Ease ease, float dur, Quaternion start, Quaternion end, int option = 0)
        {
            this.Ease = ease;
            this.Duration = dur;
            _option = ConvertUtil.ToEnum<QuaternionTweenOptions>(option);
            if (_option == QuaternionTweenOptions.Long)
            {
                _startLong = start.eulerAngles;
                _endLong = end.eulerAngles;
                _start = Quaternion.Euler(_startLong);
                _end = Quaternion.Euler(_endLong);
            }
            else
            {
                _start = start;
                _end = end;
                _startLong = _start.eulerAngles;
                _endLong = _end.eulerAngles;
            }
        }

        protected internal override void ConfigureAsRedirectTo(Ease ease, float totalDur, Quaternion current, Quaternion start, Quaternion end, int option = 0)
        {
            this.Ease = ease;
            _option = ConvertUtil.ToEnum<QuaternionTweenOptions>(option);
            if (_option == QuaternionTweenOptions.Long)
            {
                var c = current.eulerAngles;
                var s = start.eulerAngles;
                var e = end.eulerAngles;

                c.x = MathUtil.NormalizeAngleToRange(c.x, s.x, e.x, false);
                c.y = MathUtil.NormalizeAngleToRange(c.y, s.y, e.y, false);
                c.z = MathUtil.NormalizeAngleToRange(c.z, s.z, e.z, false);

                _startLong = c;
                _endLong = e;
                _start = Quaternion.Euler(_startLong);
                _end = Quaternion.Euler(_endLong);

                c -= s;
                e -= s;
                this.Duration = totalDur * (VectorUtil.NearZeroVector(e) ? 0f : 1f - c.magnitude / e.magnitude);
            }
            else
            {
                //treat as quat
                _start = current;
                _end = end;
                _startLong = _start.eulerAngles;
                _endLong = _end.eulerAngles;

                var at = Quaternion.Angle(start, end);
                if ((System.Math.Abs(at) < MathUtil.EPSILON))
                {
                    this.Duration = 0f;
                }
                else
                {
                    var ap = Quaternion.Angle(start, current);
                    this.Duration = (1f - ap / at) * totalDur;
                }
            }
        }

        protected override void ConfigureBoxed(Ease ease, float dur, object start, object end, int option = 0)
        {
            this.Configure(ease, dur, QuaternionUtil.MassageAsQuaternion(start), QuaternionUtil.MassageAsQuaternion(end), option);
        }

        protected override void ConfigureAsRedirectToBoxed(Ease ease, float dur, object current, object start, object end, int option = 0)
        {
            this.ConfigureAsRedirectTo(ease, dur, QuaternionUtil.MassageAsQuaternion(current), QuaternionUtil.MassageAsQuaternion(start), QuaternionUtil.MassageAsQuaternion(end), option);
        }

        #endregion

        #region Properties

        public Quaternion Start
        {
            get { return _start; }
            set
            {
                _start = value;
                _startLong = value.eulerAngles;
            }
        }

        public Quaternion End
        {
            get { return _end; }
            set
            {
                _end = value;
                _endLong = value.eulerAngles;
            }
        }

        public Vector3 StartEuler
        {
            get { return _startLong; }
            set
            {
                _startLong = value;
                _start = Quaternion.Euler(value);
            }
        }

        public Vector3 EndEuler
        {
            get { return _endLong; }
            set
            {
                _endLong = value;
                _end = Quaternion.Euler(value);
            }
        }

        public QuaternionTweenOptions Option
        {
            get { return _option; }
            set { _option = value; }
        }

        #endregion

        #region MemberCurve Interface

        protected override Quaternion GetValueAt(float dt, float t)
        {
            if (this.Duration == 0f) return _end;
            t = this.Ease(t, 0f, 1f, this.Duration);
            switch (_option)
            {
                case QuaternionTweenOptions.Spherical:
                    return Quaternion.SlerpUnclamped(_start, _end, t);
                case QuaternionTweenOptions.Linear:
                    return Quaternion.LerpUnclamped(_start, _end, t);
                case QuaternionTweenOptions.Long:
                    {
                        var v = Vector3.LerpUnclamped(_startLong, _endLong, t);
                        return Quaternion.Euler(v);
                    }
                default:
                    return Quaternion.identity;
            }
        }

        #endregion

    }

}
