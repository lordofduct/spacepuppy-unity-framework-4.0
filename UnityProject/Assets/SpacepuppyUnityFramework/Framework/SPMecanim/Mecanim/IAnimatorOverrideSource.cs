using UnityEngine;
using System.Collections.Generic;

namespace com.spacepuppy
{
    public interface IAnimatorOverrideSource
    {

        int GetOverrides(IList<KeyValuePair<AnimationClip, AnimationClip>> lst);

    }
}
