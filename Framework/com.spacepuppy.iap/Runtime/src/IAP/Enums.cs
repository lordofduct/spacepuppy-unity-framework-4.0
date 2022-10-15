#if SP_UNITYIAP
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Purchasing;

namespace com.spacepuppy.IAP
{

    public enum PurchaseStatus
    {
        Failed = -1,
        Complete = 0,
        Pending = 1,
    }

    [System.Flags]
    public enum ProductTypeMask
    {
        All = -1,
        Consumable = 1 << ProductType.Consumable,
        NonConsumable = 1 << ProductType.NonConsumable,
        Subscription = 1 << ProductType.Subscription
    }

}
#endif
