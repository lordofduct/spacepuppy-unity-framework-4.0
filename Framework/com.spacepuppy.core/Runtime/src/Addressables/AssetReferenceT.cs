#if SP_ADDRESSABLES
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AddressableAssets;

namespace com.spacepuppy.Addressables
{

    [System.Serializable]
    public class AssetReferenceMaterial : AssetReferenceT<Material>
    {
        /// <summary>
        /// Constructs a new reference to a Material.
        /// </summary>
        /// <param name="guid">The object guid.</param>
        public AssetReferenceMaterial(string guid) : base(guid) { }
    }

}
#endif
