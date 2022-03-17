using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Utils;
using com.spacepuppy.Events;

namespace com.spacepuppy.UI
{
    public class SPUIButton : Selectable, IPointerClickHandler, ISubmitHandler, IObservableTrigger
    {

        #region Fields

        [SerializeField]
        private SPEvent _onClick = new SPEvent("OnClick");

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

        public SPEvent OnClick => _onClick;

        #endregion

        private void SignalOnClick()
        {
            if (!this.IsActive() || !IsInteractable()) return;

            _onClick.ActivateTrigger(this, null);
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;

            SignalOnClick();
        }

        public virtual void OnSubmit(BaseEventData eventData)
        {
            SignalOnClick();

            if (!this.IsActive() || !IsInteractable()) return;

            DoStateTransition(SelectionState.Pressed, false);
            this.Invoke(() =>
            {
                DoStateTransition(currentSelectionState, false);
            }, colors.fadeDuration, SPTime.Real);
        }

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
