using UnityEngine;
using System.Collections.Generic;

namespace com.spacepuppy.Waypoints
{

    /// <summary>
    /// Acts as a long bezier spline.
    /// </summary>
    public class BezierSplinePath : IConfigurableIndexedWaypointPath
    {

        #region Fields

        private bool _isClosed;
        private List<IControlPoint> _controlPoints = new List<IControlPoint>();

        private Vector3[] _points;
        private CurveConstantSpeedTable _speedTable = new CurveConstantSpeedTable();

        #endregion

        #region CONSTRUCTOR

        public BezierSplinePath()
        {

        }

        public BezierSplinePath(IEnumerable<IControlPoint> waypoints)
        {
            _controlPoints.AddRange(waypoints);
            this.Clean_Imp();
        }

        #endregion

        #region Properties

        #endregion

        #region Methods

        private void Clean_Imp()
        {
            if (_controlPoints.Count == 0)
            {
                _speedTable.SetZero();
            }
            else if (_controlPoints.Count == 1)
            {
                _speedTable.SetZero();
            }
            else
            {
                var arr = this.GetPointArray();
                float estimatedLength = 0f;
                for (int i = 1; i < arr.Length; i++)
                {
                    estimatedLength += (arr[i] - arr[i - 1]).magnitude;
                }
                int detail = Mathf.RoundToInt(estimatedLength / 0.1f);

                _speedTable.Clean(detail, this.GetRealPositionAt);
            }
        }

        private Vector3[] GetPointArray()
        {
            int l = _controlPoints.Count;
            int cnt = l;
            if (_isClosed) cnt++;
            if (_points == null)
                _points = new Vector3[cnt];
            else if (_points.Length != cnt)
                System.Array.Resize(ref _points, cnt);

            for (int i = 0; i < l; i++)
            {
                _points[i] = _controlPoints[i].Position;
            }
            if (_isClosed)
            {
                _points[cnt - 1] = _controlPoints[0].Position;
            }

            return _points;
        }

        private Vector3 GetRealPositionAt(float t)
        {
            var arr = this.GetPointArray();
            var c = arr.Length;
            while (c > 1)
            {
                for (int i = 1; i < c; i++)
                {
                    arr[i - 1] = Vector3.Lerp(arr[i - 1], arr[i], t);
                }

                c--;
            }
            return arr[0];
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
                _speedTable.SetDirty();
            }
        }

        public float GetArcLength()
        {
            if (_speedTable.IsDirty) this.Clean_Imp();
            return _speedTable.TotalArcLength;
        }

        public float GetArcLength(float t)
        {
            if (_speedTable.IsDirty) this.Clean_Imp();

            return _speedTable.TotalArcLength * Mathf.Clamp01(t);
        }

        public Vector3 GetPositionAt(float t)
        {
            if (_controlPoints.Count == 0) return Vector3.zero;
            if (_controlPoints.Count == 1) return _controlPoints[0].Position;

            if (_speedTable.IsDirty) this.Clean_Imp();
            return this.GetRealPositionAt(_speedTable.GetConstPathPercFromTimePerc(t));
        }

        public Waypoint GetWaypointAt(float t)
        {
            if (_controlPoints.Count == 0) return Waypoint.Invalid;
            if (_controlPoints.Count == 1) return new Waypoint(_controlPoints[0]);

            t = _speedTable.GetConstPathPercFromTimePerc(t);
            var p1 = this.GetRealPositionAt(t);
            var p2 = this.GetRealPositionAt(t + 0.01f);
            return new Waypoint(p1, (p2 - p1));
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

            float range = 1f / (_controlPoints.Count - 1);
            return this.GetPositionAt(index * range + tprime * range);
        }

        public Waypoint GetWaypointAfter(int index, float tprime)
        {
            if (index < 0 || index >= _controlPoints.Count) throw new System.IndexOutOfRangeException();

            float range = 1f / (_controlPoints.Count - 1);
            return this.GetWaypointAt(index * range + tprime * range);
        }

        public float GetArcLengthAtIndex(int index)
        {
            if (index < 0 || index >= _controlPoints.Count) throw new System.IndexOutOfRangeException();
            if (_points == null) this.Clean_Imp();
            if (_points.Length < 2) return 0f;

            if (index == 0) return 0f;
            if (index == _points.Length - 1) return this.GetArcLength();
            return index * this.GetArcLength() / (_controlPoints.Count - 1);
        }

        public float GetArcLengthAfter(int index)
        {
            if (index < 0 || index >= _controlPoints.Count) throw new System.IndexOutOfRangeException();
            if (_points == null) this.Clean_Imp();
            if (_points.Length < 2) return 0f;

            if (index == _points.Length - 1)
            {
                return float.PositiveInfinity;
            }
            else
            {
                return this.GetArcLength() / (_controlPoints.Count - 1);
            }
        }

        public float GetArcLengthAfter(int index, float tprime)
        {
            return GetArcLengthAfter(index) * Mathf.Clamp01(tprime);
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
                        float range = 1f / (_controlPoints.Count - 1);
                        int i = Mathf.Clamp(Mathf.FloorToInt(t / range), 0, _controlPoints.Count - 1);
                        float dt = (t - i * range) / range;
                        return new RelativePositionData(i, dt);
                    }
            }
        }

        public void Clean()
        {
            _speedTable.SetDirty();
        }

        #endregion

        #region IConfigurableIndexedWaypointPath Interface

        public void AddControlPoint(IControlPoint controlpoint)
        {
            _controlPoints.Add(controlpoint);
            _speedTable.SetDirty();
        }

        public void InsertControlPoint(int index, IControlPoint controlpoint)
        {
            _controlPoints.Insert(index, controlpoint);
            _speedTable.SetDirty();
        }

        public void ReplaceControlPoint(int index, IControlPoint controlpoint)
        {
            _controlPoints[index] = controlpoint;
            _speedTable.SetDirty();
        }

        public void RemoveControlPointAt(int index)
        {
            _controlPoints.RemoveAt(index);
            _speedTable.SetDirty();
        }

        public void Clear()
        {
            _controlPoints.Clear();
            _speedTable.SetDirty();
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
