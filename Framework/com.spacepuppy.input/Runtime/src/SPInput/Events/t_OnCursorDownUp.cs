using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Events;
using com.spacepuppy.Utils;

namespace com.spacepuppy.SPInput.Events
{

    public class t_OnCursorDownUp : SPComponent, CursorInputLogic.ICursorDownHandler, CursorInputLogic.ICursorUpHandler, UnityEngine.EventSystems.IPointerDownHandler, UnityEngine.EventSystems.IPointerUpHandler, IObservableTrigger
    {

        #region Fields

        [SerializeField]
        private SPEvent _onDown = new SPEvent("OnDown");
        [SerializeField]
        private SPEvent _onUp = new SPEvent("OnUp");

        [SerializeField]
        private PointerFilter _pointerFilter;

        #endregion

        #region Properties

        public PointerFilter PointerFilter
        {
            get => _pointerFilter;
            set => _pointerFilter = value;
        }

        public SPEvent OnDown => _onDown;

        public SPEvent OnUp => _onUp;

        #endregion

        #region IPointer Interface

        void CursorInputLogic.ICursorDownHandler.OnCursorDown(CursorInputLogic cursor)
        {
            if (_pointerFilter != null && !_pointerFilter.IsValid(cursor)) return;

            _onDown.ActivateTrigger(this, null);
        }

        void CursorInputLogic.ICursorUpHandler.OnCursorUp(CursorInputLogic cursor)
        {
            if (_pointerFilter != null && !_pointerFilter.IsValid(cursor)) return;

            _onUp.ActivateTrigger(this, null);
        }

        void UnityEngine.EventSystems.IPointerDownHandler.OnPointerDown(UnityEngine.EventSystems.PointerEventData eventData)
        {
            if (_pointerFilter != null && !_pointerFilter.IsValid(eventData)) return;

            _onDown.ActivateTrigger(this, null);
        }

        void UnityEngine.EventSystems.IPointerUpHandler.OnPointerUp(UnityEngine.EventSystems.PointerEventData eventData)
        {
            if (_pointerFilter != null && !_pointerFilter.IsValid(eventData)) return;

            _onUp.ActivateTrigger(this, null);
        }

        #endregion

        #region IObserverableTrigger Interface

        BaseSPEvent[] IObservableTrigger.GetEvents()
        {
            return new BaseSPEvent[] { _onDown, _onUp };
        }

        #endregion

    }

}
