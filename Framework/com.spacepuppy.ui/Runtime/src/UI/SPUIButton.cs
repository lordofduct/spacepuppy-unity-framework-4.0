using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Utils;
using com.spacepuppy.Events;

namespace com.spacepuppy.UI
{

    [RequireComponent(typeof(RectTransform))]
    public class SPUIButton : Selectable, IUIComponent, IPointerClickHandler, ISubmitHandler, IObservableTrigger
    {

        [System.Flags]
        public enum ButtonMask
        {
            Left = 1 << PointerEventData.InputButton.Left,
            Right = 1 << PointerEventData.InputButton.Right,
            Middle = 1 << PointerEventData.InputButton.Middle,
        }

        #region Fields

        [SerializeField]
        private SPEvent _onClick = new SPEvent("OnClick");

        [SerializeField]
        [TimeUnitsSelector("Seconds")]
        private float _clickDuration = float.PositiveInfinity;

        [SerializeField, EnumFlags]
        private ButtonMask _acceptedButtons = ButtonMask.Left;

        [System.NonSerialized]
        private double _lastDownTime;

        #endregion

        #region CONSTRUCTOR

        public SPUIButton()
        {
#if UNITY_EDITOR
            com.spacepuppy.Dynamic.DynamicUtil.SetValue(this, "m_Transition", Transition.None);
#endif
        }

        #endregion

        #region Properties

        public float ClickDuration
        {
            get => _clickDuration;
            set => _clickDuration = value;
        }

        public SPEvent OnClick => _onClick;

        /// <summary>
        /// Reflects which mouse button effected the click. This is only set if during a click event.
        /// OnSubmit signaling a click does not set this.
        /// </summary>
        public PointerEventData.InputButton? CurrentClickButton { get; private set; }

        #endregion

        #region Methods

        private void SignalOnClick()
        {
            if (!this.IsActive() || !IsInteractable()) return;

            _onClick.ActivateTrigger(this, null);
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            var ebtn = eventData.button;
            if (((1 << (int)ebtn) & (int)_acceptedButtons) == 0) return;

            try
            {
                eventData.button = PointerEventData.InputButton.Left; //Selectable only responds to leftclick, this fakes that
                base.OnPointerDown(eventData);
                _lastDownTime = Time.unscaledTimeAsDouble;
            }
            finally
            {
                eventData.button = ebtn; //reset the button so any other IPointerXXX message receivers get the correct info
            }
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            var ebtn = eventData.button;
            if (((1 << (int)ebtn) & (int)_acceptedButtons) == 0) return;

            try
            {
                eventData.button = PointerEventData.InputButton.Left; //Selectable only responds to leftclick, this fakes that
                base.OnPointerUp(eventData);
            }
            finally
            {
                eventData.button = ebtn; //reset the button so any other IPointerXXX message receivers get the correct info
            }
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            var ebtn = eventData.button;
            if (((1 << (int)ebtn) & (int)_acceptedButtons) == 0) return;
            //if (eventData.button != PointerEventData.InputButton.Left) return;

            try
            {
                this.CurrentClickButton = ebtn;
                double delta = Time.unscaledTimeAsDouble - _lastDownTime;
                if (delta >= 0f && delta <= _clickDuration)
                {
                    SignalOnClick();
                }
            }
            finally
            {
                this.CurrentClickButton = null;
            }
        }

        public virtual void OnSubmit(BaseEventData eventData)
        {
            SignalOnClick();

            if (!this.IsActive() || !IsInteractable()) return;

            DoStateTransition(SelectionState.Pressed, false);
            this.StartCoroutine(this.OnSubmit_DelayedFadeOut(colors.fadeDuration));
        }

        private System.Collections.IEnumerable OnSubmit_DelayedFadeOut(float duration)
        {
            float t = 0f;
            while (t < duration)
            {
                yield return null;
                t += SPTime.Real.Delta;
            }

            DoStateTransition(currentSelectionState, false);
        }

        #endregion

        #region IUIComponent Interface

        public new RectTransform transform => base.transform as RectTransform;

        RectTransform IUIComponent.transform => base.transform as RectTransform;

        Component IComponent.component => this;

        [System.NonSerialized]
        private Canvas _canvas;
        Canvas IUIComponent.canvas
        {
            get
            {
                if (!_canvas) _canvas = IUIComponentExtensions.FindCanvas(this.gameObject);
                return _canvas;
            }
        }

        #endregion

        #region IObservableTrigger Interface

        BaseSPEvent[] IObservableTrigger.GetEvents()
        {
            return this.GetEvents();
        }

        protected virtual BaseSPEvent[] GetEvents()
        {
            return new BaseSPEvent[] { _onClick };
        }

        #endregion

    }
}
