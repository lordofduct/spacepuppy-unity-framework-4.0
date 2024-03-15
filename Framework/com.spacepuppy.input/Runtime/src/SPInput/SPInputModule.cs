using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

using com.spacepuppy.Utils;

namespace com.spacepuppy.SPInput
{

    public interface ISPInputModule
    {
        BaseInputModule Module { get; }
        IInputDevice GetMainInputDevice();
    }

    public abstract class SPInputModule<TInput> : StandaloneInputModule, ISPInputModule where TInput : SPInputModule.SPInput
    {

        #region Fields

        [SerializeField]
        private string _mainInputDeviceId;

        #endregion

        #region CONSTRUCTOR

        protected override void Awake()
        {
            base.Awake();

            this.m_InputOverride = this.AddOrGetComponent<TInput>();
        }

        #endregion

        #region Properties

        public string MainInputDeviceId
        {
            get { return _mainInputDeviceId; }
            set { _mainInputDeviceId = value; }
        }

        #endregion

        #region Methods

        public virtual IInputDevice GetMainInputDevice()
        {
            if (string.IsNullOrEmpty(_mainInputDeviceId))
                return Services.Get<IInputManager>()?.Main;
            else
                return Services.Get<IInputManager>()?.GetDevice(_mainInputDeviceId);
        }

        public virtual T GetMainInputDevice<T>() where T : class, IInputDevice => this.GetMainInputDevice() as T;

        #endregion

        #region ISPInputModule Interface

        BaseInputModule ISPInputModule.Module => this;

        #endregion

    }

    public class SPInputModule : SPInputModule<SPInputModule.SPInput>
    {

        #region Special Types

        public class SPInput : BaseInput
        {

            #region Fields

            private ISPInputModule _module;

            #endregion

            #region CONSTRUCTOR

            #endregion

            #region Properties

            public ISPInputModule Module => _module != null ? _module : (_module = this.GetComponent<ISPInputModule>());

            #endregion

            #region Overrides

            public override float GetAxisRaw(string axisName)
            {
                var input = this.Module?.GetMainInputDevice();
                if (input?.Contains(axisName) ?? false)
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
                var input = this.Module?.GetMainInputDevice();
                if (input?.Contains(buttonName) ?? false)
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
