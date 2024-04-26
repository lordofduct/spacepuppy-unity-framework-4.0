using UnityEngine;
using System.Collections.Generic;

namespace com.spacepuppy.SPInput
{

    /// <summary>
    /// Input signature where 'CurrentState' represents the change in mouse position (relative to screen size) scaled by MouseSensitivity. 
    /// By default delta is only registered if the left mouse button is held. Set 'MouseActiveCallback' to override this behaviour. 
    /// </summary>
    public class MouseDeltaSignature : BaseInputSignature, IDualAxleInputSignature
    {

        static DualAxisDelegate __DEFAULT_MOUSEPOS_CALLBACK;
        static DualAxisDelegate DEFAULT_MOUSEPOS_CALLBACK => __DEFAULT_MOUSEPOS_CALLBACK ??= (() => (Vector2)Input.mousePosition);
        static ButtonDelegate __DEFAULT_MOUSEACTIVE_CALLBACK;
        static ButtonDelegate DEFAULT_MOUSEACTIVE_CALLBACK => __DEFAULT_MOUSEACTIVE_CALLBACK ??= (() => Input.GetMouseButtonDown(0));

        #region Fields

        private Vector2 _lastPosition;
        private Vector2 _currentDelta;
        private Vector2 _characterizedMousePosition;
        private Vector2 _normalizedMousePosition;

        private DualAxisDelegate _mousePositionCallback;
        private ButtonDelegate _mouseActiveCallback;

        #endregion

        #region CONSTRUCTOR

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
        /// The last delta in mouse position effected by 'MouseSensitivity'.
        /// </summary>
        public Vector2 CurrentState
        {
            get
            {
                return _currentDelta;
            }
        }

        public float DeadZone { get; set; }
        public DeadZoneCutoff Cutoff { get; set; }
        public float RadialDeadZone { get; set; }
        public DeadZoneCutoff RadialCutoff { get; set; }

        #endregion

        #region Methods

        public override void Update()
        {
            var pos = _mousePositionCallback();
            if (_mouseActiveCallback())
            {
                _currentDelta = InputUtil.CutoffDualAxis((pos - _lastPosition) * this.MouseSensitivity, this.DeadZone, this.Cutoff, this.RadialDeadZone, this.RadialCutoff);
            }
            else
            {
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
            _currentDelta = default;
        }

        #endregion

    }

}
