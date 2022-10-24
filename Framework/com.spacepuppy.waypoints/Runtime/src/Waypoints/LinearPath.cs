using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Utils;

namespace com.spacepuppy.Waypoints
{
    public class LinearPath : IConfigurableIndexedWaypointPath
    {

        #region Fields

        private bool _isClosed;
        private List<IControlPoint> _controlPoints = new List<IControlPoint>();

        private Vector3[] _points; //null if dirty
        private float[] _lengths;
        private float _totalArcLength = float.NaN;

        #endregion

        #region CONSTRUCTOR

        public LinearPath()
        {

        }

        public LinearPath(IEnumerable<IControlPoint> waypoints)
        {
            _controlPoints.AddRange(waypoints);
            _points = null;
        }

        #endregion

        #region Methods

        public int GetDetailedPositions(ICollection<Vector3> coll)
        {
            if (coll == null) throw new System.ArgumentNullException("coll");
            if (_points == null) this.Clean_Imp();
            for (int i = 0; i < _points.Length; i++)
            {
                coll.Add(_points[i]);
            }
            return _points.Length;
        }

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
                _points = (_isClosed) ? new Vector3[_controlPoints.Count + 1] : new Vector3[_controlPoints.Count];
                for (int i = 0; i < _controlPoints.Count; i++) _points[i] = _controlPoints[i].Position;
                if (_isClosed) _points[_points.Length - 1] = _points[0];

                _lengths = new float[_points.Length];
                _totalArcLength = 0f;
                for (int i = 1; i < _points.Length; i++)
                {
                    float l = Vector3.Distance(_points[i], _points[i - 1]);
                    _lengths[i - 1] = l;
                    _totalArcLength += l;
                }
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
            if (_points.Length < 2) return (_points.Length == 0) ? VectorUtil.NaNVector3 : _points[0];

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
            if (_points.Length < 2) return (_points.Length == 0) ? Waypoint.Invalid : new Waypoint(_points[0], Vector3.zero);

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
            float dt = MathUtil.PercentageMinMax(t, ht, lt);
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
            if (_points.Length < 2) return (_points.Length == 0) ? VectorUtil.NaNVector3 : _points[0];

            if (index == _points.Length - 1)
            {
                if (_isClosed)
                {
                    return Vector3.Lerp(_points[index], _points[0], tprime);
                }
                else
                {
                    var pa = _points[index - 1];
                    var pb = _points[index];
                    var v = pb - pa;
                    return pa + v * tprime;
                }
            }
            else
            {
                return Vector3.Lerp(_points[index], _points[index + 1], tprime);
            }
        }

        public Waypoint GetWaypointAfter(int index, float tprime)
        {
            if (index < 0 || index >= _controlPoints.Count) throw new System.IndexOutOfRangeException();
            if (_points == null) this.Clean_Imp();
            if (_points.Length < 2) return (_points.Length == 0) ? Waypoint.Invalid : new Waypoint(_points[0], Vector3.zero);

            if (index == _points.Length - 1)
            {
                if (_isClosed)
                {
                    var pa = _points[index];
                    var pb = _points[0];
                    var v = pb - pa;
                    return new Waypoint(pa + v * tprime, v.normalized);
                }
                else
                {
                    var pa = _points[index - 1];
                    var pb = _points[index];
                    var v = pb - pa;
                    return new Waypoint(pa + v * tprime, v.normalized);
                }
            }
            else
            {
                var pa = _points[index];
                var pb = _points[index + 1];
                var v = pb - pa;
                return new Waypoint(pa + v * tprime, v.normalized);
            }
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

            if (index == _points.Length - 1)
            {
                if (_isClosed)
                {
                    return Vector3.Distance(_points[index], _points[0]);
                }
                else
                {
                    return float.PositiveInfinity;
                }
            }
            else
            {
                return Vector3.Distance(_points[index], _points[index + 1]);
            }
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
                        float dt = MathUtil.PercentageMinMax(t, ht, lt);

                        return new RelativePositionData(i % cnt, dt);
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
