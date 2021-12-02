using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Events;
using com.spacepuppy.Utils;

namespace com.spacepuppy.SPInput.Events
{

    [Infobox("Requires a CursorInputLogic to be configured. This is usually part of the InputManager.")]
    public class t_OnCursorDoubleClick : TriggerComponent, CursorInputLogic.IDoubleClickHandler
    {

        #region Fields

        [SerializeField]
        [Tooltip("Populate with the Id of the CursorFilterLogic if you want to filter for only a specific input. Otherwise leave blank to receive all clicks.")]
        private string _cursorInputerLogicFilter;

        #endregion

        #region IClickHandler Interface

        void CursorInputLogic.IDoubleClickHandler.OnDoubleClick(ICursorInputLogic sender, Collider c)
        {
            if (!string.IsNullOrEmpty(_cursorInputerLogicFilter) && sender?.Id != _cursorInputerLogicFilter) return;

            this.ActivateTrigger();
        }

        #endregion

    }

}
