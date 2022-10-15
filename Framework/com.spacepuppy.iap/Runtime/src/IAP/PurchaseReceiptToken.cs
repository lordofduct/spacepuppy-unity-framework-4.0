#if SP_UNITYIAP
using UnityEngine;
using UnityEngine.Purchasing;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Utils;

namespace com.spacepuppy.IAP
{

    [System.Serializable]
    public struct PurchaseReceiptToken
    {
        public string TransactionID;
        public string ProductID;
        public System.DateTime PurchaseDate;
    }

}
#endif
