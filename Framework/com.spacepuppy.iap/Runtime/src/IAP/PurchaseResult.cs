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
        public PurchaseStatus Status;
        public PurchaseFailureReason FailureReason;
        public string Message;

        public bool Failed => Status < PurchaseStatus.Complete;

        public bool IsPending => Status > PurchaseStatus.Complete;

        public static PurchaseResult Success(Product prod)
        {
            return new PurchaseResult()
            {
                Product = prod,
                Status = PurchaseStatus.Complete,
            };
        }

        public static PurchaseResult Pending(Product prod)
        {
            return new PurchaseResult()
            {
                Product = prod,
                Status = PurchaseStatus.Pending,
            };
        }

        public static PurchaseResult Failure(Product prod, PurchaseFailureReason reason)
        {
            return new PurchaseResult()
            {
                Product = prod,
                Status = PurchaseStatus.Failed,
                FailureReason = reason,
                Message = string.Empty,
            };
        }

        public static PurchaseResult Failure(Product prod, PurchaseFailureReason reason, string message)
        {
            return new PurchaseResult()
            {
                Product = prod,
                Status = PurchaseStatus.Failed,
                FailureReason = reason,
                Message = message ?? string.Empty,
            };
        }

        public static PurchaseResult Failure(Product prod, UnityEngine.Purchasing.Extension.PurchaseFailureDescription failureDescription)
        {
            return new PurchaseResult()
            {
                Product = prod,
                Status = PurchaseStatus.Failed,
                FailureReason = failureDescription?.reason ?? PurchaseFailureReason.Unknown,
                Message = failureDescription?.message ?? string.Empty,
            };
        }

    }

}
#endif
