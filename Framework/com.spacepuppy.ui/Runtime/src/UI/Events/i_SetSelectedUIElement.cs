using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using com.spacepuppy.Events;
using com.spacepuppy.Utils;

namespace com.spacepuppy.UI.Events
{

    public class i_SetSelectedUIElement : AutoTriggerable
    {

        #region Fields

        [SerializeField]
        private TriggerableTargetObject _target = new TriggerableTargetObject();

        #endregion

        #region Triggerable Interface

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            return UIEventUtil.SetSelectedGameObject(_target.GetTarget<GameObject>(arg));
        }

        #endregion

    }

}
