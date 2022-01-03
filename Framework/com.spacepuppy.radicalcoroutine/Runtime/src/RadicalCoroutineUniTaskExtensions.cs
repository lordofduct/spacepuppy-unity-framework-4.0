#if SP_UNITASK
using UnityEngine;
using System.Collections;
using com.spacepuppy.Collections;
using Cysharp.Threading.Tasks;

namespace com.spacepuppy
{

    public static class RadicalCoroutineUniTaskExtensions
    {

        public static UniTask.Awaiter GetAwaiter(this IRadicalYieldInstruction instruction)
        {
            if (instruction == null) throw new System.ArgumentNullException(nameof(instruction));
            return new UniTask(RadicalPromise.Create(instruction, PlayerLoopTiming.Update, System.Threading.CancellationToken.None, out var token), token).GetAwaiter();
        }

        public static RadicalUniTask AsRadicalUniTask(this IEnumerable routine)
        {
            if (routine == null) throw new System.ArgumentNullException(nameof(routine));
            return new RadicalUniTask(routine.GetEnumerator());
        }

        /// <summary>
        /// Operates an IEnumerator coroutine as a RadicalCoroutine on the UniTask engine, granting access to all IRadicalYieldInstructions in the coroutine.  
        /// In a UniTask you can await SomeRoutine().AsRadicalUniTask() to do this. 
        /// </summary>
        /// <param name="routine"></param>
        /// <returns></returns>
        public static RadicalUniTask AsRadicalUniTask(this IEnumerator routine)
        {
            if (routine == null) throw new System.ArgumentNullException(nameof(routine));
            return new RadicalUniTask(routine);
        }

        public struct RadicalUniTask
        {
            private IEnumerator _e;

            internal RadicalUniTask(IEnumerator e)
            {
                _e = e;
            }

            public UniTask.Awaiter GetAwaiter()
            {
                return new UniTask(RadicalPromise.Create(_e, PlayerLoopTiming.Update, System.Threading.CancellationToken.None, out var token), token).GetAwaiter();
            }

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

            private static ObjectCachePool<RadicalPromise> _pool = new ObjectCachePool<RadicalPromise>(-1);

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

            public static IUniTaskSource Create(IEnumerator routine, PlayerLoopTiming timing, System.Threading.CancellationToken cancellationToken, out short token)
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

    }

}

#endif