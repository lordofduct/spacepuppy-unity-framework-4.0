using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Utils;

namespace com.spacepuppy.SPInput
{

    public interface ICursorInputLogic
    {
        string Id { get; }
    }

    [Infobox("Signal Collider - only the collider receives the event\r\nSignal Rigidboy - the attached rigidbody receives the event, if not exist the collider\r\nSignal Entity - only the root gameObject receives the event\r\nBroadcast Entity - The entire entity from root gameObject and all children receive the event")]
    public class CursorInputLogic : SPComponent, ICursorInputLogic
    {

        public enum SignalTarget
        {
            SignalCollider = 0,
            SignalRigidboy = 1,
            SignalEntity = 2,
            BroadcastEntity = 3,
        }

        #region Fields

        private System.Action<IClickHandler> _clickFunctor;
        private System.Action<IDoubleClickHandler> _doubleClickFunctor;
        private System.Action<IHoverHandler> _hoverEnterFunctor;
        private System.Action<IHoverHandler> _hoverExitFunctor;
        private System.Action<ICursorHandler> _cursorDownFunctor;
        private System.Action<ICursorHandler> _cursorUpFunctor;


        [SerializeField]
        private string _id;

        [SerializeField]
        [Tooltip("The camera to use, or a proxy to it.")]
        [RespectsIProxy]
        [TypeRestriction(typeof(Camera), AllowProxy = true)]
        private UnityEngine.Object _cameraSource;

        [SerializeField]
        [Tooltip("Leave blank to use main input device.")]
        private string _deviceId;

        [SerializeField]
        [InputID]
        [Tooltip("The input id of the cursor input signature.")]
        private string _cursorInputId;

        [SerializeField]
        [InputID]
        [Tooltip("The input id of the button to use as the click button. If you want multiple buttons to be usable, you should register them all under the same input id when initializing your InputManager.")]
        private string _clickButtonInputId;

        [SerializeField]
        [Tooltip("The amount of time the player has to release the button to count as a 'click', zero or less means click will never occur.")]
        private float _clickTimeout = 0.5f;
        [SerializeField]
        [Tooltip("The duration after the last click to wait for a valid double click, zero or less means double click will never occur.")]
        private float _doubleClickTimeout;

        [SerializeField]
        [Tooltip("The mask of layers to check for when raycasting. This includes both the layers that should be clickable, and those that can block clickable things.")]
        private LayerMask _layerMask = -1;
        [SerializeField]
        [Tooltip("How to deal with trigger colliders when raycasting.")]
        private QueryTriggerInteraction _queryTriggerOption;

        [SerializeField]
        private SignalTarget _signalTarget;

        [System.NonSerialized]
        private Collider _current;
        [System.NonSerialized]
        private SPEntity _currentEntity;

        [System.NonSerialized]
        private double _lastClickTime = double.NegativeInfinity;

        #endregion

        #region CONSTRUCTOR

        protected override void Awake()
        {
            base.Awake();
            
            _clickFunctor = (o) => o.OnClick(this, _current);
            _doubleClickFunctor = (o) => o.OnDoubleClick(this, _current);
            _hoverEnterFunctor = (o) => o.OnHoverEnter(this, _current);
            _hoverExitFunctor = (o) => o.OnHoverExit(this, _current);
            _cursorDownFunctor = (o) => o.OnCursorDown(this, _current);
            _cursorUpFunctor = (o) => o.OnCursorUp(this, _current);
        }

        #endregion

        #region Properties

        public string Id
        {
            get => _id;
            set => _id = value;
        }

        public UnityEngine.Object CameraSource
        {
            get => _cameraSource;
            set => _cameraSource = value;
        }

        public string DeviceId
        {
            get => _deviceId;
            set => _deviceId = value;
        }

        public string CursorInputId
        {
            get => _cursorInputId;
            set => _cursorInputId = value;
        }

        public string ClickButtonInputId
        {
            get => _clickButtonInputId;
            set => _clickButtonInputId = value;
        }

        public float ClickTimeout
        {
            get => _clickTimeout;
            set => _clickTimeout = value;
        }

        public LayerMask LayerMask
        {
            get => _layerMask;
            set => _layerMask = value;
        }

        public QueryTriggerInteraction QueryTriggerOption
        {
            get => _queryTriggerOption;
            set => _queryTriggerOption = value;
        }

        public Collider CurrentCollider => _current;

        public SPEntity CurrentEntity => _currentEntity;

        #endregion

        #region Methods

        protected void Update()
        {
            var input = string.IsNullOrEmpty(_deviceId) ? Services.Get<IInputManager>()?.Main : Services.Get<IInputManager>()?.GetDevice(_deviceId);
            Collider coll = this.QueryCurrentColliderOver(input);

            if (coll != _current)
            {
                var ent = coll != null ? SPEntity.Pool.GetFromSource(coll) : null;
                if (ent != _currentEntity || ent == null || _currentEntity == null)
                {
                    this.DispatchHoverExit();

                    _current = coll;
                    _currentEntity = ent;
                    _lastClickTime = Time.unscaledTimeAsDouble;

                    this.DispatchHoverEnter();
                }
                else
                {
                    //we're over the same entity, just a different collider... update current collider only
                    _current = coll;
                }
            }

            switch(input?.GetButtonPress(_clickButtonInputId, _clickTimeout) ?? ButtonPress.None)
            {
                case ButtonPress.Released:
                    this.DispatchCursorUp();
                    break;
                case ButtonPress.Tapped:
                    this.DispatchCursorUp();
                    if (Time.unscaledTimeAsDouble - _lastClickTime < _doubleClickTimeout)
                    {
                        _lastClickTime = double.NegativeInfinity;
                        this.DispatchDoubleClick();
                    }
                    else
                    {
                        _lastClickTime = Time.unscaledTimeAsDouble;
                        this.DispatchClick();
                    }
                    break;
                case ButtonPress.Down:
                    this.DispatchCursorDown();
                    break;
            }
        }

        protected virtual Collider QueryCurrentColliderOver(IInputDevice input)
        {
            if (input != null)
            {
                var pos = input.GetCursorState(_cursorInputId);
                var cam = ObjUtil.GetAsFromSource<Camera>(_cameraSource, true);
                if (cam == null) return null;

                var ray = cam.ScreenPointToRay(pos);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, float.PositiveInfinity, _layerMask, _queryTriggerOption))
                {
                    return hit.collider;
                }
            }

            return null;
        }

        private void DispatchHoverEnter()
        {
            if (_current == null) return;

            switch(_signalTarget)
            {
                case SignalTarget.SignalCollider:
                    _current.gameObject.Signal(_hoverEnterFunctor);
                    break;
                case SignalTarget.SignalRigidboy:
                    {
                        var rb = _current.attachedRigidbody;
                        var go = rb != null ? rb.gameObject : _current.gameObject;
                        go.Signal(_hoverEnterFunctor);
                    }
                    break;
                case SignalTarget.SignalEntity:
                    if (_currentEntity != null)
                        _currentEntity.gameObject.Signal(_hoverEnterFunctor);
                    else
                        _current.FindRoot().Signal(_hoverEnterFunctor);
                    break;
                case SignalTarget.BroadcastEntity:
                    if (_currentEntity != null)
                        _currentEntity.gameObject.Broadcast(_hoverEnterFunctor);
                    else
                        _current.FindRoot().Broadcast(_hoverEnterFunctor);
                    break;
            }

            Messaging.Broadcast(_hoverEnterFunctor);
        }

        private void DispatchHoverExit()
        {
            if (_current == null) return;

            switch (_signalTarget)
            {
                case SignalTarget.SignalCollider:
                    _current.gameObject.Signal(_hoverExitFunctor);
                    break;
                case SignalTarget.SignalRigidboy:
                    {
                        var rb = _current.attachedRigidbody;
                        var go = rb != null ? rb.gameObject : _current.gameObject;
                        go.Signal(_hoverExitFunctor);
                    }
                    break;
                case SignalTarget.SignalEntity:
                    if (_currentEntity != null)
                        _currentEntity.gameObject.Signal(_hoverExitFunctor);
                    else
                        _current.FindRoot().Signal(_hoverExitFunctor);
                    break;
                case SignalTarget.BroadcastEntity:
                    if (_currentEntity != null)
                        _currentEntity.gameObject.Broadcast(_hoverExitFunctor);
                    else
                        _current.FindRoot().Broadcast(_hoverExitFunctor);
                    break;
            }

            Messaging.Broadcast(_hoverExitFunctor);
        }

        private void DispatchClick()
        {
            if (_current == null) return;

            switch (_signalTarget)
            {
                case SignalTarget.SignalCollider:
                    _current.gameObject.Signal(_clickFunctor);
                    break;
                case SignalTarget.SignalRigidboy:
                    {
                        var rb = _current.attachedRigidbody;
                        var go = rb != null ? rb.gameObject : _current.gameObject;
                        go.Signal(_clickFunctor);
                    }
                    break;
                case SignalTarget.SignalEntity:
                    if (_currentEntity != null)
                        _currentEntity.gameObject.Signal(_clickFunctor);
                    else
                        _current.FindRoot().Signal(_clickFunctor);
                    break;
                case SignalTarget.BroadcastEntity:
                    if (_currentEntity != null)
                        _currentEntity.gameObject.Broadcast(_clickFunctor);
                    else
                        _current.FindRoot().Broadcast(_clickFunctor);
                    break;
            }

            Messaging.Broadcast(_clickFunctor);
        }

        private void DispatchDoubleClick()
        {
            if (_current == null) return;

            switch (_signalTarget)
            {
                case SignalTarget.SignalCollider:
                    _current.gameObject.Signal(_doubleClickFunctor);
                    break;
                case SignalTarget.SignalRigidboy:
                    {
                        var rb = _current.attachedRigidbody;
                        var go = rb != null ? rb.gameObject : _current.gameObject;
                        go.Signal(_doubleClickFunctor);
                    }
                    break;
                case SignalTarget.SignalEntity:
                    if (_currentEntity != null)
                        _currentEntity.gameObject.Signal(_doubleClickFunctor);
                    else
                        _current.FindRoot().Signal(_doubleClickFunctor);
                    break;
                case SignalTarget.BroadcastEntity:
                    if (_currentEntity != null)
                        _currentEntity.gameObject.Broadcast(_doubleClickFunctor);
                    else
                        _current.FindRoot().Broadcast(_doubleClickFunctor);
                    break;
            }

            Messaging.Broadcast(_doubleClickFunctor);
        }

        private void DispatchCursorDown()
        {
            if (_current == null) return;

            switch (_signalTarget)
            {
                case SignalTarget.SignalCollider:
                    _current.gameObject.Signal(_cursorDownFunctor);
                    break;
                case SignalTarget.SignalRigidboy:
                    {
                        var rb = _current.attachedRigidbody;
                        var go = rb != null ? rb.gameObject : _current.gameObject;
                        go.Signal(_cursorDownFunctor);
                    }
                    break;
                case SignalTarget.SignalEntity:
                    if (_currentEntity != null)
                        _currentEntity.gameObject.Signal(_cursorDownFunctor);
                    else
                        _current.FindRoot().Signal(_cursorDownFunctor);
                    break;
                case SignalTarget.BroadcastEntity:
                    if (_currentEntity != null)
                        _currentEntity.gameObject.Broadcast(_cursorDownFunctor);
                    else
                        _current.FindRoot().Broadcast(_cursorDownFunctor);
                    break;
            }

            Messaging.Broadcast(_cursorDownFunctor);
        }

        private void DispatchCursorUp()
        {
            if (_current == null) return;

            switch (_signalTarget)
            {
                case SignalTarget.SignalCollider:
                    _current.gameObject.Signal(_cursorUpFunctor);
                    break;
                case SignalTarget.SignalRigidboy:
                    {
                        var rb = _current.attachedRigidbody;
                        var go = rb != null ? rb.gameObject : _current.gameObject;
                        go.Signal(_cursorUpFunctor);
                    }
                    break;
                case SignalTarget.SignalEntity:
                    if (_currentEntity != null)
                        _currentEntity.gameObject.Signal(_cursorUpFunctor);
                    else
                        _current.FindRoot().Signal(_cursorUpFunctor);
                    break;
                case SignalTarget.BroadcastEntity:
                    if (_currentEntity != null)
                        _currentEntity.gameObject.Broadcast(_cursorUpFunctor);
                    else
                        _current.FindRoot().Broadcast(_cursorUpFunctor);
                    break;
            }

            Messaging.Broadcast(_cursorUpFunctor);
        }

        #endregion

        #region Special Types

        public interface ICursorHandler
        {
            void OnCursorDown(ICursorInputLogic sender, Collider c);
            void OnCursorUp(ICursorInputLogic sender, Collider c);
        }

        public interface IHoverHandler
        {
            void OnHoverEnter(ICursorInputLogic sender, Collider c);
            void OnHoverExit(ICursorInputLogic sender, Collider c);
        }

        public interface IClickHandler
        {
            void OnClick(ICursorInputLogic sender, Collider c);
        }

        public interface IDoubleClickHandler
        {
            void OnDoubleClick(ICursorInputLogic sender, Collider c);
        }

        #endregion

    }
}
