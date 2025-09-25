using UnityEngine;
using System.Collections.Generic;

using Pathfinding;
using com.spacepuppy.Utils;
using com.spacepuppy.Async;
using System.Threading.Tasks;

namespace com.spacepuppy.Pathfinding
{


    public static class AGAStarUtils
    {

        #region Async Scan

        public static bool TryScan(this AstarPath astar)
        {
            if (AsyncScanProgressHook.IsSafe(astar))
            {
                try
                {
                    astar.Scan();
                    return true;
                }
                catch { }
            }

            return false;
        }

        public static AsyncWaitHandle BeginScanAsync(this AstarPath astar) => AsyncScanProgressHook.BeginScanAsync(astar);

        private class AsyncScanProgressHook : IAsyncWaitHandleProvider, System.Collections.IEnumerator
        {

            #region Fields

            internal AstarPath astar;
            internal IEnumerator<Progress> e;
            internal Coroutine routine;
            internal bool _complete;
            private System.Action<AsyncWaitHandle> _callback;
            private TaskCompletionSource<bool> _completionTask;

            #endregion

            #region Methods/Properties

            public bool Complete => _complete || !object.ReferenceEquals(AstarPath.active, astar);

            internal bool Validate()
            {
                if (_complete) return false;

                if (object.ReferenceEquals(AstarPath.active, astar) && astar.isScanning)
                {
                    return true;
                }
                else
                {
                    this.SignalComplete();
                    return false;
                }
            }

            internal void SignalComplete()
            {
                if (_currentScan == this) _currentScan = null;

                _complete = true;
                var s = _completionTask;
                s = null;
                var d = _callback;
                _callback = null;

                s?.SetResult(true);
                if (d != null)
                {
                    try
                    {
                        d(new AsyncWaitHandle(this, this));
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
            }

            #endregion

            #region IAsyncWaitHandleProvider Interface

            public Task GetTask(AsyncWaitHandle handle)
            {
                lock (_lock)
                {
                    if (_complete)
                    {
                        return Task.CompletedTask;
                    }
                    else
                    {
                        return (_completionTask ??= new TaskCompletionSource<bool>()).Task;
                    }
                }
            }

            public float GetProgress(AsyncWaitHandle handle)
            {
                return e?.Current.progress ?? 0f;
            }

            public object GetResult(AsyncWaitHandle handle)
            {
                return null;
            }

            public object GetYieldInstruction(AsyncWaitHandle handle)
            {
                return routine;
            }

            public bool IsComplete(AsyncWaitHandle handle)
            {
                return this.Complete;
            }

            public void OnComplete(AsyncWaitHandle handle, System.Action<AsyncWaitHandle> callback)
            {
                if (callback == null) return;

                lock (_lock)
                {
                    _callback += callback;
                }
            }

            #endregion

            #region IEnumerator Interface

            object System.Collections.IEnumerator.Current => null;

            bool System.Collections.IEnumerator.MoveNext()
            {
                lock (_lock)
                {
                    if (!this.Validate()) return false;

                    try
                    {
                        if (e?.MoveNext() ?? false)
                        {
                            return true;
                        }
                        else
                        {
                            this.SignalComplete();
                            return false;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogException(ex);
                        this.SignalComplete();
                        return astar != null ? astar.isScanning : false;
                    }
                }
            }

            void System.Collections.IEnumerator.Reset()
            {
                //do nothing
            }

            #endregion

            #region Static Interface

            private static readonly object _lock = new object();
            private static AsyncScanProgressHook _currentScan;

            internal static bool IsSafe(AstarPath astar)
            {
                if (astar == null || astar != AstarPath.active || astar.isScanning) return false;

                lock (_lock)
                {
                    if (_currentScan != null)
                    {
                        if (_currentScan.Validate())
                        {
                            return false;
                        }
                        else
                        {
                            _currentScan = null;
                            return true;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
            }

            internal static AsyncWaitHandle BeginScanAsync(AstarPath astar)
            {
                //this logic assumes only 1 AstarPath exists at a time
                if (astar == null) throw new System.ArgumentNullException(nameof(astar));
                if (astar != AstarPath.active) throw new System.ArgumentException("Attempted to scan an inactive AstarPath object, AstarPath is a singleton and only 1 active instance should ever exist.", nameof(astar));

                lock (_lock)
                {
                    if (_currentScan != null)
                    {
                        if (_currentScan.Validate())
                        {
                            return new AsyncWaitHandle(_currentScan, _currentScan);
                        }
                        else
                        {
                            _currentScan = null;
                        }
                    }
                    else if (astar.isScanning)
                    {
                        if (GameLoop.InvokeRequired)
                        {
                            var c = new RadicalCoroutine(NaiveWaitForScanComplete(astar));
                            GameLoop.UpdateHandle.Invoke(() =>
                            {
                                c.Start(GameLoop.Hook);
                            });
                            return RadicalAsyncUtil.AsAsyncWaitHandle(c);
                        }
                        else
                        {
                            var c = GameLoop.Hook.StartRadicalCoroutine(NaiveWaitForScanComplete(astar));
                            return RadicalAsyncUtil.AsAsyncWaitHandle(c);
                        }
                    }

                    _currentScan = new AsyncScanProgressHook()
                    {
                        astar = astar,
                    };
                }

                if (GameLoop.InvokeRequired)
                {
                    GameLoop.UpdateHandle.Invoke(() =>
                    {
                        _currentScan.e = astar.ScanAsync().GetEnumerator();
                        _currentScan.e.MoveNext();
                        _currentScan.routine = GameLoop.Hook.StartCoroutine(_currentScan);
                    });
                }
                else
                {
                    _currentScan.e = astar.ScanAsync().GetEnumerator();
                    _currentScan.e.MoveNext();
                    _currentScan.routine = GameLoop.Hook.StartCoroutine(_currentScan);
                }
                return new AsyncWaitHandle(_currentScan, _currentScan);
            }

            private static System.Collections.IEnumerator NaiveWaitForScanComplete(AstarPath astar)
            {
                while (astar.isScanning)
                {
                    yield return null;
                }
            }

            #endregion

        }

        #endregion

        #region GetGUO

        public static GraphUpdateObject GetGUO(this GraphUpdateScene gus)
        {
            GraphUpdateObject guo = null;
            GetGUO(gus, ref guo);
            return guo;
        }

        public static void GetGUO(this GraphUpdateScene gus, ref GraphUpdateObject guo)
        {

            if (gus.points == null || gus.points.Length == 0)
            {
                var polygonCollider = gus.GetComponent<PolygonCollider2D>();
                if (polygonCollider != null)
                {
                    var points2D = polygonCollider.points;
                    Vector3[] pts = new Vector3[points2D.Length];
                    for (int i = 0; i < pts.Length; i++)
                    {
                        var p = points2D[i] + polygonCollider.offset;
                        pts[i] = new Vector3(p.x, 0, p.y);
                    }

                    var mat = gus.transform.localToWorldMatrix * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(-90, 0, 0), Vector3.one);
                    var shape = new GraphUpdateShape(gus.points, gus.convex, mat, gus.minBoundsHeight);
                    if (guo == null) guo = new GraphUpdateObject();
                    guo.bounds = shape.GetBounds();
                    guo.shape = shape;
                }
                else
                {
                    var bounds = gus.GetBounds();
                    if (bounds.center == Vector3.zero && bounds.size == Vector3.zero)
                    {
                        Debug.LogError("Cannot apply GraphUpdateScene, no points defined and no renderer or collider attached", gus);
                    }

                    if (guo == null) guo = new GraphUpdateObject(bounds);
                    else guo.bounds = bounds;
                    guo.shape = null;
                }
            }
            else
            {
                GraphUpdateShape shape;

                // Used for compatibility with older versions
                var worldPoints = new Vector3[gus.points.Length];
                for (int i = 0; i < gus.points.Length; i++) worldPoints[i] = gus.transform.TransformPoint(gus.points[i]);
                shape = new GraphUpdateShape(worldPoints, gus.convex, Matrix4x4.identity, gus.minBoundsHeight);

                if (guo == null) guo = new GraphUpdateObject();
                guo.bounds = shape.GetBounds();
                guo.shape = shape;
            }

            guo.nnConstraint = NNConstraint.None;
            guo.modifyWalkability = gus.modifyWalkability;
            guo.setWalkability = gus.setWalkability;
            guo.addPenalty = gus.penaltyDelta;
            guo.updatePhysics = gus.updatePhysics;
            guo.updateErosion = gus.updateErosion;
            guo.resetPenaltyOnPhysics = gus.resetPenaltyOnPhysics;

            guo.modifyTag = gus.modifyTag;
            guo.setTag = gus.setTag;
        }

        #endregion

        #region TryGetNearest

        public static bool TryGetNearest(this AstarPath path, Vector3 position, out NNInfo result)
        {
            try
            {
                result = path.GetNearest(position);
                return result.node != null;
            }
            catch
            {
                result = default;
                return false;
            }
        }

        public static bool TryGetNearest(this AstarPath path, Vector3 position, NNConstraint constraint, out NNInfo result)
        {
            try
            {
                result = path.GetNearest(position, constraint);
                return result.node != null;
            }
            catch
            {
                result = default;
                return false;
            }
        }

#if !AGASTAR_5_ORGREATER
        public static bool TryGetNearest(this AstarPath path, Vector3 position, NNConstraint constraint, GraphNode hint, out NNInfo result)
        {
            try
            {
                result = path.GetNearest(position, constraint, hint);
                return result.node != null;
            }
            catch
            {
                result = default;
                return false;
            }
        }
#endif

        public static bool TryGetNearest(this AstarPath path, Ray ray, out GraphNode result)
        {
            try
            {
                result = path.GetNearest(ray);
                return result != null;
            }
            catch
            {
                result = default;
                return false;
            }
        }

#endregion

    }

}