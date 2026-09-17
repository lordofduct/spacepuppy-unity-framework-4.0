/*
 * In Unity 2023.1 the extension method AsyncOperationAwaitableExtensions.GetAwaiter(AsyncOperation) was added.
 * In UniTask 2.3.2 the extension method UnityAsyncExtensions.GetAwaiter(AsyncOperation) was conditionally removed.
 * Here we favor the UnityEngine.AsyncOperationAwaitableExtensions implementation if it exists and we're in a newer version of UniTask, otherwise we fall back on UniTask.
 */
#if UNITY_2023_1_OR_NEWER && (!SP_UNITASK || SP_UNITASK_2_3_2_OR_NEWER)
#define PREFER_UNITY_ASYNCOPERATION_GETAWAITER
using UnityEngine;
#endif
using UnityEngine.SceneManagement;
using com.spacepuppy.Scenes;
using com.spacepuppy.Async;
using AsyncOperation = UnityEngine.AsyncOperation;

#if SP_UNITASK
using Cysharp.Threading.Tasks;
#endif

namespace com.spacepuppy.Scenes
{


    public struct LoadSceneInternalResult
    {

        #region Fields

        public AsyncOperation Op;
        public Scene Scene;
        public LoadSceneParameters Parameters;

        #endregion

        #region Properties

        public LoadSceneMode Mode => Parameters.loadSceneMode;

        public float Progress => Op?.progress ?? 1f;

        public bool IsComplete => Op?.isDone ?? true;

        public bool IsValid => Scene.IsValid();

        #endregion

        #region Methods

        public async System.Threading.Tasks.Task<LoadSceneInternalResult> WaitForActivationTask()
        {
            if (Op == null) return this;

            while (!Op.isDone && Op.progress < 0.9f)
            {
                await System.Threading.Tasks.Task.Yield();
            }
            return this;
        }

#if SP_UNITASK
        public async UniTask<LoadSceneInternalResult> WaitForActivationUniTask()
        {
            if (Op == null) return this;

            while (!Op.isDone && Op.progress < 0.9f)
            {
                await UniTask.Yield();
            }
            return this;
        }
#endif

        public async System.Threading.Tasks.Task<LoadSceneInternalResult> AsTask()
        {
            if (Op == null) return this;
#if SP_UNITASK || PREFER_UNITY_ASYNCOPERATION_GETAWAITER
            await Op;
#else
            await Op.AsTask();
#endif
            return this;
        }

#if SP_UNITASK
        public async UniTask<LoadSceneInternalResult> AsUniTask()
        {
            if (Op == null) return this;

            await Op;
            return this;
        }
#endif

        public object GetYieldInstruction()
        {
            return Op;
        }

        public LoadSceneInternalResult OnComplete(System.Action<LoadSceneInternalResult> callback)
        {
            if (Op != null)
            {
                var r = this;
                Op.AsAsyncWaitHandle().OnComplete((h) => callback(r));
            }
            else
            {
                callback(this);
            }
            return this;
        }

        public LoadSceneInternalResult ResumeActivation()
        {
            if (Op != null && !Op.isDone) Op.allowSceneActivation = true;
            return this;
        }

        #endregion

        public static readonly LoadSceneInternalResult Empty = new LoadSceneInternalResult();

    }

}
