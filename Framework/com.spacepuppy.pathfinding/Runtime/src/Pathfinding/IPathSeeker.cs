using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using com.spacepuppy.Project;

namespace com.spacepuppy.Pathfinding
{
    public interface IPathSeeker
    {
        
        IPathFactory PathFactory { get; }
        
        /// <summary>
        /// Returns true if the path can be used when calculating a path with this IPathSeeker.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        bool ValidPath(IPath path);

        void CalculatePath(IPath path);
        
    }

    public interface IPathFollower
    {

        IPath CurrentPath { get; }

        void SetPath(IPath path);

        /// <summary>
        /// Resets the automatic path, clearing it, and stopping all motion.
        /// </summary>
        void ResetPath();

        /// <summary>
        /// Stop pathing to target.
        /// </summary>
        void StopPath();

        /// <summary>
        /// Resume pathing to target
        /// </summary>
        void ResumePath();

    }

    /// <summary>
    /// Contract that combines both seeker and follower
    /// </summary>
    public interface IPathAgent : IPathSeeker, IPathFollower
    {

        /// <summary>
        /// Returns true if agent is currently pathing.
        /// </summary>
        bool IsTraversing { get; }

        /// <summary>
        /// Start automatically pathing to target.
        /// </summary>
        /// <param name="target"></param>
        IPath PathTo(Vector3 target);

        /// <summary>
        /// If the path is not yet calculated (status returns NotStarted) it will be calculated by self. Then sets the current path to this path.
        /// </summary>
        /// <param name="path"></param>
        void PathTo(IPath path);
        

    }

    [System.Serializable]
    public class PathSeekerRef : SerializableInterfaceRef<IPathSeeker> { }

    [System.Serializable]
    public class PathFollowerRef : SerializableInterfaceRef<IPathFollower> { }

    [System.Serializable]
    public class PathAgentRef : SerializableInterfaceRef<IPathAgent> { }

}
