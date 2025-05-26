using UnityEngine;
using System.Collections.Generic;

namespace com.spacepuppy.Geom
{

    [System.Serializable]
    public struct Cone : IGeom
    {

        #region Fields

        [SerializeField]
        private Vector3 _start;
        [SerializeField]
        private Vector3 _end;
        [SerializeField]
        private float _startRad;
        [SerializeField]
        private float _endRad;

        #endregion

        #region CONSTRUCTOR

        public Cone(Vector3 start, Vector3 end, float startRadius, float endRadius)
        {
            _start = start;
            _end = end;
            _startRad = startRadius;
            _endRad = endRadius;
        }

        public static Cone FromAngle(Vector3 start, Vector3 end, float radians, float startRadius)
        {
            float length = (end - start).magnitude;
            return new Cone()
            {
                _start = start,
                _end = end,
                _startRad = startRadius,
                _endRad = startRadius + length * Mathf.Tan(radians),
            };
        }

        public static Cone FromAngle(Vector3 start, Vector3 dir, float range, float radians, float startRadius)
        {
            return new Cone()
            {
                _start = start,
                _end = start + dir.normalized * range,
                _startRad = startRadius,
                _endRad = startRadius + range * Mathf.Tan(radians),
            };
        }

        public static Cone FromAngle(Ray ray, float range, float radians, float startRadius = 0f)
        {
            return new Cone()
            {
                _start = ray.origin,
                _end = ray.GetPoint(range),
                _startRad = startRadius,
                _endRad = startRadius + range * Mathf.Tan(radians),
            };
        }

        #endregion

        #region Properties

        public Vector3 Start
        {
            get { return _start; }
            set { _start = value; }
        }

        public Vector3 End
        {
            get { return _end; }
            set { _end = value; }
        }

        public float StartRadius
        {
            get { return _startRad; }
            set { _startRad = value; }
        }

        public float EndRadius
        {
            get { return _endRad; }
            set { _endRad = value; }
        }

        public float Height
        {
            get
            {
                return (_end - _start).magnitude;
            }
            set
            {
                var c = this.Center;
                var up = (_end - _start).normalized;
                var change = up * value / 2.0f;
                _start = c - change;
                _end = c + change;
            }
        }

        public Vector3 Center
        {
            get
            {
                if (_end == _start)
                    return _start;
                else
                    return _start + (_end - _start) * 0.5f;
            }
            set
            {
                var change = (value - this.Center);
                _start += change;
                _end += change;
            }
        }

        public Vector3 Up
        {
            get
            {
                if (_end == _start)
                    return Vector3.up;
                else
                    return (_end - _start).normalized;
            }
        }

        #endregion

        #region Methods

        public float RadiusAtHeight(float h)
        {
            if (h <= 0f) return _startRad;

            float height = this.Height;
            float t = h / height;
            return Mathf.Lerp(_startRad, _endRad, h / height);
        }

        #endregion

        #region IGeom Interface

        public void Move(Vector3 mv)
        {
            _start += mv;
            _end += mv;
        }

        public void RotateAroundPoint(Vector3 point, Quaternion rot)
        {
            _start = point + rot * (_start - point);
            _end = point + rot * (_end - point);
        }

        public AxisInterval Project(Vector3 axis)
        {
            //TODO
            throw new System.NotImplementedException();
        }

        public Bounds GetBounds()
        {
            //TODO
            throw new System.NotImplementedException();
        }

        public Sphere GetBoundingSphere()
        {
            var rod = _end - _start;
            double r1 = rod.magnitude;
            rod /= (float)r1;
            r1 /= 2d;
            double r2 = System.Math.Max(_startRad, _endRad);

            return new Sphere(_start + rod * (float)r1, (float)System.Math.Sqrt(r1 * r1 + r2 * r2));
        }

        public IEnumerable<Vector3> GetAxes()
        {
            //TODO
            return System.Linq.Enumerable.Empty<Vector3>();
        }

        public bool Contains(Vector3 pos)
        {
            return ContainsPoint(_start, _end, _startRad, _endRad, pos);
        }

        #endregion

        #region Gizmos

        public void DrawGizmo(Matrix4x4 matrix, int circleDetail = 16, int lineCount = 4)
        {
            Vector3 s = matrix.MultiplyPoint(_start);
            Vector3 e = matrix.MultiplyPoint(_end);
            Vector3 dir = (e - s).normalized;
            Vector3 leg;
            if (Mathf.Abs(Vector3.Dot(dir, Vector3.up)) > 0.9f)
            {
                leg = Vector3.Cross(dir, Vector3.forward).normalized;
            }
            else
            {
                leg = Vector3.Cross(dir, Vector3.up).normalized;
            }

            if (lineCount > 0)
            {
                float delta = 360f / (float)lineCount;
                for (int i = 0; i < lineCount; i++)
                {
                    float a = delta * i;
                    var p = Quaternion.AngleAxis(a, dir) * leg;
                    Gizmos.DrawLine(s + p * _startRad, e + p * _endRad);
                }
            }
            if (circleDetail > 0)
            {
                float delta = 360f / (float)circleDetail;
                var lps = leg * _startRad;
                var lpe = leg * _endRad;
                for (int i = 1; i < circleDetail; i++)
                {
                    float a = delta * i;
                    var p = Quaternion.AngleAxis(a, dir) * leg;
                    var ps = p * _startRad;
                    var pe = p * _endRad;
                    if (_startRad > 0f)
                    {
                        Gizmos.DrawLine(s + lps, s + ps);
                    }
                    if (_endRad > 0f)
                    {
                        Gizmos.DrawLine(e + lpe, e + pe);
                    }
                    lps = ps;
                    lpe = pe;
                }
            }
        }

        #endregion

        #region Static Utils

        public static bool ContainsPoint(Vector3 start, Vector3 end, float startRadius, float endRadius, Vector3 pnt)
        {
            var rail = end - start;
            var rod = pnt - start;
            var dot = Vector3.Dot(rod, rail);
            var sqrRailLength = rail.sqrMagnitude;

            if (dot < 0f || dot > sqrRailLength)
            {
                return false;
            }
            else
            {
                float radius;
                if (sqrRailLength < 0.0001f)
                    radius = Mathf.Max(startRadius, endRadius);
                else
                    radius = startRadius + (endRadius - startRadius) * dot / sqrRailLength;

                if (rod.sqrMagnitude - dot * dot / sqrRailLength > radius * radius)
                    return false;
                else
                    return true;
            }
        }

        public static bool ContainsSphere(Vector3 start, Vector3 end, float startRadius, float endRadius, Vector3 pnt, float sphereRadius)
        {
            var rail = end - start;
            var rod = pnt - start;
            var dot = Vector3.Dot(rod, rail);
            var sqrRailLength = rail.sqrMagnitude;
            float sqrSphereRad = sphereRadius * sphereRadius;

            if (dot < -sqrSphereRad || dot > sqrRailLength + sqrSphereRad)
            {
                return false;
            }
            else
            {
                float radius;
                if (sqrRailLength < 0.0001f)
                    radius = Mathf.Max(startRadius, endRadius);
                else
                    radius = startRadius + (endRadius - startRadius) * dot / sqrRailLength;

                if (rod.sqrMagnitude - dot * dot / sqrRailLength > radius * radius + sqrSphereRad)
                    return false;
                else
                    return true;
            }
        }

        #endregion

    }

}
