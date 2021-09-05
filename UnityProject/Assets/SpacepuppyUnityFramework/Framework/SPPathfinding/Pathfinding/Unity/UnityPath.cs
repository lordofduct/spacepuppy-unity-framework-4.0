using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

using com.spacepuppy.Utils;
using com.spacepuppy.Collections;

namespace com.spacepuppy.Pathfinding.Unity
{

    public abstract class UnityPath : IPath
    {

        public abstract NavMeshPath NavMeshPath { get; }

        #region IPath Interface

        public IList<Vector3> Waypoints
        {
            get
            {
                return NavMeshPath?.corners ?? ArrayUtil.Empty<Vector3>();
            }
        }

        public PathCalculateStatus Status
        {
            get
            {
                switch (NavMeshPath?.status ?? NavMeshPathStatus.PathInvalid)
                {
                    case NavMeshPathStatus.PathInvalid:
                        return PathCalculateStatus.Invalid;
                    case NavMeshPathStatus.PathPartial:
                        return PathCalculateStatus.Partial;
                    case NavMeshPathStatus.PathComplete:
                        return PathCalculateStatus.Success;
                    default:
                        return PathCalculateStatus.Invalid;
                }
            }
        }

        #endregion

        #region Methods

        public abstract void CalculatePath(int areaMask);

        public abstract void CalculatePath(NavMeshQueryFilter filter);

        #endregion

    }

    public class UnityFromToPath : UnityPath
    {

        #region Fields

        private NavMeshPath _path = new NavMeshPath();

        #endregion

        #region CONSTRUCTOR

        public UnityFromToPath(Vector3 start, Vector3 target)
        {
            this.Start = start;
            this.Target = target;
        }

        #endregion

        #region Properties

        public override NavMeshPath NavMeshPath { get { return _path; } }

        public Vector3 Start
        {
            get;
            set;
        }

        public Vector3 Target
        {
            get;
            set;
        }

        #endregion

        #region Methods

        public override void CalculatePath(int areaMask)
        {
            NavMesh.CalculatePath(this.Start, this.Target, areaMask, _path);
        }

        public override void CalculatePath(NavMeshQueryFilter filter)
        {
            NavMesh.CalculatePath(this.Start, this.Target, filter, _path);
        }

        #endregion

    }
}
