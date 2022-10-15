#if SP_UNITYIAP
using UnityEngine;
using UnityEngine.Purchasing;
using System.Collections.Generic;
using com.spacepuppy.Project;

namespace com.spacepuppy.IAP
{

    public interface IProductInfo
    {
        bool IsValid { get; }

        string ProductId { get; }

        Product Product { get; }

        ProductType ProductType { get; }

        bool Enabled { get; }

        bool Owned { get; }

        bool AvailableToPurchase { get; }

        string StoreSpecifiedProductId { get; }

        decimal LocalizedPrice { get; }

        string LocalizedPriceString { get; }

        string LocalizedTitle { get; }

        string LocalizedDescription { get; }

        string LocalizedIsoCurrencyCode { get; }
    }

    [System.Serializable]
    public class IProductInfoRef : SerializableInterfaceRef<IProductInfo>
    {

    }

    [System.Serializable]
    public struct ProductInfo : IProductInfo
    {

        public const string PROP_PRODUCTID = nameof(_productId);

        #region Fields

        [SerializeField]
        [IAPCatalogProductID()]
        private string _productId;

        [System.NonSerialized]
        private Product _product;

        #endregion

        #region CONSTRUCTOR

        public ProductInfo(Product product)
        {
            _productId = product?.definition?.id;
            _product = product;
        }

        public ProductInfo(string productid)
        {
            _productId = productid;
            _product = Services.Get<IIAPManager>()?.FindProduct(productid);
        }

        #endregion

        #region Properties

        public bool IsValid => this.Product != null;

        public string ProductId => _productId;

        public Product Product => _product ?? (_product = string.IsNullOrEmpty(_productId) ? null : Services.Get<IIAPManager>()?.FindProduct(_productId));

        public ProductType ProductType => this.Product?.definition?.type ?? ProductType.Consumable;

        public bool Enabled => this.Product?.definition?.enabled ?? false;

        public bool Owned => Services.Get<IIAPManager>()?.OwnsProduct(_productId) ?? false;

        public bool AvailableToPurchase => this.Product?.availableToPurchase ?? false;

        public string StoreSpecifiedProductId => this.Product?.definition?.storeSpecificId ?? string.Empty;

        public decimal LocalizedPrice => this.Product?.metadata?.localizedPrice ?? 0m;

        public string LocalizedPriceString => this.Product?.metadata?.localizedPriceString ?? string.Empty;

        public string LocalizedTitle => this.Product?.metadata?.localizedTitle ?? string.Empty;

        public string LocalizedDescription => this.Product?.metadata?.localizedDescription ?? string.Empty;

        public string LocalizedIsoCurrencyCode => this.Product?.metadata?.isoCurrencyCode ?? string.Empty;

        public string LocalizedCurrencySymbol => this.Product?.metadata?.GetCurrencySymbol() ?? string.Empty;

        #endregion

        #region Methods

        /// <summary>
        /// Clears the cached Product allowing it to be resynced with the IIAPManager next time a property is read.
        /// </summary>
        public void SetDirty()
        {
            _product = null;
        }

        #endregion

    }

}

#endif
