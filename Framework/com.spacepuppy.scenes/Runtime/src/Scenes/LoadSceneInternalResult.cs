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

        public async System.Threading.Tasks.Task<LoadSceneInternalResult> AsTask()
        {
            if (Op == null) return this;

            await Op.AsTask();
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

        public void OnComplete(System.Action<LoadSceneInternalResult> callback)
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
        }

        #endregion

        public static readonly LoadSceneInternalResult Empty = new LoadSceneInternalResult();

    }

}
