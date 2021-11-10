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

}
