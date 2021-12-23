using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using com.spacepuppy.Utils;

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

            public float GetProgress(object token)
            {
                if (token is IProgressingYieldInstruction p)
                {
                    return p.IsComplete ? 1f : p.Progress;
                }
                else if (token is IRadicalYieldInstruction inst)
                {
                    return inst.IsComplete ? 1f : 0f;
                }
                else
                {
                    //this should never be reached as long as it remains private and is used appropriately like that in RadicalWaitHandleExtensions.AsAsyncWaitHandle
                    throw new System.InvalidOperationException("An instance of RadicalAsyncWaitHandleProvider was associated with a token that was not an IRadicalYieldInstruction.");
                }
            }

            public System.Threading.Tasks.Task GetTask(object token)
            {
                bool complete = false;

                if (token is IRadicalWaitHandle h)
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
                else if (token is IRadicalYieldInstruction inst)
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
                            GameLoop.UpdateHandle.BeginInvoke(() => GameLoop.Hook.StartRadicalCoroutine(WaitUntilHandleIsDone(inst, (a) => s.Dispose())));
                        }
                        else
                        {
                            GameLoop.Hook.StartRadicalCoroutine(WaitUntilHandleIsDone(inst, (a) => s.Dispose()));
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
                else if (token is IRadicalYieldInstruction inst)
                {
                    if (callback != null)
                    {
                        if(GameLoop.InvokeRequired)
                        {
                            GameLoop.UpdateHandle.BeginInvoke(() => GameLoop.Hook.StartRadicalCoroutine(WaitUntilHandleIsDone(inst, callback)));
                        }
                        else
                        {
                            GameLoop.Hook.StartRadicalCoroutine(WaitUntilHandleIsDone(inst, callback));
                        }
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

            public float GetProgress(object token)
            {
                return RadicalAsyncWaitHandleProvider.Default.GetProgress(token);
            }

            public System.Threading.Tasks.Task<T> GetTask(object token)
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

            System.Threading.Tasks.Task IAsyncWaitHandleProvider.GetTask(object token)
            {
                return RadicalAsyncWaitHandleProvider.Default.GetTask(token);
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
