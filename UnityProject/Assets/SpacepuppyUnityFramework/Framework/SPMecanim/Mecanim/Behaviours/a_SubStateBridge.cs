using UnityEngine;
using UnityEngine.Animations;
using System.Collections.Generic;

using com.spacepuppy.Utils;

namespace com.spacepuppy.Mecanim.Behaviours
{

    [Infobox("Attach this to a layer to allow all states of that layer to signal when they've entered/exited.\r\nYou can alternatively attach it to only a specific state to only signal on enter/exit of that specific state.\r\nThe Animator must have an IAnimatorStateMachineBridge setup and initialized.")]
    public sealed class a_SubStateBridge : BridgedStateMachineBehaviour
    {

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            this.Bridge?.SubStateBridgeContainer?.AddOrGetComponent<AnimatorSubStateBridgeContainer>()?.SignalEnterState(animator, stateInfo, layerIndex);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            this.Bridge?.SubStateBridgeContainer?.AddOrGetComponent<AnimatorSubStateBridgeContainer>()?.SignalExitState(animator, stateInfo, layerIndex);
        }

    }

}
