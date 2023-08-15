using UnityEngine;
using System.Collections.Generic;


namespace com.spacepuppy.Geom
{

    [CreateAssetMenu(fileName = "AnimationCurve", menuName = "Spacepuppy/Geom/AnimatoinCurve")]
    public class AnimationCurveAsset : ScriptableObject, IAnimationCurve
    {

        #region Fields

        [SerializeField]
        private AnimationCurve _curve = AnimationCurve.Constant(0f, 1f, 1f);

        #endregion

        #region Properties

        public AnimationCurve Curve
        {
            get => _curve;
            set => _curve = value;
        }

        #endregion

        #region IAnimationCurve Interface

        public float Evaluate(float time)
        {
            return _curve?.Evaluate(time) ?? 0f;
        }

        #endregion

    }
}
