#if SP_ADDRESSABLES
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using UnityEngine.AddressableAssets;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.spacepuppy.Addressables
{

    [System.Serializable]
    public sealed class AssetReferenceScene : AssetReference, System.ICloneable
    {

        public AssetReferenceScene(string guid) : base(guid)
        {
            
        }



        public override bool ValidateAsset(Object obj)
        {
#if UNITY_EDITOR
            return obj is SceneAsset;
#else
            return base.ValidateAsset(obj);
#endif
        }

        public override bool ValidateAsset(string mainAssetPath)
        {
#if UNITY_EDITOR
            if (typeof(SceneAsset).IsAssignableFrom(AssetDatabase.GetMainAssetTypeAtPath(mainAssetPath)))
                return true;

            var repr = AssetDatabase.LoadAllAssetRepresentationsAtPath(mainAssetPath);
            return repr != null && repr.Any(o => o is SceneAsset);
#else
            return false;
#endif
        }

#if UNITY_EDITOR
        public new SceneAsset editorAsset
        {
            get
            {
                Object baseAsset = base.editorAsset;
                SceneAsset asset = baseAsset as SceneAsset;
                if (asset == null && baseAsset != null)
                    Debug.Log("editorAsset cannot cast to SceneAsset");
                return asset;
            }
        }
#endif

        #region ICloneable Interface

        object System.ICloneable.Clone() => this.MemberwiseClone();

        public AssetReferenceScene Clone() => this.MemberwiseClone() as AssetReferenceScene;

        #endregion

    }

}

#endif