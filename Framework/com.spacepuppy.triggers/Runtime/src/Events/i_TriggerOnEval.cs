using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Events
{
    public class i_TriggerOnEval : AutoTriggerable, IObservableTrigger
    {

        #region Fields

        [SerializeField]
        private VariantReference _value = new VariantReference();

        [SerializeField]
        private SPEvent _onTrue = new SPEvent("OnTrue");
        [SerializeField]
        private SPEvent _onFalse = new SPEvent("OnFalse");

        #endregion

        #region Properties

        public SPEvent OnTrue => _onTrue;

        public SPEvent OnFalse => _onFalse;

        #endregion

        #region Triggerable Interface

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            if (_value.BoolValue)
            {
                _onTrue.ActivateTrigger(this, null);
            }
            else
            {
                _onFalse.ActivateTrigger(this, null);
            }

            return true;
        }

        #endregion

        #region IObservableTrigger Interface

        BaseSPEvent[] IObservableTrigger.GetEvents()
        {
            return new BaseSPEvent[] { _onTrue, _onFalse };
        }

        #endregion

    }
}
