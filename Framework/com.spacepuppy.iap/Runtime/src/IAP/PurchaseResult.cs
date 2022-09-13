#if SP_UNITYIAP
using UnityEngine;
using UnityEngine.Purchasing;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Utils;

namespace com.spacepuppy.IAP
{

    public struct PurchaseResult
    {

        public Product Product;
        public PurchaseFailureReason FailureReason;
        public bool Failed;

        public static PurchaseResult Success(Product prod)
        {
            return new PurchaseResult()
            {
                Product = prod,
            };
        }

        public static PurchaseResult Failure(Product prod, PurchaseFailureReason reason)
        {
            return new PurchaseResult()
            {
                Product = prod,
                FailureReason = reason,
                Failed = true,
            };
        }

    }

}
#endif
