using UnityEngine;
using System.Collections.Generic;

namespace com.spacepuppy.Project
{
    public interface IAssetGuidIdentifiable
    {
        System.Guid AssetId { get; }
    }
}
