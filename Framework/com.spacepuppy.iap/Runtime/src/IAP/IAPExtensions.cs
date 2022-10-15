#if SP_UNITYIAP
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Purchasing;
using System.Linq;
using System.Globalization;

namespace com.spacepuppy.IAP
{

    public static class IAPExtensions
    {

        public static ProductTypeMask ToMask(this ProductType ptp)
        {
            return (ProductTypeMask)(1 << (int)ptp);
        }

        public static bool Intersects(this ProductType ptp, ProductTypeMask mask)
        {
            return (ptp.ToMask() & mask) != 0;
        }

        public static bool Intersects(this ProductTypeMask maskA, ProductTypeMask maskB)
        {
            return (maskA & maskB) != 0;
        }

        public static PurchaseReceiptToken CreateToken(this UnityEngine.Purchasing.Security.IPurchaseReceipt receipt)
        {
            if (receipt == null) return default;

            return new PurchaseReceiptToken()
            {
                TransactionID = receipt.transactionID,
                ProductID = receipt.productID,
                PurchaseDate = receipt.purchaseDate.ToUniversalTime(),
            };
        }

        public static ProductInfo AsProductInfo(this UnityEngine.Purchasing.Product product)
        {
            return new ProductInfo(product);
        }

        private static Dictionary<string, System.ValueTuple<CultureInfo, RegionInfo>> _codesToCultureInfo;
        private static System.ValueTuple<CultureInfo, RegionInfo> GetCultureInfoPair(string isocode)
        {
            if (string.IsNullOrEmpty(isocode)) return default;

            System.ValueTuple<CultureInfo, RegionInfo> pair = default;
            if (_codesToCultureInfo != null && _codesToCultureInfo.TryGetValue(isocode, out pair))
            {
                return pair;
            }

            if (string.Equals(System.Globalization.RegionInfo.CurrentRegion?.ISOCurrencySymbol, isocode, System.StringComparison.OrdinalIgnoreCase))
            {
                return new(System.Globalization.CultureInfo.CurrentCulture, System.Globalization.RegionInfo.CurrentRegion); 
            }

            if (_codesToCultureInfo == null)
            {
                _codesToCultureInfo = new Dictionary<string, System.ValueTuple<CultureInfo, RegionInfo>>(System.StringComparer.OrdinalIgnoreCase);

                var pairs = System.Globalization.CultureInfo
                         .GetCultures(System.Globalization.CultureTypes.AllCultures)
                         .Where(c => !c.IsNeutralCulture)
                         .Select(c =>
                         {
                             RegionInfo ri;
                             try
                             {
                                 ri = new RegionInfo(c.Name);
                             }
                             catch
                             {
                                 ri = null;
                             }
                             return new System.ValueTuple<CultureInfo, RegionInfo>(c, ri);
                         })
                         .Where(p => p.Item2 != null);
                foreach (var p in pairs)
                {
                    _codesToCultureInfo[p.Item2.ISOCurrencySymbol] = p;
                }

                if (_codesToCultureInfo.TryGetValue(isocode, out pair))
                {
                    return pair;
                }
            }
            return pair;
        }


        public static string GetCurrencySymbol(string isocode)
        {
            if (string.IsNullOrEmpty(isocode)) return string.Empty;

            var pair = GetCultureInfoPair(isocode);
            return pair.Item2?.ISOCurrencySymbol ?? string.Empty;
        }

        public static string GetCurrencySymbol(this ProductMetadata metadata)
        {
            return metadata != null ? GetCurrencySymbol(metadata.isoCurrencyCode) : string.Empty;
        }

        public static CultureInfo GetEstimatedCultureInfoForCurrencyCode(string isocode) => GetCultureInfoPair(isocode).Item1;

        public static CultureInfo GetEstimatedCultureInfoForCurrencyCode(this ProductMetadata metadata) => GetCultureInfoPair(metadata?.isoCurrencyCode).Item1;

    }

}
#endif
