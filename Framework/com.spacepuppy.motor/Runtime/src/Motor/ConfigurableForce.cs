using UnityEngine;

using com.spacepuppy.Geom;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Motor
{

    [System.Serializable()]
    public class ConfigurableForce
    {

        public enum ForceDirection : byte
        {
            Relative = 0,
            Random = 1,
            Forward = 2,
        }

        #region Fields

        [SerializeField()]
        private ForceDirection _direction;
        [SerializeField()]
        private float _strength;
        [SerializeField()]
        private ForceMode _forceMode;
        [SerializeField]
        private PlanarSurfaceRef _planarSurface = new PlanarSurfaceRef();
        [SerializeField]
        private AnimationCurveRef _strengthDampingCurve;

        #endregion

        #region CONSTRUCTOR

        public ConfigurableForce()
        {
        }

        public ConfigurableForce(float strength)
        {
            _strength = strength;
        }

        public ConfigurableForce(float strength, ForceMode mode, ForceDirection dir)
        {
            _direction = dir;
            _strength = strength;
            _forceMode = mode;
        }

        #endregion

        #region Properties

        public ForceDirection Direction { get { return _direction; } set { _direction = value; } }

        public float Strength { get { return _strength; } set { _strength = value; } }

        public ForceMode ForceMode { get { return _forceMode; } set { _forceMode = value; } }

        public IPlanarSurface PlanarSurface
        {
            get => _planarSurface.Value;
            set => _planarSurface.Value = value;
        }

        public object StrengthDampingCurve
        {
            get => _strengthDampingCurve.Curve;
            set => _strengthDampingCurve.Curve = value;
        }

        #endregion

        #region Methods

        public Vector3 GetForce(Trans forceOrigin, Trans forceTarget)
        {
            var surf = this.PlanarSurface;
            var dir = surf != null ? GetDirection(surf, _direction, forceOrigin, forceTarget) : GetDirection(_direction, forceOrigin, forceTarget);
            float str = _strengthDampingCurve.IsValid ? _strengthDampingCurve.Evaluate((forceOrigin.Position - forceTarget.Position).magnitude) * _strength : _strength;
            return dir * str;
        }

        public Vector3 GetForce(Transform forceOrigin, Transform forceTarget)
        {
            var surf = this.PlanarSurface;
            var dir = surf != null ? GetDirection(surf, _direction, forceOrigin, forceTarget) : GetDirection(_direction, forceOrigin, forceTarget);
            float str = _strengthDampingCurve.IsValid ? _strengthDampingCurve.Evaluate((forceOrigin.position - forceTarget.position).magnitude) * _strength : _strength;
            return dir * str;
        }

        public void ApplyForce(Trans forceOrigin, Rigidbody body)
        {
            var force = this.GetForce(forceOrigin, Trans.GetGlobal(body.transform));
            body.AddForce(force, _forceMode);
        }

        public void ApplyForce(Transform forceOrigin, Rigidbody body)
        {
            var force = this.GetForce(forceOrigin, body.transform);
            body.AddForce(force, _forceMode);
        }

        public void ApplyForce(Transform forceOrigin, IForceReceiver body)
        {
            var force = this.GetForce(forceOrigin, body.transform);
            body.AddForce(force, _forceMode);
        }

        #endregion

        #region Static Methods

        public static Vector3 GetDirection(ForceDirection dir, Transform forceOrigin, Transform forceTarget)
        {
            switch (dir)
            {
                case ForceDirection.Relative:
                    return (forceTarget.position - forceOrigin.position).normalized;
                case ForceDirection.Random:
                    return RandomUtil.Standard.OnUnitSphere();
                case ForceDirection.Forward:
                    return forceOrigin.forward;
            }
            return Vector3.zero;
        }

        public static Vector3 GetDirection(ForceDirection dir, Trans forceOrigin, Trans forceTarget)
        {
            switch (dir)
            {
                case ForceDirection.Relative:
                    return (forceTarget.Position - forceOrigin.Position).normalized;
                case ForceDirection.Random:
                    return RandomUtil.Standard.OnUnitSphere();
                case ForceDirection.Forward:
                    return forceOrigin.Forward;
            }
            return Vector3.zero;
        }

        public static Vector3 GetDirection(IPlanarSurface surface, ForceDirection dir, Transform forceOrigin, Transform forceTarget)
        {
            if (surface == null) throw new System.ArgumentNullException(nameof(surface));

            switch (dir)
            {
                case ForceDirection.Relative:
                    return surface.FlattenDirection(forceOrigin.position, forceTarget.position - forceOrigin.position).normalized;
                case ForceDirection.Random:
                    return surface.ProjectVectorTo3D(forceOrigin.position, RandomUtil.Standard.OnUnitCircle());
                case ForceDirection.Forward:
                    return surface.FlattenDirection(forceOrigin.position, forceOrigin.forward).normalized;
            }
            return Vector2.zero;
        }

        public static Vector3 GetDirection(IPlanarSurface surface, ForceDirection dir, Trans forceOrigin, Trans forceTarget)
        {
            if (surface == null) throw new System.ArgumentNullException(nameof(surface));

            switch (dir)
            {
                case ForceDirection.Relative:
                    return surface.FlattenDirection(forceOrigin.Position, forceTarget.Position - forceOrigin.Position).normalized;
                case ForceDirection.Random:
                    return surface.ProjectVectorTo3D(forceOrigin.Position, RandomUtil.Standard.OnUnitCircle());
                case ForceDirection.Forward:
                    return surface.FlattenDirection(forceOrigin.Position, forceOrigin.Forward).normalized;
            }
            return Vector2.zero;
        }

        public static Vector2 GetDirection2D(IPlanarSurface surface, ForceDirection dir, Transform forceOrigin, Transform forceTarget)
        {
            if (surface == null) throw new System.ArgumentNullException(nameof(surface));

            switch (dir)
            {
                case ForceDirection.Relative:
                    return surface.ProjectVectorTo2D(forceOrigin.position, (forceTarget.position - forceOrigin.position)).normalized;
                case ForceDirection.Random:
                    return RandomUtil.Standard.OnUnitCircle();
                case ForceDirection.Forward:
                    return surface.ProjectVectorTo2D(forceOrigin.position, forceOrigin.forward).normalized;
            }
            return Vector2.zero;
        }

        public static Vector2 GetDirection2D(IPlanarSurface surface, ForceDirection dir, Trans forceOrigin, Trans forceTarget)
        {
            if (surface == null) throw new System.ArgumentNullException(nameof(surface));

            switch (dir)
            {
                case ForceDirection.Relative:
                    return surface.ProjectVectorTo2D(forceOrigin.Position, (forceTarget.Position - forceOrigin.Position)).normalized;
                case ForceDirection.Random:
                    return RandomUtil.Standard.OnUnitCircle();
                case ForceDirection.Forward:
                    return surface.ProjectVectorTo2D(forceOrigin.Position, forceOrigin.Forward).normalized;
            }
            return Vector2.zero;
        }

        #endregion

    }
}
