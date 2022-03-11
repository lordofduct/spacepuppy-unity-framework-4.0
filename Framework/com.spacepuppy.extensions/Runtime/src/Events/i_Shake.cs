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
        private SPTimePeriod _duration = 0.33f;

        #endregion

        #region Triggerable Interface

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            if ((_linear?.Count ?? 0) == 0 && (_angular?.Count ?? 0) == 0 && (_scale?.Count ?? 0) == 0) return false;

            var targ = _target.GetTarget<Transform>(arg);
            if (targ == null) return false;
            
            SPTween.KillAll(targ, "*SHAKE IT*");

            var state = _pool.GetInstance();
            state.Owner = this;
            state.CurrentPrecedence = _effectPrecedence;
            state.Targ = targ;
            state.RelativeTarg = _relativeMotionTarget.GetTarget<Transform>(arg);
            state.Cache = Trans.GetLocal(targ);

            var tween = SPTween.PlayCurve(targ, state, "*SHAKE IT*");
            tween.OnStopped += (s,e) =>
            {
                ((s as ObjectTweener)?.Curve as StateToken).Complete();
            };
            SPTween.AutoKill(tween);

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
        private class StateToken : TweenCurve, System.IDisposable
        {

            public i_Shake Owner;
            public float CurrentPrecedence;
            public Transform Targ;
            public Transform RelativeTarg;
            public Trans Cache;

            public void Complete()
            {
                if(this.Targ)
                {
                    this.Cache.SetToLocal(this.Targ);
                }
                this.Dispose();
            }

            public void Dispose()
            {
                this.Owner = null;
                this.CurrentPrecedence = 0f;
                this.Targ = null;
                this.RelativeTarg = null;
                this.Cache = default(Trans);
                this.DeInit();
                _pool.Release(this);
            }

            public override float TotalTime => Owner?._duration.Seconds ?? 0f;

            public override void Update(object targ, float dt, float t)
            {
                var _linear = Owner._linear;
                var _angular = Owner._angular;
                var _scale = Owner._scale;
                var duration = Owner._duration.Seconds;

                //linear
                if (_linear?.Count > 0)
                {
                    Vector3 adjust = Vector3.zero;
                    for (int i = 0; i < _linear.Count; i++)
                    {
                        _linear[i].Effect(ref adjust, t, duration);
                    }
                    if (RelativeTarg != null)
                    {
                        adjust = QuaternionUtil.FromToRotation(this.Targ.rotation, RelativeTarg.rotation) * adjust;
                    }
                    this.Targ.localPosition = this.Cache.Position + adjust;
                }

                //angular
                if (_angular?.Count > 0)
                {
                    Vector3 adjust = Vector3.zero;
                    for (int i = 0; i < _angular.Count; i++)
                    {
                        _angular[i].Effect(ref adjust, t, duration);
                    }
                    if (RelativeTarg != null)
                    {
                        adjust = QuaternionUtil.FromToRotation(this.Targ.rotation, RelativeTarg.rotation) * adjust;
                    }
                    this.Targ.localEulerAngles = this.Cache.Rotation.eulerAngles + adjust;
                }

                //scale
                if (_scale?.Count > 0)
                {
                    Vector3 adjust = Vector3.zero;
                    for (int i = 0; i < _scale.Count; i++)
                    {
                        _scale[i].Effect(ref adjust, t, duration);
                    }
                    this.Targ.localScale = this.Cache.Scale + adjust;
                }
            }

        }

        #endregion

    }

}