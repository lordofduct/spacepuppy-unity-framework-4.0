using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Utils;

namespace com.spacepuppy.UI
{
    public interface IUIComponent : IComponent
    {
        new RectTransform transform { get; }
    }
}
