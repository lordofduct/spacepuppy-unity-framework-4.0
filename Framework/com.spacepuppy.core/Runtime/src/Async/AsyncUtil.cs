using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using com.spacepuppy.Collections;
#if SP_UNITASK
using Cysharp.Threading.Tasks;
using UnityEngine;
#endif
namespace com.spacepuppy.Async
{

    public static class AsyncUtil
    {

        #region Component Extensions

#if SP_UNITASK
        public static async Cysharp.Threading.Tasks.UniTask WaitForStarted(this IEventfulComponent c, System.Threading.CancellationToken cancellationToken = default)
        {
            if (c == null || c.component == null || c.started) return;

            while (!c.started && c.component && !cancellationToken.IsCancellationRequested)
            {
                await UniTask.Yield();
            }
        }
        public static async Cysharp.Threading.Tasks.UniTask WaitForStarted(this SPComponent c, System.Threading.CancellationToken cancellationToken = default)
        {
            if (c == null || c.started) return;

            while (!c.started && c && !cancellationToken.IsCancellationRequested)
            {
                await UniTask.Yield();
            }
        }
#else
        public static async System.Threading.Tasks.ValueTask WaitForStarted(this IEventfulComponent c, System.Threading.CancellationToken cancellationToken = default)
        {
            if (c == null || c.component == null || c.started) return;

            while (!c.started && c.component && !cancellationToken.IsCancellationRequested)
            {
                await System.Threading.Tasks.Task.Yield();
            }
        }

        public static async System.Threading.Tasks.ValueTask WaitForStarted(this SPComponent c, System.Threading.CancellationToken cancellationToken = default)
        {
            if (c == null || c.started) return;

            while (!c.started && c && !cancellationToken.IsCancellationRequested)
            {
                await System.Threading.Tasks.Task.Yield();
            }
        }
#endif

        #endregion


        public static AsyncWaitHandle AsAsyncWaitHandle(this Task task)
        {
            if (task == null) throw new System.ArgumentNullException(nameof(task));
            return new AsyncWaitHandle(TaskAsyncWaitHandleProvider<object>.Default, task);
        }

        public static AsyncWaitHandle<T> AsAsyncWaitHandle<T>(this Task<T> task)
        {
            if (task == null) throw new System.ArgumentNullException(nameof(task));
            return new AsyncWaitHandle<T>(TaskAsyncWaitHandleProvider<T>.Default, task);
        }

        public static AsyncWaitHandle AsAsyncWaitHandle(this UnityEngine.AsyncOperation op)
        {
            if (op == null) throw new System.ArgumentNullException(nameof(op));
            return new AsyncWaitHandle(AsyncOperationAsyncWaitHandleProvider.Default, op);
        }

        public static Task AsTask(this UnityEngine.AsyncOperation op)
        {
            if (op == null) throw new System.ArgumentNullException(nameof(op));
            return new AsyncWaitHandle(AsyncOperationAsyncWaitHandleProvider.Default, op).AsTask();
        }

        /// <summary>
        /// Returns a SemaphoreSlim that upon calling 'Dispose' will release all waiting threads and return it to a cache pool for reuse.
        /// This semaphore always starts with a CurrentCount of 0 and is intended for very simple semaphore/wait scenarios.
        /// </summary>
        /// <returns></returns>
        public static SemaphoreSlim GetTempSemaphore()
        {
            return _semaphores.GetInstance();
        }

        #region Special Types

        private static readonly ObjectCachePool<ReusableSemaphore> _semaphores = new ObjectCachePool<ReusableSemaphore>(-1);

        private class ReusableSemaphore : SemaphoreSlim
        {

            public ReusableSemaphore() : base(0)
            {

            }

            protected override void Dispose(bool disposing)
            {
                this.Release();
                if (!_semaphores.Release(this))
                {
                    base.Dispose();
                }
            }
        }

        private class TaskAsyncWaitHandleProvider<T> : IAsyncWaitHandleProvider<T>
        {

            public static readonly TaskAsyncWaitHandleProvider<T> Default = new TaskAsyncWaitHandleProvider<T>();

            public float GetProgress(AsyncWaitHandle handle)
            {
                if (handle.Token is Task t)
                {
                    return t.Status >= TaskStatus.RanToCompletion ? 1f : 0f;
                }
                else
                {
                    throw new System.InvalidOperationException("An instance of TaskAsyncWaitHandleProvider was associated with a token that was not a Task.");
                }
            }

            public bool IsComplete(AsyncWaitHandle handle)
            {
                if (handle.Token is Task t)
                {
                    return t.Status >= TaskStatus.RanToCompletion;
                }
                else
                {
                    throw new System.InvalidOperationException("An instance of TaskAsyncWaitHandleProvider was associated with a token that was not a Task.");
                }
            }

            public Task<T> GetTask(AsyncWaitHandle<T> handle)
            {
                return handle.Token as Task<T>;
            }

            Task IAsyncWaitHandleProvider.GetTask(AsyncWaitHandle handle)
            {
                return handle.Token as Task;
            }

            public object GetYieldInstruction(AsyncWaitHandle handle)
            {
                if (handle.Token is Task t)
                {
                    return WaitForTaskCoroutine(t);
                }
                else
                {
                    throw new System.InvalidOperationException("An instance of TaskAsyncWaitHandleProvider was associated with a token that was not a Task.");
                }
            }

            public void OnComplete(AsyncWaitHandle<T> handle, System.Action<AsyncWaitHandle<T>> callback)
            {
                if (callback == null) return;

                if (handle.Token is Task<T> t)
                {
                    this.WaitForTaskAndFireComplete(t, callback);
                }
                else
                {
                    throw new System.InvalidOperationException("An instance of TaskAsyncWaitHandleProvider was associated with a token that was not a Task.");
                }
            }

            public void OnComplete(AsyncWaitHandle handle, System.Action<AsyncWaitHandle> callback)
            {
                if (callback == null) return;

                if (handle.Token is Task t)
                {
                    this.WaitForTaskAndFireComplete(t, callback);
                }
                else
                {
                    throw new System.InvalidOperationException("An instance of TaskAsyncWaitHandleProvider was associated with a token that was not a Task.");
                }
            }

            public T GetResult(AsyncWaitHandle<T> handle)
            {
                if (handle.Token is Task<T> t)
                {
                    return t.IsCompleted ? t.Result : default(T);
                }
                else
                {
                    return default(T);
                }
            }

            object IAsyncWaitHandleProvider.GetResult(AsyncWaitHandle handle)
            {
                if (handle.Token is Task<T> t)
                {
                    return t.IsCompleted ? t.Result : default(T);
                }
                else
                {
                    return default(T);
                }
            }



            private async void WaitForTaskAndFireComplete(Task<T> t, System.Action<AsyncWaitHandle<T>> callback)
            {
                await t;
                callback(t.AsAsyncWaitHandle<T>());
            }

            private async void WaitForTaskAndFireComplete(Task t, System.Action<AsyncWaitHandle> callback)
            {
                await t;
                callback(t.AsAsyncWaitHandle());
            }

            private System.Collections.IEnumerator WaitForTaskCoroutine(Task t)
            {
                while (t.Status < TaskStatus.RanToCompletion)
                {
                    yield return null;
                }
            }

        }

        private class AsyncOperationAsyncWaitHandleProvider : IAsyncWaitHandleProvider
        {

            public static readonly AsyncOperationAsyncWaitHandleProvider Default = new AsyncOperationAsyncWaitHandleProvider();

            public float GetProgress(AsyncWaitHandle handle)
            {
                GameLoop.AssertMainThread();

                if (handle.Token is UnityEngine.AsyncOperation op)
                {
                    return op.isDone ? 1f : op.progress;
                }
                else
                {
                    throw new System.InvalidOperationException("An instance of AsynOperationAsyncWaitHandleProvider was associated with a token that was not an AsyncOperation.");
                }
            }

            public Task GetTask(AsyncWaitHandle handle)
            {
                if (handle.Token is UnityEngine.AsyncOperation op)
                {
                    if (GameLoop.InvokeRequired)
                    {
                        var s = AsyncUtil.GetTempSemaphore();
                        GameLoop.UpdateHandle.BeginInvoke(() =>
                        {
                            if (op.isDone)
                            {
                                s.Dispose();
                            }
                            else
                            {
                                op.completed += (o) =>
                                {
                                    s.Dispose();
                                };
                            }
                        });
                        return s.WaitAsync();
                    }
                    else if (op.isDone)
                    {
                        return Task.CompletedTask;
                    }
                    else
                    {
                        var s = AsyncUtil.GetTempSemaphore();
                        op.completed += (o) =>
                        {
                            s.Dispose();
                        };
                        return s.WaitAsync();
                    }
                }
                else
                {
                    throw new System.InvalidOperationException("An instance of AsynOperationAsyncWaitHandleProvider was associated with a token that was not an AsyncOperation.");
                }
            }

            public object GetYieldInstruction(AsyncWaitHandle handle)
            {
                GameLoop.AssertMainThread();

                if (handle.Token is UnityEngine.AsyncOperation op)
                {
                    return op;
                }
                else
                {
                    throw new System.InvalidOperationException("An instance of AsynOperationAsyncWaitHandleProvider was associated with a token that was not an AsyncOperation.");
                }
            }

            public void OnComplete(AsyncWaitHandle handle, System.Action<AsyncWaitHandle> callback)
            {
                if (handle.Token is UnityEngine.AsyncOperation op)
                {
                    if (callback != null)
                    {
                        if (GameLoop.InvokeRequired)
                        {
                            GameLoop.UpdateHandle.BeginInvoke(() =>
                            {
                                if (op.isDone)
                                {
                                    callback(new AsyncWaitHandle(this, op));
                                }
                                else
                                {
                                    op.completed += (o) => callback(new AsyncWaitHandle(this, o));
                                }
                            });
                        }
                        else
                        {
                            op.completed += (o) => callback(new AsyncWaitHandle(this, o));
                        }
                    }
                }
                else
                {
                    throw new System.InvalidOperationException("An instance of AsynOperationAsyncWaitHandleProvider was associated with a token that was not an AsyncOperation.");
                }
            }

            public bool IsComplete(AsyncWaitHandle handle)
            {
                if (handle.Token is UnityEngine.AsyncOperation op)
                {
                    if (GameLoop.InvokeRequired)
                    {
                        bool result = false;
                        GameLoop.UpdateHandle.Invoke(() => result = op.isDone);
                        return result;
                    }
                    else
                    {
                        return op.isDone;
                    }
                }
                else
                {
                    throw new System.InvalidOperationException("An instance of AsynOperationAsyncWaitHandleProvider was associated with a token that was not an AsyncOperation.");
                }
            }

            public object GetResult(AsyncWaitHandle handle)
            {
                if (handle.Token is UnityEngine.AsyncOperation op)
                {
                    return op;
                }
                else
                {
                    throw new System.InvalidOperationException("An instance of AsynOperationAsyncWaitHandleProvider was associated with a token that was not an AsyncOperation.");
                }
            }

        }

        #endregion

#if SP_UNITASK

        public static AsyncWaitHandle AsAsyncWaitHandle(this UniTask task)
        {
            var awaiter = task.GetAwaiter();
            if (awaiter.IsCompleted)
            {
                return new AsyncWaitHandle();
            }
            else
            {
                return UniTaskAsycWaitHandleProvider.Create(task).AsAsyncWaitHandle();
            }
        }

        public static AsyncWaitHandle<T> AsAsyncWaitHandle<T>(this UniTask<T> task)
        {
            var awaiter = task.GetAwaiter();
            if (awaiter.IsCompleted)
            {
                return new AsyncWaitHandle<T>(awaiter.GetResult());
            }
            else
            {
                return UniTaskAsycWaitHandleProvider<T>.Create(task).AsAsyncWaitHandle();
            }
        }

        private sealed class UniTaskAsycWaitHandleProvider : CustomYieldInstruction, IUniTaskAsyncWaitHandleProvider
        {

            public static UniTaskAsycWaitHandleProvider Create(UniTask task) => new UniTaskAsycWaitHandleProvider() { _task = task };

            #region Fields

            private UniTask _task;

            #endregion

            #region CONSTRUCTOR

            private UniTaskAsycWaitHandleProvider() { }

            #endregion

            #region CustomYieldInstruction Interface

            public override bool keepWaiting => !_task.Status.IsCompleted();

            #endregion

            #region AsyncWaitHandleProvider Interface

            public AsyncWaitHandle AsAsyncWaitHandle()
            {
                return new AsyncWaitHandle(this, this);
            }

            public UniTask GetUniTask(AsyncWaitHandle handle)
            {
                return _task;
            }

            public float GetProgress(AsyncWaitHandle handle)
            {
                return _task.Status.IsCompleted() ? 1f : 0f;
            }

            public bool IsComplete(AsyncWaitHandle handle)
            {
                return _task.Status.IsCompleted();
            }

            public object GetYieldInstruction(AsyncWaitHandle handle)
            {
                return this;
            }

            public Task GetTask(AsyncWaitHandle handle)
            {
                if (GameLoop.InvokeRequired)
                {
                    Task result = Task.CompletedTask;
                    UniTask task = _task;
                    GameLoop.UpdateHandle.Invoke(() => result = task.AsTask());
                    return result;
                }
                else
                {
                    return _task.AsTask();
                }
            }

            public void OnComplete(AsyncWaitHandle handle, System.Action<AsyncWaitHandle> callback)
            {
                if (callback == null) return;

                if (GameLoop.InvokeRequired)
                {
                    UniTask task = _task;
                    GameLoop.UpdateHandle.BeginInvoke(() => task.GetAwaiter().OnCompleted(() => callback(this.AsAsyncWaitHandle())));
                }
                else
                {
                    _task.GetAwaiter().OnCompleted(() => callback(this.AsAsyncWaitHandle()));
                }
            }

            public object GetResult(AsyncWaitHandle handle)
            {
                return null;
            }

            #endregion

        }

        private sealed class UniTaskAsycWaitHandleProvider<T> : CustomYieldInstruction, IUniTaskAsyncWaitHandleProvider<T>
        {

            public static UniTaskAsycWaitHandleProvider<T> Create(UniTask<T> task) => new UniTaskAsycWaitHandleProvider<T>() { _task = task };

            #region Fields

            private UniTask<T> _task;

            #endregion

            #region CONSTRUCTOR

            private UniTaskAsycWaitHandleProvider() { }

            #endregion

            #region CustomYieldInstruction Interface

            public override bool keepWaiting => !_task.Status.IsCompleted();

            #endregion

            #region AsyncWaitHandleProvider Interface

            public AsyncWaitHandle<T> AsAsyncWaitHandle()
            {
                return new AsyncWaitHandle<T>(this, this);
            }

            UniTask IUniTaskAsyncWaitHandleProvider.GetUniTask(AsyncWaitHandle handle)
            {
                return _task;
            }

            public UniTask<T> GetUniTask(AsyncWaitHandle<T> handle)
            {
                return _task;
            }

            public float GetProgress(AsyncWaitHandle handle)
            {
                return _task.Status.IsCompleted() ? 1f : 0f;
            }

            public bool IsComplete(AsyncWaitHandle handle)
            {
                return _task.Status.IsCompleted();
            }

            public object GetYieldInstruction(AsyncWaitHandle handle)
            {
                return this;
            }

            public Task GetTask(AsyncWaitHandle handle)
            {
                return this.GetTask(handle);
            }

            public Task<T> GetTask(AsyncWaitHandle<T> handle)
            {
                if (GameLoop.InvokeRequired)
                {
                    Task<T> result = null;
                    UniTask<T> task = _task;
                    GameLoop.UpdateHandle.Invoke(() => result = task.AsTask());
                    return result ?? Task.FromResult(this.GetResult(handle));
                }
                else
                {
                    return _task.AsTask();
                }
            }

            public void OnComplete(AsyncWaitHandle handle, System.Action<AsyncWaitHandle> callback)
            {
                if (callback == null) return;

                if (GameLoop.InvokeRequired)
                {
                    UniTask<T> task = _task;
                    GameLoop.UpdateHandle.BeginInvoke(() => task.GetAwaiter().OnCompleted(() => callback(this.AsAsyncWaitHandle())));
                }
                else
                {
                    _task.GetAwaiter().OnCompleted(() => callback(this.AsAsyncWaitHandle()));
                }
            }

            public void OnComplete(AsyncWaitHandle<T> handle, System.Action<AsyncWaitHandle<T>> callback)
            {
                if (callback == null) return;

                if (GameLoop.InvokeRequired)
                {
                    UniTask<T> task = _task;
                    GameLoop.UpdateHandle.BeginInvoke(() => task.GetAwaiter().OnCompleted(() => callback(this.AsAsyncWaitHandle())));
                }
                else
                {
                    _task.GetAwaiter().OnCompleted(() => callback(this.AsAsyncWaitHandle()));
                }
            }

            public object GetResult(AsyncWaitHandle handle)
            {
                return this.GetResult(this.AsAsyncWaitHandle());
            }

            public T GetResult(AsyncWaitHandle<T> handle)
            {
                if (GameLoop.InvokeRequired)
                {
                    T result = default(T);
                    UniTask<T> task = _task;
                    GameLoop.UpdateHandle.Invoke(() => result = task.GetAwaiter().GetResult());
                    return result;
                }
                else
                {
                    return _task.GetAwaiter().GetResult();
                }
            }

            #endregion

        }

#endif

    }


#if SP_UNITASK

    public static class SPUniTaskFactory
    {

        public static UniTask Delay(int millisecondsDelay, DeltaTimeType type, PlayerLoopTiming delayTiming = PlayerLoopTiming.Update, CancellationToken cancellationToken = default)
        {
            switch (type)
            {
                case DeltaTimeType.Normal:
                    return UniTask.Delay(millisecondsDelay, false, delayTiming, cancellationToken);
                case DeltaTimeType.Real:
                    return UniTask.Delay(millisecondsDelay, true, delayTiming, cancellationToken);
                case DeltaTimeType.Smooth:
                    return PerformDelay(SPTime.Smooth, (double)millisecondsDelay / 1000d, delayTiming, cancellationToken);
                case DeltaTimeType.Custom:
                default:
                    return UniTask.Delay(millisecondsDelay, false, delayTiming, cancellationToken);
            }
        }

        public static UniTask Delay(int millisecondsDelay, ITimeSupplier supplier, PlayerLoopTiming delayTiming = PlayerLoopTiming.Update, CancellationToken cancellationToken = default)
        {
            switch (supplier != null ? SPTime.GetDeltaType(supplier) : DeltaTimeType.Normal)
            {
                case DeltaTimeType.Normal:
                    return UniTask.Delay(millisecondsDelay, false, delayTiming, cancellationToken);
                case DeltaTimeType.Real:
                    return UniTask.Delay(millisecondsDelay, true, delayTiming, cancellationToken);
                case DeltaTimeType.Smooth:
                case DeltaTimeType.Custom:
                    return PerformDelay(supplier, (double)millisecondsDelay / 1000d, delayTiming, cancellationToken);
                default:
                    return UniTask.Delay(millisecondsDelay, false, delayTiming, cancellationToken);
            }
        }

        public static UniTask Delay(System.TimeSpan delay, DeltaTimeType type, PlayerLoopTiming delayTiming = PlayerLoopTiming.Update, CancellationToken cancellationToken = default)
        {
            switch (type)
            {
                case DeltaTimeType.Normal:
                    return UniTask.Delay(delay, false, delayTiming, cancellationToken);
                case DeltaTimeType.Real:
                    return UniTask.Delay(delay, true, delayTiming, cancellationToken);
                case DeltaTimeType.Smooth:
                    return PerformDelay(SPTime.Smooth, delay.TotalSeconds, delayTiming, cancellationToken);
                case DeltaTimeType.Custom:
                default:
                    return UniTask.Delay(delay, false, delayTiming, cancellationToken);
            }
        }

        public static UniTask Delay(System.TimeSpan delay, ITimeSupplier supplier, PlayerLoopTiming delayTiming = PlayerLoopTiming.Update, CancellationToken cancellationToken = default)
        {
            switch (supplier != null ? SPTime.GetDeltaType(supplier) : DeltaTimeType.Normal)
            {
                case DeltaTimeType.Normal:
                    return UniTask.Delay(delay, false, delayTiming, cancellationToken);
                case DeltaTimeType.Real:
                    return UniTask.Delay(delay, true, delayTiming, cancellationToken);
                case DeltaTimeType.Smooth:
                case DeltaTimeType.Custom:
                    return PerformDelay(supplier, delay.TotalSeconds, delayTiming, cancellationToken);
                default:
                    return UniTask.Delay(delay, false, delayTiming, cancellationToken);
            }
        }

        static async UniTask PerformDelay(ITimeSupplier supplier, double dur, PlayerLoopTiming delayTiming, CancellationToken cancellationToken)
        {
            double start = supplier.TotalPrecise;
            while (supplier.TotalPrecise - start < dur)
            {
                if (cancellationToken.IsCancellationRequested) return;
                await UniTask.Yield(delayTiming, cancellationToken);
            }
        }

    }

#endif


}
