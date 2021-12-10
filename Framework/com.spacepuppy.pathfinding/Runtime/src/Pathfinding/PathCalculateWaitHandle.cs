using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Utils;

namespace com.spacepuppy.Pathfinding
{
    public class PathCalculateWaitHandle : IRadicalWaitHandle, IUpdateable, System.IDisposable
    {

        #region Fields

        private System.WeakReference<IPath> _path;
        private System.Action<IRadicalWaitHandle> _callback;

        #endregion

        #region CONSTRUCTOR

        public PathCalculateWaitHandle(IPath path)
        {
            if (path == null) throw new System.ArgumentNullException(nameof(path));

            _path = new System.WeakReference<IPath>(path);
        }

        #endregion

        #region Methods

        private void SignalComplete()
        {
            GameLoop.UpdatePump.Remove(this);

            var d = _callback;
            _callback = null;
            d?.Invoke(this);
        }

        #endregion

        #region IRadicalWaitHandle Interface

        public bool Cancelled
        {
            get
            {
                IPath p = null;
                return !(_path?.TryGetTarget(out p) ?? false) || p.Status == PathCalculateStatus.Invalid;
            }
        }

        public bool IsComplete
        {
            get
            {
                IPath p = null;
                return !(_path?.TryGetTarget(out p) ?? false) || p.Status == PathCalculateStatus.Invalid || p.Status > PathCalculateStatus.Calculating;
            }
        }

        public void OnComplete(System.Action<IRadicalWaitHandle> callback)
        {
            if (callback == null || this.IsComplete) return;

            if (_callback != null)
            {
                _callback += callback;
            }
            else
            {
                _callback = callback;
                GameLoop.UpdatePump.Add(this);
            }
        }

        bool IRadicalYieldInstruction.Tick(out object yieldObject)
        {
            yieldObject = null;

            IPath p = null;
            if (!(_path?.TryGetTarget(out p) ?? false)) return false;

            switch (p.Status)
            {
                case PathCalculateStatus.NotStarted:
                case PathCalculateStatus.Calculating:
                    return true;
                case PathCalculateStatus.Invalid:
                case PathCalculateStatus.Success:
                case PathCalculateStatus.Partial:
                default:
                    this.SignalComplete();
                    return false;
            }
        }

        #endregion

        #region IUpdateable Interface

        void IUpdateable.Update()
        {
            IPath p = null;
            if (!(_path?.TryGetTarget(out p) ?? false) || p.Status == PathCalculateStatus.Invalid || p.Status > PathCalculateStatus.Calculating)
            {
                this.SignalComplete();
            }
        }

        #endregion

        #region IDisposable Interface

        public void Dispose()
        {
            GameLoop.UpdatePump.Remove(this);
            _path?.SetTarget(null);
            _path = null;
            _callback = null;
        }

        #endregion

    }
}
