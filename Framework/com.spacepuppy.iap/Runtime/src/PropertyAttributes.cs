#if SP_UNITYIAP
using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Utils;

namespace com.spacepuppy
{

    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
    public class IAPCatalogProductIDAttribute : PropertyAttribute
    {

    }

}
#endif
