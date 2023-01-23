using System;

namespace com.spacepuppy.Scenes
{

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

}
