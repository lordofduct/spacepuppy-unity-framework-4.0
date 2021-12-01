using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Collections;
using com.spacepuppy.Geom;
using com.spacepuppy.Events;
using com.spacepuppy.Tween;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Events
{

    [Infobox("Leave target blank to use the currently active camera.")]
    public class i_Shake : AutoTriggerable
    {

        #region Fields

        [SerializeField]
        private float _effectPrecedence;

        [SerializeField]
        [Tooltip("Leave blank to use current active camera.")]
        [TriggerableTargetObject.Config(typeof(Transform))]
        private TriggerableTargetObject _target = new TriggerableTargetObject(null);

        [SerializeField]
        [Tooltip("Leave blank to act globally.")]
        [TriggerableTargetObject.Config(typeof(Transform))]
        private TriggerableTargetObject _relativeMotionTarget = new TriggerableTargetObject(null);

        [SerializeField]
        [ReorderableArray]
        private List<AxisEffect> _linear;

        [SerializeField]
        [ReorderableArray]
        private List<AxisEffect> _angular;

        [SerializeField]
        [ReorderableArray]
        private List<AxisEffect> _scale;

        [SerializeField]
        private SPTimePeriod _duration;

        #endregion

        #region Methods

        private System.Collections.IEnumerator DoShake(StateToken state, Transform relativeTarg)
        {
            float t = _duration.Seconds;
            while (t > 0f)
            {
                float dt = _duration.Seconds - t;
                //linear
                if (_linear.Count > 0)
                {
                    Vector3 adjust = Vector3.zero;
                    for (int i = 0; i < _linear.Count; i++)
                    {
                        _linear[i].Effect(ref adjust, dt, _duration.Seconds);
                    }
                    if (relativeTarg != null)
                    {
                        adjust = QuaternionUtil.FromToRotation(state.Targ.rotation, relativeTarg.rotation) * adjust;
                    }
                    state.Targ.localPosition = state.Cache.Position + adjust;
                }

                //angular
                if (_angular.Count > 0)
                {
                    Vector3 adjust = Vector3.zero;
                    for (int i = 0; i < _angular.Count; i++)
                    {
                        _angular[i].Effect(ref adjust, dt, _duration.Seconds);
                    }
                    if (relativeTarg != null)
                    {
                        adjust = QuaternionUtil.FromToRotation(state.Targ.rotation, relativeTarg.rotation) * adjust;
                    }
                    state.Targ.localEulerAngles = state.Cache.Rotation.eulerAngles + adjust;
                }

                //scale
                if (_scale.Count > 0)
                {
                    Vector3 adjust = Vector3.zero;
                    for (int i = 0; i < _scale.Count; i++)
                    {
                        _scale[i].Effect(ref adjust, dt, _duration.Seconds);
                    }
                    state.Targ.localScale = state.Cache.Scale + adjust;
                }

                yield return null;
                t -= _duration.TimeSupplier.Delta;
            }

            state.Cache.SetToLocal(state.Targ);
            state.Dispose();
        }

        #endregion

        #region Triggerable Interface

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            if (_linear.Count == 0 && _angular.Count == 0 && _scale.Count == 0) return false;

            var targ = _target.GetTarget<Transform>(arg);
            if (targ == null) return false;

            var manager = targ.AddOrGetComponent<RadicalCoroutineManager>();
            var routine = manager.Find(r => (r.Tag is StateToken st) && st.Targ == targ);
            StateToken state;
            if (routine != null)
            {
                state = routine.Tag as StateToken;
                if (state?.CurrentPrecedence > _effectPrecedence) return false;

                state.Cache.SetToLocal(targ);
                routine.Cancel();
                state.Dispose();
            }

            state = _pool.GetInstance();
            state.CurrentPrecedence = _effectPrecedence;
            state.Targ = targ;
            state.Cache = Trans.GetLocal(targ);
            routine = manager.StartRadicalCoroutine(this.DoShake(state, _relativeMotionTarget.GetTarget<Transform>(arg)));
            routine.Tag = state;
            return true;
        }

        #endregion

        #region Special Types

        [System.Serializable]
        public class AxisEffect
        {

            [EnumFlags]
            public EffectedAxis Axis;
            public ShakeType Shake;
            public EaseStyle IntensityEase;
            public float Intensity;
            public float Frequency;

            public float GetValue(float t, float total)
            {
                switch (this.Shake)
                {
                    case ShakeType.Sine:
                        return Mathf.Sin(t * Mathf.PI * Frequency) * Intensity * EaseMethods.GetEase(IntensityEase)(t, 1f, -1f, total);
                    case ShakeType.Triangle:
                        return (Mathf.PingPong(t * this.Frequency, 1f) * 2f - 1f) * Intensity * EaseMethods.GetEase(IntensityEase)(t, 1f, -1f, total);
                    case ShakeType.SawTooth:
                        return (((t * this.Frequency) % 1f) * 2f - 1f) * Intensity * EaseMethods.GetEase(IntensityEase)(t, 1f, -1f, total);
                    case ShakeType.Noise:
                        return (Mathf.PerlinNoise(t * Frequency, Time.unscaledTime) * 2f - 1f) * Intensity * EaseMethods.GetEase(IntensityEase)(t, 1f, -1f, total);
                    default:
                        return 0f;
                }
            }

            public void Effect(ref Vector3 v, float t, float totalTime)
            {
                var d = this.GetValue(t, totalTime);
                if ((this.Axis & EffectedAxis.X) != 0)
                {
                    v.x += d;
                }
                if ((this.Axis & EffectedAxis.Y) != 0)
                {
                    v.y += d;
                }
                if ((this.Axis & EffectedAxis.Z) != 0)
                {
                    v.z += d;
                }
            }

        }

        public enum EffectedAxis
        {
            All = -1,
            X = 1,
            Y = 2,
            Z = 4,
        }


        public enum ShakeType
        {
            Sine,
            Triangle,
            SawTooth,
            Noise
        }

        private static readonly ObjectCachePool<StateToken> _pool = new ObjectCachePool<StateToken>(-1, () => new StateToken());
        private class StateToken : System.IDisposable
        {

            public float CurrentPrecedence;
            public Transform Targ;
            public Trans Cache;

            public void Dispose()
            {
                this.CurrentPrecedence = 0f;
                this.Targ = null;
                this.Cache = default(Trans);
            }

        }

        #endregion

    }

}