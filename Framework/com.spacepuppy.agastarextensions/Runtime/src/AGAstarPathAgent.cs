using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Motor.Pathfinding;
using com.spacepuppy.Utils;

using Pathfinding;
using Pathfinding.RVO;

namespace com.spacepuppy.Pathfinding
{

    /// <summary>
    /// Behaves as a self contained pathing agent that both seeks and follows a path using Aron Granberg A* Project. 
    /// This does not need to be combined with any movement style's or anything. Just attach a Seeker and this, then call "PathTo". It'll take care of the rest.
    /// </summary>
    public class AGAstarPathAgent : DumbPathingMovementStyle, IPathAgent
    {

        #region Fields

        [SerializeField]
        [DefaultFromSelf(Relativity = EntityRelativity.Entity)]
        private Seeker _seeker;
        [SerializeField]
        private RVOController _rvoLocalAvoidanceController;

        [System.NonSerialized]
        private Path _claimedPath;

        #endregion

        #region Methods

        protected override void UpdateMovement()
        {
            if (!_rvoLocalAvoidanceController)
            {
                base.UpdateMovement();
                return;
            }

            if (this.Paused || this.CurrentPath == null)
            {
                _lastSpeed = this.Acceleration > 0f && this.Acceleration < float.PositiveInfinity ? Mathf.Clamp(_lastSpeed - this.Acceleration * this.TimeSupplier.Delta, 0f, this.Speed) : 0f;
                return;
            }

        Start:
            switch (this.Status)
            {
                case PathingStatus.Invalid:
                case PathingStatus.Complete:
                    _lastSpeed = this.Acceleration > 0f && this.Acceleration < float.PositiveInfinity ? Mathf.Clamp(_lastSpeed - this.Acceleration * this.TimeSupplier.Delta, 0f, this.Speed) : 0f;
                    break;
                case PathingStatus.Pathing:
                    if (this.CurrentPath.Status > PathCalculateStatus.Calculating)
                    {
                        Vector3 pos = this.Position;
                        var target = this.CurrentPath.GetNextTarget(pos, ref _currentIndex);
                        Vector3 dir = target - pos;
                        if (!this.Motion3D) dir.y = 0f;
                        float dist = dir.sqrMagnitude;
                        if (dist <= this.WaypointTolerance * this.WaypointTolerance)
                        {
                            _currentIndex++;
                            goto Start;
                        }
                        dir.Normalize();

                        float speed = this.Speed;
                        if (this.Acceleration > 0 && this.Acceleration < float.PositiveInfinity)
                        {
                            speed = Mathf.Clamp(_lastSpeed + (this.Acceleration * this.TimeSupplier.Delta), 0f, this.Speed);
                        }
                        _lastSpeed = speed;

                        _rvoLocalAvoidanceController.SetTarget(target, speed, this.Speed);
                        //Vector3 mv = dir * speed * this.TimeSupplier.Delta;
                        Vector3 mv = _rvoLocalAvoidanceController.CalculateMovementDelta(pos, this.TimeSupplier.Delta);
                        if (mv.sqrMagnitude > dist)
                        {
                            _currentIndex++;
                        }

                        switch (this.GravityResolution)
                        {
                            case GravitySolving.Conserve3D:
                                {
                                    var grav = Physics.gravity.normalized;
                                    mv += Vector3.Dot(this.Velocity, grav) * grav * this.TimeSupplier.Delta;
                                }
                                break;
                            case GravitySolving.Conserve2D:
                                {
                                    var grav = (Vector3)Physics2D.gravity.normalized;
                                    mv += Vector3.Dot(this.Velocity, grav) * grav * this.TimeSupplier.Delta;
                                }
                                break;
                        }
                        this.Move(mv);

                        if (this.FaceDirectionOfMotion && !VectorUtil.NearZeroVector(dir.SetY(0f)))
                        {
                            this.entityRoot.transform.rotation = Quaternion.Slerp(this.entityRoot.transform.rotation, Quaternion.LookRotation(dir.SetY(0f)), this.FaceDirectionSlerpSpeed);
                        }
                    }
                    break;
            }
        }

        #endregion

        #region IPathAgent Interface

        public IPathFactory PathFactory
        {
            get { return (_seeker as IPathSeeker)?.PathFactory ?? AGAstarPathFactory.Default; }
        }

        /// <summary>
        /// Only returns true for paths that can be used when calling CalculatePath with targets. 
        /// Paths that are suitable for CalculatePath(IPath path) vary more, and may return false for this method.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool ValidPath(IPath path)
        {
            return (_seeker as IPathSeeker)?.ValidPath(path) ?? (path is IAGAstarPath || path is Path);
        }

        public void CalculatePath(IPath path)
        {
            if (path == null) throw new System.ArgumentNullException("path");

            if (_seeker is IPathSeeker seeker)
            {
                seeker.CalculatePath(path);
            }
            else if (path is IAGAstarPath agpath)
            {
                agpath.CalculatePath(_seeker);
            }
            else if (path is Path p)
            {
                _seeker.StartPath(p, AGAstarPath.OnPathCallback);
            }
            else
            {
                throw new PathArgumentException();
            }
        }

        public IPath PathTo(Vector3 target)
        {
            var seeker = (_seeker as IPathSeeker);
            var factory = seeker?.PathFactory ?? AGAstarPathFactory.Default;
            if (factory == AGAstarPathFactory.Default)
            {
                var p = AGAstarABPath.Construct(this.entityRoot.transform.position, target);

                if (_claimedPath != null)
                {
                    _claimedPath.Release(this);
                    _claimedPath = null;
                }

                this.CalculatePath(p);
                this.SetPath(p);

                p.Claim(this);
                _claimedPath = p;
                return p;
            }
            else
            {
                var p = factory.Create(seeker ?? this, target);
                this.CalculatePath(p);
                this.SetPath(p);
                return p;
            }
        }

        public void PathTo(IPath path)
        {
            if (path == null) throw new System.ArgumentNullException(nameof(path));

            if (path.Status == PathCalculateStatus.NotStarted) this.CalculatePath(path);
            this.SetPath(path);
        }

        public override void ResetPath()
        {
            if (_claimedPath != null)
            {
                _claimedPath.Release(this);
                _claimedPath = null;
            }
            base.ResetPath();
        }

        #endregion

    }

}