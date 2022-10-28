using UnityEngine;
using UnityEngine.Serialization;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Events;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Mecanim.Events
{

    [Infobox("If attempting to use 'PurgeAnimatorOverride' mode, it will only be supported if there is a properly configured animator bridge on the target entity as well.", MessageType = InfoBoxMessageType.Warning)]
    public sealed class i_SetAnimatorParameter : AutoTriggerable
    {

        #region Fields

        [SerializeField]
        [TriggerableTargetObject.Config(typeof(Animator))]
        private TriggerableTargetObject _targetAnimator = new TriggerableTargetObject();

        [SerializeField]
        private SPAnimatorStateMachineEvent _parameters = new SPAnimatorStateMachineEvent();

        #endregion

        #region Properties

        public TriggerableTargetObject TargetAnimator => _targetAnimator;

        public SPAnimatorStateMachineEvent Parameters => _parameters;

        #endregion

        #region Triggerable Interface

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            var targ = _targetAnimator.GetTarget<Animator>(arg);
            if (targ == null) return false;

            _parameters.ActivateTrigger(targ, null);
            return true;
        }

        #endregion

    }

}
