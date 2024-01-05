namespace com.spacepuppy.Scenes
{

    public interface ISceneManagerBeganLoadGlobalHandler
    {
        void OnSceneManagerBeganLoad(LoadSceneOptions options);
    }

    public interface IBeforeSceneLoadedGlobalHandler
    {
        void OnBeforeSceneLoaded(LoadSceneOptions options);
    }

    public interface ISceneLoadedGlobalHandler
    {
        void OnSceneLoaded(LoadSceneOptions options);
    }

    /// <summary>
    /// A found message receiver to handle OnSceneLoaded event.
    /// </summary>
    public interface ILoadSceneOptionsCompleteGlobalHandler
    {

        void OnComplete(LoadSceneOptions options);

    }

    public interface IBeforeSceneUnloadedGlobalHandler
    {
        void OnBeforeSceneUnloaded(ISceneManager manager, SceneUnloadedEventArgs args);
    }

    public interface ISceneUnloadedGlobalHandler
    {
        void OnSceneUnloaded(ISceneManager manager, SceneUnloadedEventArgs args);
    }

    public interface IActiveSceneChangedGlobalHandler
    {
        void OnActiveSceneChanged(ISceneManager manager, ActiveSceneChangedEventArgs args);
    }

}
