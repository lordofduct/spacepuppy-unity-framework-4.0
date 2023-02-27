using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace com.spacepuppy.SPInput
{

    public static class InputUtil
    {

        public const float DEFAULT_AXLEBTNDEADZONE = 0.707f;

        /// <summary>
        /// Get the mouse position as a value where (0,0) is lower-left and (1,1) is upper right corner of the screen.
        /// </summary>
        /// <returns></returns>
        public static Vector2 GetNormalizedMousePosition()
        {
            var v = (Vector2)Input.mousePosition;
            v.x /= Screen.width;
            v.y /= Screen.height;
            return v;
        }

        #region Axis Extensions

        public static float CutoffAxis(float value, float deadzone, DeadZoneCutoff cutoff, bool clampNormalized = true)
        {
            if (deadzone < 0f) deadzone = 0f;

            //no larger than 1
            if (clampNormalized && Mathf.Abs(value) > 1f) return Mathf.Sign(value);

            switch (cutoff)
            {
                case DeadZoneCutoff.Scaled:
                    if (Mathf.Abs(value) < deadzone) return 0f;
                    return Mathf.Sign(value) * (Mathf.Abs(value) - deadzone) / (1f - deadzone);
                case DeadZoneCutoff.Shear:
                    return (Mathf.Abs(value) < deadzone) ? 0f : value;
                default:
                    return value;
            }
        }

        public static Vector2 CutoffDualAxis(Vector2 value, float axleDeadzone, DeadZoneCutoff axleCutoff, bool clampNormalized = true)
        {
            if (axleDeadzone < 0f) axleDeadzone = 0f;

            if (axleDeadzone > 0f)
            {
                value.x = CutoffAxis(value.x, axleDeadzone, axleCutoff);
                value.y = CutoffAxis(value.y, axleDeadzone, axleCutoff);
            }

            //no larger than 1
            if (clampNormalized && value.sqrMagnitude > 1f) return value.normalized;

            return value;
        }

        public static Vector2 CutoffDualAxis(Vector2 value, float axleDeadzone, DeadZoneCutoff axleCutoff, float radialDeadzone, DeadZoneCutoff radialCutoff, bool clampNormalized = true)
        {
            if (axleDeadzone < 0f) axleDeadzone = 0f;
            if (radialDeadzone < 0f) radialDeadzone = 0f;

            if (axleDeadzone > 0f)
            {
                value.x = CutoffAxis(value.x, axleDeadzone, axleCutoff);
                value.y = CutoffAxis(value.y, axleDeadzone, axleCutoff);
            }

            //no larger than 1
            if (clampNormalized && value.sqrMagnitude > 1f) return value.normalized;

            if (radialDeadzone > 0f)
            {
                switch (radialCutoff)
                {
                    case DeadZoneCutoff.Scaled:
                        if (value.sqrMagnitude < radialDeadzone * radialDeadzone) return Vector2.zero;
                        value = value.normalized * (value.magnitude - radialDeadzone) / (1f - radialDeadzone);
                        break;
                    case DeadZoneCutoff.Shear:
                        if (value.sqrMagnitude < radialDeadzone * radialDeadzone) value = Vector2.zero;
                        break;
                }
            }

            return value;
        }

        #endregion

        #region Button Extensions

        public static void ConsumeButtonState(this IInputDevice device, string buttonId)
        {
            if (device.GetSignature(buttonId) is IButtonInputSignature sig)
            {
                sig.Consume();
            }
        }

        public static void ConsumeButtonState<T>(this IMappedInputDevice<T> device, T buttonId) where T : struct, System.IConvertible
        {
            if (device.GetSignature(buttonId) is IButtonInputSignature sig)
            {
                sig.Consume();
            }
        }

        public static ButtonState ConsumeButtonState(ButtonState current)
        {
            switch (current)
            {
                case ButtonState.Released:
                case ButtonState.None:
                    return ButtonState.None;
                case ButtonState.Down:
                case ButtonState.Held:
                    return ButtonState.Held;
                default:
                    return ButtonState.None;
            }
        }

        /// <summary>
        /// Calculates the next button state from the current state and if the button is active or not.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="isButtonActive"></param>
        /// <returns></returns>
        public static ButtonState GetNextButtonState(ButtonState current, bool isButtonActive)
        {
            if (isButtonActive)
            {
                switch (current)
                {
                    case ButtonState.None:
                    case ButtonState.Released:
                        return ButtonState.Down;
                    case ButtonState.Down:
                    case ButtonState.Held:
                        return ButtonState.Held;
                }
            }
            else
            {
                switch (current)
                {
                    case ButtonState.None:
                    case ButtonState.Released:
                        return ButtonState.None;
                    case ButtonState.Down:
                    case ButtonState.Held:
                        return ButtonState.Released;
                }
            }

            return ButtonState.None;
        }

        /// <summary>
        /// During FixedUpdate of a InputSignature, this will calculate the current fixed button state based on the last fixed state and the current state as polled by Update. 
        /// Use this if Update does very complicated stuff that is time consuming to calculate twice in both Update and FixedUpdate. 
        /// </summary>
        /// <param name="currentFixedState"></param>
        /// <param name="current"></param>
        /// <returns></returns>
        public static ButtonState GetNextFixedButtonStateFromCurrent(ButtonState currentFixedState, ButtonState current)
        {
            switch (currentFixedState)
            {
                case ButtonState.Released:
                    return ButtonState.None;
                case ButtonState.None:
                    if (current > ButtonState.None)
                        return ButtonState.Down;
                    else
                        return ButtonState.None;
                case ButtonState.Down:
                case ButtonState.Held:
                    if (current > ButtonState.None)
                        return ButtonState.Held;
                    else
                        return ButtonState.Released;
                default:
                    return ButtonState.None;
            }
        }

        
        public static ButtonPress GetButtonPress(this IInputDevice device, string id, float duration)
        {
            if (device == null) return ButtonPress.None;

            var sig = device.GetSignature(id) as IButtonInputSignature;
            if (sig == null) return ButtonPress.None;

            return sig.CurrentState.ResolvePressState(sig.LastDownTime, duration);
        }

        public static ButtonPress GetButtonPress<T>(this IMappedInputDevice<T> device, T id, float duration) where T : struct, System.IConvertible
        {
            if (device == null) return ButtonPress.None;

            var sig = device.GetSignature(id) as IButtonInputSignature;
            if (sig == null) return ButtonPress.None;

            return sig.CurrentState.ResolvePressState(sig.LastDownTime, duration);
        }

        public static ButtonPress GetButtonPress(this IButtonInputSignature button, float duration)
        {
            if (button == null) return ButtonPress.None;

            return button.CurrentState.ResolvePressState(button.LastDownTime, duration);
        }

        /// <summary>
        /// Returns a ButtonPress state based on the last time the ButtonState was 'Down'.
        /// </summary>
        /// <param name="state">The ButtonState to resolve</param>
        /// <param name="lastDownTime">The last time in Time.unscaledTimeAsDouble that ButtonState was 'Down'</param>
        /// <param name="duration">The duration that counts as a 'tap/click'</param>
        /// <returns></returns>
        public static ButtonPress ResolvePressState(this ButtonState state, double lastDownTime, float duration)
        {
            switch(state)
            {
                case ButtonState.Released:
                    return (Time.unscaledTimeAsDouble - lastDownTime) <= duration ? ButtonPress.Tapped : ButtonPress.Released;
                case ButtonState.None:
                    return ButtonPress.None;
                case ButtonState.Down:
                    return ButtonPress.Down;
                case ButtonState.Held:
                    return (Time.unscaledTimeAsDouble - lastDownTime) <= duration ? ButtonPress.Holding : ButtonPress.Held;
                default:
                    return ButtonPress.None;
            }
        }

        public static bool GetButtonRecentlyDown(this IInputDevice device, string id, float duration)
        {
            if (device == null) return false;

            var sig = device.GetSignature(id) as IButtonInputSignature;
            if (sig == null) return false;

            return (Time.unscaledTimeAsDouble - sig.LastDownTime) <= duration;
        }

        public static bool GetButtonRecentlyDown<T>(this IMappedInputDevice<T> device, T id, float duration) where T : struct, System.IConvertible
        {
            if (device == null) return false;

            var sig = device.GetSignature(id) as IButtonInputSignature;
            if (sig == null) return false;

            return (Time.unscaledTimeAsDouble - sig.LastDownTime) <= duration;
        }

        public static bool GetButtonRecentlyDown(this IButtonInputSignature button, float duration)
        {
            if (button == null) return false;
            return (Time.unscaledTimeAsDouble - button.LastDownTime) <= duration;
        }

        public static double TimeSinceLastDown(this IButtonInputSignature button)
        {
            return button != null ? (Time.unscaledTimeAsDouble - button.LastDownTime) : double.PositiveInfinity;
        }

        public static double TimeSinceLastRelease(this IButtonInputSignature button)
        {
            return button != null ? (Time.unscaledTimeAsDouble - button.LastReleaseTime) : double.PositiveInfinity;
        }

        public static double LastButtonFullPressDuration(this IButtonInputSignature button)
        {
            if (button == null) return 0d;
            if (button.LastReleaseTime < button.LastDownTime) return 0d;
            if (double.IsInfinity(button.LastReleaseTime)) return 0d;

            return button.LastReleaseTime - button.LastDownTime;
        }

        #endregion

        #region Generic Input Extensions

        public static bool GetInputIsActivated(this IInputSignature sig)
        {
            if (sig == null) return false;

            if (sig is IButtonInputSignature)
                return (sig as IButtonInputSignature).CurrentState != ButtonState.None;
            else if (sig is IAxleInputSignature)
                return (sig as IAxleInputSignature).CurrentState > 0f;
            else if (sig is IDualAxleInputSignature)
                return (sig as IDualAxleInputSignature).CurrentState.sqrMagnitude > 0.0001f;
            else if (sig is IInputDevice)
                return (sig as IInputDevice).AnyInputActivated;

            return false;
        }

        #endregion

        #region Cursor Extensions

        public static CursorRaycastHit TestCursorOver(Camera cursorCamera, Vector2 cursorPos, float maxDistance = float.PositiveInfinity, int layerMask = Physics.AllLayers, QueryTriggerInteraction query = QueryTriggerInteraction.UseGlobal, int blockingLayerMask = 0)
        {
            if (cursorCamera)
            {
                var ray = cursorCamera.ScreenPointToRay(cursorPos);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, maxDistance, layerMask | blockingLayerMask, query) && (blockingLayerMask == 0 || ((1 << hit.collider.gameObject.layer) & blockingLayerMask) == 0))
                {
                    return (CursorRaycastHit)hit;
                }
            }

            return default(CursorRaycastHit);
        }

        public static bool TestCursorOver(Camera cursorCamera, Vector2 cursorPos, out Collider collider, float maxDistance = float.PositiveInfinity, int layerMask = Physics.AllLayers, QueryTriggerInteraction query = QueryTriggerInteraction.UseGlobal, int blockingLayerMask = 0)
        {
            collider = null;
            if (cursorCamera == null) return false;

            var ray = cursorCamera.ScreenPointToRay(cursorPos);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, maxDistance, layerMask | blockingLayerMask, query) && (blockingLayerMask == 0 || ((1 << hit.collider.gameObject.layer) & blockingLayerMask) == 0))
            {
                collider = hit.collider;
                return true;
            }

            return false;
        }

        public static bool TestCursorOverEntity(Camera cursorCamera, Vector2 cursorPos, out SPEntity entity, float maxDistance = float.PositiveInfinity, int layerMask = Physics.AllLayers, QueryTriggerInteraction query = QueryTriggerInteraction.UseGlobal, int blockingLayerMask = 0)
        {
            Collider c;
            if(TestCursorOver(cursorCamera, cursorPos, out c, maxDistance, layerMask, query, blockingLayerMask))
            {
                entity = SPEntity.Pool.GetFromSource(c);
                return !object.ReferenceEquals(entity, null);
            }
            else
            {
                entity = null;
                return false;
            }
        }

        public static bool TestCursorOverEntity<T>(Camera cursorCamera, Vector2 cursorPos, out T hitTarget, float maxDistance = float.PositiveInfinity, int layerMask = Physics.AllLayers, QueryTriggerInteraction query = QueryTriggerInteraction.UseGlobal, int blockingLayerMask = 0) where T : class
        {
            Collider c;
            if (TestCursorOver(cursorCamera, cursorPos, out c, maxDistance, layerMask, query, blockingLayerMask))
            {
                hitTarget = com.spacepuppy.Utils.ComponentUtil.FindComponent<T>(c);
                return !object.ReferenceEquals(hitTarget, null);
            }
            else
            {
                hitTarget = null;
                return false;
            }
        }

        public static CursorRaycastHit TestCursorOver2D(Camera cursorCamera, Vector2 cursorPos, float maxDistance = float.PositiveInfinity, int layerMask = Physics.AllLayers, float minDepth = float.NegativeInfinity, int blockingLayerMask = 0)
        {
            if(cursorCamera)
            {
                var hit = Physics2D.Raycast(cursorCamera.ScreenToWorldPoint(cursorPos), Vector2.zero, maxDistance, layerMask | blockingLayerMask, minDepth);
                if (hit && (blockingLayerMask == 0 || ((1 << hit.collider.gameObject.layer) & blockingLayerMask) == 0))
                {
                    return (CursorRaycastHit)hit;
                }
            }
            return default(CursorRaycastHit);
        }

        public static bool TestCursorOver2D(Camera cursorCamera, Vector2 cursorPos, out Collider2D collider, float maxDistance = float.PositiveInfinity, int layerMask = Physics.AllLayers, float minDepth = float.NegativeInfinity, int blockingLayerMask = 0)
        {
            if(cursorCamera == null)
            {
                collider = null;
                return false;
            }

            var hit = Physics2D.Raycast(cursorCamera.ScreenToWorldPoint(cursorPos), Vector2.zero, maxDistance, layerMask | blockingLayerMask, minDepth);
            if(hit && (blockingLayerMask == 0 || ((1 << hit.collider.gameObject.layer) & blockingLayerMask) == 0))
            {
                collider = hit.collider;
                return true;
            }
            else
            {
                collider = null;
                return false;
            }
        }

        #endregion

    }

}
