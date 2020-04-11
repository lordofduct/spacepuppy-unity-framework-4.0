using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Utils;

namespace com.spacepuppy.Waypoints
{

    /// <summary>
    /// Represents a catmull-rom cardinal spline.
    /// </summary>
    public class CardinalSplinePath : IConfigurableIndexedWaypointPath
    {

        #region Fields

        private const int MIN_SUBDIVISIONS_MULTIPLIER = 16;
        private const int MAX_SUBDIVISIONS_MULTIPLIER = 64;

        private bool _isClosed;
        private List<IControlPoint> _controlPoints = new List<IControlPoint>();
        private bool _useConstantSpeed = true;

        private Vector3[] _points;
        private CurveConstantSpeedTable _speedTable = new CurveConstantSpeedTable();
        private int _subdivisionNultiplier;

        #endregion

        #region CONSTRUCTOR

        public CardinalSplinePath()
        {

        }

        public CardinalSplinePath(IEnumerable<IControlPoint> waypoints)
        {
            _controlPoints.AddRange(waypoints);
            this.Clean_Imp();
        }

        #endregion

        #region Properties

        public bool UseConstantSpeed
        {
            get { return _useConstantSpeed; }
            set { _useConstantSpeed = value; }
        }

        #endregion

        #region Methods

        private void Clean_Imp()
        {
            if (_controlPoints.Count == 0)
            {
                _points = new Vector3[] { };
                _speedTable.SetZero();
                return;
            }
            else if (_controlPoints.Count == 1)
            {
                _points = new Vector3[] { _controlPoints[0].Position };
                _speedTable.SetZero();
                return;
            }
            else
            {
                //get points
                float avglength = 0f;
                Vector3 lastPos = _controlPoints[0].Position;
                _points = (_isClosed) ? new Vector3[_controlPoints.Count + 3] : new Vector3[_controlPoints.Count + 2];
                for (int i = 0; i < _controlPoints.Count; i++)
                {
                    avglength += (_controlPoints[i].Position - lastPos).sqrMagnitude;
                    lastPos = _controlPoints[i].Position;

                    _points[i + 1] = _controlPoints[i].Position;
                }
                if (_isClosed)
                {
                    _points[0] = _controlPoints[_controlPoints.Count - 1].Position;
                    _points[_points.Length - 2] = _points[1];
                    _points[_points.Length - 1] = _points[2];
                }
                else
                {
                    _points[0] = _points[1];
                    var lastPnt = _controlPoints[_controlPoints.Count - 1].Position;
                    var diffV = lastPnt - _controlPoints[_controlPoints.Count - 2].Position;
                    _points[_points.Length - 1] = lastPnt + diffV;
                }

                avglength = Mathf.Sqrt(avglength / _controlPoints.Count);
                _subdivisionNultiplier = Mathf.Clamp(Mathf.RoundToInt(avglength), MIN_SUBDIVISIONS_MULTIPLIER, MAX_SUBDIVISIONS_MULTIPLIER);
                _speedTable.Clean(_subdivisionNultiplier * _points.Length, this.GetRealPositionAt);
            }
        }

        /// <summary>
        /// Returns the position with out speed correction on the path of points. 
        /// This method does NOT validate itself, make sure the curve is clean before calling.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private Vector3 GetRealPositionAt(float t)
        {
            int numSections = _points.Length - 3;
            int tSec = Mathf.FloorToInt(t * numSections);
            int currPt = numSections - 1;
            if (currPt > tSec) currPt = tSec;
            float u = t * numSections - currPt;

            Vector3 a = _points[currPt];
            Vector3 b = _points[currPt + 1];
            Vector3 c = _points[currPt + 2];
            Vector3 d = _points[currPt + 3];
            return 0.5f * (
                    (-a + 3f * b - 3f * c + d) * (u * u * u)
                    + (2f * a - 5f * b + 4f * c - d) * (u * u)
                    + (-a + c) * u
                    + 2f * b
                    );
        }

        #endregion

        #region IWaypointPath

        public bool IsClosed
        {
            get { return _isClosed; }
            set
            {
                if (value == _isClosed) return;
                _isClosed = value;
                _points = null;
            }
        }


        public Vector3 GetPositionAt(float t)
        {
            if (_speedTable.IsDirty) this.Clean_Imp();
            if (_points.Length < 2) return (_points.Length == 0) ? VectorUtil.NaNVector3 : _points[0];

            if (_useConstantSpeed) t = _speedTable.GetConstPathPercFromTimePerc(t);

            return GetRealPositionAt(t);
        }

        public Waypoint GetWaypointAt(float t)
        {
            if (_speedTable.IsDirty) this.Clean_Imp();
            if (_points.Length < 2) return (_points.Length == 0) ? Waypoint.Invalid : new Waypoint(_points[0], Vector3.zero);

            if (_useConstantSpeed) t = _speedTable.GetConstPathPercFromTimePerc(t);

            var p1 = this.GetRealPositionAt(t);
            var p2 = this.GetRealPositionAt(t + 0.01f); //TODO - figure out a more efficient way of calculating the tangent
            return new Waypoint(p1, (p2 - p1).normalized);
        }

        public float GetArcLength()
        {
            if (_speedTable.IsDirty) this.Clean_Imp();
            return _speedTable.TotalArcLength;
        }

        public int GetDetailedPositions(ICollection<Vector3> coll, float segmentLength)
        {
            if (coll == null) throw new System.ArgumentNullException("coll");
            int detail = Mathf.FloorToInt(this.GetArcLength() / segmentLength) + 1;
            for (int i = 0; i <= detail; i++)
            {
                coll.Add(this.GetPositionAt((float)i / (float)detail));
            }
            return detail + 1;
        }

        #endregion

        #region IIndexedWaypointPath Interface

        public int Count
        {
            get { return _controlPoints.Count; }
        }

        public IControlPoint ControlPoint(int index)
        {
            return _controlPoints[index];
        }

        public int IndexOf(IControlPoint controlpoint)
        {
            return _controlPoints.IndexOf(controlpoint);
        }

        public Vector3 GetPositionAfter(int index, float t)
        {
            if (index < 0 || index >= _controlPoints.Count) throw new System.IndexOutOfRangeException();
            if (_speedTable.IsDirty) this.Clean_Imp();
            if (_points.Length < 2) return (_points.Length == 0) ? VectorUtil.NaNVector3 : _points[0];

            //we add 1 to compensate for the control points in _points/_speedTable
            int i = index * _subdivisionNultiplier + 1;
            int j = (index + 1) * _subdivisionNultiplier + 1;
            float nt = _speedTable.GetTimeAtSubdivision(i) + (_speedTable.GetTimeAtSubdivision(j) - _speedTable.GetTimeAtSubdivision(i)) * t;
            return this.GetRealPositionAt(nt);
        }

        public Waypoint GetWaypointAfter(int index, float t)
        {
            if (index < 0 || index >= _controlPoints.Count) throw new System.IndexOutOfRangeException();
            if (_speedTable.IsDirty) this.Clean_Imp();
            if (_points.Length < 2) return (_points.Length == 0) ? Waypoint.Invalid : new Waypoint(_points[0], Vector3.zero);

            //we add 1 to compensate for the control points in _points/_speedTable
            int i = index * _subdivisionNultiplier + 1;
            int j = (index + 1) * _subdivisionNultiplier + 1;
            float nt = _speedTable.GetTimeAtSubdivision(i) + (_speedTable.GetTimeAtSubdivision(j) - _speedTable.GetTimeAtSubdivision(i)) * t;

            var p1 = this.GetRealPositionAt(nt);
            var p2 = this.GetRealPositionAt(nt + 0.01f); //TODO - figure out a more efficient way of calculating the tangent
            return new Waypoint(p1, (p2 - p1).normalized);
        }

        public float GetArcLengthAfter(int index)
        {
            if (index < 0 || index >= _controlPoints.Count) throw new System.IndexOutOfRangeException();
            if (_points == null) this.Clean_Imp();
            if (_points.Length < 2) return 0f;

            //we add 1 to compensate for the control points in _points/_speedTable
            int i = index * _subdivisionNultiplier + 1;
            int j = (index + 1) * _subdivisionNultiplier + 1;
            return _speedTable.GetArcLength(i, j);
        }

        public RelativePositionData GetRelativePositionData(float t)
        {
            int cnt = _controlPoints.Count;
            switch (cnt)
            {
                case 0:
                    return new RelativePositionData(-1, 0f);
                case 1:
                    return new RelativePositionData(0, 0f);
                case 2:
                    return new RelativePositionData(0, t);
                default:
                    {
                        if (_speedTable.IsDirty) this.Clean_Imp();

                        if (_useConstantSpeed) t = _speedTable.GetConstPathPercFromTimePerc(t);

                        t = Mathf.Clamp01(t);
                        if (MathUtil.FuzzyEqual(t, 1f)) return new RelativePositionData(_controlPoints.Count - 1, 0f);

                        int index;
                        float segmentTime;
                        if (_isClosed)
                        {
                            index = Mathf.FloorToInt(cnt * t);
                            segmentTime = 1f / cnt;
                        }
                        else
                        {
                            index = Mathf.FloorToInt((cnt - 1) * t);
                            segmentTime = 1f / (cnt - 1);
                        }
                        float lt = index * segmentTime;
                        float ht = (index + 1) * segmentTime;
                        float dt = MathUtil.PercentageMinMax(t, ht, lt);

                        return new RelativePositionData(index, dt);
                    }
            }
        }

        public void Clean()
        {
            _points = null;
        }

        #endregion

        #region IConfigurableIndexedWaypointPath Interface

        public void AddControlPoint(IControlPoint controlpoint)
        {
            _controlPoints.Add(controlpoint);
            _points = null;
        }

        public void InsertControlPoint(int index, IControlPoint controlpoint)
        {
            _controlPoints.Insert(index, controlpoint);
            _points = null;
        }

        public void ReplaceControlPoint(int index, IControlPoint controlpoint)
        {
            _controlPoints[index] = controlpoint;
            _points = null;
        }

        public void RemoveControlPointAt(int index)
        {
            _controlPoints.RemoveAt(index);
            _points = null;
        }

        public void Clear()
        {
            _controlPoints.Clear();
            _points = null;
        }

        public void DrawGizmos(float segmentLength)
        {
            if (_controlPoints.Count <= 1) return;

            var length = this.GetArcLength();
            int divisions = Mathf.FloorToInt(length / segmentLength) + 1;

            for (int i = 0; i < divisions; i++)
            {
                float t1 = (float)i / (float)divisions;
                float t2 = (float)(i + 1) / (float)divisions;

                Gizmos.DrawLine(this.GetPositionAt(t1), this.GetPositionAt(t2));
            }
        }

        #endregion

        #region IEnumerable Interface

        public IEnumerator<IControlPoint> GetEnumerator()
        {
            return _controlPoints.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _controlPoints.GetEnumerator();
        }

        #endregion

    }

}
