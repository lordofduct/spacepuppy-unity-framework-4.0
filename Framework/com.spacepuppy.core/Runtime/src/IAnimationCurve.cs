using UnityEngine;
using System.Collections.Generic;
using com.spacepuppy.Project;
using com.spacepuppy.Utils;

namespace com.spacepuppy
{

    public interface IAnimationCurve
    {
        float Evaluate(float time);
    }

    [System.Serializable]
    public struct AnimationCurveRef : IAnimationCurve
    {

        #region Fields

        [SerializeReference]
        private IValue _curve;

        #endregion

        #region CONSTRUCTOR

        public AnimationCurveRef(AnimationCurve curve)
        {
            _curve = CreateInnerValuewrapper(curve) as IValue;
        }

        public AnimationCurveRef(IAnimationCurve curve)
        {
            if (curve is AnimationCurveRef direct)
            {
                _curve = direct._curve;
            }
            else
            {
                _curve = CreateInnerValuewrapper(curve) as IValue;
            }
        }

        public AnimationCurveRef(AnimationCurveRef curve)
        {
            _curve = curve._curve;
        }

        #endregion

        #region Properties

        public bool IsValid => _curve != null;

        public object Curve
        {
            get => _curve?.Value;
            set => _curve = CreateInnerValuewrapper(value) as IValue;
        }

        #endregion

        #region IAnimationCurve Interface

        public float Evaluate(float time) => _curve?.Evaluate(time) ?? 0f;

        #endregion

        #region Static Operators

        public static implicit operator AnimationCurveRef(AnimationCurve curve) => new AnimationCurveRef(curve);

        #endregion

        #region Special Types
        
        public static object CreateInnerValuewrapper(object value)
        {
            switch (value)
            {
                case AnimationCurve uac:
                    return new AnimationCurveValue() { curve = uac };
                case UnityEngine.Object obj:
                    return new ObjectRefValue() { curve = ObjUtil.GetAsFromSource<IAnimationCurve>(value) as UnityEngine.Object };
                case IAnimationCurve iac:
                    return new IAnimationCurveValue() { curve = iac };
                default:
                    return null;
            }
        }
        public static object CreateInnerValuewrapper(AnimationCurve value) => value != null ? new AnimationCurveValue() { curve = value } : null;

        public interface IValue
        {
            object Value { get; }
            float Evaluate(float time);
        }

        [System.Serializable]
        private struct AnimationCurveValue : IValue
        {
            public AnimationCurve curve;
            public object Value => curve;
            public float Evaluate(float time) => curve?.Evaluate(time) ?? 0f;
        }

        [System.Serializable]
        private struct ObjectRefValue : IValue
        {
            public UnityEngine.Object curve;
            public object Value => curve;
            public float Evaluate(float time) => (curve as IAnimationCurve)?.Evaluate(time) ?? 0f;
        }

        [System.Serializable]
        private struct IAnimationCurveValue : IValue
        {
            public IAnimationCurve curve;
            public object Value => curve;
            public float Evaluate(float time) => curve?.Evaluate(time) ?? 0f;
        }

        #endregion

    }

}
