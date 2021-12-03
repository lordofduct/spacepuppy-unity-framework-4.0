using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Events;
using com.spacepuppy.Utils;

namespace com.spacepuppy.SPInput.Events
{

    [Infobox("Requires a CursorInputLogic to be configured. This is usually part of the InputManager.")]
    public class t_OnCursorHover : SPComponent, CursorInputLogic.ICursorEnterHandler, CursorInputLogic.ICursorExitHandler, IObservableTrigger
    {

        #region Fields

        [SerializeField]
        private SPEvent _onEnter = new SPEvent("OnEnter");
        [SerializeField]
        private SPEvent _onExit = new SPEvent("OnExit");

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

        public SPEvent OnEnter => _onEnter;

        public SPEvent OnExit => _onExit;

        #endregion

        #region IClickHandler Interface

        void CursorInputLogic.ICursorEnterHandler.OnCursorEnter(CursorInputLogic sender, Collider c)
        {
            if (!string.IsNullOrEmpty(_cursorInputLogicFilter) && sender?.Id != _cursorInputLogicFilter) return;

            _onEnter.ActivateTrigger(this, null);
        }

        void CursorInputLogic.ICursorExitHandler.OnCursorExit(CursorInputLogic sender, Collider c)
        {
            if (!string.IsNullOrEmpty(_cursorInputLogicFilter) && sender?.Id != _cursorInputLogicFilter) return;

            _onExit.ActivateTrigger(this, null);
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
