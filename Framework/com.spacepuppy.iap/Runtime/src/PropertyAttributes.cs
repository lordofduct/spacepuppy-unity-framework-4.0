#if SP_UNITYIAP
using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Utils;
using com.spacepuppy.IAP;

namespace com.spacepuppy
{

    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
    public class IAPCatalogProductIDAttribute : PropertyAttribute
    {

        public ProductTypeMask ProductTypes = ProductTypeMask.All;

        public IAPCatalogProductIDAttribute() { }

        public IAPCatalogProductIDAttribute(ProductTypeMask productTypes)
        {
            ProductTypes = productTypes;
        }

    }

}
#endif
