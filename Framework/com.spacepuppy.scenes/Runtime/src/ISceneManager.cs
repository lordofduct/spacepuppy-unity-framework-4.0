using UnityEngine;
using UnityEngine.SceneManagement;
using com.spacepuppy.Scenes;

namespace com.spacepuppy
{

    public interface ISceneManager : IService
    {

        /// <summary>
        /// Occurs just before a scene is loaded, if the scene was not loaded through this ISceneManager this event will not raise.
        /// </summary>
        event System.EventHandler<LoadSceneOptions> BeforeSceneLoaded;
        /// <summary>
        /// Occurs when ISceneManager.UnloadScene is called, if the scene was not unloaded through this ISceneManager this event will not raise.
        /// </summary>
        event System.EventHandler<SceneUnloadedEventArgs> BeforeSceneUnloaded;
        /// <summary>
        /// Occurs after any scene has unloaded.
        /// </summary>
        event System.EventHandler<SceneUnloadedEventArgs> SceneUnloaded;
        /// <summary>
        /// Occurs after any scene has loaded and includes what LoadSceneOptions handled that load. 
        /// If the scene is not managed by this ISceneManager the options will be of type UnmanagedSceneLoadedEventArgs.
        /// </summary>
        event System.EventHandler<LoadSceneOptions> SceneLoaded;
        /// <summary>
        /// Signals that the scene returned by 'GetActiveScene' has changed.
        /// </summary>
        event System.EventHandler<ActiveSceneChangedEventArgs> ActiveSceneChanged;

        /// <summary>
        /// Occurs just before a LoadSceneOptions is started, multiple scenes may be loaded before LoadSceneCompleted if the LoadSceneOptions is a complex option. 
        /// This event only raises if the scene was loaded through this ISceneManager.
        /// </summary>
        event System.EventHandler<LoadSceneOptions> BeganLoad;
        /// <summary>
        /// Occurs after a LoadSceneOptions is finished and all scenes it is to load have completed loading. If the scene loaded was not directly loaded through 
        /// this ISceneManager the options will be of type UnmanagedSceneLoadedEventArgs.
        /// </summary>
        event System.EventHandler<LoadSceneOptions> CompletedLoad;

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
