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

        public static AsyncWaitHandle BeginScanAsync(this AstarPath astar)
        {
            var hook = new AsyncScanProgressHook();
            if(GameLoop.InvokeRequired)
            {
                GameLoop.UpdateHandle.Invoke(() =>
                {
                    hook.e = astar.ScanAsync().GetEnumerator();
                    hook.Routine = GameLoop.Hook.StartCoroutine(hook);
                });
            }
            else
            {
                hook.e = astar.ScanAsync().GetEnumerator();
                hook.Routine = GameLoop.Hook.StartCoroutine(hook);
            }
            return new AsyncWaitHandle(hook, hook);
        }

        private class AsyncScanProgressHook : IAsyncWaitHandleProvider, System.Collections.IEnumerator
        {
            private static readonly object _lock = new object();

            public IEnumerator<Progress> e;
            public Coroutine Routine;
            private bool _complete;
            private System.Action<AsyncWaitHandle> _callback;

            #region IAsyncWaitHandleProvider Interface

            public Task GetTask(AsyncWaitHandle handle)
            {
                lock(_lock)
                {
                    if(_complete)
                    {
                        return Task.CompletedTask;
                    }
                    else
                    {
                        var s = AsyncUtil.GetTempSemaphore();
                        _callback += (h) => s.Dispose();
                        return s.WaitAsync();
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
                return Routine;
            }

            public bool IsComplete(AsyncWaitHandle handle)
            {
                return _complete;
            }

            public void OnComplete(AsyncWaitHandle handle, System.Action<AsyncWaitHandle> callback)
            {
                if (callback == null) return;

                lock(_lock)
                {
                    _callback += callback;
                }
            }

            #endregion

            #region IEnumerator Interface

            object System.Collections.IEnumerator.Current => null;

            bool System.Collections.IEnumerator.MoveNext()
            {
                lock(_lock)
                {
                    if (e?.MoveNext() ?? false)
                    {
                        return true;
                    }
                    else
                    {
                        _complete = true;
                        var d = _callback;
                        _callback = null;
                        d?.Invoke(new AsyncWaitHandle(this, this));
                        return false;
                    }
                }
            }

            void System.Collections.IEnumerator.Reset()
            {
                //do nothing
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

    }

}