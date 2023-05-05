using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Events;
using com.spacepuppy.Utils;

namespace com.spacepuppy.SPInput.Events
{

    public class t_OnCursorDoubleClick : SPComponent, IObservableTrigger, CursorInputLogic.IDoubleClickHandler, UnityEngine.EventSystems.IPointerClickHandler
    {

        #region Fields

        [SerializeField]
        private PointerFilter _pointerFilter;

        [SerializeField()]
        [UnityEngine.Serialization.FormerlySerializedAs("_trigger")]
        private SPEvent _onCursorDoubleClick = new SPEvent("OnCursorDoubleClick");

        #endregion

        #region Properties

        public PointerFilter PointerFilter
        {
            get => _pointerFilter;
            set => _pointerFilter = value;
        }

        public SPEvent OnCursorDoubleClick => _onCursorDoubleClick;

        #endregion

        #region IClickHandler Interface

        void CursorInputLogic.IDoubleClickHandler.OnDoubleClick(CursorInputLogic cursor)
        {
            if (_pointerFilter != null && !_pointerFilter.IsValid(cursor)) return;

            _onCursorDoubleClick.ActivateTrigger(this, null);
        }

        void UnityEngine.EventSystems.IPointerClickHandler.OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData)
        {
            if (eventData.clickCount != 2) return;
            if (_pointerFilter != null && !_pointerFilter.IsValid(eventData)) return;

            _onCursorDoubleClick.ActivateTrigger(this, null);
        }

        #endregion

        #region IObservableTrigger Interface

        BaseSPEvent[] IObservableTrigger.GetEvents()
        {
            return new BaseSPEvent[] { _onCursorDoubleClick };
        }

        #endregion

    }

}
