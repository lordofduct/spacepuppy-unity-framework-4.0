using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using com.spacepuppy.Async;
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

        LoadSceneOptions LoadScene(LoadSceneOptions options);

        AsyncWaitHandle UnloadScene(Scene scene);

        /// <summary>
        /// Calls directly through to the SceneManager to load a scene, this should never be called directly.
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="mode"></param>
        /// <param name="behaviour"></param>
        /// <returns></returns>
        /// <remarks>This method is intended to allow a ISceneManager implementation to redirect through which unity api the scene is loaded other than 
        /// the default UnityEngine.SceneManagement.SceneManager. Examples include using Addressables to load scenes, or NetworkManager.Singleton.SceneManager.
        /// </remarks>
        LoadSceneInternalResult LoadSceneInternal(SceneRef sceneName, LoadSceneParameters parameters, LoadSceneBehaviour behaviour);

        /// <summary>
        /// While loading a scene this returns the related LoadSceneOptions for a scene. 
        /// Can be called like Services.Get<ISceneManager>()?.FindRelatedLoadSceneOptions(this.gameObject.scene) during Awake. 
        /// </summary>
        /// <param name="scene"></param>
        /// <returns></returns>
        LoadSceneOptions FindRelatedLoadSceneOptions(Scene scene);

    }

    /// <summary>
    /// ISceneManager extension methods for retrieving Scene's at runtime assume that UnityEngine.SceneManagement.SceneManager is being used. 
    /// If you need to create a SceneManager that gets even more granular, use this interface to force calls into your custom scene manager 
    /// for handling methods that retrieve the current active scenes.
    /// </summary>
    public interface IOverridingSceneManager : ISceneManager
    {

        int ActiveSceneCount { get; }

        Scene GetActiveScene();
        Scene GetSceneByPath(string scenePath);
        Scene GetSceneByName(string sceneName);
        Scene GetSceneByBuildIndex(int buildIndex);
        Scene GetSceneAt(int index);
        Scene CreateScene(string sceneName, CreateSceneParameters parameters);
        IEnumerable<Scene> GetAllScenes();

        /// <summary>
        /// Test if a scene by the name exists.
        /// </summary>
        /// <param name="excludeInactive">False to test if the scene exists as a loadable scene, True if to test if the scene exists and is actively loaded.</param>
        /// <returns></returns>
        bool SceneExists(string sceneName, bool excludeInactive = false);

    }

    public static class ISceneManagerExtensions
    {

        public static LoadSceneWaitHandle LoadScene(this ISceneManager sceneManager, SceneRef scene, LoadSceneMode mode = LoadSceneMode.Single, LoadSceneBehaviour behaviour = LoadSceneBehaviour.Async, object persistentToken = null)
        {
            if (sceneManager == null) throw new System.InvalidOperationException(nameof(sceneManager));

            var handle = new LoadSceneWaitHandle(scene, mode, behaviour, persistentToken);
            sceneManager.LoadScene(handle);
            return handle;
        }

        public static LoadSceneWaitHandle LoadScene(this ISceneManager sceneManager, string sceneName, LoadSceneMode mode = LoadSceneMode.Single, LoadSceneBehaviour behaviour = LoadSceneBehaviour.Async, object persistentToken = null)
        {
            if (sceneManager == null) throw new System.InvalidOperationException(nameof(sceneManager));

            var handle = new LoadSceneWaitHandle(sceneName, mode, behaviour, persistentToken);
            sceneManager.LoadScene(handle);
            return handle;
        }

        public static LoadSceneWaitHandle LoadScene(this ISceneManager sceneManager, int sceneBuildIndex, LoadSceneMode mode = LoadSceneMode.Single, LoadSceneBehaviour behaviour = LoadSceneBehaviour.Async, object persistentToken = null)
        {
            if (sceneManager == null) throw new System.InvalidOperationException(nameof(sceneManager));
            if (sceneBuildIndex < 0 || sceneBuildIndex >= SceneManager.sceneCountInBuildSettings) throw new System.IndexOutOfRangeException(nameof(sceneBuildIndex));

            var handle = new LoadSceneWaitHandle(sceneBuildIndex, mode, behaviour, persistentToken);
            sceneManager.LoadScene(handle);
            return handle;
        }

        #region IOverridingSceneManager Interface

        public static int GetActiveSceneCount(this ISceneManager manager)
        {
            if (manager is IOverridingSceneManager osm)
            {
                return osm.ActiveSceneCount;
            }
            else
            {
                return SceneManager.sceneCount;
            }
        }

        public static bool SceneExists(this ISceneManager manager, string sceneName, bool excludeInactive = false)
        {
            if (manager is IOverridingSceneManager osm)
            {
                return osm.SceneExists(sceneName, excludeInactive);
            }
            else
            {
                if (excludeInactive)
                {
                    var sc = SceneManager.GetSceneByName(sceneName);
                    return sc.IsValid();
                }
                else
                {
                    return SceneUtility.GetBuildIndexByScenePath(sceneName) >= 0;
                }
            }
        }

        public static Scene GetActiveScene(this ISceneManager manager)
        {
            if (manager is IOverridingSceneManager osm)
                return osm.GetActiveScene();
            else
                return SceneManager.GetActiveScene();
        }

        public static Scene GetSceneByPath(this ISceneManager manager, string scenePath)
        {
            if (manager is IOverridingSceneManager osm)
                return osm.GetSceneByPath(scenePath);
            else
                return SceneManager.GetSceneByPath(scenePath);
        }

        public static Scene GetSceneByName(this ISceneManager manager, string sceneName)
        {
            if (manager is IOverridingSceneManager osm)
                return osm.GetSceneByName(sceneName);
            else
                return SceneManager.GetSceneByName(sceneName);
        }

        public static Scene GetSceneByBuildIndex(this ISceneManager manager, int buildIndex)
        {
            if (manager is IOverridingSceneManager osm)
                return osm.GetSceneByBuildIndex(buildIndex);
            else
                return SceneManager.GetSceneByBuildIndex(buildIndex);
        }

        public static Scene GetSceneAt(this ISceneManager manager, int index)
        {
            if (manager is IOverridingSceneManager osm)
                return osm.GetSceneAt(index);
            else
                return SceneManager.GetSceneAt(index);
        }

        public static Scene CreateScene(this ISceneManager manager, string sceneName) => CreateScene(manager, sceneName, new CreateSceneParameters(LocalPhysicsMode.None));
        public static Scene CreateScene(this ISceneManager manager, string sceneName, CreateSceneParameters parameters)
        {
            if (manager is IOverridingSceneManager osm)
                return osm.CreateScene(sceneName, parameters);
            else
                return SceneManager.CreateScene(sceneName, parameters);
        }

        public static IEnumerable<Scene> GetAllScenes(this ISceneManager manager)
        {
            if (manager is IOverridingSceneManager osm)
            {
                return osm.GetAllScenes();
            }
            else
            {
                return GetAllScenesIterator();
            }
        }
        private static IEnumerable<Scene> GetAllScenesIterator()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                yield return SceneManager.GetSceneAt(i);
            }
        }

        #endregion

    }

}
