using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Utils;

namespace com.spacepuppy.Waypoints
{

    /// <summary>
    /// Acts like a composite of multiple bezier curves. Where one curve ends, the next starts.
    /// The path will travel through control points.
    /// </summary>
    public class BezierChainPath : IConfigurableIndexedWaypointPath
    {

        #region Fields

        private bool _isClosed;
        private List<IControlPoint> _controlPoints = new List<IControlPoint>();

        private Vector3[] _points; //null if dirty
        private float[] _lengths;
        private float _totalArcLength = float.NaN;

        #endregion

        #region CONSTRUCTOR

        public BezierChainPath()
        {

        }

        public BezierChainPath(IEnumerable<IControlPoint> waypoints)
        {
            _controlPoints.AddRange(waypoints);
            this.Clean_Imp();
        }

        #endregion

        #region Properties

        /// <summary>
        /// The default weight if a control point is added that doesn't implement IWeightedControlPoint. Value is 0.5 by default.
        /// </summary>
        public float DefaultControlPoitnWeight { get; set; } = 0.5f;

        #endregion

        #region Methods

        private void Clean_Imp()
        {
            if (_controlPoints.Count == 0)
            {
                _points = new Vector3[] { };
                _totalArcLength = float.NaN;
            }
            else if (_controlPoints.Count == 1)
            {
                _points = new Vector3[] { _controlPoints[0].Position };
                _totalArcLength = 0f;
            }
            else
            {
                _totalArcLength = 0f;

                //get points
                int cnt = _controlPoints.Count;
                if (_isClosed) cnt++;
                _points = new Vector3[(cnt - 1) * 3 + 1];
                _points[0] = _controlPoints[0].Position;
                for (int i = 1; i < cnt; i++)
                {
                    var w1 = _controlPoints[i % _controlPoints.Count]; //if we're closed, this will result in the first entry
                    var w2 = _controlPoints[i - 1];
                    var v = (w1.Position - w2.Position);
                    var s = v.magnitude / 2.0f; //distance of the control point

                    float s1 = (w1 is IWeightedControlPoint wcp1) ? wcp1.Strength : this.DefaultControlPoitnWeight;
                    float s2 = (w2 is IWeightedControlPoint wcp2) ? wcp2.Strength : this.DefaultControlPoitnWeight;
                    int j = (i - 1) * 3 + 1;
                    _points[j] = w2.Position + (w2.Heading * s1 * s);
                    _points[j + 1] = w1.Position - (w1.Heading * s2 * s);
                    _points[j + 2] = w1.Position;
                }

                //calculate lengths
                _lengths = new float[cnt];
                for (int i = 0; i < _lengths.Length - 1; i++)
                {
                    int j = i * 3;
                    var p0 = _points[j];
                    var p1 = _points[j + 1];
                    var p2 = _points[j + 2];
                    var p3 = _points[j + 3];

                    //approximation is sum of all 3 legs, and the coord from p0 to p3, all divided by 2.
                    float t = (p1 - p0).magnitude + (p2 - p1).magnitude + (p3 - p2).magnitude * (p3 - p0).magnitude;
                    _lengths[i] = t / 2.0f;
                }

                _totalArcLength = 0f;
                foreach (var l in _lengths) _totalArcLength += l;
            }
        }

        #endregion

        #region IWaypointPath Interface

        public bool IsClosed
        {
            get { return _isClosed; }
            set
            {
                if (_isClosed == value) return;
                _isClosed = value;
                _points = null;
            }
        }

        public float GetArcLength()
        {
            if (_points == null) this.Clean_Imp();
            return _totalArcLength;
        }

        public float GetArcLength(float t)
        {
            if (_points == null) this.Clean_Imp();
            return _totalArcLength * Mathf.Clamp01(t);
        }

        public Vector3 GetPositionAt(float t)
        {
            if (_points == null) this.Clean_Imp();
            if (_controlPoints.Count == 0) return VectorUtil.NaNVector3;
            if (_controlPoints.Count == 1) return _controlPoints[0].Position;
            if (_controlPoints.Count == 2) return this.GetPositionAfter(0, t);


            float len = _lengths[0];
            float tot = len;
            int i = 0;
            while (tot / _totalArcLength < t && i < _lengths.Length)
            {
                i++;
                len = _lengths[i];
                tot += len;
            }

            float lt = (tot - len) / _totalArcLength;
            float ht = tot / _totalArcLength;
            float dt = com.spacepuppy.Utils.MathUtil.PercentageMinMax(t, ht, lt);
            return this.GetPositionAfter(i, dt);
        }

        public Waypoint GetWaypointAt(float t)
        {
            if (_points == null) this.Clean_Imp();
            if (_controlPoints.Count == 0) return Waypoint.Invalid;
            if (_controlPoints.Count == 1) return new Waypoint(_controlPoints[0]);
            if (_controlPoints.Count == 2) return this.GetWaypointAfter(0, t);


            float len = _lengths[0];
            float tot = len;
            int i = 0;
            while (tot / _totalArcLength < t && i < _lengths.Length)
            {
                i++;
                len = _lengths[i];
                tot += len;
            }

            float lt = (tot - len) / _totalArcLength;
            float ht = tot / _totalArcLength;
            float dt = com.spacepuppy.Utils.MathUtil.PercentageMinMax(t, ht, lt);
            return this.GetWaypointAfter(i, dt);
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
            if (_points == null) this.Clean_Imp();
            if (_controlPoints.Count == 0) return VectorUtil.NaNVector3;
            if (_controlPoints.Count == 1) return _controlPoints[0].Position;
            if (!_isClosed && index == _controlPoints.Count - 1) return _controlPoints[index].Position;

            tprime = Mathf.Clamp01(tprime);
            var ft = 1 - tprime;

            var i = index * 3;
            var p0 = _points[i];
            var p1 = _points[i + 1];
            var p2 = _points[i + 2];
            var p3 = _points[i + 3];

            /*
             C(t) = P0*(1-t)^3 + P1*3*t(1-t)^2 + 3*P2*t^2*(1-t) + P3*t^3

             dC(t)/dt = T(t) =
             -3*P0*(1 - t)^2 + 
             P1*(3*(1 - t)^2 - 6*(1 - t)*t) + 
             P2*(6*(1 - t)*t - 3*t^2) +
             3*P3*t^2
             */
            var p = (ft * ft * ft) * p0 +
                    3 * (ft * ft) * tprime * p1 +
                    3 * (1 - tprime) * (tprime * tprime) * p2 +
                    (tprime * tprime * tprime) * p3;
            var tan = -3 * p0 * (ft * ft) +
                      p1 * (3 * (ft * ft) - 6 * ft * tprime) +
                      p2 * (6 * ft * tprime - 3 * (tprime * tprime)) +
                      3 * p3 * (tprime * tprime);

            return p;
        }

        public Waypoint GetWaypointAfter(int index, float tprime)
        {
            if (index < 0 || index >= _controlPoints.Count) throw new System.IndexOutOfRangeException();
            if (_points == null) this.Clean_Imp();
            if (_controlPoints.Count == 0) return Waypoint.Invalid;
            if (_controlPoints.Count == 1) return new Waypoint(_controlPoints[0]);
            if (!_isClosed && index == _controlPoints.Count - 1) return new Waypoint(_controlPoints[index]);

            tprime = Mathf.Clamp01(tprime);
            var ft = 1 - tprime;

            var i = index * 3;
            var p0 = _points[i];
            var p1 = _points[i + 1];
            var p2 = _points[i + 2];
            var p3 = _points[i + 3];

            /*
             C(t) = P0*(1-t)^3 + P1*3*t(1-t)^2 + 3*P2*t^2*(1-t) + P3*t^3

             dC(t)/dt = T(t) =
             -3*P0*(1 - t)^2 + 
             P1*(3*(1 - t)^2 - 6*(1 - t)*t) + 
             P2*(6*(1 - t)*t - 3*t^2) +
             3*P3*t^2
             */
            var p = (ft * ft * ft) * p0 +
                    3 * (ft * ft) * tprime * p1 +
                    3 * (1 - tprime) * (tprime * tprime) * p2 +
                    (tprime * tprime * tprime) * p3;
            var tan = -3 * p0 * (ft * ft) +
                      p1 * (3 * (ft * ft) - 6 * ft * tprime) +
                      p2 * (6 * ft * tprime - 3 * (tprime * tprime)) +
                      3 * p3 * (tprime * tprime);

            return new Waypoint(p, tan);
        }

        public float GetArcLengthAtIndex(int index)
        {
            if (index < 0 || index >= _controlPoints.Count) throw new System.IndexOutOfRangeException();
            if (_points == null) this.Clean_Imp();
            if (_points.Length < 2) return 0f;

            if (index == _points.Length - 1)
            {
                return this.GetArcLength();
            }
            else
            {
                float len = 0f;
                for (int i = 0; i < index; i++)
                {
                    len += _lengths[i];
                }
                return len;
            }
        }

        public float GetArcLengthAfter(int index)
        {
            if (index < 0 || index >= _controlPoints.Count) throw new System.IndexOutOfRangeException();
            if (_points == null) this.Clean_Imp();
            if (_points.Length < 2) return 0f;

            return _lengths[index];
        }

        public float GetArcLengthAfter(int index, float tprime)
        {
            return GetArcLengthAfter(index) * Mathf.Clamp01(tprime);
        }

        public RelativePositionData GetRelativePositionData(float t)
        {
            var cnt = _controlPoints.Count;
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
                        if (_points == null) this.Clean_Imp();

                        float len = _lengths[0];
                        float tot = len;
                        int i = 0;
                        while (tot / _totalArcLength < t && i < _lengths.Length)
                        {
                            i++;
                            len = _lengths[i];
                            tot += len;
                        }

                        float lt = (tot - len) / _totalArcLength;
                        float ht = tot / _totalArcLength;
                        float dt = com.spacepuppy.Utils.MathUtil.PercentageMinMax(t, ht, lt);

                        return new RelativePositionData(i, dt);
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
