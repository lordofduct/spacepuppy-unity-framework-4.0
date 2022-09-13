#if SP_UNITYIAP
using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Async;
using com.spacepuppy.Utils;
using UnityEngine.Purchasing;

namespace com.spacepuppy.IAP
{

    public interface IIAPManager : IService
    {

        Product FindProduct(string productId);

        AsyncWaitHandle<PurchaseResult> PurchaseAsync(string productId, bool consume);
        AsyncWaitHandle<bool> RestorePurchasesAsync();

    }

}
#endif