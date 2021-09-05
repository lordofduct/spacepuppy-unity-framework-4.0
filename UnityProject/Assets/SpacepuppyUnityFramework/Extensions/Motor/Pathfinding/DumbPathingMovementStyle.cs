using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy.Pathfinding;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Motor.Pathfinding
{

    public class DumbPathingMovementStyle : DumbMovementStyle, IPathFollower
    {

        public enum PathingStatus
        {
            Invalid = 0,
            Pathing = 1,
            Complete = 2
        }

        #region Fields

        [SerializeField()]
        private float _speed = 1.0f;
        [SerializeField]
        [MinRange(0f)]
        private float _waypointTolerance = 0.1f;
        [SerializeField]
        [Tooltip("If motion is truly 3d set this true, otherwise motion is calculated on a plane with y-up.")]
        private bool _motion3D = false;
        [SerializeField]
        private bool _faceDirectionOfMotion;
        [SerializeField]
        [Range(0f, 1f)]
        private float _faceDirectionSlerpSpeed = 0.5f;
        [SerializeField]
        private SPTime _timeSupplier;

        [System.NonSerialized]
        private IPath _currentPath;
        [System.NonSerialized]
        private int _currentIndex;
        [System.NonSerialized]
        private bool _paused;


        #endregion

        #region CONSTRUCTOR

        #endregion

        #region Properties

        public float Speed
        {
            get { return _speed; }
            set { _speed = value; }
        }

        public float WaypointTolerance
        {
            get { return _waypointTolerance; }
            set { _waypointTolerance = System.Math.Max(0f, value); }
        }

        public bool Motion3D
        {
            get { return _motion3D; }
            set { _motion3D = value; }
        }

        public bool FaceDirectionOfMotion
        {
            get { return _faceDirectionOfMotion; }
            set { _faceDirectionOfMotion = value; }
        }

        public float FaceDirectionSlerpSpeed
        {
            get { return _faceDirectionSlerpSpeed; }
            set { _faceDirectionSlerpSpeed = Mathf.Clamp01(value); }
        }

        public SPTime TimeSupplier
        {
            get { return _timeSupplier; }
            set { _timeSupplier = value; }
        }

        public PathingStatus Status
        {
            get
            {
                if (_currentPath == null || _currentPath.Status == PathCalculateStatus.Invalid)
                    return PathingStatus.Invalid;
                else if (_currentPath.Status == PathCalculateStatus.Uncalculated || _currentIndex < _currentPath.Waypoints.Count)
                    return PathingStatus.Pathing;
                else
                    return PathingStatus.Complete;
            }
        }

        public int CurrentNodeIndex
        {
            get { return _currentIndex; }
            set
            {
                _currentIndex = Mathf.Clamp(value, -1, _currentPath != null ? _currentPath.Waypoints.Count : 0);
            }
        }

        public bool PathIsPaused
        {
            get { return _paused; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns zero vector if no waypoint.
        /// </summary>
        public Vector3 GetNextWaypoint()
        {
            int index = _currentIndex;
            return _currentPath != null ? _currentPath.GetNextTarget(this.Position, ref index) : Vector3.zero;
        }

        #endregion

        #region IPathFollower Interface

        public IPath CurrentPath { get { return _currentPath; } }

        public bool IsTraversing
        {
            get
            {
                return this.Status == PathingStatus.Pathing;
            }
        }

        public virtual void SetPath(IPath path)
        {
            _currentPath = path;
            _currentIndex = -1;
            _paused = _currentPath == null;
        }

        public virtual void ResetPath()
        {
            _currentPath = null;
            _currentIndex = -1;
            _paused = true;
        }

        public virtual void ResumePath()
        {
            if (_paused && _currentPath != null)
            {
                _paused = false;
                _currentIndex = -1;
            }
        }

        public virtual void StopPath()
        {
            _paused = true;
        }

        #endregion

        #region IMovementStyle Interface

        protected override void UpdateMovement()
        {
            if (_paused || _currentPath == null) return;
            Start:
            switch (this.Status)
            {
                case PathingStatus.Invalid:
                case PathingStatus.Complete:
                    break;
                case PathingStatus.Pathing:
                    if (_currentPath.Status != PathCalculateStatus.Uncalculated)
                    {
                        Vector3 pos = this.Position;
                        var target = _currentPath.GetNextTarget(pos, ref _currentIndex);
                        Vector3 dir = target - pos;
                        if (!_motion3D) dir.y = 0f;
                        float dist = dir.sqrMagnitude;
                        if (dist <= _waypointTolerance * _waypointTolerance)
                        {
                            _currentIndex++;
                            goto Start;
                        }
                        dir.Normalize();

                        Vector3 mv = dir * _speed * _timeSupplier.Delta;
                        if (mv.sqrMagnitude > dist)
                        {
                            _currentIndex++;
                        }

                        this.Move(mv);

                        if(_faceDirectionOfMotion && !VectorUtil.NearZeroVector(dir))
                        {
                            this.entityRoot.transform.rotation = Quaternion.Slerp(this.entityRoot.transform.rotation, Quaternion.LookRotation(dir), _faceDirectionSlerpSpeed);
                        }
                    }
                    break;
            }
        }

        #endregion

    }
}
