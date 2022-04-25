using UnityEngine;

namespace com.spacepuppy.Tween
{

    public abstract class EaseSelector
    {

        public abstract Ease GetEase();

    }

    [System.Serializable]
    public class EaseStyleSelector : EaseSelector
    {

        public const string PROP_STYLE = nameof(_style);

        #region Fields

        [SerializeField]
        private EaseStyle _style;

        #endregion

        #region Properties

        public EaseStyle Style
        {
            get => _style;
            set => _style = value;
        }

        #endregion

        public override Ease GetEase()
        {
            return EaseMethods.GetEase(this.Style);
        }
    }

    [System.Serializable]
    public class EaseAnimationCurveSelector : EaseSelector
    {

        public const string PROP_CURVE = nameof(_curve);

        #region Fields

        [SerializeField]
        private AnimationCurve _curve;

        [System.NonSerialized]
        private Ease _ease;

        #endregion

        #region Properties

        public AnimationCurve Curve
        {
            get => _curve;
            set
            {
                _curve = value;
                _ease = null;
            }
        }

        #endregion

        public override Ease GetEase()
        {
            if (_ease == null && Curve != null) _ease = EaseMethods.FromAnimationCurve(_curve);
            return _ease;
        }
    }

}
