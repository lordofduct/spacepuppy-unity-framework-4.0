#if SP_UNITYIAP
using UnityEngine;
using UnityEngine.Purchasing;
using System.Collections.Generic;

namespace com.spacepuppy.IAP
{

    [CreateAssetMenu(fileName = "ProductInfo", menuName = "Spacepuppy/IAP/ProductInfo")]
    public class ProductInfoAsset : ScriptableObject, IProductInfo
    {

        #region Fields

        [SerializeField]
        [DisplayFlat]
        private ProductInfo _product;

        #endregion

        #region CONSTRUCTOR

        #endregion

        #region IProductInfo Interface

        public bool IsValid => _product.IsValid;

        public string ProductId => _product.ProductId;

        public Product Product => _product.Product;

        [ShowNonSerializedProperty(nameof(Enabled), ShowAtEditorTime = true)]
        public bool Enabled => _product.Enabled;

        [ShowNonSerializedProperty(nameof(Owned), ShowAtEditorTime = true)]
        public bool Owned => _product.Owned;

        [ShowNonSerializedProperty(nameof(AvailableToPurchase), ShowAtEditorTime = true)]
        public bool AvailableToPurchase => _product.AvailableToPurchase;

        [ShowNonSerializedProperty(nameof(ProductType), ShowAtEditorTime = true)]
        public ProductType ProductType => _product.ProductType;

        [ShowNonSerializedProperty(nameof(StoreSpecifiedProductId), ShowAtEditorTime = true)]
        public string StoreSpecifiedProductId => _product.StoreSpecifiedProductId;

        [ShowNonSerializedProperty(nameof(LocalizedPrice), ShowAtEditorTime = true)]
        public decimal LocalizedPrice => _product.LocalizedPrice;

        [ShowNonSerializedProperty(nameof(LocalizedPriceString), ShowAtEditorTime = true)]
        public string LocalizedPriceString => _product.LocalizedPriceString;

        [ShowNonSerializedProperty(nameof(LocalizedTitle), ShowAtEditorTime = true)]
        public string LocalizedTitle => _product.LocalizedTitle;

        [ShowNonSerializedProperty(nameof(LocalizedDescription), ShowAtEditorTime = true)]
        public string LocalizedDescription => _product.LocalizedDescription;

        [ShowNonSerializedProperty(nameof(LocalizedIsoCurrencyCode), ShowAtEditorTime = true)]
        public string LocalizedIsoCurrencyCode => _product.LocalizedIsoCurrencyCode;

        [ShowNonSerializedProperty(nameof(LocalizedCurrencySymbol), ShowAtEditorTime = true)]
        public string LocalizedCurrencySymbol => _product.LocalizedCurrencySymbol;

        #endregion

    }

}

#endif
