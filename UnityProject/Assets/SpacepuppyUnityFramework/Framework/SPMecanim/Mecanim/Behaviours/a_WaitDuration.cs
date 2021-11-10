using UnityEngine;
using UnityEngine.Animations;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Geom;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Mecanim.Behaviours
{

    public class a_WaitDuration : ActiveBridgedStateMachineBehaviour
    {

        #region Fields

        [SerializeField]
        private Interval _duration;
        [Space]
        [SerializeField]
        private SPAnimatorStateMachineEvent _onComplete;

        private float _currentT = float.PositiveInfinity;

        #endregion

        protected override void OnEnter(AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
        {
            _currentT = RandomUtil.Standard.Range(_duration.Max, _duration.Min);
        }

        protected override void OnUpdate(AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
        {
            _currentT -= Time.deltaTime;
            if (_currentT <= 0f)
            {
                _currentT = float.PositiveInfinity;
                if (_onComplete.HasReceivers) _onComplete.ActivateTrigger(this, null);
            }
        }

    }

}