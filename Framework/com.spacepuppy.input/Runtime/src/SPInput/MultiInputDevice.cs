using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace com.spacepuppy.SPInput
{

    public class MultiInputDevice : IInputDevice
    {

        #region Fields

        private string _id;
        private InputDeviceCollection<IInputDevice> _devices = new InputDeviceCollection<IInputDevice>();
        private IInputDevice _currentDevice;
        private System.Func<IInputDevice, bool> _predicate;

        #endregion

        #region CONSTRUCTOR

        public MultiInputDevice(string id)
        {
            _id = id;
            _predicate = null;
        }

        public MultiInputDevice(string id, System.Func<IInputDevice, bool> predicate)
        {
            _id = id;
            _predicate = predicate;
        }

        #endregion

        #region Properties

        public ICollection<IInputDevice> Devices => _devices;

        public IInputDevice CurrentDevice => _currentDevice ?? (_currentDevice = _devices.FirstOrDefault());

        public System.Func<IInputDevice, bool> TestCurrentDeviceSwapPredicate
        {
            get => _predicate;
            set => _predicate = value;
        }

        #endregion

        #region Methods

        public bool SetCurrentInputDevice(IInputDevice device)
        {
            if (_devices.Contains(device))
            {
                _currentDevice = device;
                return true;
            }
            return false;
        }

        #endregion

        #region IInputDevice Interface

        public string Id => _id;

        public bool Active { get; set; } = true;

        public float Precedence { get; set; } = 0f;

        public bool Contains(string id)
        {
            if (_currentDevice?.Contains(id) ?? false) return true;

            foreach (var dev in _devices)
            {
                if (dev.Contains(id)) return true;
            }
            return false;
        }

        public float GetAxleState(string id) => this.CurrentDevice?.GetAxleState(id) ?? default;

        public ButtonState GetButtonState(string id, bool consume = false) => this.CurrentDevice?.GetButtonState(id, consume) ?? default;

        public Vector2 GetCursorState(string id) => this.CurrentDevice?.GetCursorState(id) ?? default;

        public Vector2 GetDualAxleState(string id) => this.CurrentDevice?.GetDualAxleState(id) ?? default;

        public IInputSignature GetSignature(string id) => this.CurrentDevice?.GetSignature(id) ?? default;

        public IEnumerable<IInputSignature> GetSignatures() => this.CurrentDevice?.GetSignatures() ?? Enumerable.Empty<IInputSignature>();

        public virtual void Reset()
        {
            foreach (var dev in _devices)
            {
                dev?.Reset();
            }
        }

        public virtual void Update()
        {
            bool found = false;
            foreach (var dev in _devices)
            {
                dev?.Update();
                if (!found && (_predicate?.Invoke(dev) ?? false))
                {
                    found = true;
                    _currentDevice = dev;
                }
            }
        }

        public virtual void FixedUpdate()
        {
            foreach (var dev in _devices)
            {
                dev?.FixedUpdate();
            }
        }

        #endregion

        #region Static Utils

        public static readonly System.Func<IInputDevice, bool> DefaultTestCurrentDeviceSwapPredicate = (device) => device?.GetAnyButtonDown() ?? false;

        public static readonly System.Func<IInputDevice, bool> TestIfAnyButtonDown = (device) => device?.GetAnyButtonDown() ?? false;

        public static readonly System.Func<IInputDevice, bool> TestIfAnyInputActivated = (device) => device?.GetAnyInputActivated() ?? false;

        #endregion

    }

    public class MultiMappedInputDevice<T> : IMappedInputDevice<T> where T : struct, System.IConvertible
    {

        #region Fields

        private string _id;
        private InputDeviceCollection<IMappedInputDevice<T>> _devices = new InputDeviceCollection<IMappedInputDevice<T>>();
        private IMappedInputDevice<T> _currentDevice;
        private System.Func<IInputDevice, bool> _predicate;

        #endregion

        #region CONSTRUCTOR

        public MultiMappedInputDevice(string id)
        {
            _id = id;
            _predicate = MultiInputDevice.DefaultTestCurrentDeviceSwapPredicate;
        }

        public MultiMappedInputDevice(string id, System.Func<IInputDevice, bool> predicate)
        {
            _id = id;
            _predicate = predicate ?? MultiInputDevice.DefaultTestCurrentDeviceSwapPredicate;
        }

        #endregion

        #region Properties

        public ICollection<IMappedInputDevice<T>> Devices => _devices;

        public IMappedInputDevice<T> CurrentDevice => _currentDevice ?? (_currentDevice = _devices.FirstOrDefault());

        public System.Func<IInputDevice, bool> TestCurrentDeviceSwapPredicate
        {
            get => _predicate;
            set => _predicate = value ?? MultiInputDevice.DefaultTestCurrentDeviceSwapPredicate;
        }

        #endregion

        #region Methods

        public bool SetCurrentInputDevice(IMappedInputDevice<T> device)
        {
            if (_devices.Contains(device))
            {
                _currentDevice = device;
                return true;
            }
            return false;
        }

        #endregion

        #region IInputDevice Interface

        public string Id => _id;

        public bool Active { get; set; } = true;

        public float Precedence { get; set; } = 0f;

        public bool Contains(string id)
        {
            if (_currentDevice?.Contains(id) ?? false) return true;

            foreach (var dev in _devices)
            {
                if (dev.Contains(id)) return true;
            }
            return false;
        }

        public float GetAxleState(string id) => this.CurrentDevice?.GetAxleState(id) ?? default;

        public ButtonState GetButtonState(string id, bool consume = false) => this.CurrentDevice?.GetButtonState(id, consume) ?? default;

        public Vector2 GetCursorState(string id) => this.CurrentDevice?.GetCursorState(id) ?? default;

        public Vector2 GetDualAxleState(string id) => this.CurrentDevice?.GetDualAxleState(id) ?? default;

        public ButtonState GetButtonState(T btn, bool consume = false) => this.CurrentDevice?.GetButtonState(btn, consume) ?? default;

        public float GetAxleState(T axis) => this.CurrentDevice?.GetAxleState(axis) ?? default;

        public Vector2 GetDualAxleState(T axis) => this.CurrentDevice?.GetDualAxleState(axis) ?? default;

        public Vector2 GetCursorState(T mapping) => this.CurrentDevice?.GetCursorState(mapping) ?? default;

        public IInputSignature GetSignature(string id) => this.CurrentDevice?.GetSignature(id) ?? default;

        public IInputSignature GetSignature(T id) => this.CurrentDevice?.GetSignature(id) ?? default;

        public IEnumerable<IInputSignature> GetSignatures() => this.CurrentDevice?.GetSignatures() ?? Enumerable.Empty<IInputSignature>();

        public virtual void Reset()
        {
            foreach (var dev in _devices)
            {
                dev?.Reset();
            }
        }

        public virtual void Update()
        {
            bool found = false;
            foreach (var dev in _devices)
            {
                dev?.Update();
                if (!found && (_predicate?.Invoke(dev) ?? false))
                {
                    found = true;
                    _currentDevice = dev;
                }
            }
        }

        public virtual void FixedUpdate()
        {
            foreach (var dev in _devices)
            {
                dev?.FixedUpdate();
            }
        }

        #endregion

    }

    /// <summary>
    /// Designed to be a small collection of inputdevices that don't allow null entries or duplicate entries.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class InputDeviceCollection<T> : ICollection<T> where T : IInputDevice
    {

        private List<T> _coll = new List<T>(3);

        public int Count => _coll.Count;

        bool ICollection<T>.IsReadOnly => false;

        public void Add(T item)
        {
            if (item == null) throw new System.ArgumentNullException(nameof(item));
            if (_coll.Contains(item)) return;
            _coll.Add(item);
        }

        public void Clear() => _coll.Clear();

        public bool Contains(T item) => item != null && _coll.Contains(item);

        public void CopyTo(T[] array, int arrayIndex) => _coll.CopyTo(array, arrayIndex);

        public bool Remove(T item) => _coll.Remove(item);

        public List<T>.Enumerator GetEnumerator() => _coll.GetEnumerator();
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => _coll.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _coll.GetEnumerator();
    }

}
