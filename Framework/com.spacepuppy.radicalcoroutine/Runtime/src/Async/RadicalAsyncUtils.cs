using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using com.spacepuppy.Utils;

#if SP_UNITASK
using Cysharp.Threading.Tasks;
#endif

namespace com.spacepuppy.Async
{
    public static class RadicalAsyncUtil
    {

        public static AsyncWaitHandle AsAsyncWaitHandle(this IRadicalYieldInstruction inst)
        {
            return new AsyncWaitHandle(RadicalAsyncWaitHandleProvider.Default, inst);
        }

        public static AsyncWaitHandle<T> AsAsyncWaitHandle<T>(this RadicalWaitHandle<T> handle)
        {
            return new AsyncWaitHandle<T>(RadicalAsyncWaitHandleProvider<T>.Default, handle);
        }

        public static Task AsTask(this IRadicalYieldInstruction inst)
        {
            return new AsyncWaitHandle(RadicalAsyncWaitHandleProvider.Default, inst).AsTask();
        }

        public static Task<T> AsTask<T>(this RadicalWaitHandle<T> inst)
        {
            return new AsyncWaitHandle<T>(RadicalAsyncWaitHandleProvider<T>.Default, inst).AsTask();
        }

        #region Special Types

        /// <summary>
        /// Acts as a IAsyncWaitHandleProvider to map IRadicalYieldInstructions to the AsyncWaitHandle struct.
        /// </summary>
        private class RadicalAsyncWaitHandleProvider : IAsyncWaitHandleProvider
#if SP_UNITASK
            , IUniTaskAsyncWaitHandleProvider
#endif
        {

            public static readonly RadicalAsyncWaitHandleProvider Default = new RadicalAsyncWaitHandleProvider();

            public float GetProgress(AsyncWaitHandle handle)
            {
                if (handle.Token is IProgressingYieldInstruction p)
                {
                    return p.IsComplete ? 1f : p.Progress;
                }
                else if (handle.Token is IRadicalYieldInstruction inst)
                {
                    return inst.IsComplete ? 1f : 0f;
                }
                else
                {
                    //this should never be reached as long as it remains private and is used appropriately like that in RadicalWaitHandleExtensions.AsAsyncWaitHandle
                    throw new System.InvalidOperationException("An instance of RadicalAsyncWaitHandleProvider was associated with a token that was not an IRadicalYieldInstruction.");
                }
            }

            public System.Threading.Tasks.Task GetTask(AsyncWaitHandle handle)
            {
                bool complete = false;

                if (handle.Token is IRadicalWaitHandle h)
                {
                    if (GameLoop.InvokeRequired)
                    {
                        GameLoop.UpdateHandle.Invoke(() => complete = h.IsComplete);
                    }
                    else
                    {
                        complete = h.IsComplete;
                    }

                    if (complete)
                    {
                        return Task.CompletedTask;
                    }
                    else
                    {
                        var s = AsyncUtil.GetTempSemaphore();
                        h.OnComplete((r) =>
                        {
                            s.Dispose();
                        });
                        return s.WaitAsync();
                    }
                }
                else if (handle.Token is IRadicalYieldInstruction inst)
                {
                    if (GameLoop.InvokeRequired)
                    {
                        GameLoop.UpdateHandle.Invoke(() => complete = inst.IsComplete);
                    }
                    else
                    {
                        complete = inst.IsComplete;
                    }

                    if (complete)
                    {
                        return Task.CompletedTask;
                    }
                    else
                    {
                        var s = AsyncUtil.GetTempSemaphore();
                        if (GameLoop.InvokeRequired)
                        {
                            GameLoop.UpdateHandle.BeginInvoke(() => GameLoop.Hook.StartPooledRadicalCoroutine(WaitUntilHandleIsDone(inst, (a) => s.Dispose())));
                        }
                        else
                        {
                            GameLoop.Hook.StartPooledRadicalCoroutine(WaitUntilHandleIsDone(inst, (a) => s.Dispose()));
                        }
                        return s.WaitAsync();
                    }
                }
                else
                {
                    //this should never be reached as long as it remains private and is used appropriately like that in RadicalWaitHandleExtensions.AsAsyncWaitHandle
                    throw new System.InvalidOperationException("An instance of RadicalAsyncWaitHandleProvider was associated with a token that was not an IRadicalYieldInstruction.");
                }
            }

#if SP_UNITASK
            public UniTask GetUniTask(AsyncWaitHandle handle)
            {
                if (handle.Token is IRadicalYieldInstruction inst)
                {
                    bool complete = false;
                    if (GameLoop.InvokeRequired)
                    {
                        GameLoop.UpdateHandle.Invoke(() => complete = inst.IsComplete);
                    }
                    else
                    {
                        complete = inst.IsComplete;
                    }

                    if (complete)
                    {
                        return UniTask.CompletedTask;
                    }
                    else
                    {
                        return inst.AsUniTask();
                    }
                }
                else
                {
                    //this should never be reached as long as it remains private and is used appropriately like that in RadicalWaitHandleExtensions.AsAsyncWaitHandle
                    throw new System.InvalidOperationException("An instance of RadicalAsyncWaitHandleProvider was associated with a token that was not an IRadicalYieldInstruction.");
                }
            }
#endif

            public object GetYieldInstruction(AsyncWaitHandle handle)
            {
                if (handle.Token is System.Collections.IEnumerator e)
                {
                    return e;
                }
                else if (handle.Token is IRadicalYieldInstruction h)
                {
                    return WaitUntilHandleIsDone(h);
                }
                else
                {
                    //this should never be reached as long as it remains private and is used appropriately like that in RadicalWaitHandleExtensions.AsAsyncWaitHandle
                    throw new System.InvalidOperationException("An instance of RadicalAsyncWaitHandleProvider was associated with a token that was not an IRadicalYieldInstruction.");
                }
            }

            public bool IsComplete(AsyncWaitHandle handle)
            {
                if (handle.Token is IRadicalYieldInstruction h)
                {
                    return h.IsComplete;
                }
                else
                {
                    //this should never be reached as long as it remains private and is used appropriately like that in RadicalWaitHandleExtensions.AsAsyncWaitHandle
                    throw new System.InvalidOperationException("An instance of RadicalAsyncWaitHandleProvider was associated with a token that was not an IRadicalYieldInstruction.");
                }
            }

            public void OnComplete(AsyncWaitHandle handle, System.Action<AsyncWaitHandle> callback)
            {
                if (handle.Token is IRadicalWaitHandle h)
                {
                    if (GameLoop.InvokeRequired)
                    {
                        GameLoop.UpdateHandle.BeginInvoke(() =>
                        {
                            if (h.IsComplete)
                            {
                                callback(h.AsAsyncWaitHandle());
                            }
                            else
                            {
                                h.OnComplete((r) => callback(new AsyncWaitHandle(this, r)));
                            }
                        });
                    }
                    else
                    {
                        if (h.IsComplete)
                        {
                            callback(h.AsAsyncWaitHandle());
                        }
                        else
                        {
                            h.OnComplete((r) => callback(new AsyncWaitHandle(this, r)));
                        }
                    }
                }
                else if (handle.Token is IRadicalYieldInstruction inst)
                {
                    if (callback != null)
                    {
                        if (GameLoop.InvokeRequired)
                        {
                            GameLoop.UpdateHandle.BeginInvoke(() => GameLoop.Hook.StartPooledRadicalCoroutine(WaitUntilHandleIsDone(inst, callback)));
                        }
                        else
                        {
                            GameLoop.Hook.StartPooledRadicalCoroutine(WaitUntilHandleIsDone(inst, callback));
                        }
                    }
                }
                else
                {
                    //this should never be reached as long as it remains private and is used appropriately like that in RadicalWaitHandleExtensions.AsAsyncWaitHandle
                    throw new System.InvalidOperationException("An instance of RadicalAsyncWaitHandleProvider was associated with a token that was not an IRadicalYieldInstruction.");
                }
            }

            public object GetResult(AsyncWaitHandle handle)
            {
                return handle.Token as IRadicalYieldInstruction;
            }

            private System.Collections.IEnumerator WaitUntilHandleIsDone(IRadicalYieldInstruction h, System.Action<AsyncWaitHandle> callback = null)
            {
                while (!h.IsComplete)
                {
                    yield return null;
                }
                callback?.Invoke(h.AsAsyncWaitHandle());
            }

        }

        private class RadicalAsyncWaitHandleProvider<T> : IAsyncWaitHandleProvider<T>
#if SP_UNITASK
            , IUniTaskAsyncWaitHandleProvider<T>
#endif
        {

            public static readonly RadicalAsyncWaitHandleProvider<T> Default = new RadicalAsyncWaitHandleProvider<T>();

            public float GetProgress(AsyncWaitHandle handle)
            {
                return RadicalAsyncWaitHandleProvider.Default.GetProgress(handle);
            }

            public System.Threading.Tasks.Task<T> GetTask(AsyncWaitHandle<T> handle)
            {
                if (handle.Token is RadicalWaitHandle<T> h)
                {
                    bool complete = false;
                    if (GameLoop.InvokeRequired)
                    {
                        GameLoop.UpdateHandle.Invoke(() => complete = h.IsComplete);
                    }
                    else
                    {
                        complete = h.IsComplete;
                    }

                    if (complete)
                    {
                        return Task.FromResult(h.Result);
                    }
                    else
                    {
                        return WaitForComplete(h);
                    }
                }
                else
                {
                    //this should never be reached as long as it remains private and is used appropriately like that in RadicalWaitHandleExtensions.AsAsyncWaitHandle
                    throw new System.InvalidOperationException("An instance of RadicalAsyncWaitHandleProvider was associated with a token that was not an RadicalWaitHandle<T>.");
                }
            }

            System.Threading.Tasks.Task IAsyncWaitHandleProvider.GetTask(AsyncWaitHandle handle)
            {
                return RadicalAsyncWaitHandleProvider.Default.GetTask(handle);
            }

#if SP_UNITASK
            public UniTask GetUniTask(AsyncWaitHandle handle)
            {
                return RadicalAsyncWaitHandleProvider.Default.GetUniTask(handle);
            }

            public UniTask<T> GetUniTask(AsyncWaitHandle<T> handle)
            {
                if (handle.Token is RadicalWaitHandle<T> h)
                {
                    bool complete = false;
                    if (GameLoop.InvokeRequired)
                    {
                        GameLoop.UpdateHandle.Invoke(() => complete = h.IsComplete);
                    }
                    else
                    {
                        complete = h.IsComplete;
                    }

                    if (complete)
                    {
                        return UniTask.FromResult(h.Result);
                    }
                    else
                    {
                        return WaitForComplete_UT(h);
                    }
                }
                else
                {
                    //this should never be reached as long as it remains private and is used appropriately like that in RadicalWaitHandleExtensions.AsAsyncWaitHandle
                    throw new System.InvalidOperationException("An instance of RadicalAsyncWaitHandleProvider was associated with a token that was not an IRadicalYieldInstruction.");
                }
            }
            private async UniTask<T> WaitForComplete_UT(RadicalWaitHandle<T> handle)
            {
                await UniTask.WaitUntil(() => handle.IsComplete);
                return handle.Result;
            }
#endif

            public object GetYieldInstruction(AsyncWaitHandle handle)
            {
                return RadicalAsyncWaitHandleProvider.Default.GetYieldInstruction(handle);
            }

            public bool IsComplete(AsyncWaitHandle handle)
            {
                return RadicalAsyncWaitHandleProvider.Default.IsComplete(handle);
            }

            public void OnComplete(AsyncWaitHandle handle, System.Action<AsyncWaitHandle> callback)
            {
                RadicalAsyncWaitHandleProvider.Default.OnComplete(handle, callback);
            }

            public void OnComplete(AsyncWaitHandle<T> handle, System.Action<AsyncWaitHandle<T>> callback)
            {
                if (handle.Token is RadicalWaitHandle<T> h)
                {
                    if (GameLoop.InvokeRequired)
                    {
                        GameLoop.UpdateHandle.BeginInvoke(() =>
                        {
                            if (h.IsComplete)
                            {
                                callback(h.AsAsyncWaitHandle());
                            }
                            else
                            {
                                h.OnComplete((r) => callback(new AsyncWaitHandle<T>(this, r)));
                            }
                        });
                    }
                    else if (h.IsComplete)
                    {
                        callback(h.AsAsyncWaitHandle());
                    }
                    else
                    {
                        h.OnComplete((r) => callback(new AsyncWaitHandle<T>(this, r)));
                    }
                }
                else
                {
                    //this should never be reached as long as it remains private and is used appropriately like that in RadicalWaitHandleExtensions.AsAsyncWaitHandle
                    throw new System.InvalidOperationException("An instance of RadicalAsyncWaitHandleProvider was associated with a token that was not an RadicalWaitHandle<T>.");
                }
            }

            public T GetResult(AsyncWaitHandle<T> handle)
            {
                if (handle.Token is RadicalWaitHandle<T> h)
                {
                    return h.Result;
                }
                else
                {
                    throw new System.InvalidOperationException("An instance of RadicalAsyncWaitHandleProvider was associated with a token that was not an RadicalWaitHandle<T>.");
                }
            }

            object IAsyncWaitHandleProvider.GetResult(AsyncWaitHandle handle)
            {
                if (handle.Token is RadicalWaitHandle<T> h)
                {
                    return h.Result;
                }
                else
                {
                    throw new System.InvalidOperationException("An instance of RadicalAsyncWaitHandleProvider was associated with a token that was not an RadicalWaitHandle<T>.");
                }
            }


            private async Task<T> WaitForComplete(RadicalWaitHandle<T> handle)
            {
                var s = AsyncUtil.GetTempSemaphore();
                if (GameLoop.InvokeRequired)
                {
                    GameLoop.UpdateHandle.BeginInvoke(() =>
                    {
                        if (handle.IsComplete)
                        {
                            s.Dispose();
                        }
                        else
                        {
                            handle.OnComplete((r) =>
                            {
                                s.Dispose();
                            });
                        }
                    });
                }
                else
                {
                    handle.OnComplete((r) =>
                    {
                        s.Dispose();
                    });
                }
                await s.WaitAsync();
                return handle.Result;
            }

        }

        #endregion

#if SP_UNITASK

        public static UniTask AsUniTask(this IRadicalYieldInstruction instruction)
        {
            if (instruction == null) throw new System.ArgumentNullException(nameof(instruction));
            if (instruction.IsComplete) return UniTask.CompletedTask;

            return new UniTask(RadicalPromise.Create(instruction, PlayerLoopTiming.Update, System.Threading.CancellationToken.None, out var token), token);
        }

        public static UniTask.Awaiter GetAwaiter(this IRadicalYieldInstruction instruction)
        {
            if (instruction == null) throw new System.ArgumentNullException(nameof(instruction));
            if (instruction.IsComplete) return UniTask.CompletedTask.GetAwaiter();

            return new UniTask(RadicalPromise.Create(instruction, PlayerLoopTiming.Update, System.Threading.CancellationToken.None, out var token), token).GetAwaiter();
        }

        /// <summary>
        /// Operates an IEnumerator coroutine as a RadicalCoroutine on the UniTask engine, granting access to all IRadicalYieldInstructions in the coroutine.  
        /// In a UniTask you can await SomeRoutine().AsRadicalUniTask() to do this. 
        /// </summary>
        /// <param name="routine"></param>
        /// <returns></returns>
        public static UniTask AsRadicalUniTask(this System.Collections.IEnumerable routine)
        {
            if (routine == null) throw new System.ArgumentNullException(nameof(routine));
            return new UniTask(RadicalPromise.Create(routine.GetEnumerator(), PlayerLoopTiming.Update, System.Threading.CancellationToken.None, out var token), token);
        }

        /// <summary>
        /// Operates an IEnumerator coroutine as a RadicalCoroutine on the UniTask engine, granting access to all IRadicalYieldInstructions in the coroutine.  
        /// In a UniTask you can await SomeRoutine().AsRadicalUniTask() to do this. 
        /// </summary>
        /// <param name="routine"></param>
        /// <returns></returns>
        public static UniTask AsRadicalUniTask(this System.Collections.IEnumerator routine)
        {
            if (routine == null) throw new System.ArgumentNullException(nameof(routine));
            return new UniTask(RadicalPromise.Create(routine, PlayerLoopTiming.Update, System.Threading.CancellationToken.None, out var token), token);
        }

        private class RadicalPromise : IUniTaskSource, IPlayerLoopItem, System.IDisposable
        {

            #region Fields

            private RadicalCoroutine _routine;
            private System.Threading.CancellationToken _cancellationToken;
            private int _initialFrame;
            private bool _running;
            private bool _calledGetResult;

            private UniTaskCompletionSourceCore<object> core;

            #endregion

            #region IUniTaskSource Interface

            public void GetResult(short token)
            {
                try
                {
                    _calledGetResult = true;
                    core.GetResult(token);
                }
                finally
                {
                    if (!_running)
                    {
                        this.Dispose();
                    }
                }
            }

            public UniTaskStatus GetStatus(short token)
            {
                return core.GetStatus(token);
            }

            public UniTaskStatus UnsafeGetStatus()
            {
                return core.UnsafeGetStatus();
            }

            public void OnCompleted(System.Action<object> continuation, object state, short token)
            {
                core.OnCompleted(continuation, state, token);
            }

            #endregion

            #region IPlayerLoopItem Interface

            public bool MoveNext()
            {
                if (_calledGetResult)
                {
                    _running = false;
                    this.Dispose();
                    return false;
                }

                if (_routine == null) // invalid status, returned but loop running?
                {
                    return false;
                }

                if (_cancellationToken.IsCancellationRequested)
                {
                    _running = false;
                    core.TrySetCanceled(_cancellationToken);
                    return false;
                }

                if (_initialFrame == -1)
                {
                    // Time can not touch in threadpool.
                    if (PlayerLoopHelper.IsMainThread)
                    {
                        _initialFrame = Time.frameCount;
                    }
                }
                else if (_initialFrame == Time.frameCount)
                {
                    return true; // already executed in first frame, skip.
                }

                try
                {
                    if (_routine.ManualTick(GameLoop.Hook))
                    {
                        return true;
                    }
                }
                catch (System.Exception ex)
                {
                    _running = false;
                    core.TrySetException(ex);
                    return false;
                }

                _running = false;
                core.TrySetResult(null);
                return false;
            }

            #endregion

            #region IDisposable Interface

            public void Dispose()
            {
                core.Reset();
                _routine?.Dispose(true);
                _routine = null;
                _cancellationToken = default;
                _running = false;
                _calledGetResult = false;
                _pool.Release(this);
            }

            #endregion

            #region Factory

            private static com.spacepuppy.Collections.ObjectCachePool<RadicalPromise> _pool = new com.spacepuppy.Collections.ObjectCachePool<RadicalPromise>(-1);

            public static IUniTaskSource Create(IRadicalYieldInstruction instruction, PlayerLoopTiming timing, System.Threading.CancellationToken cancellationToken, out short token)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return AutoResetUniTaskCompletionSource.CreateFromCanceled(cancellationToken, out token);
                }

                var result = _pool.GetInstance();
                TaskTracker.TrackActiveTask(result, 3);

                result._routine = RadicalCoroutine.Create(instruction);
                result._cancellationToken = cancellationToken;
                result._initialFrame = -1;
                result._running = true;
                result._calledGetResult = false;

                PlayerLoopHelper.AddAction(timing, result);

                token = result.core.Version;
                return result;
            }

            public static IUniTaskSource Create(System.Collections.IEnumerator routine, PlayerLoopTiming timing, System.Threading.CancellationToken cancellationToken, out short token)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return AutoResetUniTaskCompletionSource.CreateFromCanceled(cancellationToken, out token);
                }

                var result = _pool.GetInstance();
                TaskTracker.TrackActiveTask(result, 3);

                result._routine = RadicalCoroutine.Create(routine);
                result._cancellationToken = cancellationToken;
                result._initialFrame = -1;
                result._running = true;
                result._calledGetResult = false;

                PlayerLoopHelper.AddAction(timing, result);

                token = result.core.Version;
                return result;
            }

            #endregion

        }

#endif

    }
}
