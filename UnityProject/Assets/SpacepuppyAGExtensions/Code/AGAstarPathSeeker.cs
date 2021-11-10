using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Utils;

using Pathfinding;

namespace com.spacepuppy.Pathfinding
{
    
    public class AGAstarPathSeeker : Seeker, IComponent, IPathSeeker
    {

        #region Fields
        
        private GameObject _entityRoot;

        #endregion
        
        #region Properties

        public GameObject entityRoot
        {
            get
            {
                if (object.ReferenceEquals(_entityRoot, null))
                    _entityRoot = this.FindRoot();
                return _entityRoot;
            }
        }

        #endregion

        #region IPathSeeker Interface

        public IPathFactory PathFactory
        {
            get { return AGAstarPathFactory.Default; }
        }

        public IPath CreatePath(Vector3 target)
        {
            return AGAstarABPath.Construct(this.entityRoot.transform.position, target);
        }

        /// <summary>
        /// Only returns true for paths that can be used when calling CalculatePath with targets. 
        /// Paths that are suitable for CalculatePath(IPath path) vary more, and may return false for this method.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool ValidPath(IPath path)
        {
            return path is IAGAstarPath || path is Path;
        }

        public void CalculatePath(IPath path)
        {
            if (path == null) throw new System.ArgumentNullException("path");
            if (path is IAGAstarPath agpath)
            {
                agpath.CalculatePath(this);
            }
            else if (path is Path p)
            {
                this.StartPath(p, AGAstarPath.OnPathCallback);
            }
            else
            {
                throw new PathArgumentException();
            }
        }

        #endregion

        #region IComponent Interface

        Component IComponent.component
        {
            get
            {
                return this;
            }
        }

        #endregion

    }

}