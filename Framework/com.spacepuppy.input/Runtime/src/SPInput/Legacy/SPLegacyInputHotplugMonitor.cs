using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy.Utils;
using com.spacepuppy.Project;

namespace com.spacepuppy.SPInput.Legacy
{

    public class SPLegacyInputHotplugMonitor : SPComponent
    {

        public event System.EventHandler JoystickHotplugged;

        #region Fields

        [SerializeField, DefaultFromSelf, Tooltip("Optional - InputManager associated with this hotplug monitor.")]
        private InterfaceRef<IInputManager> _inputManager = new();

        [SerializeField]
        private float _joystickHotplugPollingFrequency = 1f;

        [SerializeField, Tooltip("If true, and InputManager exists, this will signal the IJoystickHotpluggedGlobalHandler message globally.")]
        private bool _signalHotplugGlobally;

        [System.NonSerialized]
        private string[] _hotplugJoystickNames;
        [System.NonSerialized]
        private double _lastHotplugTest;

        #endregion

        #region CONSTRUCTOR

        protected override void Start()
        {
            _hotplugJoystickNames = Input.GetJoystickNames();
            _lastHotplugTest = Time.unscaledTimeAsDouble;

            base.Start();
        }

        #endregion

        #region Properties

        public IInputManager InputManager
        {
            get => _inputManager.Value;
            set => _inputManager.Value = value;
        }

        public float JoystickHotplugPollingFrequency
        {
            get => _joystickHotplugPollingFrequency;
            set => _joystickHotplugPollingFrequency = value;
        }

        public bool SignalHotplugGlobally
        {
            get => _signalHotplugGlobally;
            set => _signalHotplugGlobally = value;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Retrieves the joystick names array.
        /// </summary>
        /// <remarks>note that this is a cached if polling is active and modifying the collection may cause JoystickHotplugged event to raise</remarks>
        /// <returns></returns>
        public string[] GetJoystickNames() => _hotplugJoystickNames;

        protected virtual void Update()
        {
            if ((Time.unscaledTimeAsDouble - _lastHotplugTest) >= _joystickHotplugPollingFrequency)
            {
                var names = Input.GetJoystickNames();
                _lastHotplugTest = Time.unscaledTimeAsDouble;
                if (_hotplugJoystickNames == null || _hotplugJoystickNames.Length != names.Length || !_hotplugJoystickNames.SequenceEqual(names, System.StringComparer.Ordinal))
                {
                    _hotplugJoystickNames = names;
                    this.OnJoystickHotplugged();
                }
            }
        }

        protected virtual void OnJoystickHotplugged()
        {
            this.JoystickHotplugged?.Invoke(this, System.EventArgs.Empty);

            if (_signalHotplugGlobally)
            {
                var manager = _inputManager.Value;
                if (manager.IsAlive())
                {
                    Messaging.Broadcast<IJoystickHotpluggedGlobalHandler, IInputManager>(manager, (o, a) => o.OnJoystickHotplugged(a));
                }
            }
        }

        #endregion

    }

}
