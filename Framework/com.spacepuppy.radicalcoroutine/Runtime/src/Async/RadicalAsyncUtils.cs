using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using com.spacepuppy.Utils;

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
                    if(GameLoop.InvokeRequired)
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
                    if(GameLoop.InvokeRequired)
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
                        if(GameLoop.InvokeRequired)
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
                    if(GameLoop.InvokeRequired)
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
                    else if(h.IsComplete)
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
                if(handle.Token is RadicalWaitHandle<T> h)
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
                if(handle.Token is RadicalWaitHandle<T> h)
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
                if(GameLoop.InvokeRequired)
                {
                    GameLoop.UpdateHandle.BeginInvoke(() =>
                    {
                        if(handle.IsComplete)
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

    }
}
