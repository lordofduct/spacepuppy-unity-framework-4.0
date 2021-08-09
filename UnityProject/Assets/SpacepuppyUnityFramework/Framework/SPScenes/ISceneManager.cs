using UnityEngine;
using UnityEngine.SceneManagement;
using com.spacepuppy.Scenes;

namespace com.spacepuppy
{

    public interface ISceneManager : IService
    {

        event System.EventHandler<LoadSceneOptions> BeforeSceneLoaded;
        event System.EventHandler<SceneUnloadedEventArgs> BeforeSceneUnloaded;
        event System.EventHandler<SceneUnloadedEventArgs> SceneUnloaded;
        event System.EventHandler<LoadSceneOptions> SceneLoaded;
        event System.EventHandler<ActiveSceneChangedEventArgs> ActiveSceneChanged;

        /// <summary>
        /// A MonoBehaviour that can be used to hook coroutines into that lives through the load.
        /// </summary>
        MonoBehaviour Hook { get; }

        void LoadScene(LoadSceneOptions options);
        LoadSceneWaitHandle LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, LoadSceneBehaviour behaviour = LoadSceneBehaviour.Async, object persistentToken = null);
        LoadSceneWaitHandle LoadScene(int sceneBuildIndex, LoadSceneMode mode = LoadSceneMode.Single, LoadSceneBehaviour behaviour = LoadSceneBehaviour.Async, object persistentToken = null);

        AsyncOperation UnloadScene(Scene scene);
        Scene GetActiveScene();

        /// <summary>
        /// Test if a scene by the name exists.
        /// </summary>
        /// <param name="excludeInactive">False to test if the scene exists as a loadable scene, True if to test if the scene exists and is actively loaded.</param>
        /// <returns></returns>
        bool SceneExists(string sceneName, bool excludeInactive = false);

    }

}
