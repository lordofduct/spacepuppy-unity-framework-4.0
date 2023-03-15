using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace com.spacepuppy.Mecanim
{

    public interface ISubStateBridgeMessageHandler
    {

        void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex);
        void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex);

    }

    /// <summary>
    /// This requires a SPAnimatorOverrideLayers attached with the Animator and overrides to always be funneled through it.
    /// </summary>
    public interface IAnimatorOverrideLayerHandler
    {
        void OnOverrideApplied(SPAnimatorOverrideLayers layers);
    }

    /// <summary>
    /// This requires a SPAnimatorEventProcessor attached with the Animator.
    /// </summary>
    public interface IAnimationEventHandler
    {
        void OnAnimationEvent(AnimationEventMessage ev);
    }

}
