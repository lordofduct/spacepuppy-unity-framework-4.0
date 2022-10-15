#if SP_UNITYIAP
using UnityEngine;
using UnityEngine.Purchasing;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Utils;
using com.spacepuppy.Events;

#if SP_UNITASK
using Cysharp.Threading.Tasks;
#endif

namespace com.spacepuppy.IAP.Events
{
    public class i_PerformIAPPurchase : Triggerable
    {

#if UNITY_EDITOR
        public const string PROP_PRODUCTID = nameof(_productId);
        public const string PROP_ACTIONTYPE = nameof(_actionType);
        public const string PROP_CONSUMEPURCHASE = nameof(_consumePurchase);
        public const string PROP_ONSUCCESS = nameof(_onSuccess);
        public const string PROP_ONFAILURE = nameof(_onFailure);
#endif

        public enum ActionTypes
        {
            /// <summary>
            /// This button will display localized product title and price. Clicking will trigger a purchase.
            /// </summary>
            Purchase,
            /// <summary>
            /// This button will display a static string for restoring previously purchased non-consumable
            /// and subscriptions. Clicking will trigger this restoration process, on supported app stores.
            /// </summary>
            Restore
        }

        #region Fields

        [SerializeField]
        [IAPCatalogProductID]
        private string _productId;

        [SerializeField]
        private ActionTypes _actionType;

        [SerializeField]
        private bool _consumePurchase = true;

        [SerializeField]
        [SPEvent.Config("product (Product)")]
        private SPEvent _onSuccess = new SPEvent("OnSuccess");

        [SerializeField]
        [SPEvent.Config("product (Product)")]
        private SPEvent _onPending = new SPEvent("OnPending");

        [SerializeField]
        [SPEvent.Config("{product (Product), reason (PurchaseFailureReason)}?")]
        private SPEvent _onFailure = new SPEvent("OnFailure");

        #endregion

        #region Properties

        public string ProductId
        {
            get => _productId;
            set => _productId = value;
        }

        public ActionTypes ActionType
        {
            get => _actionType;
            set => _actionType = value;
        }

        public SPEvent OnSuccess => _onSuccess;

        public SPEvent OnFailure => _onFailure;

        #endregion


        #region Triggerable Interface

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            var iap = Services.Get<IIAPManager>();
            if (iap == null)
            {
                if (_onFailure.HasReceivers) _onFailure.ActivateTrigger(this, PurchaseResult.Failure(null, PurchaseFailureReason.PurchasingUnavailable));
                return false;
            }

            _ = this.DoTrigger(iap);
            return true;
        }

#if SP_UNITASK
        private async UniTaskVoid DoTrigger(IIAPManager iap)
#else
        private async System.Threading.Tasks.Task DoTrigger(IIAPManager iap)
#endif
        {

            switch (_actionType)
            {
                case ActionTypes.Purchase:
                    {
                        var result = await iap.PurchaseAsync(_productId, _consumePurchase);
                        if (result.Failed)
                        {
                            if (_onFailure.HasReceivers) _onFailure.ActivateTrigger(this, result);
                        }
                        else
                        {
                            if (_onSuccess.HasReceivers) _onSuccess.ActivateTrigger(this, result.Product);
                        }
                    }
                    break;
                case ActionTypes.Restore:
                    {
                        var result = await iap.RestorePurchasesAsync();
                        if (result)
                        {
                            if (_onSuccess.HasReceivers) _onSuccess.ActivateTrigger(this, null);
                        }
                        else
                        {
                            if (_onFailure.HasReceivers) _onFailure.ActivateTrigger(this, null);
                        }
                    }
                    break;
            }

        }

#endregion
    }
}
#endif
