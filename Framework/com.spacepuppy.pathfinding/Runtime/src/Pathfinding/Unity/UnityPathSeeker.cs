using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

namespace com.spacepuppy.Pathfinding.Unity
{
    public class UnityPathSeeker : SPComponent, IPathSeeker
    {

        #region Fields

        [SerializeField]
        private int _areaMask = -1;

        #endregion

        #region CONSTRUCTOR
        
        #endregion

        #region IPathSeeker Interface
        
        public IPathFactory PathFactory
        {
            get { return UnityPathFactory.Default; }
        }

        public bool ValidPath(IPath path)
        {
            return (path is UnityPath);
        }

        public void CalculatePath(IPath path)
        {
            if (path is UnityPath p)
            {
                p.CalculatePath(_areaMask);
            }
            else
            {
                throw new PathArgumentException();
            }
        }

        void IPathSeeker.CancelPaths()
        {
            //not supported
            Debug.LogWarning($"{nameof(UnityPathSeeker)} does not support canceling path calculations vis {nameof(IPathSeeker.CancelPaths)}.");
        }

        #endregion

    }
}
