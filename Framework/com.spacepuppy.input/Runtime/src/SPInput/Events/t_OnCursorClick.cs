using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Events;
using com.spacepuppy.Utils;

namespace com.spacepuppy.SPInput.Events
{

    public class t_OnCursorClick : SPComponent, IObservableTrigger, CursorInputLogic.IClickHandler, UnityEngine.EventSystems.IPointerClickHandler
    {

        #region Fields

        [SerializeField]
        private PointerFilter _pointerFilter;

        [SerializeField]
        [Tooltip("If CursorInputLogic is configure to dispatch OnClick always, this allows you to ignore it if it was a double click.")]
        private bool _ignoreDoubleClick;

        [SerializeField()]
        [UnityEngine.Serialization.FormerlySerializedAs("_trigger")]
        private SPEvent _onCursorClick = new SPEvent("OnCursorClick");

        #endregion

        #region Properties

        public PointerFilter PointerFilter
        {
            get => _pointerFilter;
            set => _pointerFilter = value;
        }

        public bool IgnoreDoubleClick
        {
            get => _ignoreDoubleClick;
            set => _ignoreDoubleClick = value;
        }

        public SPEvent OnCursorClick => _onCursorClick;

        #endregion

        #region IClickHandler Interface

        void CursorInputLogic.IClickHandler.OnClick(CursorInputLogic cursor)
        {
            if (_ignoreDoubleClick && (cursor?.LastClickWasDoubleClick ?? false)) return;
            if (_pointerFilter != null && !_pointerFilter.IsValid(cursor)) return;

            _onCursorClick.ActivateTrigger(this, null);
        }

        void UnityEngine.EventSystems.IPointerClickHandler.OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData)
        {
            if (_ignoreDoubleClick && eventData.clickCount == 2) return;
            if (_pointerFilter != null && !_pointerFilter.IsValid(eventData)) return;

            _onCursorClick.ActivateTrigger(this, null);
        }

        #endregion

        #region IObservableTrigger Interface

        BaseSPEvent[] IObservableTrigger.GetEvents()
        {
            return new BaseSPEvent[] { _onCursorClick };
        }

        #endregion

    }

}
