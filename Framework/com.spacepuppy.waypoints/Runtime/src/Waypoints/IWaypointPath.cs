using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Geom;

namespace com.spacepuppy.Waypoints
{
    public interface IWaypointPath
    {

        bool IsClosed { get; set; }

        float GetArcLength();
        float GetArcLength(float t);
        Vector3 GetPositionAt(float t);
        Waypoint GetWaypointAt(float t);

    }

    public interface IIndexedWaypointPath : IWaypointPath, IEnumerable<IControlPoint>
    {

        int Count { get; }
        IControlPoint ControlPoint(int index);
        int IndexOf(IControlPoint waypoint);
        Vector3 GetPositionAfter(int index, float tprime);
        Waypoint GetWaypointAfter(int index, float tprime);
        /// <summary>
        /// Get the arclength from start to controlpoint at 'index'.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        float GetArcLengthAtIndex(int index);
        /// <summary>
        /// Get the arclength between controlpoint at 'index' and the next control point.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        float GetArcLengthAfter(int index);
        /// <summary>
        /// Returns data pertaining to the relative position between the 2 control points on either side of 't'.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        RelativePositionData GetRelativePositionData(float t);


        void Clean();

    }

    public struct RelativePositionData
    {
        /// <summary>
        /// The index after which the position was.
        /// </summary>
        public int Index;
        /// <summary>
        /// The 't' value relative to that last index. Can be passed into GetWaypointAfter(int index, float t) to get a waypoint.
        /// </summary>
        public float TPrime;

        public RelativePositionData(int index, float t)
        {
            this.Index = index;
            this.TPrime = t;
        }
    }

    public interface IConfigurableIndexedWaypointPath : IIndexedWaypointPath
    {
        void AddControlPoint(IControlPoint waypoint);
        void InsertControlPoint(int index, IControlPoint waypoint);
        void ReplaceControlPoint(int index, IControlPoint waypoint);
        void RemoveControlPointAt(int index);
        void Clear();

    }

    public static class WaypointPathExtensions
    {

        public enum PathType
        {
            Cardinal = 0,
            Linear = 1,
            BezierChain = 2,
            BezierSpline = 3
        }

        public static int GetDetailedPositions(this IWaypointPath path, ICollection<Vector3> coll, float segmentLength)
        {
            if (path == null) throw new System.ArgumentNullException(nameof(path));
            if (coll == null) throw new System.ArgumentNullException(nameof(coll));

            if (path is LinearPath lin) return lin.GetDetailedPositions(coll);

            int detail = Mathf.FloorToInt(path.GetArcLength() / segmentLength) + 1;
            for (int i = 0; i <= detail; i++)
            {
                coll.Add(path.GetPositionAt((float)i / (float)detail));
            }
            return detail + 1;
        }

        /// <summary>
        /// Tries to find the nearest point on a segment defined from index to index + 1 for a WaypointPath. 
        /// Since for everything but 'Linear' curves, solving for the accurate point is time consuming. This 
        /// instead finds the closest point on a line between the 2 waypoints, then finds the Waypoint that 
        /// 't' distance between the same 2 waypoints to act as a quick "nearest" estimate.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static Waypoint EstimateNearestWaypointAfter(this IIndexedWaypointPath path, int index, Vector3 pos, out float tout)
        {
            if (path == null) throw new System.ArgumentNullException("path");
            if (index < 0 || index >= path.Count) throw new System.IndexOutOfRangeException();

            switch (path.Count)
            {
                case 0:
                    tout = 0f;
                    return Waypoint.Invalid;
                case 1:
                    tout = 0f;
                    return new Waypoint(path.ControlPoint(0));
                default:
                    {
                        if (index == path.Count - 1)
                        {
                            tout = 1f;
                            return path.GetWaypointAt(1f);
                        }

                        var p0 = path.ControlPoint(index).Position;
                        var p1 = path.ControlPoint(index + 1).Position;
                        var forw = (p1 - p0);
                        var seglen = forw.magnitude;
                        forw /= seglen;
                        tout = Mathf.Clamp01(Vector3.Dot(pos - p0, forw) / seglen);
                        return path.GetWaypointAfter(index, tout);
                    }
            }
        }

        public static Waypoint EstimateNearestWaypointAfter(this IIndexedWaypointPath path, int index, Vector3 pos)
        {
            if (path == null) throw new System.ArgumentNullException("path");
            if (index < 0 || index >= path.Count) throw new System.IndexOutOfRangeException();

            switch (path.Count)
            {
                case 0:
                    return Waypoint.Invalid;
                case 1:
                    return new Waypoint(path.ControlPoint(0));
                default:
                    {
                        if (index == path.Count - 1) return path.GetWaypointAt(1f);

                        var p0 = path.ControlPoint(index).Position;
                        var p1 = path.ControlPoint(index + 1).Position;
                        var forw = (p1 - p0);
                        var seglen = forw.magnitude;
                        forw /= seglen;
                        return path.GetWaypointAfter(index, Mathf.Clamp01(Vector3.Dot(pos - p0, forw) / seglen));
                    }
            }
        }

        public static IConfigurableIndexedWaypointPath CreatePath(PathType type)
        {
            switch (type)
            {
                case PathType.Cardinal:
                    return new CardinalSplinePath();
                case PathType.Linear:
                    return new LinearPath();
                case PathType.BezierChain:
                    return new BezierChainPath();
                case PathType.BezierSpline:
                    return new BezierSplinePath();
                default:
                    return new CardinalSplinePath();
            }
        }

        public static IConfigurableIndexedWaypointPath CreatePath(PathType type, IEnumerable<IControlPoint> waypoints, bool isClosed, bool cloneWaypoints)
        {
            IConfigurableIndexedWaypointPath path = CreatePath(type);
            path.IsClosed = isClosed;
            if (waypoints != null)
            {
                foreach (var wp in waypoints)
                {
                    if (cloneWaypoints)
                        path.AddControlPoint(new Waypoint(wp));
                    else
                        path.AddControlPoint(wp);
                }
            }
            return path;
        }

        public static void DrawGizmos(this IConfigurableIndexedWaypointPath path, float segmentLength)
        {
            DrawGizmos(path, segmentLength, Trans.Identity);
        }

        public static void DrawGizmos(this IConfigurableIndexedWaypointPath path, float segmentLength, Trans trans)
        {
            if (path.Count <= 1) return;
            if (segmentLength <= 0.001f) segmentLength = 0.001f;

            var length = path.GetArcLength();
            int divisions = Mathf.FloorToInt(length / segmentLength) + 1;

            for (int i = 0; i < divisions; i++)
            {
                float t1 = (float)i / (float)divisions;
                float t2 = (float)(i + 1) / (float)divisions;

                Gizmos.DrawLine(path.GetPositionAt(t1), path.GetPositionAt(t2));
                //Gizmos.DrawLine(trans.TransformPoint(path.GetPositionAt(t1)), trans.TransformPoint(path.GetPositionAt(t2)));
            }
        }


    }

}
