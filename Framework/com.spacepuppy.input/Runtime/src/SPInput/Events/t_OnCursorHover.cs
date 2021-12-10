using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Events;
using com.spacepuppy.Utils;
using UnityEngine.EventSystems;

namespace com.spacepuppy.SPInput.Events
{

    public class t_OnCursorHover : SPComponent, CursorInputLogic.ICursorEnterHandler, CursorInputLogic.ICursorExitHandler, UnityEngine.EventSystems.IPointerEnterHandler, UnityEngine.EventSystems.IPointerExitHandler, IObservableTrigger
    {

        #region Fields

        [SerializeField]
        private SPEvent _onEnter = new SPEvent("OnEnter");
        [SerializeField]
        private SPEvent _onExit = new SPEvent("OnExit");

        [SerializeField]
        private PointerFilter _pointerFilter;

        #endregion

        #region Properties

        public PointerFilter PointerFilter
        {
            get => _pointerFilter;
            set => _pointerFilter = value;
        }

        public SPEvent OnEnter => _onEnter;

        public SPEvent OnExit => _onExit;

        #endregion

        #region IPointer Interface

        void CursorInputLogic.ICursorEnterHandler.OnCursorEnter(CursorInputLogic cursor)
        {
            if (_pointerFilter != null && !_pointerFilter.IsValid(cursor)) return;

            _onEnter.ActivateTrigger(this, null);
        }

        void CursorInputLogic.ICursorExitHandler.OnCursorExit(CursorInputLogic cursor)
        {
            if (_pointerFilter != null && !_pointerFilter.IsValid(cursor)) return;

            _onExit.ActivateTrigger(this, null);
        }

        void UnityEngine.EventSystems.IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if (_pointerFilter != null && !_pointerFilter.IsValid(eventData)) return;

            _onEnter.ActivateTrigger(this, null);
        }

        void UnityEngine.EventSystems.IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            if (_pointerFilter != null && !_pointerFilter.IsValid(eventData)) return;

            _onEnter.ActivateTrigger(this, null);
        }

        #endregion

        #region IObserverableTrigger Interface

        BaseSPEvent[] IObservableTrigger.GetEvents()
        {
            return new BaseSPEvent[] { _onEnter, _onExit };
        }

        #endregion

    }

}
