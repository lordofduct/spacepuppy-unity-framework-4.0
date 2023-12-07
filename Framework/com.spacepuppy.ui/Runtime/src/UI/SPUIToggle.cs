using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using com.spacepuppy.Events;

namespace com.spacepuppy.UI
{

    public class SPUIToggle : Toggle, IObservableTrigger
    {

        #region Fields

        [SerializeField]
        private SPEvent _onToggleIsOn = new SPEvent("OnToggleIsOn");

        [SerializeField]
        private SPEvent _onToggleIsOff = new SPEvent("OnToggleIsOff");

        #endregion

        #region CONSTRUCTOR

        protected override void Awake()
        {
            base.Awake();

            this.onValueChanged.AddListener(this_onValueChanged);
        }

        #endregion

        #region Properties

        public SPEvent OnToggleIsOn => _onToggleIsOn;

        public SPEvent OnToggleIsOff => _onToggleIsOff;

        #endregion

        #region Methods

        void this_onValueChanged(bool value)
        {
            if (value)
            {
                _onToggleIsOn.ActivateTrigger(this, null);
            }
            else
            {
                _onToggleIsOff.ActivateTrigger(this, null);
            }
        }

        #endregion

        #region IObservableTrigger Interface

        BaseSPEvent[] IObservableTrigger.GetEvents() => new BaseSPEvent[] { _onToggleIsOn, _onToggleIsOff };

        #endregion

    }

}
