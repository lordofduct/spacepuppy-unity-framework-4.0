using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using com.spacepuppy.UI;
using com.spacepuppy.Events;

namespace com.spacepuppy
{

    public sealed class t_UIOnSubmit : SPComponent, IMStartOrEnableReceiver, ISubmitHandler
    {

        #region Fields

        [SerializeField]
        private SPEvent _onSubmit = new("OnSubmit");

        [System.NonSerialized]
        private TextInputFieldTarget _inputField = new();
        [System.NonSerialized]
        private int _lastTriggeredFrame;

        #endregion

        #region CONSTRUCTOR

        void IMStartOrEnableReceiver.OnStartOrEnable()
        {
            _inputField.Target = this.gameObject;
            _inputField.OnSubmit += _inputField_OnSubmit;
        }


        protected override void OnDisable()
        {
            _inputField.OnSubmit -= _inputField_OnSubmit;
            base.OnDisable();
        }

        #endregion

        #region Properties

        public SPEvent OnSubmit => _onSubmit;

        #endregion

        #region Methods

        private void TrySignal()
        {
            int frame = Time.frameCount;
            if (frame != _lastTriggeredFrame)
            {
                _lastTriggeredFrame = frame;
                _onSubmit.ActivateTrigger(this, null);
            }
        }

        private void _inputField_OnSubmit(object sender, TempEventArgs e)
        {
            this.TrySignal();
        }

        #endregion

        #region ISubmitHandler Interface

        void ISubmitHandler.OnSubmit(BaseEventData eventData)
        {
            this.TrySignal();
        }

        #endregion

    }

}
