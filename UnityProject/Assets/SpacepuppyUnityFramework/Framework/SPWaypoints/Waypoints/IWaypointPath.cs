using UnityEngine;
using System.Collections.Generic;

namespace com.spacepuppy.Waypoints
{
    public interface IWaypointPath
    {

        bool IsClosed { get; set; }

        float GetArcLength();
        Vector3 GetPositionAt(float t);
        Waypoint GetWaypointAt(float t);

        int GetDetailedPositions(ICollection<Vector3> coll, float segmentLength);
    }

    public interface IIndexedWaypointPath : IWaypointPath, IEnumerable<IControlPoint>
    {

        int Count { get; }
        IControlPoint ControlPoint(int index);
        int IndexOf(IControlPoint waypoint);
        Vector3 GetPositionAfter(int index, float t);
        Waypoint GetWaypointAfter(int index, float t);
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

    }

}
