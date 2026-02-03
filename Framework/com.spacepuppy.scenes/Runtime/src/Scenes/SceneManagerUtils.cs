using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using com.spacepuppy.Async;
using com.spacepuppy.Utils;
using com.spacepuppy.Collections;



#if SP_UNITASK
using Cysharp.Threading.Tasks;
#endif

#if SP_ADDRESSABLES
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.ResourceLocations;
#endif

namespace com.spacepuppy.Scenes
{

    public static class SceneManagerUtils
    {

        #region Static Utils

        public static LoadSceneBehaviour RestrictAsyncAndAwait(this LoadSceneBehaviour value)
        {
            return value == LoadSceneBehaviour.AsyncAndWait ? LoadSceneBehaviour.Async : value;
        }

        public static LoadSceneOptions LoadScene(LoadSceneOptions options)
        {
            var manager = Services.Get<ISceneManager>() ?? InternalSceneManager.Instance;
            return manager.LoadScene(options);
        }

        /// <summary>
        /// Attempts to load the scene from the registered ISceneManager if it exists, if it doesn't, it falls back on the default UnityEngine.SceneManager.SceneManager.
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="mode"></param>
        /// <param name="behaviour"></param>
        /// <param name="persistentToken"></param>
        /// <returns></returns>
        public static LoadSceneWaitHandle LoadScene(string sceneName, LoadSceneMode mode, LoadSceneBehaviour behaviour, object persistentToken = null)
        {
            var manager = Services.Get<ISceneManager>() ?? InternalSceneManager.Instance;
            var handle = new LoadSceneWaitHandle(sceneName, mode, behaviour, persistentToken);
            manager.LoadScene(handle);
            return handle;
        }

        /// <summary>
        /// Attempts to load the scene from the registered ISceneManager if it exists, if it doesn't, it falls back on the default UnityEngine.SceneManager.SceneManager.
        /// </summary>
        /// <param name="sceneBuildIndex"></param>
        /// <param name="mode"></param>
        /// <param name="behaviour"></param>
        /// <param name="persistentToken"></param>
        /// <returns></returns>
        /// <exception cref="System.IndexOutOfRangeException"></exception>
        public static LoadSceneWaitHandle LoadScene(int sceneBuildIndex, LoadSceneMode mode, LoadSceneBehaviour behaviour, object persistentToken = null)
        {
            if (sceneBuildIndex < 0 || sceneBuildIndex >= SceneManager.sceneCountInBuildSettings) throw new System.IndexOutOfRangeException(nameof(sceneBuildIndex));

            var manager = Services.Get<ISceneManager>() ?? InternalSceneManager.Instance;
            var handle = new LoadSceneWaitHandle(sceneBuildIndex, mode, behaviour, persistentToken);
            manager.LoadScene(handle);
            return handle;
        }



        public static LoadSceneInternalResult LoadSceneInternal(SceneRef sceneref, LoadSceneParameters parameters, LoadSceneBehaviour behaviour)
        {
            int bindex;
            if (sceneref.IsBuildIndexReference(out bindex))
            {
                return bindex >= 0 && bindex < SceneManager.sceneCountInBuildSettings ? LoadSceneInternal(bindex, parameters, behaviour) : default;
            }

            return LoadSceneInternal(sceneref.SceneName, parameters, behaviour);
        }

        public static LoadSceneInternalResult LoadSceneInternal(string sceneName, LoadSceneParameters parameters, LoadSceneBehaviour behaviour)
        {
            switch (behaviour)
            {
                case LoadSceneBehaviour.Standard:
                    {
                        var scene = SceneManager.LoadScene(sceneName, parameters);
                        return new LoadSceneInternalResult()
                        {
                            Op = null,
                            Scene = scene,
                            Parameters = parameters,
                        };
                    }
                case LoadSceneBehaviour.Async:
                    {
                        var op = SceneManager.LoadSceneAsync(sceneName, parameters);
                        int buildIndex = SceneUtility.GetBuildIndexByScenePath(sceneName);
                        Scene scene;
                        if (buildIndex >= 0)
                        {
                            scene = SceneManager.GetSceneByBuildIndex(buildIndex);
                        }
                        else
                        {
                            scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
                        }
                        return new LoadSceneInternalResult()
                        {
                            Op = op,
                            Scene = scene,
                            Parameters = parameters,
                        };
                    }
                case LoadSceneBehaviour.AsyncAndWait:
                    {
                        var op = SceneManager.LoadSceneAsync(sceneName, parameters);
                        op.allowSceneActivation = false;
                        int buildIndex = SceneUtility.GetBuildIndexByScenePath(sceneName);
                        Scene scene;
                        if (buildIndex >= 0)
                        {
                            scene = SceneManager.GetSceneByBuildIndex(buildIndex);
                        }
                        else
                        {
                            scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
                        }
                        return new LoadSceneInternalResult()
                        {
                            Op = op,
                            Scene = scene,
                            Parameters = parameters,
                        };
                    }
                default:
                    throw new System.InvalidOperationException("Unsupported LoadSceneBehaviour.");
            }
        }

        public static LoadSceneInternalResult LoadSceneInternal(int buildIndex, LoadSceneParameters parameters, LoadSceneBehaviour behaviour)
        {
            switch (behaviour)
            {
                case LoadSceneBehaviour.Standard:
                    {
                        var scene = SceneManager.LoadScene(buildIndex, parameters);
                        return new LoadSceneInternalResult()
                        {
                            Op = null,
                            Scene = scene,
                            Parameters = parameters,
                        };
                    }
                case LoadSceneBehaviour.Async:
                    {
                        var op = SceneManager.LoadSceneAsync(buildIndex, parameters);
                        var scene = SceneManager.GetSceneByBuildIndex(buildIndex);
                        return new LoadSceneInternalResult()
                        {
                            Op = op,
                            Scene = scene,
                            Parameters = parameters,
                        };
                    }
                case LoadSceneBehaviour.AsyncAndWait:
                    {
                        var op = SceneManager.LoadSceneAsync(buildIndex, parameters);
                        op.allowSceneActivation = false;
                        var scene = SceneManager.GetSceneByBuildIndex(buildIndex);
                        return new LoadSceneInternalResult()
                        {
                            Op = op,
                            Scene = scene,
                            Parameters = parameters,
                        };
                    }
                default:
                    throw new System.InvalidOperationException("Unsupported LoadSceneBehaviour.");
            }
        }

#if SP_ADDRESSABLES

        /// <summary>
        /// Loads an addressable scene.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="mode"></param>
        /// <param name="behaviour></param>
        /// <returns>The Op field of LoadSceneInternalResult may not be set depending the version of Addressables used. The operator is reflected out and if Addressables lib changes the name it can't be retrieved.</returns>
        public static AsyncWaitHandle<(SceneInstance, LoadSceneInternalResult)> LoadAddressableSceneInternal(object key, LoadSceneMode mode = LoadSceneMode.Single, LoadSceneBehaviour behaviour = LoadSceneBehaviour.Async)
        {
            var handle = UnityEngine.AddressableAssets.Addressables.LoadSceneAsync(key, mode, behaviour != LoadSceneBehaviour.AsyncAndWait);
            return DoLoadAddressableScene(handle, mode).AsAsyncWaitHandle();
        }

        public static AsyncWaitHandle<(SceneInstance, LoadSceneInternalResult)> LoadAddressableSceneInternal(IResourceLocation loc, LoadSceneMode mode = LoadSceneMode.Single, LoadSceneBehaviour behaviour = LoadSceneBehaviour.Async)
        {
            var handle = UnityEngine.AddressableAssets.Addressables.LoadSceneAsync(loc, mode, behaviour != LoadSceneBehaviour.AsyncAndWait);
            return DoLoadAddressableScene(handle, mode).AsAsyncWaitHandle();
        }

#if SP_UNITASK
        private static async UniTask<(SceneInstance, LoadSceneInternalResult)> DoLoadAddressableScene(AsyncOperationHandle<SceneInstance> handle, LoadSceneMode mode)
        {
            await handle;
#else
        private static async System.Threading.Tasks.Task<(SceneInstance, UnityLoadResult)> DoLoadAddressableScene(AsyncOperationHandle<SceneInstance> handle, LoadSceneMode mode)
        {
            await handle.Task;
#endif

            var sceneinst = handle.Result;
            AsyncOperation op = null;

            var fieldinfo = typeof(SceneInstance).GetField("m_Operation", System.Reflection.BindingFlags.NonPublic);
            if (fieldinfo != null && fieldinfo.FieldType == typeof(AsyncOperation))
            {
                op = fieldinfo.GetValue(sceneinst) as AsyncOperation;
            }

            var result = new LoadSceneInternalResult()
            {
                Op = op,
                Scene = sceneinst.Scene,
                Parameters = new LoadSceneParameters(mode),
            };
            return (sceneinst, result);
        }
#endif

        #endregion

        #region Scene Extensions

        public static T FindObjectOfType<T>(this Scene scene) where T : class
        {
            using (var roots = TempCollection.GetList<GameObject>())
            {
                scene.GetRootGameObjects(roots);
                if (roots.Count == 0) return null;

                if (TypeUtil.IsType(typeof(T), typeof(GameObject)))
                {
                    return roots.First() as T;
                }
                else
                {
                    foreach (var go in roots)
                    {
                        if (go.GetComponentInChildren(out T c))
                        {
                            return c;
                        }
                    }
                }
            }

            return null;
        }

        public static T[] FindObjectsOfType<T>(this Scene scene) where T : class
        {
            using (var roots = TempCollection.GetList<GameObject>())
            {
                scene.GetRootGameObjects(roots);
                if (roots.Count == 0) return ArrayUtil.Empty<T>();

                using (var outputs = TempCollection.GetList<T>())
                {
                    if (TypeUtil.IsType(typeof(T), typeof(GameObject)))
                    {
                        foreach (var go in roots)
                        {
                            var arr = go.GetComponentsInChildren<Transform>();
                            foreach (var t in arr)
                            {
                                outputs.Add(t.gameObject as T);
                            }
                        }
                    }
                    else
                    {
                        foreach (var go in roots)
                        {
                            var arr = go.GetComponentsInChildren<T>();
                            outputs.AddRange(arr);
                        }
                    }

                    return outputs.ToArray();
                }
            }
        }

        public static int FindObjectsOfType<T>(this Scene scene, ICollection<T> buffer) where T : class
        {
            if (buffer == null) throw new System.ArgumentNullException(nameof(buffer));

            using (var roots = TempCollection.GetList<GameObject>())
            {
                scene.GetRootGameObjects(roots);
                if (roots.Count == 0) return 0;

                int cnt = 0;
                if (TypeUtil.IsType(typeof(T), typeof(GameObject)))
                {
                    foreach (var go in roots)
                    {
                        var arr = go.GetComponentsInChildren<Transform>();
                        cnt += arr.Length;
                        foreach (var t in arr)
                        {
                            buffer.Add(t.gameObject as T);
                        }
                    }
                }
                else
                {
                    foreach (var go in roots)
                    {
                        var arr = go.GetComponentsInChildren<T>();
                        cnt += arr.Length;
                        foreach (var c in arr)
                        {
                            buffer.Add(c);
                        }
                    }
                }
                return cnt;
            }
        }

        #endregion

    }

}
