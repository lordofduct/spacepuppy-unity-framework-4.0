 using UnityEngine;
 using System.Collections;
using com.spacepuppy.Geom;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Events
{

	public class i_RandomizePosition : AutoTriggerable
	{

        #region Fields

        [SerializeField]
        [TriggerableTargetObject.Config(typeof(Transform), DefaultFromSelf = true)]
        private TriggerableTargetObject _target;

        [SerializeField]
        private Interval _x;
        [SerializeField]
        private Interval _y;
        [SerializeField]
        private Interval _z;

        [SerializeField]
        private bool _setLocalPosition;
        [SerializeField]
        private bool _adjustPositionRelative;

        [SerializeField]
        private RandomRef _rng;

        #endregion

        #region CONSTRUCTOR

        public i_RandomizePosition() : base()
        {
            this.ActivateOn = ActivateEvent.OnStart;
        }

        #endregion

        #region Properties

        public TriggerableTargetObject Target { get { return _target; } }

        public Interval X { get { return _x; } set { _x = value; } }

        public Interval Y { get { return _y; } set { _y = value; } }

        public Interval Z { get { return _z; } set { _z = value; } }

        public bool SetLocalPosition { get { return _setLocalPosition; } set { _setLocalPosition = value; } }

        public bool AdjustPositionRelative { get { return _adjustPositionRelative; } set { _adjustPositionRelative = value; } }

        public IRandom RNG
        {
            get { return _rng.Value; }
            set { _rng.Value = value; }
        }

        #endregion

        #region Triggerable Interface

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            var targ = _target.GetTarget<Transform>(arg);
            if (targ == null) return false;

            var rng = _rng.Value ?? RandomUtil.Standard;
            var pos = new Vector3(
                rng.Range(_x.Max, _x.Min),
                rng.Range(_y.Max, _y.Min),
                rng.Range(_z.Max, _z.Min));

            if (_setLocalPosition)
            {
                targ.localPosition = _adjustPositionRelative ? targ.localPosition + pos : pos;
            }
            else
            {
                targ.position = _adjustPositionRelative ? targ.position + pos : pos;
            }
            return true;
        }

        #endregion

    }

}