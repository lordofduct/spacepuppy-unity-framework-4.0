using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Events;
using com.spacepuppy.Utils;

namespace com.spacepuppy.SPInput.Events
{

    [Infobox("Requires a CursorInputLogic to be configured. This is usually part of the InputManager.\r\n\r\nThis occurs if the cursor button is pressed and not released within the ClickTimeout.")]
    public class t_OnCursorHeld : TriggerComponent, CursorInputLogic.ICursorButtonHeldHandler
    {

        #region Fields

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

        #endregion

        #region IClickHandler Interface

        void CursorInputLogic.ICursorButtonHeldHandler.OnButtonHeld(CursorInputLogic sender, Collider c)
        {
            if (!string.IsNullOrEmpty(_cursorInputLogicFilter) && sender?.Id != _cursorInputLogicFilter) return;

            this.ActivateTrigger();
        }

        #endregion

    }
}
