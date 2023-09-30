using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy.Utils;

namespace com.spacepuppy.Pathfinding.Unity
{

    [RequireComponentInEntity(typeof(NavMeshAgent))]
    public class UnityStandardPathAgent : SPComponent, IPathAgent
    {

        #region Fields

        [SerializeField]
        [DefaultFromSelf(Relativity = EntityRelativity.Entity)]
        private NavMeshAgent _agent;

        [SerializeField]
        [Tooltip("Path is consider complete if the remaining distance is <= this OR the NavMeshAgent.stoppingDistance.")]
        private float _nearGoalThreshold = 0f;

        [System.NonSerialized]
        private CurrentPathRef _pathRef;

        #endregion

        #region CONSTRUCTOR

        protected override void Awake()
        {
            base.Awake();

            if (_agent == null)
            {
                if(!this.GetComponent<NavMeshAgent>(out _agent))
                {
                    Debug.LogWarning("No NavMeshAgent attached to this UnityPathAgent.");
                    this.enabled = false;
                }
            }

        }

        #endregion

        #region IPathAgent Interface

        public IPath CurrentPath { get { return _pathRef ?? (_pathRef = new CurrentPathRef(this)); } }

        public bool IsTraversing
        {
            get
            {
                return _agent.hasPath && !_agent.isStopped && _agent.remainingDistance > Mathf.Max(_nearGoalThreshold, _agent.stoppingDistance);
            }
        }

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
            if (object.ReferenceEquals(_agent, null)) throw new System.InvalidOperationException("UnityPathAgent was not configured correctly.");

            if(path is UnityPath p)
            {
                p.CalculatePath(_agent.areaMask);
            }
            else
            {
                throw new PathArgumentException();
            }
        }

        void IPathSeeker.CancelPaths()
        {
            //not supported
            Debug.LogWarning($"{nameof(UnityStandardPathAgent)} does not support canceling path calculations vis {nameof(IPathSeeker.CancelPaths)}.");
        }

        public void SetPath(IPath path)
        {
            if (object.ReferenceEquals(_agent, null)) throw new System.InvalidOperationException("UnityPathAgent was not configured correctly.");

            if(path is UnityPath p)
            {
                _agent.SetPath(p.NavMeshPath);
            }
            else
            {
                throw new PathArgumentException();
            }

        }

        public IPath PathTo(Vector3 target)
        {
            if (object.ReferenceEquals(_agent, null)) throw new System.InvalidOperationException("UnityPathAgent was not configured correctly.");
            _agent.SetDestination(target);
            return this.CurrentPath;
        }

        public void PathTo(IPath path)
        {
            if (object.ReferenceEquals(_agent, null)) throw new System.InvalidOperationException("UnityPathAgent was not configured correctly.");

            if (path is UnityPath p)
            {
                if (p.Status == PathCalculateStatus.NotStarted) p.CalculatePath(_agent.areaMask);
                _agent.SetPath(p.NavMeshPath);
            }
            else
            {
                throw new PathArgumentException();
            }
        }

        public void ResetPath()
        {
            if (object.ReferenceEquals(_agent, null)) throw new System.InvalidOperationException("UnityPathAgent was not configured correctly.");
            _agent.ResetPath();
        }

        public void StopPath()
        {
            if (object.ReferenceEquals(_agent, null)) throw new System.InvalidOperationException("UnityPathAgent was not configured correctly.");
            _agent.isStopped = true;
        }

        public void ResumePath()
        {
            if (object.ReferenceEquals(_agent, null)) throw new System.InvalidOperationException("UnityPathAgent was not configured correctly.");
            _agent.isStopped = false;
        }

        #endregion

        #region Special Types

        private class CurrentPathRef : UnityPath
        {

            private UnityStandardPathAgent _owner;

            public CurrentPathRef(UnityStandardPathAgent owner)
            {
                _owner = owner;
            }

            public override NavMeshPath NavMeshPath { get { return _owner?._agent?.path; } }

            public override void CalculatePath(int areaMask)
            {
                //do nothing
            }

            public override void CalculatePath(NavMeshQueryFilter filter)
            {
                //do nothing
            }

        }

        #endregion

    }
}
