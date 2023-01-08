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
            //if (!Application.isPlaying && UnityEditor.PrefabUtility.GetPrefabType(obj) == UnityEditor.PrefabType.Prefab)
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

        public static T Instantiate<T>(T obj, Transform parent) where T : UnityEngine.Object
        {
#if UNITY_EDITOR
            //if (!Application.isPlaying && UnityEditor.PrefabUtility.GetPrefabType(obj) == UnityEditor.PrefabType.Prefab)
            if (!Application.isPlaying && UnityEditor.PrefabUtility.GetPrefabAssetType(obj) != UnityEditor.PrefabAssetType.NotAPrefab)
            {
                var result = UnityEditor.PrefabUtility.InstantiatePrefab(obj) as T;
                var go = GameObjectUtil.GetGameObjectFromSource(result);
                if (go != null)
                    go.transform.SetParent(parent);
                return result;
            }
            else
            {
                return UnityEngine.Object.Instantiate(obj, parent);
            }
#else
                return UnityEngine.Object.Instantiate(obj, parent);
#endif
        }

        public static T Instantiate<T>(T obj, Vector3 position, Quaternion rotation, Transform parent) where T : UnityEngine.Object
        {
#if UNITY_EDITOR
            //if (!Application.isPlaying && UnityEditor.PrefabUtility.GetPrefabType(obj) == UnityEditor.PrefabType.Prefab)
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