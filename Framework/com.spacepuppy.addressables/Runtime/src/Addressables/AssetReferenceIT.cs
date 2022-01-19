using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.spacepuppy.Addressables
{

    /// <summary>
    /// Similar to AssetReferenceT, but designed for interfaces since AssetReferenceT constrains to UnityEngine.Object.
    /// </summary>
    /// <typeparam name="TInterface"></typeparam>
    public class AssetReferenceIT<TInterface> : AssetReference where TInterface : class
    {

        static AssetReferenceIT()
        {
#if UNITY_EDITOR
            if (!typeof(TInterface).IsInterface) throw new System.InvalidOperationException("An attempt was made to derive from AssetReferenceIT with a type that is not an interface. Use AssetReferenceT for concrete types instead.");
#endif
        }

        public AssetReferenceIT(string guid) : base(guid)
        {
        }

        public virtual AsyncOperationHandle<TInterface> LoadAssetAsync()
        {
            return LoadAssetAsync<TInterface>();
        }

        public override bool ValidateAsset(Object obj)
        {
            var type = obj.GetType();
            return typeof(TInterface).IsAssignableFrom(type);
        }

        public override bool ValidateAsset(string mainAssetPath)
        {
#if UNITY_EDITOR
            if (typeof(TInterface).IsAssignableFrom(AssetDatabase.GetMainAssetTypeAtPath(mainAssetPath)))
                return true;

            var repr = AssetDatabase.LoadAllAssetRepresentationsAtPath(mainAssetPath);
            return repr != null && repr.Any(o => o is TInterface);
#else
            return false;
#endif
        }

#if UNITY_EDITOR
        public new TInterface editorAsset
        {
            get
            {
                Object baseAsset = base.editorAsset;
                TInterface asset = baseAsset as TInterface;
                if (asset == null && baseAsset != null)
                    Debug.Log("editorAsset cannot cast to " + typeof(TInterface));
                return asset;
            }
        }
#endif

    }

}