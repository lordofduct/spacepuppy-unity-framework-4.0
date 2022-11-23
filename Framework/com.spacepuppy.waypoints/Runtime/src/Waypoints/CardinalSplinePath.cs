using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Utils;
using Codice.Client.Commands;

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
                _points = _isClosed ? new Vector3[_controlPoints.Count + 3] : new Vector3[_controlPoints.Count + 2];
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
                int multiplier = Mathf.Clamp(Mathf.RoundToInt(avglength), MIN_SUBDIVISIONS_MULTIPLIER, MAX_SUBDIVISIONS_MULTIPLIER);
                _speedTable.Clean(multiplier * (_controlPoints.Count - 1), this.GetRealPositionAt);
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
            int numSections = _controlPoints.Count - 1;
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

        public float GetArcLength(float t)
        {
            if (_speedTable.IsDirty) this.Clean_Imp();

            if (_useConstantSpeed)
            {
                return _speedTable.TotalArcLength * Mathf.Clamp01(t);
            }
            else
            {
                return _speedTable.GetArcLengthAtTimePerc(t);
            }
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

        public Vector3 GetPositionAfter(int index, float tprime)
        {
            if (index < 0 || index >= _controlPoints.Count) throw new System.IndexOutOfRangeException();
            if (_controlPoints.Count < 2) return (_controlPoints.Count == 0) ? VectorUtil.NaNVector3 : _controlPoints[0].Position;
            if (_speedTable.IsDirty) this.Clean_Imp();

            float a, b, t;
            if (index == _controlPoints.Count - 1)
            {
                return _controlPoints[index].Position;
            }
            else if (_useConstantSpeed)
            {
                a = _speedTable.GetArcLength(0, index * (_speedTable.SubdivisionCount / (_controlPoints.Count - 1)));
                b = _speedTable.GetArcLength(0, (index + 1) * (_speedTable.SubdivisionCount / (_controlPoints.Count - 1)));
                t = Mathf.Lerp(a, b, tprime) / _speedTable.TotalArcLength;
                return this.GetPositionAt(t);
            }
            else
            {
                a = 1f / (float)(_controlPoints.Count - 1);
                b = (float)index * a;
                b += a * tprime;
                return this.GetRealPositionAt(b);
            }
        }

        public Waypoint GetWaypointAfter(int index, float tprime)
        {
            if (index < 0 || index >= _controlPoints.Count) throw new System.IndexOutOfRangeException();
            if (_controlPoints.Count < 2) return (_controlPoints.Count == 0) ? Waypoint.Invalid : new Waypoint(_controlPoints[0].Position, Vector3.zero);
            if (_speedTable.IsDirty) this.Clean_Imp();

            float a, b, t;
            if (index == _controlPoints.Count - 1)
            {
                var cp = _controlPoints[index];
                return new Waypoint(cp.Position, cp.Heading);
            }
            else if (_useConstantSpeed)
            {
                a = _speedTable.GetArcLength(0, index * (_speedTable.SubdivisionCount / (_controlPoints.Count - 1)));
                b = _speedTable.GetArcLength(0, (index + 1) * (_speedTable.SubdivisionCount / (_controlPoints.Count - 1)));
                t = Mathf.Lerp(a, b, tprime) / _speedTable.TotalArcLength;
                return this.GetWaypointAt(t);
            }
            else
            {
                a = 1f / (float)(_controlPoints.Count - 1);
                b = (float)index * a;
                b += a * tprime;

                var p1 = this.GetRealPositionAt(b);
                var p2 = this.GetRealPositionAt(b + 0.01f); //TODO - figure out a more efficient way of calculating the tangent
                return new Waypoint(p1, (p2 - p1).normalized);
            }
        }

        public float GetArcLengthAtIndex(int index)
        {
            if (index < 0 || index >= _controlPoints.Count) throw new System.IndexOutOfRangeException();
            if (_controlPoints.Count < 2) return 0f;
            if (index == _controlPoints.Count - 1) return 0f; //length is 0 after the last index
            if (_points == null) this.Clean_Imp();

            if (index == 0) return 0f;
            if (index == _controlPoints.Count - 1) return this.GetArcLength();
            return _speedTable.GetArcLength(0, Mathf.CeilToInt(_speedTable.SubdivisionCount * (float)index / (float)(_controlPoints.Count - 1)));
        }

        public float GetArcLengthAfter(int index)
        {
            if (index < 0 || index >= _controlPoints.Count) throw new System.IndexOutOfRangeException();
            if (_controlPoints.Count < 2) return 0f;
            if (index == _controlPoints.Count - 1) return 0f; //length is 0 after the last index
            if (_points == null) this.Clean_Imp();

            float st = 1f / (float)(_controlPoints.Count - 1);
            float tlow = (float)index * st;
            float thigh = (float)(index + 1) * st;

            int ilow = Mathf.FloorToInt(tlow * _speedTable.SubdivisionCount);
            int ihigh = Mathf.CeilToInt(thigh * _speedTable.SubdivisionCount);
            return _speedTable.GetArcLength(ilow, ihigh);
        }

        public float GetArcLengthAfter(int index, float tprime)
        {
            if (_useConstantSpeed)
            {
                return GetArcLengthAfter(index) * Mathf.Clamp01(tprime);
            }
            else
            {
                //similar logic found in GetPositionAfter, we estimate the t based on tprime
                float a = 1f / (float)(_controlPoints.Count - 1);
                float t = (float)index * a;
                t += a * tprime;
                return GetArcLength(t) - GetArcLengthAtIndex(index);
            }
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
