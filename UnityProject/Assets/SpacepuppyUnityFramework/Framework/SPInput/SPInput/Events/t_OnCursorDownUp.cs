using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Events;
using com.spacepuppy.Utils;

namespace com.spacepuppy.SPInput.Events
{

    [Infobox("Requires a CursorInputLogic to be configured. This is usually part of the InputManager.\r\n\r\nNote that Down/Up are not guaranteed to happen. If the user presses down on object A and then moves to object B and releases. Only A gets the DOWN event, and only B gets the UP event.")]
    public class t_OnCursorDownUp : SPComponent, CursorInputLogic.ICursorActivateHandler, IObservableTrigger
    {

        #region Fields

        [SerializeField]
        private SPEvent _onDown = new SPEvent("OnDown");
        [SerializeField]
        private SPEvent _onUp = new SPEvent("OnUp");

        [SerializeField]
        [Tooltip("Populate with the Id of the CursorFilterLogic if you want to filter for only a specific input. Otherwise leave blank to receive all clicks.")]
        private string _cursorInputLogicFilter;

        #endregion

        #region Properties

        public string CursorInputLogicFilter
        {
            get => _cursorInputLogicFilter;
            set => _cursorInputLogicFilter = value;
        }

        public SPEvent OnDown => _onDown;

        public SPEvent OnUp => _onUp;

        #endregion

        #region IClickHandler Interface

        void CursorInputLogic.ICursorActivateHandler.OnCursorDown(CursorInputLogic sender, Collider c)
        {
            if (!string.IsNullOrEmpty(_cursorInputLogicFilter) && sender?.Id != _cursorInputLogicFilter) return;

            _onDown.ActivateTrigger(this, null);
        }

        void CursorInputLogic.ICursorActivateHandler.OnCursorUp(CursorInputLogic sender, Collider c)
        {
            if (!string.IsNullOrEmpty(_cursorInputLogicFilter) && sender?.Id != _cursorInputLogicFilter) return;

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
