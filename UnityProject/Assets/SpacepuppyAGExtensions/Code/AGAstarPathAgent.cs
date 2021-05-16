using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Motor;
using com.spacepuppy.Motor.Pathfinding;
using com.spacepuppy.Utils;

using Pathfinding;

namespace com.spacepuppy.Pathfinding
{
    
    public class AGAstarPathAgent : PathingMovementStyle, IPathAgent
    {

        #region Fields

        [SerializeField]
        [DefaultFromSelf(Relativity = EntityRelativity.Entity)]
        private Seeker _seeker;

        [System.NonSerialized]
        private AGAstarABPath _path;

        #endregion

        #region IPathAgent Interface
        
        public IPathFactory PathFactory
        {
            get { return AGAstarPathFactory.Default; }
        }

        /// <summary>
        /// Only returns true for paths that can be used when calling CalculatePath with targets. 
        /// Paths that are suitable for CalculatePath(IPath path) vary more, and may return false for this method.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool ValidPath(IPath path)
        {
            return path is AGAstarABPath;
        }

        public void CalculatePath(IPath path)
        {
            if (path == null) throw new System.ArgumentNullException("path");

            var p = AGAstarPath.GetInnerPath(path);
            if (p == null) throw new PathArgumentException();
            _seeker.StartPath(p, AGAstarPath.OnPathCallback);
        }

        public void PathTo(Vector3 target)
        {
            if (_path == null) _path = new AGAstarABPath();
            _path.UpdateTarget(this.entityRoot.transform.position, target);
            _seeker.StartPath(_path, AGAstarPath.OnPathCallback);
            this.SetPath(_path);
        }

        public void PathTo(IPath path)
        {
            if (path == null) throw new System.ArgumentNullException("path");

            var p = AGAstarPath.GetInnerPath(path);
            if (p == null) throw new PathArgumentException();
            _seeker.StartPath(p, AGAstarPath.OnPathCallback);
            this.SetPath(_path);
        }

        #endregion

    }

}