using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Utils;

using Pathfinding;
using System;

namespace com.spacepuppy.Pathfinding
{

    public interface IAGAstarPath : IPath
    {

        void CalculatePath(Seeker seeker);

    }

    public class AGAstarABPath : ABPath, IAGAstarPath
    {

        #region Fields

        private bool _calculationStarted;

        #endregion

        #region CONSTRUCTOR

        public AGAstarABPath()
            : base()
        {
            this.Reset();
        }

        public AGAstarABPath(Vector3 start, Vector3 end)
            : base()
        {
            this.Reset();
            this.UpdateStartEnd(start, end);
        }

        public new static AGAstarABPath Construct(Vector3 start, Vector3 end, OnPathDelegate callback = null)
        {
            var p = PathPool.GetPath<AGAstarABPath>();

            p.Setup(start, end, callback);
            return p;
        }

        #endregion

        #region IAGAstarPath Interface

        IList<Vector3> IPath.Waypoints
        {
            get
            {
                return this.vectorPath;
            }
        }

        PathCalculateStatus IPath.Status
        {
            get
            {
                switch (this.CompleteState)
                {
                    case PathCompleteState.NotCalculated:
                        return _calculationStarted ? PathCalculateStatus.Calculating : PathCalculateStatus.NotStarted;
                    case PathCompleteState.Error:
                        return PathCalculateStatus.Invalid;
                    case PathCompleteState.Partial:
                        return PathCalculateStatus.Partial;
                    case PathCompleteState.Complete:
                        return this.IsDone() ? PathCalculateStatus.Success : PathCalculateStatus.Calculating;
                    default:
                        return PathCalculateStatus.Invalid;
                }
            }
        }

        public virtual void CalculatePath(Seeker seeker)
        {
            if (seeker == null) throw new System.ArgumentNullException(nameof(seeker));

            _calculationStarted = true;
            seeker.StartPath(this, AGAstarPath.OnPathCallback);
        }

        #endregion
        
    }

    public sealed class AGAstarPath : IAGAstarPath
    {
        #region Fields

        private Path _path;
        private bool _calculationStarted;

        #endregion

        #region CONSTRUCTOR

        private AGAstarPath()
        {
            //block constructor
        }

        public AGAstarPath(Path path)
        {
            if (path == null) throw new System.ArgumentNullException("path");
            _path = path;
        }

        #endregion

        #region Properties

        public Path InnerPath
        {
            get { return _path; }
        }

        #endregion

        #region IPath Interface

        public PathCalculateStatus Status
        {
            get
            {
                switch (_path.CompleteState)
                {
                    case PathCompleteState.NotCalculated:
                        return _calculationStarted ? PathCalculateStatus.Calculating : PathCalculateStatus.NotStarted;
                    case PathCompleteState.Error:
                        return PathCalculateStatus.Invalid;
                    case PathCompleteState.Partial:
                        return PathCalculateStatus.Partial;
                    case PathCompleteState.Complete:
                        return _path.IsDone() ? PathCalculateStatus.Success : PathCalculateStatus.Calculating;
                    default:
                        return PathCalculateStatus.Invalid;
                }
            }
        }

        public IList<Vector3> Waypoints
        {
            get
            {
                return _path.vectorPath;
            }
        }

        public void CalculatePath(Seeker seeker)
        {
            if (seeker == null) throw new System.ArgumentNullException(nameof(seeker));

            _calculationStarted = true;
            seeker.StartPath(_path, AGAstarPath.OnPathCallback);
        }

        #endregion

        #region Static Interface

        private static OnPathDelegate _onPathCallback;
        public static OnPathDelegate OnPathCallback
        {
            get
            {
                if (_onPathCallback == null) _onPathCallback = new OnPathDelegate((Path p) =>
                {
                    if (p.CompleteState == PathCompleteState.Complete &&
                        p is ABPath &&
                        (p.vectorPath.Count == 0 || !VectorUtil.FuzzyEquals(p.vectorPath[p.vectorPath.Count - 1], (p as ABPath).endPoint)))
                    {
                        p.vectorPath.Add((p as ABPath).endPoint);
                    }
                });
                return _onPathCallback;
            }
        }

        public static IPath Create(Vector3 start, Vector3 end)
        {
            return new AGAstarABPath(start, end);
        }

        public static AGAstarPath Create(Path path)
        {
            return new AGAstarPath(path);
        }

        public static AGAstarPath CreateRandom(Vector3 start, int length)
        {
            return new AGAstarPath(RandomPath.Construct(start, length, null));
        }

        public static AGAstarPath CreateRandom(Vector3 start, int length, System.Action<RandomPath> config)
        {
            var path = RandomPath.Construct(start, length, null);
            if (config != null) config(path);
            return new AGAstarPath(path);
        }

        public static AGAstarPath CreateFlee(Vector3 start, Vector3 avoid, int searchLength)
        {
            return new AGAstarPath(FleePath.Construct(start, avoid, searchLength));
        }

        #endregion

    }

}
