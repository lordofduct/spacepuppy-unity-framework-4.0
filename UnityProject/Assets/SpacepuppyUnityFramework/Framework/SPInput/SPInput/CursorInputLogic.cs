using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Utils;

namespace com.spacepuppy.SPInput
{

    /// <summary>
    /// This component handles cursor click logic better than OnMouseDown/OnMouseUp/OnMouseXXX events from Unity. 
    /// By default this class accesses the IInputManager from SPInput to get the cursor location, cursor button, and to perform the raycast. 
    /// This is housed in the CursorInputLogic.Resolver which can be replaced with a custom implementation. 
    /// Just implement the ICursorLogicResolver interface and then attach it. This is serializable (by ref) as well. 
    /// Just inherit from CursorInputLogic and pass in the object to the constructor. 
    /// </summary>
    [Infobox("Signal Collider - only the collider receives the event\r\nSignal Rigidboy - the attached rigidbody receives the event, if not exist the collider\r\nSignal Entity - only the root gameObject receives the event\r\nBroadcast Entity - The entire entity from root gameObject and all children receive the event")]
    [DefaultExecutionOrder(-31989)]
    public class CursorInputLogic : SPComponent
    {

        public enum SignalTargetOptions
        {
            SignalCollider = 0,
            SignalRigidboy = 1,
            SignalEntity = 2,
            BroadcastEntity = 3,
        }

        public event System.EventHandler OnClick;
        public event System.EventHandler OnDoubleClick;
        public event System.EventHandler CursorEnter;
        public event System.EventHandler CursorExit;
        public event System.EventHandler CursorButtonDown;
        public event System.EventHandler CursorButtonHeld;
        public event System.EventHandler CursorButtonUp;

        #region Fields

        private System.Action<IClickHandler> _clickFunctor;
        private System.Action<IDoubleClickHandler> _doubleClickFunctor;
        private System.Action<ICursorEnterHandler> _cursorEnterFunctor;
        private System.Action<ICursorExitHandler> _cursorExitFunctor;
        private System.Action<ICursorButtonHeldHandler> _cursorHeldFunctor;
        private System.Action<ICursorActivateHandler> _cursorDownFunctor;
        private System.Action<ICursorActivateHandler> _cursorUpFunctor;


        [SerializeField]
        private string _id;

        [SerializeField]
        [Tooltip("The amount of time the player has to release the button to count as a 'click', zero or less means click will never occur.")]
        private float _clickTimeout = 0.5f;
        [SerializeField]
        [Tooltip("The duration after the last click to wait for a valid double click, zero or less means double click will never occur.")]
        private float _doubleClickTimeout;
        [SerializeField]
        [Tooltip("If this is true then click event will fire on Double Clicks as well.")]
        private bool _dispatchClickEventAlways;

        [SerializeReference]
        [DisplayFlat]
        private ICursorInputResolver _resolver = new CursorInputResolver();

        [System.NonSerialized]
        private Collider _current;
        [System.NonSerialized]
        private SPEntity _currentEntity;

        [System.NonSerialized]
        private double _lastDownTime = double.NegativeInfinity;
        [System.NonSerialized]
        private double _lastUpTime = double.NegativeInfinity;
        [System.NonSerialized]
        private int _clickCount = 0;
        [System.NonSerialized]
        private ButtonState _buttonState = ButtonState.None;

        #endregion

        #region CONSTRUCTOR

        public CursorInputLogic()
        {
            _resolver = new CursorInputResolver();
        }

        public CursorInputLogic(ICursorInputResolver resolver)
        {
            _resolver = resolver ?? new CursorInputResolver();
        }

        protected override void Awake()
        {
            base.Awake();
            
            _clickFunctor = (o) => o.OnClick(this, _current);
            _doubleClickFunctor = (o) => o.OnDoubleClick(this, _current);
            _cursorEnterFunctor = (o) => o.OnCursorEnter(this, _current);
            _cursorExitFunctor = (o) => o.OnCursorExit(this, _current);
            _cursorHeldFunctor = (o) => o.OnButtonHeld(this, _current);
            _cursorDownFunctor = (o) => o.OnCursorDown(this, _current);
            _cursorUpFunctor = (o) => o.OnCursorUp(this, _current);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Id used to identify this CursorInputLogic so you triggers can desitinguish between different cursors. 
        /// See CursorInputLogicFilter property of t_OnCurssorXXX.
        /// </summary>
        public string Id
        {
            get => _id;
            set => _id = value;
        }

        public ICursorInputResolver Resolver
        {
            get => _resolver;
            set => _resolver = value;
        }

        public float ClickTimeout
        {
            get => _clickTimeout;
            set => _clickTimeout = value;
        }

        public float DoubleClickTimeout
        {
            get => _doubleClickTimeout;
            set => _doubleClickTimeout = value;
        }

        /// <summary>
        /// When true the OnClick event will be fired in tandem with the Double Click event. 
        /// You can check the ClickSequenceCount to tell if it was a double click or not (even = double).
        /// </summary>
        public bool DispatchClickEventAlways
        {
            get => _dispatchClickEventAlways;
            set => _dispatchClickEventAlways = value;
        }

        /// <summary>
        /// Current Collider the cursor is over.
        /// </summary>
        public Collider CurrentCollider => _current;

        /// <summary>
        /// The current entity, if any, that the Collider is attached to.
        /// </summary>
        public SPEntity CurrentEntity => _currentEntity;

        /// <summary>
        /// Last time the cursor's click button was registered as being Down (think Input.GetButtonDown).
        /// </summary>
        public double LastButtonDownTime => _lastDownTime;

        /// <summary>
        /// Last time the cursor's click button was registered as being up (think Input.GetButtonUp).
        /// </summary>
        public double LastButtonUpTime => _lastUpTime;

        /// <summary>
        /// The number of times that the button was clicked repeatedly. The duration of time valid between click sequences is DoubleClickTimeout. 
        /// You can read this value to check if a "OnClick" event occured as the 3rd click after a OnDoubleClick event.
        /// </summary>
        public int ClickSequenceCount => _clickCount;

        public bool LastClickWasDoubleClick => _clickCount > 0 && (_clickCount % 2) == 0;

        public ButtonState CurrentButtonState => _buttonState;

        #endregion

        #region Methods

        protected void Update()
        {
            Collider coll = _resolver?.QueryCurrentColliderOver();

            if (coll != _current)
            {
                var ent = coll != null ? SPEntity.Pool.GetFromSource(coll) : null;
                if (ent != _currentEntity || ent == null || _currentEntity == null)
                {
                    this.DispatchHoverExit();

                    _current = coll;
                    _currentEntity = ent;
                    _clickCount = 0;

                    this.DispatchHoverEnter();
                }
                else
                {
                    //we're over the same entity, just a different collider... update current collider only
                    _current = coll;
                }
            }

            _buttonState = _resolver?.GetClickButtonState() ?? ButtonState.None;
            switch (_buttonState.ResolvePressState(_lastDownTime, _clickTimeout))
            {
                case ButtonPress.Released:
                    _lastUpTime = Time.unscaledDeltaTime;
                    _clickCount = 0;
                    this.DispatchCursorUp();
                    break;
                case ButtonPress.Tapped:
                    {
                        double delta = Time.unscaledTimeAsDouble - _lastUpTime;
                        _lastUpTime = Time.unscaledTimeAsDouble;
                        _clickCount++;
                        this.DispatchCursorUp();

                        if (delta < _doubleClickTimeout && (_clickCount % 2) == 0)
                        {
                            if (_dispatchClickEventAlways) this.DispatchClick();
                            this.DispatchDoubleClick();
                        }
                        else
                        {
                            this.DispatchClick();
                        }
                    }
                    break;
                case ButtonPress.None:
                    if(_clickCount > 0 && (Time.unscaledTimeAsDouble - _lastUpTime) > _doubleClickTimeout)
                    {
                        _clickCount = 0;
                    }
                    break;
                case ButtonPress.Down:
                    if (_clickCount > 0 && (Time.unscaledTimeAsDouble - _lastUpTime) > _doubleClickTimeout)
                    {
                        _clickCount = 0;
                    }
                    _lastDownTime = Time.unscaledTimeAsDouble;
                    this.DispatchCursorDown();
                    break;
                case ButtonPress.Held:
                    _clickCount = 0;

                    break;
            }
        }

        private void DispatchHoverEnter()
        {
            var target = _resolver?.GetDispatchTarget(_current, _currentEntity);
            if (target == null) return;

            if (_resolver.UseBroadcast)
            {
                target.Broadcast(_cursorEnterFunctor);
            }
            else
            {
                target.Signal(_cursorEnterFunctor);
            }

            Messaging.Broadcast(_cursorEnterFunctor);
            this.CursorEnter?.Invoke(this, System.EventArgs.Empty);
        }

        private void DispatchHoverExit()
        {
            var target = _resolver?.GetDispatchTarget(_current, _currentEntity);
            if (target == null) return;

            if (_resolver.UseBroadcast)
            {
                target.Broadcast(_cursorExitFunctor);
            }
            else
            {
                target.Signal(_cursorExitFunctor);
            }

            Messaging.Broadcast(_cursorExitFunctor);
            this.CursorExit?.Invoke(this, System.EventArgs.Empty);
        }

        private void DispatchClick()
        {
            var target = _resolver?.GetDispatchTarget(_current, _currentEntity);
            if (target == null) return;

            if (_resolver.UseBroadcast)
            {
                target.Broadcast(_clickFunctor);
            }
            else
            {
                target.Signal(_clickFunctor);
            }

            Messaging.Broadcast(_clickFunctor);
            this.OnClick?.Invoke(this, System.EventArgs.Empty);
        }

        private void DispatchDoubleClick()
        {
            var target = _resolver?.GetDispatchTarget(_current, _currentEntity);
            if (target == null) return;

            if (_resolver.UseBroadcast)
            {
                target.Broadcast(_doubleClickFunctor);
            }
            else
            {
                target.Signal(_doubleClickFunctor);
            }

            Messaging.Broadcast(_doubleClickFunctor);
            this.OnDoubleClick?.Invoke(this, System.EventArgs.Empty);
        }

        private void DispatchCursorDown()
        {
            var target = _resolver?.GetDispatchTarget(_current, _currentEntity);
            if (target == null) return;

            if (_resolver.UseBroadcast)
            {
                target.Broadcast(_cursorDownFunctor);
            }
            else
            {
                target.Signal(_cursorDownFunctor);
            }

            Messaging.Broadcast(_cursorDownFunctor);
            this.CursorButtonDown?.Invoke(this, System.EventArgs.Empty);
        }

        private void DispatchCursorUp()
        {
            var target = _resolver?.GetDispatchTarget(_current, _currentEntity);
            if (target == null) return;

            if (_resolver.UseBroadcast)
            {
                target.Broadcast(_cursorUpFunctor);
            }
            else
            {
                target.Signal(_cursorUpFunctor);
            }

            Messaging.Broadcast(_cursorUpFunctor);
            this.CursorButtonUp?.Invoke(this, System.EventArgs.Empty);
        }

        private void DispatchCursorHeld()
        {
            var target = _resolver?.GetDispatchTarget(_current, _currentEntity);
            if (target == null) return;

            if (_resolver.UseBroadcast)
            {
                target.Broadcast(_cursorHeldFunctor);
            }
            else
            {
                target.Signal(_cursorHeldFunctor);
            }

            Messaging.Broadcast(_cursorHeldFunctor);
            this.CursorButtonHeld?.Invoke(this, System.EventArgs.Empty);
        }

        #endregion

        #region Special Types

        /// <summary>
        /// Up/Down message handler for cursor button.
        /// </summary>
        public interface ICursorActivateHandler
        {
            void OnCursorDown(CursorInputLogic sender, Collider c);
            void OnCursorUp(CursorInputLogic sender, Collider c);
        }

        /// <summary>
        /// Message handler for when the cursor enters a new target.
        /// </summary>
        public interface ICursorEnterHandler
        {
            void OnCursorEnter(CursorInputLogic sender, Collider c);
        }

        /// <summary>
        /// Message handler for when the cursor exist a target.
        /// </summary>
        public interface ICursorExitHandler
        {
            void OnCursorExit(CursorInputLogic sender, Collider c);
        }

        /// <summary>
        /// Message handler for when the cursor presses down on the button for longer than the ClickTimeout.
        /// </summary>
        public interface ICursorButtonHeldHandler
        {
            void OnButtonHeld(CursorInputLogic sender, Collider c);
        }

        /// <summary>
        /// Message handler for when the cursor presses down and releases within the ClickTimeout.
        /// </summary>
        public interface IClickHandler
        {
            void OnClick(CursorInputLogic sender, Collider c);
        }

        /// <summary>
        /// Message handler for when the cursor presses down and release twice in succession within the ClickTimeout and DoubleClickTimeout.
        /// </summary>
        public interface IDoubleClickHandler
        {
            void OnDoubleClick(CursorInputLogic sender, Collider c);
        }


        public interface ICursorInputResolver
        {
            bool UseBroadcast { get; }

            Collider QueryCurrentColliderOver();
            ButtonState GetClickButtonState();
            GameObject GetDispatchTarget(Collider collider, SPEntity entity);
        }


        [System.Serializable]
        public class CursorInputResolver : ICursorInputResolver
        {

            #region Fields

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
            private SignalTargetOptions _signalTarget;

            #endregion

            #region Methods

            public bool UseBroadcast => _signalTarget >= SignalTargetOptions.BroadcastEntity;

            public IInputDevice GetInputDevice()
            {
                return string.IsNullOrEmpty(_deviceId) ? Services.Get<IInputManager>()?.Main : Services.Get<IInputManager>()?.GetDevice(_deviceId);
            }

            public virtual Collider QueryCurrentColliderOver()
            {
                var input = this.GetInputDevice();

                if (input != null)
                {
                    var pos = input.GetCursorState(_cursorInputId);
                    var cam = ObjUtil.GetAsFromSource<Camera>(_cameraSource, true);
                    if (cam == null) return null;

                    return CursorInputLogic.QueryCurrentColliderOver(cam, pos, _layerMask, _queryTriggerOption);
                }

                return null;
            }

            public virtual ButtonState GetClickButtonState()
            {
                return this.GetInputDevice()?.GetButtonState(_clickButtonInputId) ?? ButtonState.None;
            }

            public virtual GameObject GetDispatchTarget(Collider collider, SPEntity entity)
            {
                if (collider == null) return null;

                switch (_signalTarget)
                {
                    case SignalTargetOptions.SignalCollider:
                        return collider.gameObject;
                    case SignalTargetOptions.SignalRigidboy:
                        {
                            var rb = collider.attachedRigidbody;
                            return rb != null ? rb.gameObject : collider.gameObject;
                        }
                    case SignalTargetOptions.SignalEntity:
                        if (entity != null)
                            return entity.gameObject;
                        else
                            return collider.FindRoot();
                    case SignalTargetOptions.BroadcastEntity:
                        if (entity != null)
                            return entity.gameObject;
                        else
                            return collider.FindRoot();
                    default:
                        return null;
                }
            }

            #endregion

        }

        public static Collider QueryCurrentColliderOver(Camera cam, Vector2 cursorpos, LayerMask mask, QueryTriggerInteraction queryTriggerOption)
        {
            var ray = cam.ScreenPointToRay(cursorpos);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, float.PositiveInfinity, mask, queryTriggerOption))
            {
                return hit.collider;
            }
            return null;
        }

        #endregion

    }
}
