using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.spacepuppy.Utils
{

    public static class SpawnUtil
    {

        public static T Instantiate<T>(T obj) where T : UnityEngine.Object
        {
#if UNITY_EDITOR
            if (!Application.isPlaying && UnityEditor.PrefabUtility.GetPrefabAssetType(obj) != UnityEditor.PrefabAssetType.NotAPrefab)
            {
                return UnityEditor.PrefabUtility.InstantiatePrefab(obj) as T;
            }
            else
            {
                return UnityEngine.Object.Instantiate(obj);
            }
#else
            return UnityEngine.Object.Instantiate(obj);
#endif
        }

        public static T Instantiate<T>(T obj, Transform parent, bool instantiateInWorldSpace = false) where T : UnityEngine.Object
        {
#if UNITY_EDITOR
            if (!Application.isPlaying && UnityEditor.PrefabUtility.GetPrefabAssetType(obj) != UnityEditor.PrefabAssetType.NotAPrefab)
            {
                var result = UnityEditor.PrefabUtility.InstantiatePrefab(obj, parent) as T;
                var go = GameObjectUtil.GetGameObjectFromSource(result);
                GameObject pgo = GameObjectUtil.GetGameObjectFromSource(obj);
                if (go && pgo)
                {
                    if (instantiateInWorldSpace)
                    {
                        go.transform.position = pgo.transform.position;
                        go.transform.rotation = pgo.transform.rotation;
                    }
                    else
                    {
                        go.transform.localPosition = pgo.transform.position;
                        go.transform.localRotation = pgo.transform.rotation;
                    }
                }
                return result;
            }
            else
            {
                return UnityEngine.Object.Instantiate(obj, parent, instantiateInWorldSpace);
            }
#else
            return UnityEngine.Object.Instantiate(obj, parent, instantiateInWorldSpace);
#endif
        }

        public static T Instantiate<T>(T obj, Vector3 position, Quaternion rotation, Transform parent = null) where T : UnityEngine.Object
        {
#if UNITY_EDITOR
            if (!Application.isPlaying && UnityEditor.PrefabUtility.GetPrefabAssetType(obj) != UnityEditor.PrefabAssetType.NotAPrefab)
            {
                var result = UnityEditor.PrefabUtility.InstantiatePrefab(obj) as T;
                var go = GameObjectUtil.GetGameObjectFromSource(result);
                if (go != null)
                {
                    go.transform.SetParent(parent, false);
                    go.transform.position = position;
                    go.transform.rotation = rotation;
                }
                return result;
            }
            else
            {
                return UnityEngine.Object.Instantiate(obj, position, rotation, parent);
            }
#else
            return UnityEngine.Object.Instantiate(obj, position, rotation, parent);
#endif
        }

    }

}