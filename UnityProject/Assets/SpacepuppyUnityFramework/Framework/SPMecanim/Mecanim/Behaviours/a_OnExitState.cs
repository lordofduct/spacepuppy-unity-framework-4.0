using UnityEngine;
using UnityEngine.Animations;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Mecanim;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Mecanim.Behaviours
{

    public class a_OnExitState : StateMachineBehaviour
    {

        [SerializeField]
        private SPAnimatorStateMachineEvent _onExit;

        //this one is called
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_onExit.HasReceivers) _onExit.ActivateTrigger(animator, null);
        }

    }

}
