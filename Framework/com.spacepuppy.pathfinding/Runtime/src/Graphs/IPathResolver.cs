using System.Collections.Generic;
using System.Linq;
#if SP_UNITASK
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif

using com.spacepuppy.Async;

namespace com.spacepuppy.Graphs
{

    public interface IPathResolver<T>
    {

        T Start { get; set; }
        T Goal { get; set; }

        IList<T> Reduce();
        int Reduce(IList<T> path);

    }

    public enum StepPathingResult
    {
        Failed = -1,
        Idle = 0,
        Calculating = 1,
        Complete = 2,
    }

    public interface ISteppingPathResolver<T> : IPathResolver<T>
    {
        /// <summary>
        /// Start the stepping path resolver for reducing.
        /// </summary>
        void BeginSteppedReduce();
        /// <summary>
        /// Take a step at reducing the path resolver.
        /// </summary>
        StepPathingResult Step();
        /// <summary>
        /// Get the result of reducing the path.
        /// </summary>
        /// <param name="path"></param>
        int EndSteppedReduce(IList<T> path);
        /// <summary>
        /// Reset the resolver so a new Step sequence could be started.
        /// </summary>
        void Reset();
    }

    public static class PathResolverExtensions
    {

#if SP_UNITASK

        public static AsyncWaitHandle<IList<T>> ReduceAsync<T>(this ISteppingPathResolver<T> resolver, int stepsPerFrame)
        {
            return ReduceAsync_UniTask<T>(resolver, stepsPerFrame).AsAsyncWaitHandle();
        }
        public static async UniTask<IList<T>> ReduceAsync_UniTask<T>(this ISteppingPathResolver<T> resolver, int stepsPerFrame)
        {
            if (!GameLoop.IsMainThread) await UniTask.Yield();
            if (stepsPerFrame <= 0) stepsPerFrame = int.MaxValue;

            resolver.BeginSteppedReduce();
            while (true)
            {
                for (int i = 0; i < stepsPerFrame; i++)
                {
                    if (resolver.Step() != StepPathingResult.Calculating)
                    {
                        goto ContinueOnComplete;
                    }
                }
                await UniTask.Yield();
            }
        ContinueOnComplete:

            var lst = new List<T>();
            resolver.EndSteppedReduce(lst);
            return lst;
        }

        public static AsyncWaitHandle<int> ReduceAsync<T>(this ISteppingPathResolver<T> resolver, IList<T> path, int stepsPerFrame)
        {
            return ReduceAsync_UniTask<T>(resolver, path, stepsPerFrame).AsAsyncWaitHandle();
        }
        public static async UniTask<int> ReduceAsync_UniTask<T>(this ISteppingPathResolver<T> resolver, IList<T> path, int stepsPerFrame)
        {
            if (!GameLoop.IsMainThread) await UniTask.Yield();
            if (stepsPerFrame <= 0) stepsPerFrame = int.MaxValue;

            resolver.BeginSteppedReduce();
            while (true)
            {
                for (int i = 0; i < stepsPerFrame; i++)
                {
                    if (resolver.Step() != StepPathingResult.Calculating)
                    {
                        goto ContinueOnComplete;
                    }
                }
                await UniTask.Yield();
            }
        ContinueOnComplete:

            return resolver.EndSteppedReduce(path);
        }

#else

        public static AsyncWaitHandle<IList<T>> ReduceAsync<T>(this ISteppingPathResolver<T> resolver, int stepsPerFrame)
        {
            if (GameLoop.IsMainThread)
            {
                return ReduceAsync_AsTask<T>(resolver, stepsPerFrame).AsAsyncWaitHandle();
            }
            else
            {
                var handle = new RadicalWaitHandle<IList<T>>();
                GameLoop.UpdateHandle.Invoke(() =>
                {
                    _ = ReduceAsync_AsTask<T>(resolver, stepsPerFrame, handle);
                });
                return handle.AsAsyncWaitHandle();
            }
        }
        private static async Task<IList<T>> ReduceAsync_AsTask<T>(ISteppingPathResolver<T> resolver, int stepsPerFrame, RadicalWaitHandle<IList<T>> handle = null)
        {
            try
            {
                if (stepsPerFrame <= 0) stepsPerFrame = int.MaxValue;

                resolver.BeginSteppedReduce();
                while (true)
                {
                    for (int i = 0; i < stepsPerFrame; i++)
                    {
                        if (resolver.Step() != StepPathingResult.Calculating)
                        {
                            goto ContinueOnComplete;
                        }
                    }
                    await Task.Yield();
                }
            ContinueOnComplete:

                var lst = new List<T>();
                resolver.EndSteppedReduce(lst);
                handle?.SignalComplete(lst);
                return lst;
            }
            catch(System.Exception ex)
            {
                handle?.SignalCancelled();
                throw ex;
            }
        }

        public static AsyncWaitHandle<int> ReduceAsync<T>(this ISteppingPathResolver<T> resolver, IList<T> path, int stepsPerFrame)
        {
            if (GameLoop.IsMainThread)
            {
                return ReduceAsync_AsTask<T>(resolver, path, stepsPerFrame).AsAsyncWaitHandle();
            }
            else
            {
                var handle = new RadicalWaitHandle<int>();
                GameLoop.UpdateHandle.Invoke(() =>
                {
                    _ = ReduceAsync_AsTask<T>(resolver, path, stepsPerFrame, handle);
                });
                return handle.AsAsyncWaitHandle();
            }
        }
        private static async Task<int> ReduceAsync_AsTask<T>(ISteppingPathResolver<T> resolver, IList<T> path, int stepsPerFrame, RadicalWaitHandle<int> handle = null)
        {
            try
            {
                if (stepsPerFrame <= 0) stepsPerFrame = int.MaxValue;

                resolver.BeginSteppedReduce();
                while (true)
                {
                    for (int i = 0; i < stepsPerFrame; i++)
                    {
                        if (resolver.Step() != StepPathingResult.Calculating)
                        {
                            goto ContinueOnComplete;
                        }
                    }
                    await Task.Yield();
                }
            ContinueOnComplete:

                int cnt = resolver.EndSteppedReduce(path);
                handle?.SignalComplete(cnt);
                return cnt;
            }
            catch (System.Exception ex)
            {
                handle?.SignalCancelled();
                throw ex;
            }
        }

#endif

    }

}
