#if SP_UNITYIAP
using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Utils;
using com.spacepuppy.Events;

namespace com.spacepuppy.IAP.Events
{

    public sealed class i_ConfirmIAPReceipt : AutoTriggerable
    {

        #region Fields

        [SerializeField]
        [IAPCatalogProductID(ProductTypeMask.NonConsumable | ProductTypeMask.Subscription)]
        private string _productId;

        [SerializeField]
        [SPEvent.Config("product (Product)")]
        private SPEvent _valid = new SPEvent("Valid");
        [SerializeField]
        [SPEvent.Config("product (Product)")]
        private SPEvent _invalid = new SPEvent("Invalid");

        #endregion

        #region Properties

        public string ProductId
        {
            get => _productId;
            set => _productId = value;
        }

        public SPEvent Valid => _valid;

        public SPEvent Invalid => _invalid;

        #endregion

        #region Triggerable Interface

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            var prod = Services.Get<IIAPManager>()?.FindProduct(_productId);
            bool hasreceipt = prod?.hasReceipt ?? false;
            if (hasreceipt)
            {
                _valid.ActivateTrigger(this, prod);
            }
            else
            {
                _invalid.ActivateTrigger(this, prod);
            }

            return true;
        }

        #endregion

    }

}
#endif
