using UnityEngine;
using UnityEngine.Serialization;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Geom;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Events
{

    public class i_RandomizeRotation : AutoTriggerable
    {

        public enum ConfiguredAxis
        {
            X = CartesianAxis.X,
            Y = CartesianAxis.Y,
            Z = CartesianAxis.Z,
            Any = 3
        }

        #region Fields

        [SerializeField()]
        [TriggerableTargetObject.Config(typeof(Transform), DefaultFromSelf = true)]
        private TriggerableTargetObject _target;
        [SerializeField()]
        [FormerlySerializedAs("Axis")]
        private ConfiguredAxis _axis = ConfiguredAxis.Z;
        [SerializeField()]
        [Interval.Config(MinValue = -360f, MaxValue = 360f)]
        private Interval _angleRange = Interval.MinMax(0f, 360f);
        [SerializeField()]
        [Tooltip("An interval to round toward when calculating angle. If set to 45, will result in an angle at some multiple of 45.")]
        [FormerlySerializedAs("Detail")]
        private float _angleInterval;

        [SerializeField]
        [UnityEngine.Serialization.FormerlySerializedAs("SetLocalRotation")]
        private bool _setLocalRotation;
        [SerializeField]
        [UnityEngine.Serialization.FormerlySerializedAs("RotateRelativeToCurrentRotation")]
        private bool _rotateRelativeToCurrentRotation;

        [SerializeField]
        private RandomRef _rng;

        #endregion

        #region CONSTRUCTOR

        public i_RandomizeRotation() : base()
        {
            this.ActivateOn = ActivateEvent.OnStart;
        }

        #endregion

        #region Properties

        public TriggerableTargetObject Target { get { return _target; } }

        public ConfiguredAxis Axis { get { return _axis; } set { _axis = value; } }

        public Interval AngleRange { get { return _angleRange; } set { _angleRange = value; } }

        public float AngleInterval { get { return _angleInterval; } set { _angleInterval = value; } }

        public bool SetLocalRotation { get { return _setLocalRotation; } set { _setLocalRotation = value; } }

        public bool RotateRelativeToCurrentRotation { get { return _rotateRelativeToCurrentRotation; } set { _rotateRelativeToCurrentRotation = value; } }

        public IRandom RNG
        {
            get { return _rng.Value; }
            set { _rng.Value = value; }
        }

        #endregion

        #region ITriggerable Interface

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            var targ = _target.GetTarget<Transform>(arg);
            if (targ == null) return false;

            var rng = _rng.Value ?? RandomUtil.Standard;

            var ax = (this._axis == ConfiguredAxis.Any) ? rng.OnUnitSphere() : TransformUtil.GetAxis((CartesianAxis)this._axis);
            var a = _angleRange.GetPercentage(rng.Next());
            if (this._angleInterval > 0) a = MathUtil.RoundToInterval(a, this._angleInterval, _angleRange.Min);

            //apply rotation
            var q = (_rotateRelativeToCurrentRotation) ? (_setLocalRotation ? targ.localRotation : targ.rotation) : Quaternion.identity;
            q *= Quaternion.AngleAxis(a, ax);

            //set it back to transform
            if (_setLocalRotation)
                targ.localRotation = q;
            else
                targ.rotation = q;
            return true;
        }

        #endregion

    }

}