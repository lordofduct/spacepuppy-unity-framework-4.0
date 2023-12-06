using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using com.spacepuppy.Utils;

namespace com.spacepuppy.SPInput
{

    [DefaultExecutionOrder(SPInputManager.DEFAULT_EXECUTION_ORDER)]
    public class SPInputManager : ServiceComponent<IInputManager>, IInputManager
    {
        public const int DEFAULT_EXECUTION_ORDER = -31990;

        #region Fields

        private Dictionary<string, IInputDevice> _dict = new Dictionary<string, IInputDevice>();
        private IInputDevice _default_main;
        private IInputDevice _override_main;

        [SerializeField]
        private bool _monitorJoystickHotplug;
        [SerializeField]
        private float _joystickHotplugPollingFrequency = 1f;

        [System.NonSerialized]
        private string[] _hotplugJoystickNames;
        [System.NonSerialized]
        private double _lastHotplugTest;

        #endregion

        #region CONSTRUCTOR

        public SPInputManager()
        {

        }

        public SPInputManager(Services.AutoRegisterOption autoRegister, Services.MultipleServiceResolutionOption multipleServiceResolution, Services.UnregisterResolutionOption unregisterResolution)
            : base(autoRegister, multipleServiceResolution, unregisterResolution)
        {

        }

        protected override void OnValidAwake()
        {
            base.OnValidAwake();

            if (_monitorJoystickHotplug)
            {
                _hotplugJoystickNames = Input.GetJoystickNames();
                _lastHotplugTest = Time.unscaledTimeAsDouble;
            }
            else
            {
                _hotplugJoystickNames = null;
                _lastHotplugTest = double.NegativeInfinity;
            }
        }

        #endregion

        #region Properties

        public bool MonitorJoystickHotplug
        {
            get => _monitorJoystickHotplug;
            set => _monitorJoystickHotplug = value;
        }

        public float JoystickHotplugPollingFrequency
        {
            get => _joystickHotplugPollingFrequency;
            set => _joystickHotplugPollingFrequency = value;
        }

        #endregion

        #region Methods

        protected virtual void FixedUpdate()
        {
            var e = _dict.GetEnumerator();
            while (e.MoveNext())
            {
                if (e.Current.Value.Active) e.Current.Value.FixedUpdate();
            }
        }

        /// <summary>
        /// Call once per frame
        /// </summary>
        protected virtual void Update()
        {
            if (_monitorJoystickHotplug && (Time.unscaledTimeAsDouble - _lastHotplugTest) >= _joystickHotplugPollingFrequency)
            {
                var names = Input.GetJoystickNames();
                _lastHotplugTest = Time.unscaledTimeAsDouble;
                if (_hotplugJoystickNames == null || _hotplugJoystickNames.Length != names.Length || !_hotplugJoystickNames.SequenceEqual(names, System.StringComparer.Ordinal))
                {
                    _hotplugJoystickNames = names;
                    this.OnJoystickHotplugged();
                }
            }

            var e = _dict.GetEnumerator();
            while (e.MoveNext())
            {
                if (e.Current.Value.Active) e.Current.Value.Update();
            }
        }

        /// <summary>
        /// Retrieves the joystick names array.
        /// </summary>
        /// <remarks>note that this is a cached if polling is active and modifying the collection may cause JoystickHotplugged event to raise</remarks>
        /// <returns></returns>
        protected string[] GetJoystickNames() => _hotplugJoystickNames ?? Input.GetJoystickNames();

        protected virtual void OnJoystickHotplugged()
        {
            Messaging.Broadcast<IJoystickHotpluggedGlobalHandler, IInputManager>(this, (o, a) => o.OnJoystickHotplugged(a));
        }

        #endregion

        #region IInputManager Interface

        public int Count { get { return _dict.Count; } }

        public IInputDevice this[string id]
        {
            get
            {
                return this.GetDevice(id);
            }
        }

        public IInputDevice Main
        {
            get
            {
                if (_override_main != null) return _override_main;
                if (_default_main == null)
                {
                    var e = _dict.GetEnumerator();
                    while (e.MoveNext())
                    {
                        _default_main = e.Current.Value;
                    }
                }
                return _default_main;
            }
            set
            {
                _override_main = value;
            }
        }

        public virtual bool TryGetDevice(string id, out IInputDevice device)
        {
            return _dict.TryGetValue(id, out device);
        }

        #endregion

        #region Collection Interface

        public void Add(string id, IInputDevice dev)
        {
            if (this.Contains(dev) && !(_dict.ContainsKey(id) && _dict[id] == dev)) throw new System.ArgumentException("Manager already contains input device for other player.");

            if (_dict.Count == 0) _default_main = dev;
            _dict[id] = dev;
        }

        public bool Remove(string id)
        {
            if (_default_main != null)
            {
                IInputDevice device;
                if (_dict.TryGetValue(id, out device) && device == _default_main)
                {
                    if (_dict.Remove(id))
                    {
                        _default_main = null;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return _dict.Remove(id);
                }
            }
            else
            {
                return _dict.Remove(id);
            }
        }

        public bool Remove(IInputDevice device)
        {
            foreach (var pair in _dict)
            {
                if (pair.Value == device)
                {
                    _dict.Remove(pair.Key);
                    return true;
                }
            }

            return false;
        }

        public bool Contains(string id)
        {
            return _dict.ContainsKey(id);
        }

        public bool Contains(IInputDevice dev)
        {
            return (_dict.Values as ICollection<IInputDevice>).Contains(dev);
        }

        public void ClearDevices()
        {
            _default_main = null;
            _dict.Clear();
        }

        public string GetId(IInputDevice dev)
        {
            foreach (var pair in _dict)
            {
                if (pair.Value == dev) return pair.Key;
            }

            throw new System.ArgumentException("Unknown input device.");
        }

        #endregion

        #region IEnumerable Interface

        public IEnumerable<string> GetIds() => _dict.Keys;

        public IEnumerable<IInputDevice> GetDevices() => _dict.Values;

        public Enumerator GetEnumerator() => new Enumerator(this);

        IEnumerator<IInputDevice> IEnumerable<IInputDevice>.GetEnumerator() => this.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this.GetEnumerator();

        public struct Enumerator : IEnumerator<IInputDevice>
        {

            private Dictionary<string, IInputDevice>.Enumerator _e;

            public Enumerator(SPInputManager manager)
            {
                _e = manager ? manager._dict.GetEnumerator() : default;
            }

            public IInputDevice Current => _e.Current.Value;

            object System.Collections.IEnumerator.Current => _e.Current.Value;

            public void Dispose() => _e.Dispose();

            public bool MoveNext() => _e.MoveNext();

            void System.Collections.IEnumerator.Reset() => ((System.Collections.IEnumerator)_e).Reset();
        }

        #endregion

    }

    public interface IJoystickHotpluggedGlobalHandler
    {
        void OnJoystickHotplugged(IInputManager inputmanager);
    }

}
