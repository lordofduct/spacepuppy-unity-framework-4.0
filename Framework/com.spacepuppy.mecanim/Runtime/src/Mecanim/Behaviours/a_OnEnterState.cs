using UnityEngine;
using UnityEngine.Animations;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Mecanim;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Mecanim.Behaviours
{

    public class a_OnEnterState : StateMachineBehaviour
    {

        [SerializeField]
        private SPAnimatorStateMachineEvent _onEnter;

        //this one is called
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_onEnter.HasReceivers) _onEnter.ActivateTrigger(animator, null);
        }

    }

}
