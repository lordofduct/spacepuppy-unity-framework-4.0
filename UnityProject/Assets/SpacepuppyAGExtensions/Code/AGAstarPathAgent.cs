using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Motor;
using com.spacepuppy.Motor.Pathfinding;
using com.spacepuppy.Utils;

using Pathfinding;
using System;

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
        private AGAstarABPath _reusableABPath; //a recyclable path object for based Vector3 paths

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
            return (_seeker as IPathSeeker)?.ValidPath(path) ?? AGAstarPath.GetInnerPath(path) != null;
        }

        public void CalculatePath(IPath path)
        {
            if (path == null) throw new System.ArgumentNullException("path");

            if (_seeker is IPathSeeker seeker)
            {
                seeker.CalculatePath(path);
            }
            else
            {
                var p = AGAstarPath.GetInnerPath(path);
                if (p == null) throw new PathArgumentException();

                _seeker.StartPath(p, AGAstarPath.OnPathCallback);
            }
        }

        public IPath PathTo(Vector3 target)
        {
            var seeker = (_seeker as IPathSeeker);
            var factory = seeker?.PathFactory ?? AGAstarPathFactory.Default;
            if(factory == AGAstarPathFactory.Default)
            {
                if (_reusableABPath == null) _reusableABPath = new AGAstarABPath();
                _reusableABPath.UpdateTarget(this.entityRoot.transform.position, target);

                this.CalculatePath(_reusableABPath);
                this.SetPath(_reusableABPath);
                return _reusableABPath;
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

            this.CalculatePath(path);
            this.SetPath(path);
        }

        #endregion

    }

}