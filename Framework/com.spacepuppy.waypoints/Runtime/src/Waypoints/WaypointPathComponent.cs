using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy.Dynamic;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Waypoints
{

    public class WaypointPathComponent : SPComponent
    {

        #region Fields

        [Tooltip("The algorithm used for determining the path through the waypoints. NOTE - BezierSpline doesn't pass through the points and shouldn't be used with large numbers of waypoints, especially if the points animated.")]
        [SerializeField()]
        private WaypointPathExtensions.PathType _pathType;
        [SerializeField()]
        [Tooltip("The curve makes a complete trip around to waypoint 0.")]
        private bool _closed;
        [SerializeField()]
        [Tooltip("When pathing on this path, use values relative to this transform instead of the global values.")]
        private Transform _transformRelativeTo;
        [SerializeField()]
        private TransformControlPoint[] _controlPoints = ArrayUtil.Empty<TransformControlPoint>();

        [Tooltip("If the control points move at runtime, and you'd like the WaypointPath to automatically update itself, flag this true.")]
        [SerializeField()]
        private bool _controlPointsAnimate;

        [System.NonSerialized()]
        private IConfigurableIndexedWaypointPath _path;
        [System.NonSerialized()]
        private bool _cleaning;

        #endregion

        #region CONSTRUCTOR

        protected override void Awake()
        {
            base.Awake();

            for (int i = 0; i < _controlPoints.Length; i++)
            {
                if (_controlPoints[i] != null) _controlPoints[i].Initialize(this);
            }
            _path = GetPath(this, false);

            if (_controlPointsAnimate)
            {
                _cleaning = true;
                this.StartPooledRadicalCoroutine(this.AutoCleanRoutine(), RadicalCoroutineDisableMode.Pauses);
            }
        }

        #endregion

        #region Properties

        public int Count
        {
            get { return _controlPoints.Length; }
        }

        public WaypointPathExtensions.PathType Type
        {
            get { return _pathType; }
            set
            {
                if (_pathType == value) return;
                _pathType = value;
                _path = null;
            }
        }

        public bool Closed
        {
            get { return _closed; }
            set
            {
                _closed = value;
                if (_path != null) _path.IsClosed = _closed;
            }
        }

        public bool WaypointsAnimate
        {
            get { return _controlPointsAnimate; }
            set
            {
                if (_controlPointsAnimate == value) return;
                _controlPointsAnimate = value;
                if (_controlPointsAnimate && !_cleaning)
                {
                    _cleaning = true;
                    this.StartPooledRadicalCoroutine(this.AutoCleanRoutine(), RadicalCoroutineDisableMode.Pauses);
                }
            }
        }

        public Transform TransformRelativeTo
        {
            get { return _transformRelativeTo; }
            set { _transformRelativeTo = value; }
        }

        public IConfigurableIndexedWaypointPath Path
        {
            get { return _path; }
        }

        #endregion

        #region Methods

        public IConfigurableIndexedWaypointPath GetPathClone()
        {
            return GetPath(this, true);
        }

        public void SetControlPoints(IEnumerable<Transform> controlpoints)
        {
            if (_controlPoints != null)
            {
                for (int i = 0; i < _controlPoints.Length; i++)
                {
                    if (_controlPoints[i] != null) _controlPoints[i].Initialize(this);
                }
            }

            _controlPoints = (from t in controlpoints select t.AddOrGetComponent<TransformControlPoint>()).ToArray();
            this.Clean();
        }

        public void Clean()
        {
            if (_path != null)
            {
                _path.IsClosed = _closed;
                _path.Clear();
                foreach (var wp in _controlPoints) _path.AddControlPoint(wp);
            }
        }

        public Transform GetNodeTransform(int index)
        {
            if (index < 0 || index >= _controlPoints.Length) return null;
            return _controlPoints[index].transform;
        }

        private System.Collections.IEnumerator AutoCleanRoutine()
        {
            _cleaning = true;
            yield return null;

            while (_controlPointsAnimate)
            {
                try
                {
                    _path.IsClosed = _closed;
                    if (_controlPoints.Length != _path.Count)
                    {
                        //refill path
                        _path.Clear();
                        for (int i = 0; i < _controlPoints.Length; i++)
                        {
                            _path.AddControlPoint(_controlPoints[i]);
                        }
                    }
                    else
                    {
                        bool needsCleaning = false;
                        for (int i = 0; i < _controlPoints.Length; i++)
                        {
                            if (!object.ReferenceEquals(_controlPoints[i], _path.ControlPoint(i)))
                            {
                                _path.ReplaceControlPoint(i, _controlPoints[i]);
                            }
                            else if (!needsCleaning && !Waypoint.Compare(_path.ControlPoint(i), _controlPoints[i]))
                            {
                                needsCleaning = true;
                            }
                        }

                        if (needsCleaning)
                        {
                            _path.Clean();
                        }
                    }
                }
                catch(System.Exception ex)
                {
                    _cleaning = false;
                    throw ex;
                }

                yield return null;
            }
        }


        /// <summary>
        /// This adds the required component to a Transform to make it a control point with this WaypointPathComponent.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public virtual TransformControlPoint InitializeTransformAsControlPoint(Transform t)
        {
            if (t == null) return null;

            t.RemoveComponents<TransformControlPoint>();
            var cp = t.AddComponent<TransformControlPoint>();
            cp.Initialize(this);
            return cp;
        }

        #endregion

        #region Static Interface

        public static IConfigurableIndexedWaypointPath GetPath(WaypointPathComponent c, bool cloneWaypoints)
        {
            return WaypointPathExtensions.CreatePath(c._pathType, c._controlPoints, c._closed, cloneWaypoints);
        }

        #endregion

        #region Special Types

#if UNITY_EDITOR

        public static class EditorHelper
        {

            public static void InsertAfter(WaypointPathComponent comp, TransformControlPoint waypointToInsert, TransformControlPoint waypointToFollow)
            {
                if (Application.isPlaying) return;

                var lst = new List<TransformControlPoint>(comp._controlPoints);
                int index = lst.IndexOf(waypointToFollow);
                if (index < 0)
                {
                    lst.Add(waypointToInsert);
                }
                else
                {
                    lst.Insert(index + 1, waypointToInsert);
                }
                waypointToInsert.Initialize(comp);
                comp._controlPoints = lst.ToArray();
            }

        }

#endif

        #endregion

    }

}
