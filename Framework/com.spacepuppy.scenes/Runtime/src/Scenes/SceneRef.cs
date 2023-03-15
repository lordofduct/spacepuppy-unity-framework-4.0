#pragma warning disable 0649 // variable declared but not used.

using UnityEngine;
using UnityEngine.SceneManagement;

namespace com.spacepuppy.Scenes
{

    /// <summary>
    /// Acts as a serializable reference to a scene in the editor. 
    /// The scene is not actually stored, but rather the name and guid of the scene. 
    /// The SceneName can also be set to a value formatted #0 to reference by build index. 
    /// Lastly the guid could be used for identity in separate packages like Addressables, 
    /// the integration with which is manual and not directly supported via SceneRef.LoadScene.
    /// </summary>
    [System.Serializable]
    public struct SceneRef
    {

        #region Fields

        [SerializeField]
        private string _sceneName;

        [SerializeField]
        private SerializableGuid _guid;

        #endregion

        #region CONSTRUCTOR

        public SceneRef(string sceneName)
        {
            _sceneName = sceneName;
            _guid = default;
        }

        public SceneRef(string sceneName, SerializableGuid guid)
        {
            _sceneName = sceneName;
            _guid = guid;
        }

        public SceneRef(string sceneName, System.Guid guid)
        {
            _sceneName = sceneName;
            _guid = guid;
        }

        #endregion

        #region Properties

        public string SceneName => _sceneName;

        public System.Guid AssetGuid => _guid;

        #endregion

        #region Methods

        public bool SceneNameIsValidInBuildSettings()
        {
            return SceneUtility.GetBuildIndexByScenePath(_sceneName) >= 0;
        }

        /// <summary>
        /// True if the SceneName is in the format #0 and the guid is empty. This doesn't guarantee that the index is a valid buildindex, just that it's formatted as such. 
        /// Use IsValidBuildIndexReference to validate if the build index is valid in the SceneManager. Use this to determine if the SceneName is not intended to be used 
        /// as a SceneName directly.
        /// </summary>
        /// <param name="buildIndex">The index value, -1 if not found.</param>
        /// <returns></returns>
        public bool IsBuildIndexReference(out int buildIndex)
        {
            if ((_sceneName ?? string.Empty).StartsWith('#') && _guid == System.Guid.Empty && System.Text.RegularExpressions.Regex.IsMatch(_sceneName, @"#\d+"))
            {
                if (!int.TryParse(_sceneName.Substring(1), out buildIndex))
                {
                    buildIndex = -1;
                    return true;
                }

                return true;
            }
            buildIndex = -1;
            return false;
        }

        /// <summary>
        /// Similar to IsBuildIndexReference, but validates that the index is valid in the SceneManager. 
        /// </summary>
        /// <param name="buildIndex"></param>
        /// <returns></returns>
        public bool IsValidBuildIndexReference(out int buildIndex)
        {
            if ((_sceneName ?? string.Empty).StartsWith('#') && _guid == System.Guid.Empty && System.Text.RegularExpressions.Regex.IsMatch(_sceneName, @"#\d+"))
            {
                if (!int.TryParse(_sceneName.Substring(1), out buildIndex) || buildIndex < 0 || buildIndex >= SceneManager.sceneCountInBuildSettings)
                {
                    buildIndex = -1;
                    return false;
                }

                return true;
            }
            buildIndex = -1;
            return false;
        }

        public string ResolveSceneName()
        {
            int bindex;
            if (this.IsBuildIndexReference(out bindex))
            {
                return bindex >= 0 && bindex < SceneManager.sceneCountInBuildSettings ? SceneUtility.GetScenePathByBuildIndex(bindex) : string.Empty;
            }
            return _sceneName ?? string.Empty;
        }

        public LoadSceneWaitHandle LoadScene(LoadSceneMode mode, LoadSceneBehaviour behaviour, object persistentToken = null)
        {
            int bindex;
            if (this.IsBuildIndexReference(out bindex))
            {
                return bindex >= 0 && bindex < SceneManager.sceneCountInBuildSettings ? SceneManagerUtils.LoadScene(bindex, mode, behaviour, persistentToken) : null;
            }

            return SceneManagerUtils.LoadScene(_sceneName, mode, behaviour, persistentToken);
        }

        #endregion

        #region Conversion

        public static explicit operator string(SceneRef sceneRef)
        {
            return sceneRef._sceneName;
        }

        public static explicit operator SceneRef(string sceneName)
        {
            return new SceneRef(sceneName);
        }

        #endregion

    }

}
