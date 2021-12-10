using UnityEngine;
using System.Collections.Generic;
using com.spacepuppy.Geom;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Events
{

    [Infobox("All scale sizes are applied to the transform.localScale. You can not meaningfully effect global scale.")]
    public class i_RandomizeScale : AutoTriggerable
    {

        #region Fields

        [SerializeField]
        [TriggerableTargetObject.Config(typeof(Transform), DefaultFromSelf = true)]
        private TriggerableTargetObject _target;

        [SerializeField]
        [ReorderableArray]
        private List<ScaleImpactInfo> _effects;

        [SerializeField]
        private RandomRef _rng;

        #endregion

        #region CONSTRUCTOR

        public i_RandomizeScale() : base()
        {
            this.ActivateOn = ActivateEvent.OnStart;
        }

        #endregion

        #region Properties

        public TriggerableTargetObject Target => _target;

        public List<ScaleImpactInfo> Effects => _effects;

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

            for(int i = 0; i < _effects.Count; i++)
            {
                _effects[i].Apply(targ, _rng.Value);
            }
            return true;
        }

        #endregion

        #region Special Types

        public enum ScaleMode
        {
            All = -1,
            X = 1,
            Y = 2,
            Z = 4,
        }

        [System.Serializable]
        public struct ScaleImpactInfo
        {
            [EnumFlags]
            [SerializeField]
            public ScaleMode Axis;
            [SerializeField]
            public Interval Range;
            [SerializeField]
            public bool Relative;

            public void Apply(Transform t, IRandom rng)
            {
                var sc = t.localScale;
                var r = (rng ?? RandomUtil.Standard).Range(Range.Max, Range.Min);
                if ((Axis & ScaleMode.X) != 0)
                {
                    sc.x = Relative ? sc.x + r : r;
                }
                if ((Axis & ScaleMode.Y) != 0)
                {
                    sc.y = Relative ? sc.y + r : r;
                }
                if ((Axis & ScaleMode.Z) != 0)
                {
                    sc.z = Relative ? sc.z + r : r;
                }
                t.localScale = sc;
            }
        }

        #endregion

    }

}