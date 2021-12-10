using UnityEngine;

namespace com.spacepuppy.Mecanim
{

    public enum StateMachineBehaviourUpdateTransitionState
    {
        Inactive = 0,
        Entering = 1,
        Active = 2,
        Exiting = 3
    }

    [System.Flags]
    public enum AnimatorControllerParameterTypeMask
    {
        Any = -1,
        Float = 1,
        Int = 2,
        Bool = 4,
        Trigger = 8,
    }

}
