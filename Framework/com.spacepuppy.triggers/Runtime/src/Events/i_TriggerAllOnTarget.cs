using UnityEngine;
using System.Collections.Generic;

namespace com.spacepuppy.Events
{

    [Infobox("While this appears redundant to i_Trigger, this in actuality allows for configuring the target more finely using the 'Find' command. Useful for finding targets in the scene or a parent.")]
    public class i_TriggerAllOnTarget : AutoTriggerable
    {

        #region Fields

        [SerializeField]
        private TriggerableTargetObject _target = new TriggerableTargetObject();

        [SerializeField]
        private bool _passAlongArg;

        #endregion

        #region Properties

        public TriggerableTargetObject Target => _target;

        public bool PassAlongArg
        {
            get => _passAlongArg;
            set => _passAlongArg = value;
        }

        #endregion

        #region Triggerable Interface

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            var targ = _target.GetTarget<UnityEngine.Object>(arg);
            if (!targ) return false;

            if (!_passAlongArg) arg = null;
            EventTriggerEvaluator.Current.TriggerAllOnTarget(targ, arg, this, arg); ;
            return true;
        }

        #endregion

    }

}
