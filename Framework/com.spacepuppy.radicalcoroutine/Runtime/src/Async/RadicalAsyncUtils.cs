using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace com.spacepuppy.Async
{
    public static class RadicalAsyncUtils
    {

        public static AsyncWaitHandle AsAsyncWaitHandle(this IRadicalYieldInstruction inst)
        {
            return new AsyncWaitHandle(RadicalAsyncWaitHandleProvider.Default, inst);
        }

        public static AsyncWaitHandle<T> AsAsyncWaitHandle<T>(this RadicalWaitHandle<T> handle)
        {
            return new AsyncWaitHandle<T>(RadicalAsyncWaitHandleProvider<T>.Default, handle);
        }

        #region Special Types

        /// <summary>
        /// Acts as a IAsyncWaitHandleProvider to map IRadicalYieldInstructions to the AsyncWaitHandle struct.
        /// </summary>
        private class RadicalAsyncWaitHandleProvider : IAsyncWaitHandleProvider
        {

            public static readonly RadicalAsyncWaitHandleProvider Default = new RadicalAsyncWaitHandleProvider();

            public System.Threading.Tasks.Task GetAwaitable(object token)
            {
                if (token is IRadicalWaitHandle h)
                {
                    var s = AsyncUtil.GetTempSemaphore();
                    h.OnComplete((r) =>
                    {
                        s.Dispose();
                    });
                    return s.WaitAsync();
                }
                else if (token is IRadicalYieldInstruction inst)
                {
                    return System.Threading.Tasks.Task.Run(async () =>
                    {
                        while (!inst.IsComplete)
                        {
                            await System.Threading.Tasks.Task.Yield();
                        }
                    });
                }
                else
                {
                    //this should never be reached as long as it remains private and is used appropriately like that in RadicalWaitHandleExtensions.AsAsyncWaitHandle
                    throw new System.InvalidOperationException("An instance of RadicalAsyncWaitHandleProvider was associated with a token that was not an IRadicalYieldInstruction.");
                }
            }

            public object GetYieldInstruction(object token)
            {
                if (token is System.Collections.IEnumerator e)
                {
                    return e;
                }
                else if (token is IRadicalYieldInstruction h)
                {
                    return WaitUntilHandleIsDone(h);
                }
                else
                {
                    //this should never be reached as long as it remains private and is used appropriately like that in RadicalWaitHandleExtensions.AsAsyncWaitHandle
                    throw new System.InvalidOperationException("An instance of RadicalAsyncWaitHandleProvider was associated with a token that was not an IRadicalYieldInstruction.");
                }
            }

            public bool IsComplete(object token)
            {
                if (token is IRadicalYieldInstruction h)
                {
                    return h.IsComplete;
                }
                else
                {
                    //this should never be reached as long as it remains private and is used appropriately like that in RadicalWaitHandleExtensions.AsAsyncWaitHandle
                    throw new System.InvalidOperationException("An instance of RadicalAsyncWaitHandleProvider was associated with a token that was not an IRadicalYieldInstruction.");
                }
            }

            public void OnComplete(object token, System.Action<AsyncWaitHandle> callback)
            {
                if (token is IRadicalWaitHandle h)
                {
                    h.OnComplete((r) => callback(new AsyncWaitHandle(this, r)));
                }
                else if (token is IRadicalYieldInstruction inst)
                {
                    if (callback != null)
                    {
                        System.Threading.Tasks.Task.Run(async () =>
                        {
                            while (!inst.IsComplete)
                            {
                                await System.Threading.Tasks.Task.Yield();
                            }
                            callback(new AsyncWaitHandle(this, inst));
                        });
                    }
                }
                else
                {
                    //this should never be reached as long as it remains private and is used appropriately like that in RadicalWaitHandleExtensions.AsAsyncWaitHandle
                    throw new System.InvalidOperationException("An instance of RadicalAsyncWaitHandleProvider was associated with a token that was not an IRadicalYieldInstruction.");
                }
            }

            public object GetResult(object token)
            {
                return token as IRadicalYieldInstruction;
            }

            private System.Collections.IEnumerator WaitUntilHandleIsDone(IRadicalYieldInstruction h)
            {
                while (!h.IsComplete)
                {
                    yield return null;
                }
            }

        }

        private class RadicalAsyncWaitHandleProvider<T> : IAsyncWaitHandleProvider<T>
        {

            public static readonly RadicalAsyncWaitHandleProvider<T> Default = new RadicalAsyncWaitHandleProvider<T>();

            public System.Threading.Tasks.Task<T> GetAwaitable(object token)
            {
                if (token is RadicalWaitHandle<T> h)
                {
                    if(h.IsComplete)
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

            System.Threading.Tasks.Task IAsyncWaitHandleProvider.GetAwaitable(object token)
            {
                return RadicalAsyncWaitHandleProvider.Default.GetAwaitable(token);
            }

            public object GetYieldInstruction(object token)
            {
                return RadicalAsyncWaitHandleProvider.Default.GetYieldInstruction(token);
            }

            public bool IsComplete(object token)
            {
                return RadicalAsyncWaitHandleProvider.Default.IsComplete(token);
            }

            public void OnComplete(object token, System.Action<AsyncWaitHandle> callback)
            {
                RadicalAsyncWaitHandleProvider.Default.OnComplete(token, callback);
            }

            public void OnComplete(object token, System.Action<AsyncWaitHandle<T>> callback)
            {
                if (token is RadicalWaitHandle<T> h)
                {
                    h.OnComplete((r) => callback(new AsyncWaitHandle<T>(this, r)));
                }
                else
                {
                    //this should never be reached as long as it remains private and is used appropriately like that in RadicalWaitHandleExtensions.AsAsyncWaitHandle
                    throw new System.InvalidOperationException("An instance of RadicalAsyncWaitHandleProvider was associated with a token that was not an RadicalWaitHandle<T>.");
                }
            }

            public T GetResult(object token)
            {
                if(token is RadicalWaitHandle<T> h)
                {
                    return h.Result;
                }
                else
                {
                    throw new System.InvalidOperationException("An instance of RadicalAsyncWaitHandleProvider was associated with a token that was not an RadicalWaitHandle<T>.");
                }
            }

            object IAsyncWaitHandleProvider.GetResult(object token)
            {
                if(token is RadicalWaitHandle<T> h)
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
                handle.OnComplete((r) =>
                {
                    s.Dispose();
                });
                await s.WaitAsync();
                return handle.Result;
            }

        }

        #endregion

    }
}
