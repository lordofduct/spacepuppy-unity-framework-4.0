using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using com.spacepuppy.Collections;

namespace com.spacepuppy.Async
{
    public static class AsyncUtil
    {

        public static AsyncWaitHandle AsAsyncWaitHandle(this Task task)
        {
            return new AsyncWaitHandle(TaskAsyncWaitHandleProvider<object>.Default, task);
        }

        public static AsyncWaitHandle<T> AsAsyncWaitHandle<T>(this Task<T> task)
        {
            return new AsyncWaitHandle<T>(TaskAsyncWaitHandleProvider<T>.Default, task);
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
                this.Release(int.MaxValue);
                if(!_semaphores.Release(this))
                {
                    base.Dispose();
                }
            }
        }

        private class TaskAsyncWaitHandleProvider<T> : IAsyncWaitHandleProvider<T>
        {

            public static readonly TaskAsyncWaitHandleProvider<T> Default = new TaskAsyncWaitHandleProvider<T>();

            public bool IsComplete(object token)
            {
                if(token is Task t)
                {
                    return t.Status >= TaskStatus.RanToCompletion;
                }
                else
                {
                    throw new System.InvalidOperationException("An instance of TaskAsyncWaitHandleProvider was associated with a token that was not a Task.");
                }
            }

            public Task<T> GetAwaitable(object token)
            {
                return token as Task<T>;
            }

            Task IAsyncWaitHandleProvider.GetAwaitable(object token)
            {
                return token as Task;
            }

            public object GetYieldInstruction(object token)
            {
                if (token is Task t)
                {
                    return WaitForTaskCoroutine(t);
                }
                else
                {
                    throw new System.InvalidOperationException("An instance of TaskAsyncWaitHandleProvider was associated with a token that was not a Task.");
                }
            }

            public void OnComplete(object token, Action<AsyncWaitHandle<T>> callback)
            {
                if (callback == null) return;

                if(token is Task<T> t)
                {
                    this.WaitForTaskAndFireComplete(t, callback);
                }
                else
                {
                    throw new System.InvalidOperationException("An instance of TaskAsyncWaitHandleProvider was associated with a token that was not a Task.");
                }
            }

            public void OnComplete(object token, Action<AsyncWaitHandle> callback)
            {
                if (callback == null) return;

                if (token is Task t)
                {
                    this.WaitForTaskAndFireComplete(t, callback);
                }
                else
                {
                    throw new System.InvalidOperationException("An instance of TaskAsyncWaitHandleProvider was associated with a token that was not a Task.");
                }
            }

            public T GetResult(object token)
            {
                if (token is Task<T> t)
                {
                    return t.IsCompleted ? t.Result : default(T);
                }
                else
                {
                    return default(T);
                }
            }

            object IAsyncWaitHandleProvider.GetResult(object token)
            {
                if (token is Task<T> t)
                {
                    return t.IsCompleted ? t.Result : default(T);
                }
                else
                {
                    return default(T);
                }
            }



            private async void WaitForTaskAndFireComplete(Task<T> t, Action<AsyncWaitHandle<T>> callback)
            {
                await t;
                callback(t.AsAsyncWaitHandle<T>());
            }

            private async void WaitForTaskAndFireComplete(Task t, Action<AsyncWaitHandle> callback)
            {
                await t;
                callback(t.AsAsyncWaitHandle());
            }

            private System.Collections.IEnumerator WaitForTaskCoroutine(Task t)
            {
                while(t.Status < TaskStatus.RanToCompletion)
                {
                    yield return null;
                }
            }

        }

        #endregion

    }
}
