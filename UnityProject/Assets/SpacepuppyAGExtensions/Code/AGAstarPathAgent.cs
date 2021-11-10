using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Motor;
using com.spacepuppy.Motor.Pathfinding;
using com.spacepuppy.Utils;

using Pathfinding;

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

        [System.NonSerialized]
        private Path _claimedPath;

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
            else if(path is IAGAstarPath agpath)
            {
                agpath.CalculatePath(_seeker);
            }
            else if(path is Path p)
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
            if(factory == AGAstarPathFactory.Default)
            {
                var p = AGAstarABPath.Construct(this.entityRoot.transform.position, target);

                if(_claimedPath != null)
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