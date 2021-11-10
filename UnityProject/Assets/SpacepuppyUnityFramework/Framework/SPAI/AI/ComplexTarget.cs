using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Pathfinding;
using com.spacepuppy.Sensors;
using com.spacepuppy.Geom;
using com.spacepuppy.Utils;
using System;

namespace com.spacepuppy.AI
{

    public enum ComplexTargetType : byte
    {
        Null = 0,
        GameObjectSource = 1,
        Transform = 2,
        Vector2 = 3,
        Vector3 = 4
    }

    public struct ComplexTarget
    {

        #region Fields

        public readonly ComplexTargetType TargetType;
        private readonly object _target;
        private readonly object _aux;
        private readonly Vector3 _vector;

        #endregion

        #region CONSTRUCTOR

        public ComplexTarget(IGameObjectSource aspect, object aux = null)
        {
            if (aspect != null)
            {
                TargetType = ComplexTargetType.GameObjectSource;
                _target = aspect;
            }
            else
            {
                TargetType = ComplexTargetType.Null;
                _target = null;
            }
            _vector = Vector3.zero;
            _aux = aux;
        }

        public ComplexTarget(Transform target, object aux = null)
        {
            if (target != null)
            {
                TargetType = ComplexTargetType.Transform;
                _target = target;
            }
            else
            {
                TargetType = ComplexTargetType.Null;
                _target = null;
            }
            _vector = Vector3.zero;
            _aux = aux;
        }

        public ComplexTarget(Vector2 location, object aux = null)
        {
            TargetType = ComplexTargetType.Vector2;
            _target = null;
            _vector = (Vector3)location;
            _aux = aux;
        }

        public ComplexTarget(Vector3 location, object aux = null)
        {
            TargetType = ComplexTargetType.Vector3;
            _target = null;
            _vector = location;
            _aux = aux;
        }

        public ComplexTarget(IPath path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            TargetType = ComplexTargetType.Vector3;
            _target = null;
            _vector = path.Waypoints.LastOrDefault();
            _aux = path;
        }

        #endregion

        #region Properties

        public Vector2 Position2D
        {
            get
            {
                switch (this.TargetType)
                {
                    case ComplexTargetType.Null:
                        return Vector2.zero;
                    case ComplexTargetType.GameObjectSource:
                        {
                            var a = _target as IGameObjectSource;
                            if (a.IsNullOrDestroyed()) return Vector2.zero;
                            else if (this.Surface != null) return this.Surface.ProjectPosition2D(a.transform.position);
                            else return ConvertUtil.ToVector2(a.transform.position);
                        }
                    case ComplexTargetType.Transform:
                        {
                            var t = _target as Transform;
                            if (t.IsNullOrDestroyed()) return Vector2.zero;
                            else if (this.Surface != null) return this.Surface.ProjectPosition2D(t.position);
                            else return ConvertUtil.ToVector2(t.position);
                        }
                    case ComplexTargetType.Vector2:
                        return ConvertUtil.ToVector2(_vector);
                    case ComplexTargetType.Vector3:
                        if (_aux is IPlanarSurface surf) return surf.ProjectPosition2D(_vector);
                        else return ConvertUtil.ToVector2(_vector);
                    default:
                        return Vector2.zero;
                }
            }
        }

        public Vector3 Position
        {
            get
            {
                switch (this.TargetType)
                {
                    case ComplexTargetType.Null:
                        return Vector3.zero;
                    case ComplexTargetType.GameObjectSource:
                        {
                            var a = _target as IGameObjectSource;
                            if (a == null) return Vector3.zero;
                            else return a.transform.position;
                        }
                    case ComplexTargetType.Transform:
                        {
                            var t = _target as Transform;
                            if (t == null) return Vector3.zero;
                            else return t.position;
                        }
                    case ComplexTargetType.Vector2:
                        if (this.Surface != null) return this.Surface.ProjectPosition3D(ConvertUtil.ToVector2(_vector));
                        else return _vector.SetZ(0f);
                    case ComplexTargetType.Vector3:
                        return _vector;
                    default:
                        return Vector3.zero;
                }
            }
        }

        public IGameObjectSource Target { get { return _target as IGameObjectSource; } }

        public Transform Transform
        {
            get
            {
                switch (TargetType)
                {
                    case ComplexTargetType.GameObjectSource:
                        var a = _target as IGameObjectSource;
                        if (a == null) return null;
                        else return a.transform;
                    case ComplexTargetType.Transform:
                        return _target as Transform;
                    default:
                        return null;
                }
            }
        }

        public object Aux { get { return _aux; } }

        public IPlanarSurface Surface { get { return _aux as IPlanarSurface; } }

        public IPath Path { get { return _aux as IPath; } }

        public bool IsNull
        {
            get
            {
                switch (this.TargetType)
                {
                    case ComplexTargetType.Null:
                        return true;
                    case ComplexTargetType.GameObjectSource:
                    case ComplexTargetType.Transform:
                        return _target.IsNullOrDestroyed();
                    case ComplexTargetType.Vector2:
                    case ComplexTargetType.Vector3:
                    default:
                        return false;
                }
            }
        }

        #endregion

        #region Static Methods

        public static ComplexTarget FromObject(object targ)
        {
            if (targ == null) return new ComplexTarget();

            if (targ is Vector2)
                return new ComplexTarget((Vector2)targ);
            else if (targ is Vector3)
                return new ComplexTarget((Vector3)targ);
            else if (targ is IPath)
                return new ComplexTarget(targ as IPath);
            else if (targ is IGameObjectSource)
                return new ComplexTarget(targ as IGameObjectSource);
            else if (targ is GameObject)
                return new ComplexTarget((targ as GameObject).transform);
            else if (targ is Transform)
                return new ComplexTarget(targ as Transform);
            else if (targ is Component)
                return new ComplexTarget((targ as Component).transform);
            else
                return new ComplexTarget();
        }

        public static ComplexTarget Null { get { return new ComplexTarget(); } }

        public static implicit operator ComplexTarget(Transform o)
        {
            return new ComplexTarget(o);
        }

        public static implicit operator ComplexTarget(Vector2 v)
        {
            return new ComplexTarget(v);
        }

        public static implicit operator ComplexTarget(Vector3 v)
        {
            return new ComplexTarget(v);
        }

        public static implicit operator ComplexTarget(GameObject go)
        {
            if (go == null) return new ComplexTarget();
            else return new ComplexTarget(go.transform);
        }

        public static implicit operator ComplexTarget(Component c)
        {
            if (c == null) return new ComplexTarget();
            if (c is IGameObjectSource)
                return new ComplexTarget(c as IGameObjectSource);
            else if (c is Transform)
                return new ComplexTarget(c as Transform);
            else
                return new ComplexTarget(c.transform);
        }

        #endregion

    }

}
