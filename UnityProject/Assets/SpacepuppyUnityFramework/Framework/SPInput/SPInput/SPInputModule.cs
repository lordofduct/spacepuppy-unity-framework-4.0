using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

using com.spacepuppy.Utils;

namespace com.spacepuppy.SPInput
{
    public class SPInputModule : StandaloneInputModule
    {

        #region Fields

        [SerializeField]
        private string _mainInputDeviceId;

        #endregion

        #region CONSTRUCTOR

        protected override void Awake()
        {
            base.Awake();

            var module = this.AddComponent<SPInput>();
            module.Module = this;
            this.m_InputOverride = module;
        }

        #endregion

        #region Properties

        public string MainInputDeviceId
        {
            get { return _mainInputDeviceId; }
            set { _mainInputDeviceId = value; }
        }

        #endregion

        public override void Process()
        {
            base.Process();

        }


        #region Special Types

        public class SPInput : BaseInput
        {

            #region Fields

            private SPInputModule _module;

            #endregion

            #region Properties

            public SPInputModule Module
            {
                get { return _module; }
                set { _module = value; }
            }

            #endregion

            #region Overrides

            public override float GetAxisRaw(string axisName)
            {
                var serv = Services.Get<IInputManager>();
                if (serv == null) return base.GetAxisRaw(axisName);

                var input = string.IsNullOrEmpty(_module?._mainInputDeviceId) ? serv.Main : serv.GetDevice(_module._mainInputDeviceId);
                if (input == null) return base.GetAxisRaw(axisName);

                if(input.Contains(axisName))
                {
                    return input.GetAxleState(axisName);
                }
                else
                {
                    return base.GetAxisRaw(axisName);
                }
            }

            public override bool GetButtonDown(string buttonName)
            {
                var serv = Services.Get<IInputManager>();
                if (serv == null) return base.GetButtonDown(buttonName);

                var input = string.IsNullOrEmpty(_module?._mainInputDeviceId) ? serv.Main : serv.GetDevice(_module._mainInputDeviceId);
                if (input == null) return base.GetButtonDown(buttonName);

                if (input.Contains(buttonName))
                {
                    return input.GetButtonState(buttonName) == spacepuppy.SPInput.ButtonState.Down;
                }
                else
                {
                    return base.GetButtonDown(buttonName);
                }
            }

            #endregion

        }

        #endregion

    }
}
