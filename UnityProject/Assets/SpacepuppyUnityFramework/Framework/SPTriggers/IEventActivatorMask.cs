#pragma warning disable 0649 // variable declared but not used.
using UnityEngine;
using System.Collections.Generic;
using com.spacepuppy.Utils;

namespace com.spacepuppy
{

    public interface IEventActivatorMask
    {

        bool Intersects(UnityEngine.Object obj);

    }

    [System.Serializable]
    public sealed class EventActivatorMaskRef : Project.SerializableInterfaceRef<IEventActivatorMask>
    {

    }

}
