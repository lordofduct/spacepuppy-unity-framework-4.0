using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Events;
using com.spacepuppy.Utils;

namespace com.spacepuppy.SPInput.Events
{

    public class t_OnCursorDoubleClick : TriggerComponent, CursorInputLogic.IDoubleClickHandler, UnityEngine.EventSystems.IPointerClickHandler
    {

        #region Fields

        [SerializeField]
        private PointerFilter _pointerFilter;

        #endregion

        #region Properties

        public PointerFilter PointerFilter
        {
            get => _pointerFilter;
            set => _pointerFilter = value;
        }

        #endregion

        #region IClickHandler Interface

        void CursorInputLogic.IDoubleClickHandler.OnDoubleClick(CursorInputLogic cursor)
        {
            if (_pointerFilter != null && !_pointerFilter.IsValid(cursor)) return;

            this.ActivateTrigger();
        }

        void UnityEngine.EventSystems.IPointerClickHandler.OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData)
        {
            if (eventData.clickCount != 2) return;
            if (_pointerFilter != null && !_pointerFilter.IsValid(eventData)) return;

            this.ActivateTrigger();
        }

        #endregion

    }

}
