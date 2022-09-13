#if SP_UNITYIAP
using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Utils;
using com.spacepuppy.Events;

namespace com.spacepuppy.IAP.Events
{

    public sealed class i_ConfirmIAPReceiptStateMachine : AutoTriggerable, IMStartOrEnableReceiver
    {

        #region Fields

        [SerializeField]
        private bool _forwardTrigger;
        [SerializeField]
        [IAPCatalogProductID]
        private string _productId;

        [SerializeField]
        private GameObject _validState;
        [SerializeField]
        private GameObject _invalidState;

        #endregion

        #region CONSTRUCTOR

        void IMStartOrEnableReceiver.OnStartOrEnable()
        {
            this.Sync();
        }

        #endregion

        #region Properties

        public bool ForwardTrigger
        {
            get => _forwardTrigger;
            set => _forwardTrigger = value;
        }

        public string ProductId
        {
            get => _productId;
            set => _productId = value;
        }

        public GameObject ValidState
        {
            get => _validState;
            set => _validState = value;
        }

        public GameObject InvalidState
        {
            get => _invalidState;
            set => _invalidState = value;
        }

        #endregion

        #region Methods

        public void Sync()
        {
            bool hasreceipt = Services.Get<IIAPManager>()?.FindProduct(_productId)?.hasReceipt ?? false;
            if (_validState) _validState.SetActive(hasreceipt);
            if (_invalidState) _invalidState.SetActive(!hasreceipt);
        }

        #endregion

        #region Triggerable Interface

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            if (_forwardTrigger)
            {
                bool hasreceipt = Services.Get<IIAPManager>()?.FindProduct(_productId)?.hasReceipt ?? false;
                if (hasreceipt && _validState) EventTriggerEvaluator.Current.TriggerAllOnTarget(_validState, arg, sender, arg);
                else if (!hasreceipt && _invalidState) EventTriggerEvaluator.Current.TriggerAllOnTarget(_invalidState, arg, sender, arg);
            }

            return true;
        }

        #endregion

    }
}
#endif
