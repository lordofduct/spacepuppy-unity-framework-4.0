using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Utils;
using com.spacepuppy.Collections;

namespace com.spacepuppy.SPInput
{

    [Infobox("When a CursorHoverContext hovers over this the respective state will be enabled. On exit all states will be disabled.")]
    public sealed class CursorHoverStateMachine : MonoBehaviour, CursorInputLogic.ICursorEnterHandler, CursorInputLogic.ICursorExitHandler
    {

        public enum States
        {
            Inactive = 0,
            ActiveMismatch = 1,
            ActiveMatch = 2
        }

        public const string PROP_ACTIVESTATES = nameof(_activeStates);

        #region Fields

        [SerializeField]
        private StateInfo[] _activeStates;

        [SerializeField]
        private GameObject _hoverInactiveState;

        [SerializeField]
        private GameObject _hoverActiveButMismatchState;

        [System.NonSerialized]
        private GameObject _currentState;
        [System.NonSerialized]
        private CursorContext _currentContext;
        [System.NonSerialized]
        private CursorInputLogic _currentCursor;

        private HashSet<CursorInputLogic> _activeCursors;

        #endregion

        #region CONSTRUCTOR

        private void OnEnable()
        {
            this.SetState(States.Inactive);
        }

        #endregion

        #region Properties

        public States CurrentState
        {
            get;
            private set;
        }

        #endregion

        #region Methods

        private void SetState(States state)
        {
            switch (this.CurrentState = state)
            {
                case States.Inactive:
                    {
                        foreach (var info in _activeStates)
                        {
                            if (info.State && info.State != _hoverInactiveState) info.State.SetActive(false);
                        }
                        if (_hoverActiveButMismatchState && _hoverActiveButMismatchState != _hoverInactiveState) _hoverActiveButMismatchState.SetActive(false);
                        if (_hoverInactiveState) _hoverInactiveState.SetActive(true);
                    }
                    break;
                case States.ActiveMismatch:
                    {
                        foreach (var info in _activeStates)
                        {
                            if (info.State && info.State != _hoverInactiveState) info.State.SetActive(false);
                        }
                        if (_hoverActiveButMismatchState) _hoverActiveButMismatchState.SetActive(true);
                        if (_hoverInactiveState && _hoverInactiveState != _hoverActiveButMismatchState) _hoverInactiveState.SetActive(false);
                    }
                    break;
                case States.ActiveMatch:
                    {
                        foreach (var info in _activeStates)
                        {
                            if (info.State && info.State != _currentState) info.State.SetActive(false);
                        }
                        if (_hoverActiveButMismatchState && _hoverActiveButMismatchState != _currentState) _hoverActiveButMismatchState.SetActive(false);
                        if (_hoverInactiveState && _hoverInactiveState != _currentState) _hoverInactiveState.SetActive(false);
                        if (_currentState) _currentState.SetActive(true);
                    }
                    break;
            }
        }

        void CursorInputLogic.ICursorEnterHandler.OnCursorEnter(CursorInputLogic cursor)
        {
            if (_hoverActiveButMismatchState)
            {
                if (_activeCursors == null) _activeCursors = new HashSet<CursorInputLogic>();
                _activeCursors.Add(cursor);
            }

            if (_currentContext == null || !_currentContext.isActiveAndEnabled)
            {
                _currentState = null;
                foreach (var info in _activeStates)
                {
                    _currentContext = CursorContext.GetContext(cursor, info.Token);
                    if (_currentContext)
                    {
                        _currentState = info.State;
                        break;
                    }
                }
                _currentCursor = _currentContext ? cursor : null;
            }

            if (_currentState)
            {
                this.SetState(States.ActiveMatch);
            }
            else
            {
                this.SetState(States.ActiveMismatch);
            }
        }

        void CursorInputLogic.ICursorExitHandler.OnCursorExit(CursorInputLogic cursor)
        {
            if (_activeCursors != null) _activeCursors.Remove(cursor);

            if (cursor == _currentCursor || _currentContext == null || !_currentContext.isActiveAndEnabled)
            {
                _currentState = null;
                _currentContext = null;
                _currentCursor = null;
                this.SetState(_activeCursors?.Count > 0 ? States.ActiveMismatch : States.Inactive);
            }
        }

        #endregion

        #region Special Types

        [System.Serializable]
        public struct StateInfo
        {
            [SelectableObject]
            public UnityEngine.Object Token;
            public GameObject State;
        }

        #endregion

    }
}
