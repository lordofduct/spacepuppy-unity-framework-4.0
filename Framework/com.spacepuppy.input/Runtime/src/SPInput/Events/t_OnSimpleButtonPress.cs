#pragma warning disable 0649 // variable declared but not used.

using UnityEngine;

using com.spacepuppy.Events;

namespace com.spacepuppy.SPInput.Events
{

    public sealed class t_OnSimpleButtonPress : SPComponent, IObservableTrigger
    {

        #region Fields

        [SerializeField]
        [Tooltip("Leave blank to use main input device.")]
        private string _deviceId;

        [SerializeField]
        [DisableOnPlay]
        [InputID]
        private string _inputId;

        [SerializeField()]
        [UnityEngine.Serialization.FormerlySerializedAs("_trigger")]
        private SPEvent _onSimpleButtonPress = new SPEvent("OnSimpleButtonPress");

        #endregion

        #region CONSTRUCTOR

        #endregion

        #region Properties

        public string DeviceId
        {
            get
            {
                return _deviceId;
            }
            set
            {
                _deviceId = value;
            }
        }

        public string InputId
        {
            get
            {
                return _inputId;
            }
            set
            {
                _inputId = value;
            }
        }

        public SPEvent OnSimpleButtonPress => _onSimpleButtonPress;

        #endregion

        #region Methods

        private void Update()
        {
            var service = Services.Get<IInputManager>();
            if(service != null)
            {
                var input = string.IsNullOrEmpty(_deviceId) ? service.Main : service.GetDevice(_deviceId);
                if (input == null) return;

                if (input.GetButtonState(_inputId) == ButtonState.Down)
                {
                    _onSimpleButtonPress.ActivateTrigger(this, null);
                }
            }
            else
            {
                if (Input.GetButtonDown(_inputId))
                {
                    _onSimpleButtonPress.ActivateTrigger(this, null);
                }
            }
        }

        #endregion

        #region IObservableTrigger Interface

        BaseSPEvent[] IObservableTrigger.GetEvents()
        {
            return new BaseSPEvent[] { _onSimpleButtonPress };
        }

        #endregion

    }

}
