using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace com.spacepuppy.Scenes
{

    public static class SceneManagerUtils
    {

        #region Static Utils

        public static LoadSceneBehaviour RestrictAsyncAndAwait(this LoadSceneBehaviour value)
        {
            return value == LoadSceneBehaviour.AsyncAndWait ? LoadSceneBehaviour.Async : value;
        }

        public static LoadSceneWaitHandle LoadScene(string sceneName, LoadSceneMode mode, LoadSceneBehaviour behaviour, object persistentToken = null)
        {
            var manager = Services.Get<ISceneManager>();
            if (manager != null) return manager.LoadScene(sceneName, mode, behaviour);

            var handle = new LoadSceneWaitHandle(sceneName, mode, behaviour, persistentToken);
            handle.Begin(null);
            return handle;
        }

        public static LoadSceneWaitHandle LoadScene(int sceneBuildIndex, LoadSceneMode mode, LoadSceneBehaviour behaviour, object persistentToken = null)
        {
            if (sceneBuildIndex < 0 || sceneBuildIndex >= SceneManager.sceneCountInBuildSettings) throw new System.IndexOutOfRangeException("sceneBuildIndex");

            var manager = Services.Get<ISceneManager>();
            if (manager != null) return manager.LoadScene(sceneBuildIndex, mode, behaviour);

            string sceneName = SceneUtility.GetScenePathByBuildIndex(sceneBuildIndex);

            var handle = new LoadSceneWaitHandle(sceneName, mode, behaviour, persistentToken);
            handle.Begin(null);
            return handle;
        }

        #endregion

    }

}
