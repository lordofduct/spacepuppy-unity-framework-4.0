using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Events;
using com.spacepuppy.Utils;

namespace com.spacepuppy.SPInput.Events
{

    [Infobox("Requires a CursorInputLogic to be configured. This is usually part of the InputManager.\r\n\r\nNote that Down/Up are not guaranteed to happen. If the user presses down on object A and then moves to object B and releases. Only A gets the DOWN event, and only B gets the UP event.")]
    public class t_OnCursorDownUp : SPComponent, CursorInputLogic.ICursorHandler
    {

        #region Fields

        [SerializeField]
        private SPEvent _onDown;
        [SerializeField]
        private SPEvent _onUp;

        [SerializeField]
        [Tooltip("Populate with the Id of the CursorFilterLogic if you want to filter for only a specific input. Otherwise leave blank to receive all clicks.")]
        private string _cursorInputerLogicFilter;

        #endregion

        #region IClickHandler Interface

        void CursorInputLogic.ICursorHandler.OnCursorDown(ICursorInputLogic sender, Collider c)
        {
            if (!string.IsNullOrEmpty(_cursorInputerLogicFilter) && sender?.Id != _cursorInputerLogicFilter) return;

            _onDown.ActivateTrigger(this, null);
        }

        void CursorInputLogic.ICursorHandler.OnCursorUp(ICursorInputLogic sender, Collider c)
        {
            if (!string.IsNullOrEmpty(_cursorInputerLogicFilter) && sender?.Id != _cursorInputerLogicFilter) return;

            _onUp.ActivateTrigger(this, null);
        }

        #endregion

    }

}
