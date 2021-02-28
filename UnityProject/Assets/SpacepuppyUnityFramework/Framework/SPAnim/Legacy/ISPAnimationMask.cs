#pragma warning disable 0649 // variable declared but not used.
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace com.spacepuppy.Anim.Legacy
{

    public interface ISPAnimationMask
    {

        void Apply(SPLegacyAnimController controller, AnimationState state);
        void Redact(SPLegacyAnimController controller, AnimationState state);

    }

    [System.Serializable]
    public sealed class SPAnimMaskSerializedRef : com.spacepuppy.Project.SerializableInterfaceRef<ISPAnimationMask>
    {

    }

}
