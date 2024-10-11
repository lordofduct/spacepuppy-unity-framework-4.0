using UnityEngine;
using System.Collections.Generic;

namespace com.spacepuppy.SPInput
{

    /// <summary>
    /// Input signature where 'CurrentState' represents the change in mouse position (relative to screen size) scaled by MouseSensitivity. 
    /// The default configuration is CreateNormalizedAxisOnMouseDown due to historical reasons, use the factory methods in place of constructor for 
    /// more nuanced configuration.
    /// </summary>
    public class MouseDeltaSignature : BaseInputSignature, IDualAxleInputSignature, ICursorInputSignature
    {

        static DualAxisDelegate __DEFAULT_MOUSEPOS_CALLBACK;
        static DualAxisDelegate DEFAULT_MOUSEPOS_CALLBACK => __DEFAULT_MOUSEPOS_CALLBACK ??= (() => (Vector2)Input.mousePosition);
        static ButtonDelegate __DEFAULT_MOUSEACTIVE_CALLBACK;
        static ButtonDelegate DEFAULT_MOUSEACTIVE_CALLBACK => __DEFAULT_MOUSEACTIVE_CALLBACK ??= (() => Input.GetMouseButtonDown(0));
        static ButtonDelegate __DEFAULT_MOUSEALWAYSACTIVE_CALLBACK;
        static ButtonDelegate DEFAULT_MOUSEALWAYSACTIVE_CALLBACK => __DEFAULT_MOUSEACTIVE_CALLBACK ??= (() => true);

        #region Fields

        private Vector2 _lastPosition;
        private Vector2 _trueDelta;
        private Vector2 _currentDelta;
        private Vector2 _characterizedMousePosition;
        private Vector2 _normalizedMousePosition;

        private DualAxisDelegate _mousePositionCallback;
        private ButtonDelegate _mouseActiveCallback;

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Creates a default MouseDeltaSignature, its default state of which is the same as calling 'CreateNormalizedAxisOnMouseDown'. 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="mousePositionCallback"></param>
        /// <param name="mouseActiveCallback"></param>
        public MouseDeltaSignature(string id, DualAxisDelegate mousePositionCallback = null, ButtonDelegate mouseActiveCallback = null) : base(id)
        {
            this.MousePositionCallback = mousePositionCallback;
            this.MouseActiveCallback = mouseActiveCallback;
            this.Reset();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Callback that returns the current mouse position used to calculate the 'CurrentState'. Defaults to 'Input.mousePosition'.
        /// </summary>
        public DualAxisDelegate MousePositionCallback
        {
            get => _mousePositionCallback;
            set => _mousePositionCallback = value ?? DEFAULT_MOUSEPOS_CALLBACK;
        }

        /// <summary>
        /// Callback that returns if the 'CurrentState' should be updating currently. Defaults to 'Input.GetMouseButton(0)'.
        /// </summary>
        public ButtonDelegate MouseActiveCallback
        {
            get => _mouseActiveCallback;
            set => _mouseActiveCallback = value ?? DEFAULT_MOUSEACTIVE_CALLBACK;
        }

        /// <summary>
        /// Sensitivity of the mouse delta.
        /// </summary>
        public float MouseSensitivity { get; set; } = 1f;

        /// <summary>
        /// Reflects the position of the mouse starting at the center of the screen and moved by 'CurrentState' every frame. 
        /// </summary>
        public Vector2 MousePosition => _characterizedMousePosition;

        /// <summary>
        /// The last true position of the mouse returned by 'MousePositionCallback', this and 'MousePosition' are unrelated. 
        /// </summary>
        public Vector2 RealMousePosition => _lastPosition;

        /// <summary>
        /// The same as 'MousePosition', but normalized from 0->1 where 0,0 is the lower left of the screen and 1,1 is the upper right. 
        /// </summary>
        public Vector2 NormalizedMousePosition => _normalizedMousePosition;

        /// <summary>
        /// The last delta in mouse position effected by 'MouseSensitivity'. Named 'CurrentState' to match signature of IDualAxleInputSignature interface.
        /// </summary>
        public Vector2 CurrentState => _currentDelta;

        /// <summary>
        /// Same as CurrentState.
        /// </summary>
        public Vector2 Delta => _currentDelta;

        /// <summary>
        /// The delta in pixels with no scaling or cutoff.
        /// </summary>
        public Vector2 TrueDelta => _trueDelta;

        /// <summary>
        /// Cuts off the axis if it extends past a length of 1.
        /// </summary>
        public bool CutoffAxis { get; set; } = true;

        /// <summary>
        /// Deadzone when normalizing axis. Only used if CutoffAxis is true.
        /// </summary>
        public float DeadZone { get; set; }
        /// <summary>
        /// DeadZoneCutoff when normalizing axis. Only used if CutoffAxis is true.
        /// </summary>
        public DeadZoneCutoff Cutoff { get; set; }
        /// <summary>
        /// RadialDeadZone when normalizing axis. Only used if CutoffAxis is true.
        /// </summary>
        public float RadialDeadZone { get; set; }
        /// <summary>
        /// Radial DeadZoneCutoff when normalizing axis. Only used if CutoffAxis is true.
        /// </summary>
        public DeadZoneCutoff RadialCutoff { get; set; }

        #endregion

        #region Methods

        public override void Update()
        {
            var pos = _mousePositionCallback();
            if (_mouseActiveCallback())
            {
                _trueDelta = pos - _lastPosition;
                _currentDelta = this.CutoffAxis ? InputUtil.CutoffDualAxis(_trueDelta * this.MouseSensitivity, this.DeadZone, this.Cutoff, this.RadialDeadZone, this.RadialCutoff) : _trueDelta * this.MouseSensitivity;
            }
            else
            {
                _trueDelta = default;
                _currentDelta = default;
            }

            _characterizedMousePosition.x = Mathf.Clamp(_normalizedMousePosition.x + _currentDelta.x, 0f, Screen.width);
            _characterizedMousePosition.y = Mathf.Clamp(_normalizedMousePosition.y + _currentDelta.y, 0f, Screen.height);
            _normalizedMousePosition.x = _characterizedMousePosition.x / Screen.width;
            _normalizedMousePosition.y = _characterizedMousePosition.y / Screen.height;

            _lastPosition = pos;
        }

        public void ResetMousePosition()
        {
            _characterizedMousePosition = new Vector2(Screen.width / 2f, Screen.height / 2f);
            _normalizedMousePosition = new Vector2(0.5f, 0.5f);
        }

        public void ResetMousePosition(Vector2 pos)
        {
            _characterizedMousePosition.x = Mathf.Clamp(pos.x, 0f, Screen.width);
            _characterizedMousePosition.y = Mathf.Clamp(pos.y, 0f, Screen.height);
            _normalizedMousePosition.x = _characterizedMousePosition.x / Screen.width;
            _normalizedMousePosition.y = _characterizedMousePosition.y / Screen.height;
        }

        public override void Reset()
        {
            this.ResetMousePosition();
            _lastPosition = _mousePositionCallback();
            _trueDelta = default;
            _currentDelta = default;
        }

        bool IInputSignature.GetInputIsActivated() => InputUtil.GetInputIsActivatedDefault(this as IAxleInputSignature);

        #endregion

        #region Static Factory

        /// <summary>
        /// Delta is defined as a Vector2 cutoff to length 1 registered only when mouse button 0 is down.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="mousePositionCallback"></param>
        /// <param name="mouseActiveCallback"></param>
        /// <returns></returns>
        public static MouseDeltaSignature CreateNormalizedAxisOnMouseDown(string id, float sensitivity = 1f, DualAxisDelegate mousePositionCallback = null, ButtonDelegate mouseActiveCallback = null)
        {
            return new MouseDeltaSignature(id, mousePositionCallback ?? DEFAULT_MOUSEPOS_CALLBACK, mouseActiveCallback ?? DEFAULT_MOUSEACTIVE_CALLBACK)
            {
                MouseSensitivity = sensitivity,
                CutoffAxis = true,
            };
        }

        /// <summary>
        /// Delta is defined as a Vector2 cutoff to length 1 registered always regardless of mouse button.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="mousePositionCallback"></param>
        /// <param name="mouseActiveCallback"></param>
        /// <returns></returns>
        public static MouseDeltaSignature CreateNormalizedAxis(string id, float sensitivity = 1f, DualAxisDelegate mousePositionCallback = null, ButtonDelegate mouseActiveCallback = null)
        {
            return new MouseDeltaSignature(id, mousePositionCallback ?? DEFAULT_MOUSEPOS_CALLBACK, mouseActiveCallback ?? DEFAULT_MOUSEALWAYSACTIVE_CALLBACK)
            {
                MouseSensitivity = sensitivity,
                CutoffAxis = true,
            };
        }

        public static MouseDeltaSignature CreateAxisOnMouseDown(string id, float sensitivity = 1f, DualAxisDelegate mousePositionCallback = null, ButtonDelegate mouseActiveCallback = null)
        {
            return new MouseDeltaSignature(id, mousePositionCallback ?? DEFAULT_MOUSEPOS_CALLBACK, mouseActiveCallback ?? DEFAULT_MOUSEACTIVE_CALLBACK)
            {
                MouseSensitivity = sensitivity,
                CutoffAxis = false,
            };
        }

        public static MouseDeltaSignature CreateAxis(string id, float sensitivity = 1f, DualAxisDelegate mousePositionCallback = null, ButtonDelegate mouseActiveCallback = null)
        {
            return new MouseDeltaSignature(id, mousePositionCallback ?? DEFAULT_MOUSEPOS_CALLBACK, mouseActiveCallback ?? DEFAULT_MOUSEALWAYSACTIVE_CALLBACK)
            {
                MouseSensitivity = sensitivity,
                CutoffAxis = false,
            };
        }

        #endregion

    }

}
