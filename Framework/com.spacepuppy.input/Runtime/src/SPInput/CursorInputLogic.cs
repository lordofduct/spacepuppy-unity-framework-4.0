using UnityEngine;
using System.Collections.Generic;

using UnityEngine.EventSystems;
using com.spacepuppy.Utils;
using com.spacepuppy.Collections;

namespace com.spacepuppy.SPInput
{

    /// <summary>
    /// This component represents an in world cursor click logic independent of the EventSystem. This acts similarly 
    /// to the OnMouseDown/OnMouseUp/etc events but with some more robust features. 
    /// The mode is howed in the CursorInputLogic.Resolver which can be replaced with a custom implementation. 
    /// Just implement the ICursorLogicResolver within your project and it'll become available as a "mode" in the dropdown.
    /// </summary>
    [Infobox("This is a simple cursor implementation independent of the EventSystem. You only need this if the EventSystem isn't available or if you want to have a cursor indepdent of the EventSystem.")]
    [DefaultExecutionOrder(CursorInputLogic.DEFAULT_EXECUTION_ORDER)]
    public sealed class CursorInputLogic : SPComponent
    {
        public const int DEFAULT_EXECUTION_ORDER = -31989;

        #region Multiton Interface

        private static readonly MultitonPool<CursorInputLogic> _pool = new MultitonPool<CursorInputLogic>();
        public static MultitonPool<CursorInputLogic> Pool => _pool;

        #endregion

        public event System.EventHandler OnClick;
        public event System.EventHandler OnDoubleClick;
        public event System.EventHandler CursorEnter;
        public event System.EventHandler CursorExit;
        public event System.EventHandler CursorButtonDown;
        public event System.EventHandler CursorButtonHeld;
        public event System.EventHandler CursorButtonUp;
        public event System.EventHandler BeginDrag;
        public event System.EventHandler EndDrag;

        #region Fields

        private System.Action<IClickHandler> _clickFunctor;
        private System.Action<IDoubleClickHandler> _doubleClickFunctor;
        private System.Action<ICursorEnterHandler> _cursorEnterFunctor;
        private System.Action<ICursorExitHandler> _cursorExitFunctor;
        private System.Action<ICursorButtonHeldHandler> _cursorHeldFunctor;
        private System.Action<ICursorDownHandler> _cursorDownFunctor;
        private System.Action<ICursorUpHandler> _cursorUpFunctor;
        private System.Action<ICursorBeginDragHandler> _beginDragFunctor;
        private System.Action<ICursorEndDragHandler> _endDragFunctor;


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
        [SerializeRefPicker(typeof(ICursorInputResolver), AlwaysExpanded = false, DisplayBox = true)]
        private ICursorInputResolver _resolver = new EventSystemCursorInputResolver();

        [System.NonSerialized]
        private GameObject _current;
        [System.NonSerialized]
        private SPEntity _currentEntity;

        [System.NonSerialized]
        private double _lastDownTime = double.NegativeInfinity;
        [System.NonSerialized]
        private double _lastUpTime = double.NegativeInfinity;
        [System.NonSerialized]
        private int _clickCount = 0;

        #endregion

        #region CONSTRUCTOR

        public CursorInputLogic()
        {
            _resolver = new EventSystemCursorInputResolver();
        }

        public CursorInputLogic(ICursorInputResolver resolver)
        {
            _resolver = resolver ?? new EventSystemCursorInputResolver();
        }

        protected override void Awake()
        {
            base.Awake();

            _clickFunctor = (o) => o.OnClick(this);
            _doubleClickFunctor = (o) => o.OnDoubleClick(this);
            _cursorEnterFunctor = (o) => o.OnCursorEnter(this);
            _cursorExitFunctor = (o) => o.OnCursorExit(this);
            _cursorHeldFunctor = (o) => o.OnButtonHeld(this);
            _cursorDownFunctor = (o) => o.OnCursorDown(this);
            _cursorUpFunctor = (o) => o.OnCursorUp(this);
            _beginDragFunctor = (o) => o.OnBeginDrag(this);
            _endDragFunctor = (o) => o.OnEndDrag(this);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            _pool.AddReference(this);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (_current)
            {
                this.DispatchHoverExit();
                _current = null;
                _currentEntity = null;
            }

            _clickCount = 0;
            if (this.DragInitiated)
            {
                this.DispatchEndDrag();
                this.DragInitiated = false;
            }

            this.CurrentButtonState = ButtonState.None;
            this.CurrentButtonPress = ButtonPress.None;

            _pool.RemoveReference(this);
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
        /// Current GameObject the cursor is over.
        /// </summary>
        public GameObject Current => _current;

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

        public ButtonState CurrentButtonState { get; private set; }

        public ButtonPress CurrentButtonPress { get; private set; }

        public Vector2 CursorPosition => _resolver?.CursorPosition ?? Vector2.zero;

        public Vector2 LastButtonDownPosition { get; private set; }

        public GameObject LastButtonDownTarget { get; private set; }

        public bool DragInitiated { get; private set; }

        #endregion

        #region Methods

        public CursorRaycastHit Raycast(bool ignoreCursorIsBlocked = false)
        {
            return _resolver?.Raycast(ignoreCursorIsBlocked) ?? default(CursorRaycastHit);
        }

        /// <summary>
        /// Creates a ray from the cursor in 3d. (What constitutes the ray depends on the ICursorInputResolver, 2d cursor modes may return unusable results)
        /// </summary>
        /// <returns></returns>
        public Ray GetRay()
        {
            return _resolver?.GetRay() ?? default(Ray);
        }

        private void Update()
        {
            var go = _resolver?.Raycast().gameObject;

            if (go != _current)
            {
                var ent = go != null ? SPEntity.Pool.GetFromSource(go) : null;
                if (ent != _currentEntity || ent == null || _currentEntity == null)
                {
                    this.DispatchHoverExit();

                    _current = go;
                    _currentEntity = ent;
                    _clickCount = 0;

                    this.DispatchHoverEnter();
                }
                else
                {
                    //we're over the same entity, just a different collider... update current collider only
                    _current = go;
                }
            }

            this.CurrentButtonState = _resolver?.GetClickButtonState() ?? ButtonState.None;
            switch (this.CurrentButtonPress = this.CurrentButtonState.ResolvePressState(_lastDownTime, _clickTimeout))
            {
                case ButtonPress.Released:
                    _lastUpTime = Time.unscaledDeltaTime;
                    _clickCount = 0;
                    if (this.DragInitiated)
                    {
                        this.DispatchEndDrag();
                        this.DragInitiated = false;
                    }
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

                        if (this.DragInitiated)
                        {
                            this.DispatchEndDrag();
                            this.DragInitiated = false;
                        }
                    }
                    break;
                case ButtonPress.None:
                    if (_clickCount > 0 && (Time.unscaledTimeAsDouble - _lastUpTime) > _doubleClickTimeout)
                    {
                        _clickCount = 0;
                    }
                    if (this.DragInitiated)
                    {
                        //this would only happen if a Resolver returned 'None' instead of 'Released'... this can cause issues so this resolves that
                        this.DispatchEndDrag();
                        this.DragInitiated = false;
                    }
                    break;
                case ButtonPress.Down:
                    if (_clickCount > 0 && (Time.unscaledTimeAsDouble - _lastUpTime) > _doubleClickTimeout)
                    {
                        _clickCount = 0;
                    }
                    _lastDownTime = Time.unscaledTimeAsDouble;
                    this.LastButtonDownPosition = _resolver?.CursorPosition ?? Vector2.zero;
                    this.LastButtonDownTarget = _current;
                    this.DispatchCursorDown();
                    break;
                case ButtonPress.Holding:
                    if (!this.DragInitiated && (_resolver?.TestBeginDrag(this) ?? false))
                    {
                        this.DragInitiated = true;
                        this.DispatchBeginDrag();
                    }
                    break;
                case ButtonPress.Held:
                    _clickCount = 0;
                    if (!this.DragInitiated && (_resolver?.TestBeginDrag(this) ?? false))
                    {
                        this.DragInitiated = true;
                        this.DispatchBeginDrag();
                    }
                    break;
            }
        }

        private void DispatchHoverEnter()
        {
            var target = _resolver?.GetDispatchTarget(this);

            if (target != null)
            {
                if (_resolver.UseBroadcast)
                {
                    target.Broadcast(_cursorEnterFunctor);
                }
                else
                {
                    target.Signal(_cursorEnterFunctor);
                }
            }

            Messaging.Broadcast(_cursorEnterFunctor);
            this.CursorEnter?.Invoke(this, System.EventArgs.Empty);
        }

        private void DispatchHoverExit()
        {
            var target = _resolver?.GetDispatchTarget(this);

            if (target != null)
            {
                if (_resolver.UseBroadcast)
                {
                    target.Broadcast(_cursorExitFunctor);
                }
                else
                {
                    target.Signal(_cursorExitFunctor);
                }
            }

            Messaging.Broadcast(_cursorExitFunctor);
            this.CursorExit?.Invoke(this, System.EventArgs.Empty);
        }

        private void DispatchClick()
        {
            var target = _resolver?.GetDispatchTarget(this);

            if (target != null)
            {
                if (_resolver.UseBroadcast)
                {
                    target.Broadcast(_clickFunctor);
                }
                else
                {
                    target.Signal(_clickFunctor);
                }
            }

            Messaging.Broadcast(_clickFunctor);
            this.OnClick?.Invoke(this, System.EventArgs.Empty);
        }

        private void DispatchDoubleClick()
        {
            var target = _resolver?.GetDispatchTarget(this);

            if (target != null)
            {
                if (_resolver.UseBroadcast)
                {
                    target.Broadcast(_doubleClickFunctor);
                }
                else
                {
                    target.Signal(_doubleClickFunctor);
                }
            }

            Messaging.Broadcast(_doubleClickFunctor);
            this.OnDoubleClick?.Invoke(this, System.EventArgs.Empty);
        }

        private void DispatchCursorDown()
        {
            var target = _resolver?.GetDispatchTarget(this);

            if (target != null)
            {
                if (_resolver.UseBroadcast)
                {
                    target.Broadcast(_cursorDownFunctor);
                }
                else
                {
                    target.Signal(_cursorDownFunctor);
                }
            }

            Messaging.Broadcast(_cursorDownFunctor);
            this.CursorButtonDown?.Invoke(this, System.EventArgs.Empty);
        }

        private void DispatchCursorUp()
        {
            var target = _resolver?.GetDispatchTarget(this);

            if (target != null)
            {
                if (_resolver.UseBroadcast)
                {
                    target.Broadcast(_cursorUpFunctor);
                }
                else
                {
                    target.Signal(_cursorUpFunctor);
                }
            }

            Messaging.Broadcast(_cursorUpFunctor);
            this.CursorButtonUp?.Invoke(this, System.EventArgs.Empty);
        }

        private void DispatchCursorHeld()
        {
            var target = _resolver?.GetDispatchTarget(this);

            if (target != null)
            {
                if (_resolver.UseBroadcast)
                {
                    target.Broadcast(_cursorHeldFunctor);
                }
                else
                {
                    target.Signal(_cursorHeldFunctor);
                }
            }

            Messaging.Broadcast(_cursorHeldFunctor);
            this.CursorButtonHeld?.Invoke(this, System.EventArgs.Empty);
        }

        private void DispatchBeginDrag()
        {
            var target = _resolver?.GetDispatchTarget(this);

            if (target != null)
            {
                if (_resolver.UseBroadcast)
                {
                    target.Broadcast(_beginDragFunctor);
                }
                else
                {
                    target.Signal(_beginDragFunctor);
                }
            }

            Messaging.Broadcast(_beginDragFunctor);
            this.BeginDrag?.Invoke(this, System.EventArgs.Empty);
        }

        private void DispatchEndDrag()
        {
            var target = _resolver?.GetDispatchTarget(this);

            if (target != null)
            {
                if (_resolver.UseBroadcast)
                {
                    target.Broadcast(_endDragFunctor);
                }
                else
                {
                    target.Signal(_endDragFunctor);
                }
            }

            Messaging.Broadcast(_endDragFunctor);
            this.EndDrag?.Invoke(this, System.EventArgs.Empty);
        }

        #endregion

        #region Special Types

        /// <summary>
        /// Down message handler for cursor button.
        /// </summary>
        public interface ICursorDownHandler
        {
            void OnCursorDown(CursorInputLogic cursor);
        }

        /// <summary>
        /// Up message handler for cursor button. This occurs regarldess of click timeout. You can check ClickCount > 0 to see if it's a click.
        /// </summary>
        public interface ICursorUpHandler
        {
            void OnCursorUp(CursorInputLogic cursor);
        }

        /// <summary>
        /// Message handler for when the cursor enters a new target.
        /// </summary>
        public interface ICursorEnterHandler
        {
            void OnCursorEnter(CursorInputLogic cursor);
        }

        /// <summary>
        /// Message handler for when the cursor exist a target.
        /// </summary>
        public interface ICursorExitHandler
        {
            void OnCursorExit(CursorInputLogic cursor);
        }

        /// <summary>
        /// Message handler for when the cursor presses down on the button for longer than the ClickTimeout.
        /// </summary>
        public interface ICursorButtonHeldHandler
        {
            void OnButtonHeld(CursorInputLogic cursor);
        }

        /// <summary>
        /// Message handler for when the cursor presses down and releases within the ClickTimeout.
        /// </summary>
        public interface IClickHandler
        {
            void OnClick(CursorInputLogic cursor);
        }

        /// <summary>
        /// Message handler for when the cursor presses down and release twice in succession within the ClickTimeout and DoubleClickTimeout.
        /// </summary>
        public interface IDoubleClickHandler
        {
            void OnDoubleClick(CursorInputLogic cursor);
        }

        /// <summary>
        /// Message handler for when someone has initiated a drag.
        /// </summary>
        public interface ICursorBeginDragHandler
        {
            void OnBeginDrag(CursorInputLogic cursor);
        }

        /// <summary>
        /// Message handler for when someone has ended a drag.
        /// </summary>
        public interface ICursorEndDragHandler
        {
            void OnEndDrag(CursorInputLogic cursor);
        }

        public struct CursorActivateEventData
        {

            public CursorInputLogic Cursor;
            public object Token;
            public CursorRaycastHit Hit;

            /// <summary>
            /// The number of times Used was called.
            /// </summary>
            public int UseCount { get; private set; }

            /// <summary>
            /// A ICursorActivatedHandler should call this to signal it consumed the activate event. 
            /// This will allow activators to know if the target "succeeded" at being used. 
            /// Some scripts like i_ActivateUnderCursor will signal different success/fail events based on this count.
            /// </summary>
            public void Use()
            {
                this.UseCount++;
            }

        }

        public interface ICursorInputResolver
        {
            Vector2 CursorPosition { get; }
            /// <summary>
            /// Should the target returned by GetDispatchTarget have the message broadcast rather than signaled (see: Messaging.Broadcast vs Messaging.Signal)
            /// </summary>
            bool UseBroadcast { get; }
            /// <summary>
            /// Return true if the cursor is blocked and Raycast would return null no matter what.
            /// </summary>
            bool CursorIsBlocked { get; }

            Ray GetRay();
            CursorRaycastHit Raycast(bool ignoreCursorIsBlocked = false);
            ButtonState GetClickButtonState();
            GameObject GetDispatchTarget(CursorInputLogic cursor);
            bool TestBeginDrag(CursorInputLogic cursor);
        }


        public enum SignalTargetOptions
        {
            SignalCollider = 0, //signals the GameObject the collider is attached only
            SignalRigidbody = 1, //signals the GameObject the rigidbody is attached only
            SignalEntity = 2, //signal the entity the collider is attached only
            //3 - unknown state, we're using the 4 bit to flag broadcast
            BroadcastCollider = 4, //broadcast to all GameObjects inside the collider attached
            BroadcastRigidbody = 5, //broadcast to all GameObjects inside the rigidbody
            BroadcastEntity = 6, //broadcast to all GameObjects inside the entity the collider is attached
        }

        [System.Serializable]
        public class EventSystemCursorInputResolver : ICursorInputResolver
        {

            #region Fields

            [Infobox("Taps into the EventSystem and InputModule attached to it to resolve position/press from the mousePosition and MouseButton/Up/Down respectively.")]
            [SerializeField]
            [Tooltip("The camera to use, or a proxy to it.")]
            [RespectsIProxy]
            [TypeRestriction(typeof(Camera), AllowProxy = true)]
            private UnityEngine.Object _cameraSource;

            [SerializeField]
            private int _mouseButtonIndex;

            [SerializeField]
            [Tooltip("The mask of layers to check for when raycasting. This includes both the layers that should be clickable, and those that can block clickable things.")]
            private LayerMask _layerMask = -1;
            [SerializeField]
            [Tooltip("Layers that only block but do not receive events.")]
            private LayerMask _blockingLayerMask = 0;
            [SerializeField]
            [Tooltip("How to deal with trigger colliders when raycasting.")]
            private QueryTriggerInteraction _queryTriggerOption;

            [SerializeField]
            [Tooltip("If the EventSystem reports that its pointer is over something, then this raycaster will consider itself not over anything.")]
            private bool _eventSystemUIBlocks;

            [SerializeField]
            private SignalTargetOptions _signalTarget;

            #endregion

            #region Properties

            public UnityEngine.Object CameraSource
            {
                get => _cameraSource;
                set => _cameraSource = IProxyExtensions.FilterAsProxyOrType<Camera>(value) as UnityEngine.Object;
            }

            public int MouseButtonIndex
            {
                get => _mouseButtonIndex;
                set => _mouseButtonIndex = value;
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

            public bool EventSystemUIBlock
            {
                get => _eventSystemUIBlocks;
                set => _eventSystemUIBlocks = value;
            }

            public SignalTargetOptions SignalTargetOptions
            {
                get => _signalTarget;
                set => _signalTarget = value;
            }

            #endregion

            #region Methods

            public Vector2 CursorPosition => EventSystem.current?.currentInputModule?.input?.mousePosition ?? Vector2.zero;

            public virtual bool UseBroadcast => _signalTarget >= SignalTargetOptions.BroadcastCollider;

#if (UNITY_IOS || UNITY_ANDROID)
            public virtual bool CursorIsBlocked => _eventSystemUIBlocks && ((EventSystem.current?.IsPointerOverGameObject() ?? false) || (EventSystem.current?.IsPointerOverGameObject(0) ?? false));
#else
            public virtual bool CursorIsBlocked => _eventSystemUIBlocks && (EventSystem.current?.IsPointerOverGameObject() ?? false);
#endif

            public virtual Ray GetRay()
            {
                var cam = ObjUtil.GetAsFromSource<Camera>(_cameraSource, true);
                if (cam == null) return default(Ray);

                return cam.ScreenPointToRay(EventSystem.current?.currentInputModule?.input?.mousePosition ?? Vector2.zero);
            }

            public virtual CursorRaycastHit Raycast(bool ignoreCursorIsBlocked = false)
            {
                if (!ignoreCursorIsBlocked && this.CursorIsBlocked) return default(CursorRaycastHit);

                var input = EventSystem.current?.currentInputModule?.input;
                if (input != null)
                {
                    var pos = input.mousePosition;

                    var cam = ObjUtil.GetAsFromSource<Camera>(_cameraSource, true);
                    if (cam == null) return default(CursorRaycastHit);

                    return InputUtil.TestCursorOver(cam, pos, float.PositiveInfinity, _layerMask, _queryTriggerOption, _blockingLayerMask);
                }

                return default(CursorRaycastHit);
            }

            public virtual ButtonState GetClickButtonState()
            {
                var input = EventSystem.current?.currentInputModule?.input;
                if (input != null)
                {
                    if (input.GetMouseButtonDown(_mouseButtonIndex))
                        return ButtonState.Down;
                    else if (input.GetMouseButtonUp(_mouseButtonIndex))
                        return ButtonState.Released;
                    else if (input.GetMouseButton(_mouseButtonIndex))
                        return ButtonState.Held;
                }
                return ButtonState.None;
            }

            public virtual GameObject GetDispatchTarget(CursorInputLogic cursor)
            {
                return CursorInputLogic.GetDispatchTarget(cursor, _signalTarget);
            }

            public virtual bool TestBeginDrag(CursorInputLogic cursor)
            {
                if (cursor == null) return false;

                int dist = EventSystem.current?.pixelDragThreshold ?? 0;
                return (cursor.LastButtonDownPosition - this.CursorPosition).sqrMagnitude > (dist * dist);
            }

            #endregion

        }

        [System.Serializable]
        public class EventSystem2DCursorInputResolver : ICursorInputResolver
        {

            #region Fields

            [Infobox("Taps into the EventSystem and InputModule attached to it to resolve position/press from the mousePosition and MouseButton/Up/Down respectively.")]
            [SerializeField]
            [Tooltip("The camera to use, or a proxy to it.")]
            [RespectsIProxy]
            [TypeRestriction(typeof(Camera), AllowProxy = true)]
            private UnityEngine.Object _cameraSource;

            [SerializeField]
            private int _mouseButtonIndex;

            [SerializeField]
            [Tooltip("The mask of layers to check for when raycasting. This includes both the layers that should be clickable, and those that can block clickable things.")]
            private LayerMask _layerMask = -1;
            [SerializeField]
            [Tooltip("Layers that only block but do not receive events.")]
            private LayerMask _blockingLayerMask = 0;

            [SerializeField]
            [Tooltip("If the EventSystem reports that its pointer is over something, then this raycaster will consider itself not over anything.")]
            private bool _eventSystemUIBlocks;

            [SerializeField]
            private SignalTargetOptions _signalTarget;

            #endregion

            #region Properties

            public UnityEngine.Object CameraSource
            {
                get => _cameraSource;
                set => _cameraSource = IProxyExtensions.FilterAsProxyOrType<Camera>(value) as UnityEngine.Object;
            }

            public int MouseButtonIndex
            {
                get => _mouseButtonIndex;
                set => _mouseButtonIndex = value;
            }

            public LayerMask LayerMask
            {
                get => _layerMask;
                set => _layerMask = value;
            }

            public bool EventSystemUIBlock
            {
                get => _eventSystemUIBlocks;
                set => _eventSystemUIBlocks = value;
            }

            public SignalTargetOptions SignalTargetOptions
            {
                get => _signalTarget;
                set => _signalTarget = value;
            }

            #endregion

            #region Methods

            public Vector2 CursorPosition => EventSystem.current?.currentInputModule?.input?.mousePosition ?? Vector2.zero;

            public virtual bool UseBroadcast => _signalTarget >= SignalTargetOptions.BroadcastCollider;

#if (UNITY_IOS || UNITY_ANDROID)
            public virtual bool CursorIsBlocked => _eventSystemUIBlocks && ((EventSystem.current?.IsPointerOverGameObject() ?? false) || (EventSystem.current?.IsPointerOverGameObject(0) ?? false));
#else
            public virtual bool CursorIsBlocked => _eventSystemUIBlocks && (EventSystem.current?.IsPointerOverGameObject() ?? false);
#endif

            public virtual Ray GetRay()
            {
                var cam = ObjUtil.GetAsFromSource<Camera>(_cameraSource, true);
                if (cam == null) return default(Ray);

                return cam.ScreenPointToRay(EventSystem.current?.currentInputModule?.input?.mousePosition ?? Vector2.zero);
            }

            public virtual CursorRaycastHit Raycast(bool ignoreCursorIsBlocked = false)
            {
                if (!ignoreCursorIsBlocked && this.CursorIsBlocked) return default(CursorRaycastHit);

                var input = EventSystem.current?.currentInputModule?.input;
                if (input != null)
                {
                    var pos = input.mousePosition;

                    var cam = ObjUtil.GetAsFromSource<Camera>(_cameraSource, true);
                    if (cam == null) return default(CursorRaycastHit);

                    return InputUtil.TestCursorOver2D(cam, pos, float.PositiveInfinity, _layerMask, float.NegativeInfinity, _blockingLayerMask);
                }

                return default(CursorRaycastHit);
            }

            public virtual ButtonState GetClickButtonState()
            {
                var input = EventSystem.current?.currentInputModule?.input;
                if (input != null)
                {
                    if (input.GetMouseButtonDown(_mouseButtonIndex))
                        return ButtonState.Down;
                    else if (input.GetMouseButtonUp(_mouseButtonIndex))
                        return ButtonState.Released;
                    else if (input.GetMouseButton(_mouseButtonIndex))
                        return ButtonState.Held;
                }
                return ButtonState.None;
            }

            public virtual GameObject GetDispatchTarget(CursorInputLogic cursor)
            {
                return CursorInputLogic.GetDispatchTarget(cursor, _signalTarget, true);
            }

            public virtual bool TestBeginDrag(CursorInputLogic cursor)
            {
                if (cursor == null) return false;

                int dist = EventSystem.current?.pixelDragThreshold ?? 0;
                return (cursor.LastButtonDownPosition - this.CursorPosition).sqrMagnitude > (dist * dist);
            }

            #endregion

        }

        [System.Serializable]
        public class InputManagerCursorInputResolver : ICursorInputResolver
        {

            #region Fields

            [Infobox("Taps into the IInputManager registered as a service to resolve cursor position and press from the respective device and input id's configured.")]
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
            private float _pixelDragThreshold = 10f;

            [SerializeField]
            [Tooltip("The mask of layers to check for when raycasting. This includes both the layers that should be clickable, and those that can block clickable things.")]
            private LayerMask _layerMask = -1;
            [SerializeField]
            [Tooltip("Layers that only block but do not receive events.")]
            private LayerMask _blockingLayerMask = 0;
            [SerializeField]
            [Tooltip("How to deal with trigger colliders when raycasting.")]
            private QueryTriggerInteraction _queryTriggerOption;

            [SerializeField]
            private SignalTargetOptions _signalTarget;

            #endregion

            #region Properties

            public UnityEngine.Object CameraSource
            {
                get => _cameraSource;
                set => _cameraSource = IProxyExtensions.FilterAsProxyOrType<Camera>(value) as UnityEngine.Object;
            }

            public string DeviceID
            {
                get => _deviceId;
                set => _deviceId = value;
            }

            public string CursorInputID
            {
                get => _cursorInputId;
                set => _cursorInputId = value;
            }

            public string ClickButtonInputID
            {
                get => _clickButtonInputId;
                set => _clickButtonInputId = value;
            }

            public float PixelDragThreshold
            {
                get => _pixelDragThreshold;
                set => _pixelDragThreshold = value;
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

            public SignalTargetOptions SignalTargetOptions
            {
                get => _signalTarget;
                set => _signalTarget = value;
            }

            #endregion

            #region Methods

            public Vector2 CursorPosition => this.GetInputDevice()?.GetCursorState(_cursorInputId) ?? Vector2.zero;

            public virtual bool UseBroadcast => _signalTarget >= SignalTargetOptions.BroadcastCollider;

            public virtual bool CursorIsBlocked => false;

            public IInputDevice GetInputDevice()
            {
                return string.IsNullOrEmpty(_deviceId) ? Services.Get<IInputManager>()?.Main : Services.Get<IInputManager>()?.GetDevice(_deviceId);
            }

            public virtual Ray GetRay()
            {
                var cam = ObjUtil.GetAsFromSource<Camera>(_cameraSource, true);
                if (cam == null) return default(Ray);

                return cam.ScreenPointToRay(this.GetInputDevice()?.GetCursorState(_cursorInputId) ?? Vector2.zero);
            }

            public virtual CursorRaycastHit Raycast(bool ignoreCursorIsBlocked = false)
            {
                if (!ignoreCursorIsBlocked && this.CursorIsBlocked) return default(CursorRaycastHit);

                var input = this.GetInputDevice();
                if (input != null)
                {
                    var pos = input.GetCursorState(_cursorInputId);

                    var cam = ObjUtil.GetAsFromSource<Camera>(_cameraSource, true);
                    if (cam == null) return default(CursorRaycastHit);

                    return InputUtil.TestCursorOver(cam, pos, float.PositiveInfinity, _layerMask, _queryTriggerOption, _blockingLayerMask);
                }

                return default(CursorRaycastHit);
            }

            public virtual ButtonState GetClickButtonState()
            {
                return this.GetInputDevice()?.GetButtonState(_clickButtonInputId) ?? ButtonState.None;
            }

            public virtual GameObject GetDispatchTarget(CursorInputLogic cursor)
            {
                return CursorInputLogic.GetDispatchTarget(cursor, _signalTarget);
            }

            public virtual bool TestBeginDrag(CursorInputLogic cursor)
            {
                if (cursor == null) return false;

                return (cursor.LastButtonDownPosition - this.CursorPosition).sqrMagnitude > (_pixelDragThreshold * _pixelDragThreshold);
            }

            #endregion

        }

        [System.Serializable]
        public class InputManager2DCursorInputResolver : ICursorInputResolver
        {

            #region Fields

            [Infobox("Taps into the IInputManager registered as a service to resolve cursor position and press from the respective device and input id's configured.")]
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
            private float _pixelDragThreshold = 10f;

            [SerializeField]
            [Tooltip("The mask of layers to check for when raycasting. This includes both the layers that should be clickable, and those that can block clickable things.")]
            private LayerMask _layerMask = -1;
            [SerializeField]
            [Tooltip("Layers that only block but do not receive events.")]
            private LayerMask _blockingLayerMask = 0;

            [SerializeField]
            private SignalTargetOptions _signalTarget;

            #endregion

            #region Properties

            public UnityEngine.Object CameraSource
            {
                get => _cameraSource;
                set => _cameraSource = IProxyExtensions.FilterAsProxyOrType<Camera>(value) as UnityEngine.Object;
            }

            public string DeviceID
            {
                get => _deviceId;
                set => _deviceId = value;
            }

            public string CursorInputID
            {
                get => _cursorInputId;
                set => _cursorInputId = value;
            }

            public string ClickButtonInputID
            {
                get => _clickButtonInputId;
                set => _clickButtonInputId = value;
            }

            public float PixelDragThreshold
            {
                get => _pixelDragThreshold;
                set => _pixelDragThreshold = value;
            }

            public LayerMask LayerMask
            {
                get => _layerMask;
                set => _layerMask = value;
            }

            public SignalTargetOptions SignalTargetOptions
            {
                get => _signalTarget;
                set => _signalTarget = value;
            }

            #endregion

            #region Methods

            public Vector2 CursorPosition => this.GetInputDevice()?.GetCursorState(_cursorInputId) ?? Vector2.zero;

            public virtual bool UseBroadcast => _signalTarget >= SignalTargetOptions.BroadcastCollider;

            public virtual bool CursorIsBlocked => false;

            public IInputDevice GetInputDevice()
            {
                return string.IsNullOrEmpty(_deviceId) ? Services.Get<IInputManager>()?.Main : Services.Get<IInputManager>()?.GetDevice(_deviceId);
            }

            public virtual Ray GetRay()
            {
                var cam = ObjUtil.GetAsFromSource<Camera>(_cameraSource, true);
                if (cam == null) return default(Ray);

                return cam.ScreenPointToRay(this.GetInputDevice()?.GetCursorState(_cursorInputId) ?? Vector2.zero);
            }

            public virtual CursorRaycastHit Raycast(bool ignoreCursorIsBlocked = false)
            {
                if (!ignoreCursorIsBlocked && this.CursorIsBlocked) return default(CursorRaycastHit);

                var input = this.GetInputDevice();
                if (input != null)
                {
                    var pos = input.GetCursorState(_cursorInputId);

                    var cam = ObjUtil.GetAsFromSource<Camera>(_cameraSource, true);
                    if (cam == null) return default(CursorRaycastHit);

                    return InputUtil.TestCursorOver2D(cam, pos, float.PositiveInfinity, _layerMask, float.NegativeInfinity, _blockingLayerMask);
                }

                return default(CursorRaycastHit);
            }

            public virtual ButtonState GetClickButtonState()
            {
                return this.GetInputDevice()?.GetButtonState(_clickButtonInputId) ?? ButtonState.None;
            }

            public virtual GameObject GetDispatchTarget(CursorInputLogic cursor)
            {
                return CursorInputLogic.GetDispatchTarget(cursor, _signalTarget, true);
            }

            public virtual bool TestBeginDrag(CursorInputLogic cursor)
            {
                if (cursor == null) return false;

                return (cursor.LastButtonDownPosition - this.CursorPosition).sqrMagnitude > (_pixelDragThreshold * _pixelDragThreshold);
            }

            #endregion

        }

        public static GameObject GetDispatchTarget(CursorInputLogic cursor, SignalTargetOptions option, bool treatAs2d = false)
        {
            if (cursor.Current == null) return null;

            switch (option)
            {
                case SignalTargetOptions.SignalCollider:
                case SignalTargetOptions.BroadcastCollider:
                    return cursor.Current;
                case SignalTargetOptions.SignalRigidbody:
                case SignalTargetOptions.BroadcastRigidbody:
                    if (treatAs2d)
                    {
                        var c = cursor.Current.GetComponent<Collider2D>();
                        return c != null && c.attachedRigidbody != null ? c.attachedRigidbody.gameObject : cursor.Current;
                    }
                    else
                    {
                        var c = cursor.Current.GetComponent<Collider>();
                        return c != null && c.attachedRigidbody != null ? c.attachedRigidbody.gameObject : cursor.Current;
                    }
                case SignalTargetOptions.SignalEntity:
                case SignalTargetOptions.BroadcastEntity:
                    if (cursor.CurrentEntity != null)
                        return cursor.CurrentEntity.gameObject;
                    else
                        return cursor.Current.FindRoot();
                default:
                    return null;
            }
        }

        #endregion

    }
}
