using UnityEngine;
using UnityEngine.Serialization;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Events;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Mecanim.Events
{

    public class i_PurgeAnimatorOverride : AutoTriggerable
    {

        #region Fields

        [SerializeField]
        [TriggerableTargetObject.Config(typeof(Animator))]
        private TriggerableTargetObject _target;

        [SerializeField]
        [Tooltip("The token used to identify the layer to purge.")]
        private string _token;

        #endregion

        #region Triggerable Interface

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            var targ = _target.GetTarget<Animator>(arg);
            if (targ == null) return false;

            targ.RemoveOverride(_token);
            return true;
        }

        #endregion

    }

}
