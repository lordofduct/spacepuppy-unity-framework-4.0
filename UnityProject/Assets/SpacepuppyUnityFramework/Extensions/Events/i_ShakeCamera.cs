using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Geom;
using com.spacepuppy.Events;
using com.spacepuppy.Tween;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Events
{

    [Infobox("Leave target blank to use the currently active camera.")]
    public class i_ShakeCamera : AutoTriggerable
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
        private SPTimePeriod _duration;

        #endregion

        #region Methods

        private System.Collections.IEnumerator DoShake(Transform targ, Trans cache, Transform relativeTarg)
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
                    if(relativeTarg != null)
                    {
                        adjust = QuaternionUtil.FromToRotation(targ.rotation, relativeTarg.rotation) * adjust;
                    }
                    targ.localPosition = cache.Position + adjust;
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
                        adjust = QuaternionUtil.FromToRotation(targ.rotation, relativeTarg.rotation) * adjust;
                    }
                    targ.localEulerAngles = cache.Rotation.eulerAngles + adjust;
                }

                yield return null;
                t -= _duration.TimeSupplier.Delta;
            }

            cache.SetToLocal(targ);
        }

        #endregion

        #region TriggerableMechanism Interface

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            if (_linear.Count == 0 && _angular.Count == 0) return false;
            
            var targ = _target.GetTarget<Transform>(arg);
            if (targ == null)
            {
                var cam = com.spacepuppy.Cameras.CameraPool.Main;
                if (cam != null && cam.camera != null)
                    targ = cam.camera.transform;
                else
                    return false;
            }

            return targ.AddOrGetComponent<CameraShakeEffectToken>().TryBegin(this, _relativeMotionTarget.GetTarget<Transform>(arg));
        }

        #endregion

        #region Special Types

        [System.Serializable]
        public class AxisEffect
        {

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
                if(this.Axis == EffectedAxis.All)
                {
                    var d = this.GetValue(t, totalTime);
                    v.x += d;
                    v.y += d;
                    v.z += d;
                }
                else
                {
                    float d = v.Get((CartesianAxis)this.Axis) + this.GetValue(t, totalTime);
                    VectorUtil.Set(ref v, (CartesianAxis)this.Axis, d);
                }
            }

        }

        public enum EffectedAxis
        {
            X = CartesianAxis.X,
            Y = CartesianAxis.Y,
            Z = CartesianAxis.Z,
            All = 3
        }

        public enum ShakeType
        {
            Sine,
            Triangle,
            SawTooth,
            Noise
        }



        private class CameraShakeEffectToken : MonoBehaviour
        {

            public float CurrentPrecedence;

            private RadicalCoroutine _routine;
            private Trans _cache;
            
            public bool TryBegin(i_ShakeCamera config, Transform relativeTarg)
            {
                if(_routine != null && !_routine.Finished)
                {
                    if (config._effectPrecedence < this.CurrentPrecedence) return false;

                    _routine.Cancel();
                    _cache.SetToLocal(this.transform);
                }

                this.CurrentPrecedence = config._effectPrecedence;
                _cache = Trans.GetLocal(this.transform);
                _routine = this.StartRadicalCoroutine(config.DoShake(this.transform, _cache, relativeTarg));
                return true;
            }
            
        }

        #endregion

    }

}