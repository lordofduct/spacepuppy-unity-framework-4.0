using UnityEngine;

using com.spacepuppy.Geom;
using System.Runtime.CompilerServices;

namespace com.spacepuppy.Utils
{

    public static class TransformUtil
    {

        #region Convert Methods

        public static Trans ToTrans(this Transform trans)
        {
            return Trans.GetGlobal(trans);
        }

        public static Trans ToRelativeTrans(this Transform trans, Transform relativeTo)
        {
            var m = trans.localToWorldMatrix;
            return Trans.Transform(relativeTo.worldToLocalMatrix * m);
        }

        public static Trans ToLocalTrans(this Transform trans)
        {
            return Trans.GetLocal(trans);
        }

        public static Matrix4x4 GetMatrix(this Transform trans)
        {
            return trans.localToWorldMatrix;
        }

        public static Matrix4x4 GetRelativeMatrix(this Transform trans, Transform relativeTo)
        {
            var m = trans.localToWorldMatrix;
            return relativeTo.worldToLocalMatrix * m;
        }

        public static Matrix4x4 GetLocalMatrix(this Transform trans)
        {
            return Matrix4x4.TRS(trans.localPosition, trans.localRotation, trans.localScale);
        }

        #endregion



        #region Matrix Methods

        public static Vector3 GetTranslation(this Matrix4x4 m)
        {
            var col = m.GetColumn(3);
            return new Vector3(col.x, col.y, col.z);
        }

        public static Quaternion GetRotation(this Matrix4x4 m)
        {
            // Adapted from: http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/index.htm
            Quaternion q = new Quaternion();
            q.w = Mathf.Sqrt(Mathf.Max(0, 1 + m[0, 0] + m[1, 1] + m[2, 2])) / 2;
            q.x = Mathf.Sqrt(Mathf.Max(0, 1 + m[0, 0] - m[1, 1] - m[2, 2])) / 2;
            q.y = Mathf.Sqrt(Mathf.Max(0, 1 - m[0, 0] + m[1, 1] - m[2, 2])) / 2;
            q.z = Mathf.Sqrt(Mathf.Max(0, 1 - m[0, 0] - m[1, 1] + m[2, 2])) / 2;
            q.x *= Mathf.Sign(q.x * (m[2, 1] - m[1, 2]));
            q.y *= Mathf.Sign(q.y * (m[0, 2] - m[2, 0]));
            q.z *= Mathf.Sign(q.z * (m[1, 0] - m[0, 1]));
            return q;
        }

        public static Vector3 GetScale(this Matrix4x4 m)
        {
            //var xs = m.GetColumn(0);
            //var ys = m.GetColumn(1);
            //var zs = m.GetColumn(2);

            //var sc = new Vector3();
            //sc.x = Vector3.Magnitude(new Vector3(xs.x, xs.y, xs.z));
            //sc.y = Vector3.Magnitude(new Vector3(ys.x, ys.y, ys.z));
            //sc.z = Vector3.Magnitude(new Vector3(zs.x, zs.y, zs.z));

            //return sc;

            return new Vector3(m.GetColumn(0).magnitude, m.GetColumn(1).magnitude, m.GetColumn(2).magnitude);
        }

        #endregion

        #region GetAxis

        public static Vector3 GetAxis(CartesianAxis axis)
        {
            switch (axis)
            {
                case CartesianAxis.Xneg:
                    return Vector3.left;
                case CartesianAxis.Yneg:
                    return Vector3.down;
                case CartesianAxis.Zneg:
                    return Vector3.back;
                case CartesianAxis.X:
                    return Vector3.right;
                case CartesianAxis.Y:
                    return Vector3.up;
                case CartesianAxis.Z:
                    return Vector3.forward;
            }

            return Vector3.zero;
        }

        public static Vector3 GetAxis(this Transform trans, CartesianAxis axis)
        {
            if (trans == null) throw new System.ArgumentNullException("trans");

            switch (axis)
            {
                case CartesianAxis.Xneg:
                    return -trans.right;
                case CartesianAxis.Yneg:
                    return -trans.up;
                case CartesianAxis.Zneg:
                    return -trans.forward;
                case CartesianAxis.X:
                    return trans.right;
                case CartesianAxis.Y:
                    return trans.up;
                case CartesianAxis.Z:
                    return trans.forward;
            }

            return Vector3.zero;
        }

        public static Vector3 GetAxis(this Transform trans, CartesianAxis axis, bool inLocalSpace)
        {
            if (trans == null) throw new System.ArgumentNullException("trans");

            Vector3 v = Vector3.zero;
            switch (axis)
            {
                case CartesianAxis.Xneg:
                    v = -trans.right;
                    break;
                case CartesianAxis.Yneg:
                    v = -trans.up;
                    break;
                case CartesianAxis.Zneg:
                    v = -trans.forward;
                    break;
                case CartesianAxis.X:
                    v = trans.right;
                    break;
                case CartesianAxis.Y:
                    v = trans.up;
                    break;
                case CartesianAxis.Z:
                    v = trans.forward;
                    break;
            }

            return (inLocalSpace) ? trans.InverseTransformDirection(v) : v;
        }

        #endregion

        #region Parent

        public static Vector3 ParentTransformPoint(this Transform t, Vector3 pnt)
        {
            if (t.parent == null) return pnt;
            return t.parent.TransformPoint(pnt);
        }

        public static Vector3 ParentInverseTransformPoint(this Transform t, Vector3 pnt)
        {
            if (t.parent == null) return pnt;
            return t.parent.InverseTransformPoint(pnt);
        }

        public static Quaternion ParentTransformRotation(this Transform t, Quaternion rot)
        {
            if (t.parent == null) return rot;
            return t.parent.InverseTransformRotation(rot);
        }

        public static Quaternion ParentInverseTransformRotation(this Transform t, Quaternion rot)
        {
            if (t.parent == null) return rot;
            return t.parent.InverseTransformRotation(rot);
        }

        #endregion

        #region Transform Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ZeroOutPositionAndRotation(this GameObject go)
        {
#if UNITY_2021_3_OR_NEWER
            go.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
#else
            go.transform.position = Vector3.zero;
            go.transform.rotation = Quaternion.identity;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ZeroOutPositionAndRotation(this Transform trans)
        {
#if UNITY_2021_3_OR_NEWER
            trans.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
#else
            trans.position = Vector3.zero;
            trans.rotation = Quaternion.identity;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ZeroOutLocalPositionAndRotation(this GameObject go)
        {
#if UNITY_2021_3_OR_NEWER
            go.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
#else
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ZeroOutLocalPositionAndRotation(this Transform trans)
        {
#if UNITY_2021_3_OR_NEWER
            trans.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
#else
            trans.localPosition = Vector3.zero;
            trans.localRotation = Quaternion.identity;
#endif
        }

        /// <summary>
        /// Zeroes out position, rotation, and optionally scale of transform as well as velocity and angularVelocity of rigidbody attached to GameObject.
        /// </summary>
        /// <param name="go"></param>
        /// <param name="bIgnoreScale"></param>
        /// <param name="bGlobal"></param>
        public static void ZeroOut(this GameObject go, bool bIgnoreScale, bool bGlobal = false, bool bIgnoreRigidbody = false)
        {
            if (bGlobal)
            {
#if UNITY_2021_3_OR_NEWER
                go.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
#else
                go.transform.position = Vector3.zero;
                go.transform.rotation = Quaternion.identity;
#endif
                if (!bIgnoreScale) go.transform.localScale = Vector3.one;
            }
            else
            {
#if UNITY_2021_3_OR_NEWER
                go.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
#else
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
#endif
                if (!bIgnoreScale) go.transform.localScale = Vector3.one;
            }

            if (!bIgnoreRigidbody)
            {
                var rb = go.GetComponent<Rigidbody>();
                if (rb != null && !rb.isKinematic)
                {
#if UNITY_2023_3_OR_NEWER
                    rb.linearVelocity = Vector3.zero;
#else
                    rb.velocity = Vector3.zero;
#endif
                    rb.angularVelocity = Vector3.zero;

                }
            }
        }

        /// <summary>
        /// Zeroes out position, rotation, and optionally scale of transform.
        /// </summary>
        /// <param name="go"></param>
        /// <param name="bIgnoreScale"></param>
        /// <param name="bGlobal"></param>
        public static void ZeroOut(this Transform trans, bool bIgnoreScale, bool bGlobal = false)
        {
            if (bGlobal)
            {
#if UNITY_2021_3_OR_NEWER
                trans.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
#else
                trans.position = Vector3.zero;
                trans.rotation = Quaternion.identity;
#endif
                if (!bIgnoreScale) trans.localScale = Vector3.one;
            }
            else
            {
#if UNITY_2021_3_OR_NEWER
                trans.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
#else
                trans.localPosition = Vector3.zero;
                trans.localRotation = Quaternion.identity;
#endif
                if (!bIgnoreScale) trans.localScale = Vector3.one;
            }
        }

        public static void ZeroOut(this Rigidbody body)
        {
            if (body.isKinematic) return;

#if UNITY_2023_3_OR_NEWER
            body.linearVelocity = Vector3.zero;
#else
            body.velocity = Vector3.zero;
#endif
            body.angularVelocity = Vector3.zero;
        }

        public static void CopyTransform(this GameObject dst, GameObject src, bool bSetScale = false)
        {
            if (src == null) throw new System.ArgumentNullException("src");
            if (dst == null) throw new System.ArgumentNullException("dst");
            CopyTransform(src.transform, dst.transform);
        }

        public static void CopyTransform(this Transform dst, Transform src, bool bSetScale = false)
        {
            if (src == null) throw new System.ArgumentNullException("src");
            if (dst == null) throw new System.ArgumentNullException("dst");

            Trans.GetGlobal(src).SetToGlobal(dst, bSetScale);
        }



        public static Vector3 GetRelativePosition(this Transform trans, Transform relativeTo)
        {
            return relativeTo.InverseTransformPoint(trans.position);
        }

        public static Quaternion GetRelativeRotation(this Transform trans, Transform relativeTo)
        {
            //return trans.rotation * Quaternion.Inverse(relativeTo.rotation);
            return Quaternion.Inverse(relativeTo.rotation) * trans.rotation;
            //return Quaternion.Inverse(trans.rotation) * relativeTo.rotation;
        }

        /// <summary>
        /// Multiply a vector by only the scale part of a transformation
        /// </summary>
        /// <param name="m"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector3 ScaleVector(this Matrix4x4 m, Vector3 v)
        {
            var sc = m.GetScale();
            return Matrix4x4.Scale(sc).MultiplyPoint(v);
        }

        public static Vector3 ScaleVector(this Trans t, Vector3 v)
        {
            return Matrix4x4.Scale(t.Scale).MultiplyPoint(v);
        }

        public static Vector3 ScaleVector(this Transform t, Vector3 v)
        {
            var sc = t.localToWorldMatrix.GetScale();
            return Matrix4x4.Scale(sc).MultiplyPoint(v);
        }

        /// <summary>
        /// Inverse multiply a vector by on the scale part of a transformation
        /// </summary>
        /// <param name="m"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector3 InverseScaleVector(this Matrix4x4 m, Vector3 v)
        {
            var sc = m.inverse.GetScale();
            return Matrix4x4.Scale(sc).MultiplyPoint(v);
        }

        public static Vector3 InvserScaleVector(this Trans t, Vector3 v)
        {
            var sc = t.Matrix.inverse.GetScale();
            return Matrix4x4.Scale(sc).MultiplyPoint(v);
        }

        public static Vector3 InverseScaleVector(this Transform t, Vector3 v)
        {
            var sc = t.worldToLocalMatrix.GetScale();
            return Matrix4x4.Scale(sc).MultiplyPoint(v);
        }

        public static Quaternion TranformRotation(this Matrix4x4 m, Quaternion rot)
        {
            return rot * m.GetRotation();
        }

        public static Quaternion TransformRotation(Trans t, Quaternion rot)
        {
            return rot * t.Rotation;
        }

        public static Quaternion TransformRotation(this Transform t, Quaternion rot)
        {
            return rot * t.rotation;
        }

        public static Quaternion InverseTranformRotation(this Matrix4x4 m, Quaternion rot)
        {
            //return rot * Quaternion.Inverse(m.GetRotation());
            return Quaternion.Inverse(m.GetRotation()) * rot;
        }

        public static Quaternion InverseTransformRotation(Trans t, Quaternion rot)
        {
            //return rot * Quaternion.Inverse(t.Rotation);
            return Quaternion.Inverse(t.Rotation) * rot;
        }

        public static Quaternion InverseTransformRotation(this Transform t, Quaternion rot)
        {
            //return rot * Quaternion.Inverse(t.rotation);
            return Quaternion.Inverse(t.rotation) * rot;
        }

        /// <summary>
        /// Apply a transform to a Trans.
        /// </summary>
        /// <param name="m"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Trans TransformTrans(this Matrix4x4 m, Trans t)
        {
            t.Matrix *= m;
            return t;
        }

        public static Trans TransformTrans(this Trans t, Trans t2)
        {
            t2.Matrix *= t.Matrix;
            return t2;
        }

        public static Trans TransformTrans(this Transform t, Trans t2)
        {
            t2.Matrix *= t.localToWorldMatrix;
            return t2;
        }

        public static Trans InverseTransformTrans(this Matrix4x4 m, Trans t)
        {
            t.Matrix *= m.inverse;
            return t;
        }

        public static Trans InverseTransformTrans(this Trans t, Trans t2)
        {
            t2.Matrix *= t.Matrix.inverse;
            return t2;
        }

        public static Trans InverseTransformTrans(this Transform t, Trans t2)
        {
            t2.Matrix *= t.worldToLocalMatrix;
            return t2;
        }

        /// <summary>
        /// Transform a ray by a transformation
        /// </summary>
        /// <param name="m"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static Ray TransformRay(this Matrix4x4 m, Ray r)
        {
            return new Ray(m.MultiplyPoint(r.origin), m.MultiplyVector(r.direction));
        }

        public static Ray TransformRay(this Trans t, Ray r)
        {
            return new Ray(t.TransformPoint(r.origin), t.TransformDirection(r.direction));
        }

        public static Ray TransformRay(this Transform t, Ray r)
        {
            return new Ray(t.TransformPoint(r.origin), t.TransformDirection(r.direction));
        }

        /// <summary>
        /// Inverse transform a ray by a transformation
        /// </summary>
        /// <param name="m"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static Ray InverseTransformRay(this Matrix4x4 m, Ray r)
        {
            m = m.inverse;
            return new Ray(m.MultiplyPoint(r.origin), m.MultiplyVector(r.direction));
        }

        public static Ray InverseTransformRay(this Trans t, Ray r)
        {
            return new Ray(t.InverseTransformPoint(r.origin), t.InverseTransformDirection(r.direction));
        }

        public static Ray InverseTransformRay(this Transform t, Ray r)
        {
            return new Ray(t.InverseTransformPoint(r.origin), t.InverseTransformDirection(r.direction));
        }

        /// <summary>
        /// Transform ray cast info by a transformation
        /// </summary>
        /// <param name="m"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static RaycastInfo TransformRayCastInfo(this Matrix4x4 m, RaycastInfo r)
        {
            float dist = r.Distance;
            var sc = m.GetScale();
            if (sc.sqrMagnitude != 1.0f)
            {
                var v = r.Direction * r.Distance;
                v = Matrix4x4.Scale(sc).MultiplyPoint(v);
                dist = v.magnitude;
            }

            return new RaycastInfo(m.MultiplyPoint(r.Origin), m.MultiplyVector(r.Direction), dist);
        }

        public static RaycastInfo TransformRayCastInfo(this Trans t, RaycastInfo r)
        {
            float dist = r.Distance;
            var sc = t.Scale;
            if (sc.sqrMagnitude != 1.0f)
            {
                var v = r.Direction * r.Distance;
                v = Matrix4x4.Scale(sc).MultiplyPoint(v);
                dist = v.magnitude;
            }

            return new RaycastInfo(t.TransformPoint(r.Origin), t.TransformDirection(r.Direction), dist);
        }

        public static RaycastInfo TransformRayCastInfo(this Transform t, RaycastInfo r)
        {
            float dist = r.Distance;
            var sc = t.localToWorldMatrix.GetScale();
            if (sc.sqrMagnitude != 1.0f)
            {
                var v = r.Direction * r.Distance;
                v = Matrix4x4.Scale(sc).MultiplyPoint(v);
                dist = v.magnitude;
            }

            return new RaycastInfo(t.TransformPoint(r.Origin), t.TransformDirection(r.Direction), dist);
        }

#endregion

        #region Transpose Methods

        /// <summary>
        /// Set the position and rotation of a Transform as if its origin were that of 'anchor'. 
        /// Anchor should be in world space.
        /// </summary>
        /// <param name="trans">The transform to transpose.</param>
        /// <param name="anchor">The point around which to transpose in world space.</param>
        /// <param name="position">The new position in world space.</param>
        /// <param name="rotation">The new rotation in world space.</param>
        public static void TransposeAroundGlobalAnchor(this Transform trans, Vector3 anchor, Vector3 position, Quaternion rotation)
        {
            anchor = trans.InverseTransformPoint(anchor);
            if (trans.parent != null)
            {
                position = trans.parent.InverseTransformPoint(position);
                rotation = trans.parent.InverseTransformRotation(rotation);
            }

            LocalTransposeAroundAnchor(trans, anchor, position, rotation);
        }

        /// <summary>
        /// Set the position and rotation of a Transform as if its origin were that of 'anchor'. 
        /// Anchor should be in world space.
        /// </summary>
        /// <param name="trans">The transform to transpose.</param>
        /// <param name="anchor">The point around which to transpose in world space.</param>
        /// <param name="position">The new position in world space.</param>
        /// <param name="rotation">The new rotation in world space.</param>
        public static void TransposeAroundGlobalAnchor(this Transform trans, Trans anchor, Vector3 position, Quaternion rotation)
        {
            //anchor.Matrix *= trans.worldToLocalMatrix;
            anchor.Matrix = trans.worldToLocalMatrix * anchor.Matrix;
            if (trans.parent != null)
            {
                position = trans.parent.InverseTransformPoint(position);
                rotation = trans.parent.InverseTransformRotation(rotation);
            }

            LocalTransposeAroundAnchor(trans, anchor, position, rotation);
        }

        /// <summary>
        /// Set the position and rotation of a Transform as if its origin were that of 'anchor'. 
        /// Anchor should be local to the Transform where <0,0,0> would be the same as its true origin.
        /// </summary>
        /// <param name="trans">The transform to transpose.</param>
        /// <param name="anchor">The point around which to transpose in local space.</param>
        /// <param name="position">The new position in world space.</param>
        /// <param name="rotation">The new rotation in world space.</param>
        public static void TransposeAroundAnchor(this Transform trans, Vector3 anchor, Vector3 position, Quaternion rotation)
        {
            if (trans.parent != null)
            {
                position = trans.parent.InverseTransformPoint(position);
                rotation = trans.parent.InverseTransformRotation(rotation);
            }

            LocalTransposeAroundAnchor(trans, anchor, position, rotation);
        }

        /// <summary>
        /// Set the position and rotation of a Transform as if its origin were that of 'anchor'. 
        /// Anchor should be local to the Transform where <0,0,0> would be the same as its true origin.
        /// </summary>
        /// <param name="trans">The transform to transpose.</param>
        /// <param name="anchor">The point around which to transpose in local space.</param>
        /// <param name="position">The new position in world space.</param>
        /// <param name="rotation">The new rotation in world space.</param>
        public static void TransposeAroundAnchor(this Transform trans, Trans anchor, Vector3 position, Quaternion rotation)
        {
            if (trans.parent != null)
            {
                position = trans.parent.InverseTransformPoint(position);
                rotation = trans.parent.InverseTransformRotation(rotation);
            }

            LocalTransposeAroundAnchor(trans, anchor, position, rotation);
        }

        /// <summary>
        /// Set the position and rotation of a Transform as if its origin were that of 'anchor'. 
        /// Anchor should be local to the Transform where <0,0,0> would be the same as its true origin.
        /// </summary>
        /// <param name="trans">The transform to transpose.</param>
        /// <param name="anchor">The point around which to transpose in world space.</param>
        /// <param name="position">The new position in world space.</param>
        /// <param name="rotation">The new rotation in world space.</param>
        public static void TransposeAroundAnchor(this Transform trans, Transform anchor, Vector3 position, Quaternion rotation)
        {
            if (trans.parent != null)
            {
                position = trans.parent.InverseTransformPoint(position);
                rotation = trans.parent.InverseTransformRotation(rotation);
            }

            LocalTransposeAroundAnchor(trans, anchor.ToRelativeTrans(trans), position, rotation);
        }

        /// <summary>
        /// Set the localPosition and localRotation of a Transform as if its origin were that of 'anchor'. 
        /// Anchor should be local to the Transform where <0,0,0> would be the same as its true origin.
        /// </summary>
        /// <param name="trans">The transform to transpose.</param>
        /// <param name="anchor">The point around which to transpose relative to the transform.</param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        public static void LocalTransposeAroundAnchor(this Transform trans, Vector3 anchor, Vector3 position, Quaternion rotation)
        {
            anchor = rotation * Vector3.Scale(anchor, trans.localScale);
#if UNITY_2021_3_OR_NEWER
            trans.SetLocalPositionAndRotation(position - anchor, rotation);
#else
            trans.localPosition = position - anchor;
            trans.localRotation = rotation;
#endif
        }

        public static void LocalTransposeAroundAnchor(this Transform trans, Trans anchor, Vector3 position, Quaternion rotation)
        {
            var anchorPos = rotation * Vector3.Scale(anchor.Position, trans.localScale);
#if UNITY_2021_3_OR_NEWER
            trans.SetLocalPositionAndRotation(position - anchorPos, anchor.Rotation * rotation);
#else
            trans.localPosition = position - anchorPos;
            trans.localRotation = anchor.Rotation * rotation;
#endif
        }

        public static void LocalTransposeAroundAnchor(this Transform trans, Transform anchor, Vector3 position, Quaternion rotation)
        {
            var m = anchor.GetRelativeMatrix(trans);

            var anchorPos = rotation * Vector3.Scale(m.GetTranslation(), trans.localScale);
#if UNITY_2021_3_OR_NEWER
            trans.SetLocalPositionAndRotation(position - anchorPos, m.GetRotation() * rotation);
#else
            trans.localPosition = position - anchorPos;
            trans.localRotation = m.GetRotation() * rotation;
#endif
        }

        #endregion

        #region Camera-Like

        public static Ray ViewportPointToRay(Vector2 vp, Matrix4x4 proj, Matrix4x4 trs)
        {
            var worldToCam = Matrix4x4.Scale(new Vector3(1f, 1f, -1f)) * trs.inverse;
            var m = proj * worldToCam;
            var mInv = m.inverse;
            // near clipping plane point
            Vector4 p = new Vector4(vp.x * 2f - 1f, vp.y * 2f - 1f, -1f, 1f);
            var p0 = mInv * p;
            p0 /= p0.w;
            // far clipping plane point
            p.z = 1;
            var p1 = mInv * p;
            p1 /= p1.w;
            return new Ray(p0, (p1 - p0).normalized);
        }

        #endregion

    }

}
