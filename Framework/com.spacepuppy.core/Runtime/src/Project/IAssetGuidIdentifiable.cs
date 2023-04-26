using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Utils;

namespace com.spacepuppy.Project
{

    public interface IAssetGuidIdentifiable
    {
        System.Guid AssetId { get; }
    }

    public class AssetGuidIdentifiableEqualityComparer<T> : IEqualityComparer<T> where T : IAssetGuidIdentifiable
    {

        public static readonly AssetGuidIdentifiableEqualityComparer<T> Default = new AssetGuidIdentifiableEqualityComparer<T>();

        public bool Equals(T x, T y)
        {
            bool xnull = object.ReferenceEquals(x, null);
            bool ynull = object.ReferenceEquals(y, null);
            if (xnull && ynull) return true;
            if (xnull) return y is UnityEngine.Object oy ? !ObjUtil.IsObjectAlive(oy) : false;
            if (ynull) return x is UnityEngine.Object ox ? !ObjUtil.IsObjectAlive(ox) : false;

            return x.AssetId == y.AssetId;
        }

        public int GetHashCode(T obj)
        {
            return obj != null ? obj.AssetId.GetHashCode() : 0;
        }
    }

    public class AssetGuidIdentifiableEqualityComparer : AssetGuidIdentifiableEqualityComparer<IAssetGuidIdentifiable>
    {

        public new static readonly AssetGuidIdentifiableEqualityComparer Default = new AssetGuidIdentifiableEqualityComparer();

    }

}
