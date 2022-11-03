using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy.Utils;

namespace com.spacepuppy.Geom
{

    public static class GeomUtil
    {

        public const float DOT_EPSILON = 0.0001f;
        public static BoundingSphereAlgorithm DefaultBoundingSphereAlgorithm = BoundingSphereAlgorithm.FromBounds;


        #region Intersections

        public static bool Intersects(this IGeom geom1, IGeom geom2)
        {
            if (geom1 == null || geom2 == null) return false;

            var s1 = geom1.GetBoundingSphere();
            var s2 = geom2.GetBoundingSphere();
            if ((s1.Center - s2.Center).magnitude > (s1.Radius + s2.Radius)) return false;

            foreach (var a in geom1.GetAxes().Union(geom2.GetAxes()))
            {
                if (geom1.Project(a).Intersects(geom2.Project(a))) return true;
            }

            return false;
        }

        public static bool Intersects(this IGeom geom, Bounds bounds)
        {
            //TODO - re-implement independent of geom, may speed this up
            var geom2 = new AABBox(bounds);
            return Intersects(geom, geom2);
        }

        /// <summary>
        /// Find intersecting line of two planes
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static Line IntersectionOfPlanes(Plane p1, Plane p2)
        {
            var line = new Line();
            line.Direction = Vector3.Cross(p1.normal, p2.normal);
            Vector3 ldir = Vector3.Cross(p2.normal, line.Direction);

            float d = Vector3.Dot(p1.normal, ldir);

            //if d is to close to 0, planes are parallel
            if (Mathf.Abs(d) > 0.005f)
            {
                Vector3 p1Top2 = (p1.normal * p1.distance) - (p2.normal * p2.distance);
                float t = Vector3.Dot(p1.normal, p1Top2) / d;
                line.Point = (p2.normal * p2.distance) + t * ldir;
                return line;
            }
            else
            {
                throw new System.Exception("both planes are parallel");
            }
        }
        public static bool IntersectionOfPlanes(Plane p1, Plane p2, out Line line)
        {
            line = new Line();
            line.Direction = Vector3.Cross(p1.normal, p2.normal);
            Vector3 ldir = Vector3.Cross(p2.normal, line.Direction);

            float d = Vector3.Dot(p1.normal, ldir);

            //if d is to close to 0, planes are parallel
            if (Mathf.Abs(d) > 0.005f)
            {
                Vector3 p1Top2 = (p1.normal * p1.distance) - (p2.normal * p2.distance);
                float t = Vector3.Dot(p1.normal, p1Top2) / d;
                line.Point = (p2.normal * p2.distance) + t * ldir;
                return true;
            }
            else
            {
                line = new Line();
                return false;
            }
        }

        /// <summary>
        /// Find point at which 3 planes intersect
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <returns></returns>
        public static Vector3 IntersectionOfPlanes(Plane p1, Plane p2, Plane p3)
        {
            try
            {
                var line = IntersectionOfPlanes(p1, p2);
                var intersection = IntersectionOfLineAndPlane(line, p3);
                return intersection;
            }
            catch (System.Exception)
            {
                throw new System.Exception("two or more planes are parallel, no intersection found");
            }
        }
        public static bool IntersectionOfPlanes(Plane p1, Plane p2, Plane p3, out Vector3 point)
        {
            try
            {
                var line = IntersectionOfPlanes(p1, p2);
                point = IntersectionOfLineAndPlane(line, p3);
                return true;
            }
            catch (System.Exception)
            {
                point = Vector3.zero;
                return false;
            }
        }

        /// <summary>
        /// Find interesection location of a line and a plane
        /// </summary>
        /// <param name="line"></param>
        /// <param name="plane"></param>
        /// <returns></returns>
        public static Vector3 IntersectionOfLineAndPlane(Line line, Plane plane)
        {
            //calc dist...
            float dotnum = Vector3.Dot((plane.normal * plane.distance) - line.Point, plane.normal);
            float dotdenom = Vector3.Dot(line.Direction, plane.normal);

            if (dotdenom != 0.0f)
            {
                float len = dotnum / dotdenom;
                var v = line.Direction * len;
                return line.Point + v;
            }
            else
            {
                throw new System.Exception("line and plane are parallel");
            }
        }
        public static bool IntersectionOfLineAndPlane(Line line, Plane plane, out Vector3 point)
        {
            //calc dist...
            float dotnum = Vector3.Dot((plane.normal * plane.distance) - line.Point, plane.normal);
            float dotdenom = Vector3.Dot(line.Direction, plane.normal);

            if (dotdenom != 0.0f)
            {
                float len = dotnum / dotdenom;
                var v = line.Direction * len;
                point = line.Point + v;
                return true;
            }
            else
            {
                point = Vector3.zero;
                return false;
            }
        }

        #endregion

        #region Plane Extension Methods

        public static Vector3 ProjectVectorOnPlane(this Plane pl, Vector3 v)
        {
            return v - (Vector3.Dot(v, pl.normal) * pl.normal);
        }

        public static float AngleBetweenVectorAndPlane(this Plane pl, Vector3 v)
        {
            float dot = Vector3.Dot(v.normalized, pl.normal);
            float a = Mathf.Acos(dot);
            return MathUtil.PI_2 - a;
        }

        /// <summary>
        /// Create a reflection of a point, as if the plane was a mirror you were looking into.
        /// </summary>
        /// <param name="pl"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector3 ReflectPointThroughPlane(this Plane pl, Vector3 point)
        {
            var d = Vector3.Dot(pl.normal, point) - pl.distance;
            return point - pl.normal * d * 2f;
        }

        #endregion

        #region GetBounds

        public static Sphere GetGlobalBoundingSphere(this Renderer rend)
        {
            return GetGlobalBoundingSphere(rend, GeomUtil.DefaultBoundingSphereAlgorithm);
        }

        public static Sphere GetGlobalBoundingSphere(this Renderer rend, BoundingSphereAlgorithm algorithm)
        {
            if (algorithm != BoundingSphereAlgorithm.FromBounds && rend is SkinnedMeshRenderer)
            {
                return Sphere.FromMesh((rend as SkinnedMeshRenderer).sharedMesh, algorithm, Trans.GetGlobal(rend.transform));
            }
            else if (algorithm != BoundingSphereAlgorithm.FromBounds && rend is MeshRenderer && rend.HasComponent<MeshFilter>())
            {
                return Sphere.FromMesh((rend as MeshRenderer).GetComponent<MeshFilter>().sharedMesh, algorithm, Trans.GetGlobal(rend.transform));
            }
            else
            {
                var bounds = rend.bounds;
                return new Sphere(bounds.center, bounds.extents.magnitude);
            }
        }

        public static Sphere GetGlobalBoundingSphere(this Collider c)
        {
            return Sphere.FromCollider(c, GeomUtil.DefaultBoundingSphereAlgorithm, false);
        }

        public static Sphere GetGlobalBoundingSphere(this Collider c, BoundingSphereAlgorithm algorithm)
        {
            return Sphere.FromCollider(c, algorithm, false);
        }

        public static Sphere GetLocalBoundingSphere(this Renderer rend)
        {
            return GetLocalBoundingSphere(rend, GeomUtil.DefaultBoundingSphereAlgorithm);
        }

        public static Sphere GetLocalBoundingSphere(this Renderer rend, BoundingSphereAlgorithm algorithm)
        {
            if (algorithm != BoundingSphereAlgorithm.FromBounds && rend is SkinnedMeshRenderer)
            {
                return Sphere.FromMesh((rend as SkinnedMeshRenderer).sharedMesh, algorithm);
            }
            else if (algorithm != BoundingSphereAlgorithm.FromBounds && rend is MeshRenderer && rend.HasComponent<MeshFilter>())
            {
                return Sphere.FromMesh((rend as MeshRenderer).GetComponent<MeshFilter>().sharedMesh, algorithm);
            }
            else
            {
                var bounds = rend.bounds;
                var c = rend.transform.InverseTransformPoint(bounds.center);
                var v = rend.transform.InverseTransformDirection(bounds.extents);
                return new Sphere(c, v.magnitude);
            }
        }

        public static Sphere GetLocalBoundingSphere(this Collider c)
        {
            return Sphere.FromCollider(c, GeomUtil.DefaultBoundingSphereAlgorithm, true);
        }

        public static Sphere GetLocalBoundingSphere(this Collider c, BoundingSphereAlgorithm algorithm)
        {
            return Sphere.FromCollider(c, algorithm, true);
        }

        public static Sphere GetGlobalBoundingSphere(this GameObject go, bool donotRecurseChildren = false, bool ignoreColliders = false, bool ignoreRenderers = false)
        {
            return GetGlobalBoundingSphere(go, GeomUtil.DefaultBoundingSphereAlgorithm, donotRecurseChildren, ignoreColliders, ignoreRenderers);
        }

        public static Sphere GetGlobalBoundingSphere(this GameObject go, BoundingSphereAlgorithm algorithm, bool donotRecurseChildren = false, bool ignoreColliders = false, bool ignoreRenderers = false)
        {
            if (go == null) throw new System.ArgumentNullException(nameof(go));

            Sphere s = default;
            bool found = false;
            if (!ignoreRenderers)
            {
                using (var renderers = com.spacepuppy.Collections.TempCollection.GetList<Renderer>())
                {
                    if (donotRecurseChildren)
                    {
                        go.GetComponents<Renderer>(renderers);
                    }
                    else
                    {
                        go.GetComponentsInChildren<Renderer>(renderers);
                    }

                    foreach (var r in renderers)
                    {
                        if (found)
                        {
                            s.Encapsulate(r.GetGlobalBoundingSphere(algorithm));
                        }
                        else
                        {
                            found = true;
                            s = r.GetGlobalBoundingSphere(algorithm);
                        }
                    }
                }
            }
            if (!ignoreColliders)
            {
                using (var colliders = com.spacepuppy.Collections.TempCollection.GetList<Collider>())
                {
                    if (donotRecurseChildren)
                    {
                        go.GetComponents<Collider>(colliders);
                    }
                    else
                    {
                        go.GetComponentsInChildren<Collider>(colliders);
                    }

                    foreach (var c in colliders)
                    {
                        if (found)
                        {
                            s.Encapsulate(c.GetGlobalBoundingSphere(algorithm));
                        }
                        else
                        {
                            found = true;
                            s = c.GetGlobalBoundingSphere(algorithm);
                        }
                    }
                }
            }

            return found ? s : new Sphere(go.transform.position, 0.0f);
        }

        public static Bounds GetGlobalBounds(this GameObject go, bool donotRecurseChildren = false, bool ignoreColliders = false, bool ignoreRenderers = false)
        {
            if (go == null) throw new System.ArgumentNullException(nameof(go));

            Bounds b = default;
            bool found = false;
            if (!ignoreRenderers)
            {
                using (var renderers = com.spacepuppy.Collections.TempCollection.GetList<Renderer>())
                {
                    if (donotRecurseChildren)
                    {
                        go.GetComponents<Renderer>(renderers);
                    }
                    else
                    {
                        go.GetComponentsInChildren<Renderer>(renderers);
                    }

                    foreach (var r in renderers)
                    {
                        if (found)
                        {
                            b.Encapsulate(r.bounds);
                        }
                        else
                        {
                            found = true;
                            b = r.bounds;
                        }
                    }
                }
            }
            if (!ignoreColliders)
            {
                using (var colliders = com.spacepuppy.Collections.TempCollection.GetList<Collider>())
                {
                    if (donotRecurseChildren)
                    {
                        go.GetComponents<Collider>(colliders);
                    }
                    else
                    {
                        go.GetComponentsInChildren<Collider>(colliders);
                    }

                    foreach (var c in colliders)
                    {
                        if (found)
                        {
                            b.Encapsulate(c.bounds);
                        }
                        else
                        {
                            found = true;
                            b = c.bounds;
                        }
                    }
                }
            }

            return found ? b : new Bounds(go.transform.position, Vector3.zero);
        }

        public static Sphere? GetGlobalBoundingSphere(this IEnumerable<Renderer> renderers)
        {
            return GetGlobalBoundingSphere(renderers, GeomUtil.DefaultBoundingSphereAlgorithm);
        }

        public static Sphere? GetGlobalBoundingSphere(this IEnumerable<Renderer> renderers, BoundingSphereAlgorithm algorithm)
        {
            if (renderers == null) return null;

            Sphere s = default;
            bool found = false;
            foreach (var r in renderers)
            {
                if (found)
                {
                    s.Encapsulate(r.GetGlobalBoundingSphere(algorithm));
                }
                else
                {
                    found = true;
                    s = r.GetGlobalBoundingSphere(algorithm);
                }
            }
            return found ? s : null;
        }

        public static Bounds? GetGlobalBounds(this IEnumerable<Renderer> renderers)
        {
            if (renderers == null) return null;

            Bounds b = default;
            bool found = false;
            foreach (var r in renderers)
            {
                if (found)
                {
                    b.Encapsulate(r.bounds);
                }
                else
                {
                    found = true;
                    b = r.bounds;
                }
            }
            return found ? b : null;
        }

        public static Sphere? GetGlobalBoundingSphere(this IEnumerable<Collider> colliders)
        {
            return GetGlobalBoundingSphere(colliders, GeomUtil.DefaultBoundingSphereAlgorithm);
        }

        public static Sphere? GetGlobalBoundingSphere(this IEnumerable<Collider> colliders, BoundingSphereAlgorithm algorithm)
        {
            if (colliders == null) return null;

            Sphere s = default;
            bool found = false;
            foreach (var c in colliders)
            {
                if (found)
                {
                    s.Encapsulate(c.GetGlobalBoundingSphere(algorithm));
                }
                else
                {
                    found = true;
                    s = c.GetGlobalBoundingSphere(algorithm);
                }
            }
            return found ? s : null;
        }

        public static Bounds? GetGlobalBounds(this IEnumerable<Collider> colliders)
        {
            if (colliders == null) return null;

            Bounds b = default;
            bool found = false;
            foreach (var c in colliders)
            {
                if (found)
                {
                    b.Encapsulate(c.bounds);
                }
                else
                {
                    found = true;
                    b = c.bounds;
                }
            }
            return found ? b : null;
        }

        #endregion

        #region GetColliderGeom

        public static Vector3 GetCenter(this Collider c, bool local = false)
        {
            if (c is CharacterController)
                return (local) ? (c as CharacterController).center : c.transform.TransformPoint((c as CharacterController).center);
            else if (c is CapsuleCollider)
                return (local) ? (c as CapsuleCollider).center : c.transform.TransformPoint((c as CapsuleCollider).center);
            else if (c is SphereCollider)
                return (local) ? (c as SphereCollider).center : c.transform.TransformPoint((c as SphereCollider).center);
            else if (c is BoxCollider)
                return (local) ? (c as BoxCollider).center : c.transform.TransformPoint((c as BoxCollider).center);
            else
                return (local) ? c.transform.InverseTransformPoint(c.bounds.center) : c.bounds.center;
        }

        /// <summary>
        /// Attempts to calculate geometry for a collider. Not tested yet.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static IPhysicsGeom GetGeom(this Collider c, bool local = false)
        {
            return GetGeom(c, GeomUtil.DefaultBoundingSphereAlgorithm, local);
        }

        /// <summary>
        /// Attempts to calculate geometry for a collider. Not tested yet.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static IPhysicsGeom GetGeom(this Collider c, BoundingSphereAlgorithm algorithm, bool local = false)
        {
            if (c == null) return null;

            if (c is CharacterController)
            {
                return Capsule.FromCollider(c as CharacterController, local);
            }
            if (c is CapsuleCollider)
            {
                return Capsule.FromCollider(c as CapsuleCollider, local);
            }
            else if (c is BoxCollider)
            {
                return Box.FromCollider(c as BoxCollider, local);
            }
            else if (c is SphereCollider)
            {
                return Sphere.FromCollider(c as SphereCollider, local);
            }
            else if (algorithm != BoundingSphereAlgorithm.FromBounds && c is MeshCollider)
            {
                if (local)
                {
                    return Sphere.FromMesh((c as MeshCollider).sharedMesh, algorithm);
                }
                else
                {
                    return Sphere.FromMesh((c as MeshCollider).sharedMesh, algorithm, Trans.GetGlobal(c.transform));
                }
            }
            else
            {
                //otherwise just return bounds as AABBox
                return AABBox.FromCollider(c, local);
            }
        }

        #endregion

        public static Vector3 CircumCenter(Vector3 a, Vector3 b, Vector3 c)
        {
            var a2b = Mathf.Pow(a.magnitude, 2.0f) * b;
            var b2a = Mathf.Pow(b.magnitude, 2.0f) * a;
            var aCrossb = Vector3.Cross(a, b);
            var numer = Vector3.Cross(a2b - b2a, aCrossb);
            var denom = 2.0f * Mathf.Pow(aCrossb.magnitude, 2.0f);
            return numer / denom + c;
        }

    }

}
