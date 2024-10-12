using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.spacepuppy.Anim.Legacy
{

    public interface IAnimControllerMask
    {

        bool CanPlay(IAnimatable anim);
        bool CanPlay(AnimationClip clip, AnimSettings settings);

    }

    [System.Serializable]
    public class AnimControllerMaskSerializedRef : com.spacepuppy.Project.InterfaceRef<IAnimControllerMask>
    {

    }

}
