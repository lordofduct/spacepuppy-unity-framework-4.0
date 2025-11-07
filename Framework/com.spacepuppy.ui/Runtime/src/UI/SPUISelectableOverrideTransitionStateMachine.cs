using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using com.spacepuppy.Events;
using com.spacepuppy.Utils;

namespace com.spacepuppy.UI
{

    [RequireComponent(typeof(SelectableOverride))]
    public sealed class SPUISelectableOverrideTransitionStateMachine : SPComponent, IMStartOrEnableReceiver
    {

        #region Fields

        [SerializeField, ForceFromSelf]
        private SelectableOverride _selectableOverride;

        [Header("States")]
        [SerializeField]
        private GameObject _normalState;
        [SerializeField]
        private GameObject _highlightedState;
        [SerializeField]
        private GameObject _pressedState;
        [SerializeField]
        private GameObject _selectedState;
        [SerializeField]
        private GameObject _disabledState;

        private SPEventTrackedListenerToken _token;

        #endregion

        #region CONSTRUCTOR

        protected override void Awake()
        {
            if (!_selectableOverride) _selectableOverride = this.GetComponent<SelectableOverride>();
            base.Awake();
        }

        void IMStartOrEnableReceiver.OnStartOrEnable()
        {
            _token.Dispose();
            if (_selectableOverride)
            {
                _token = _selectableOverride.OnSelectStateChanged.AddTrackedListener((s, e) => this.Sync());
            }
            this.Sync();
        }

        protected override void OnDisable()
        {
            _token.Dispose();
            base.OnDisable();
        }

        #endregion

        #region Properties

        public SelectableOverride SelectableOverride => _selectableOverride;

        public GameObject NormalState { get => _normalState; set => _normalState = value; }
        public GameObject HighlightedState { get => _highlightedState; set => _highlightedState = value; }
        public GameObject PressedState { get => _pressedState; set => _pressedState = value; }
        public GameObject SelectedState { get => _selectedState; set => _selectedState = value; }
        public GameObject DisabledState { get => _disabledState; set => _disabledState = value; }

        #endregion

        #region Methods

        public void Sync()
        {
            switch (_selectableOverride ? _selectableOverride.CurrentSelectionState : SelectableOverride.SelectionState.Disabled)
            {
                case SelectableOverride.SelectionState.Normal:
                    {
                        _highlightedState.TrySetActive(false);
                        _pressedState.TrySetActive(false);
                        _selectedState.TrySetActive(false);
                        _disabledState.TrySetActive(false);
                        _normalState.TrySetActive(true);
                    }
                    break;
                case SelectableOverride.SelectionState.Highlighted:
                    {
                        _normalState.TrySetActive(false);
                        _pressedState.TrySetActive(false);
                        _selectedState.TrySetActive(false);
                        _disabledState.TrySetActive(false);
                        _highlightedState.TrySetActive(true);
                    }
                    break;
                case SelectableOverride.SelectionState.Pressed:
                    {
                        _normalState.TrySetActive(false);
                        _highlightedState.TrySetActive(false);
                        _selectedState.TrySetActive(false);
                        _disabledState.TrySetActive(false);
                        _pressedState.TrySetActive(true);
                    }
                    break;
                case SelectableOverride.SelectionState.Selected:
                    {
                        _normalState.TrySetActive(false);
                        _highlightedState.TrySetActive(false);
                        _pressedState.TrySetActive(false);
                        _disabledState.TrySetActive(false);
                        _selectedState.TrySetActive(true);
                    }
                    break;
                case SelectableOverride.SelectionState.Disabled:
                default:
                    {
                        _normalState.TrySetActive(false);
                        _highlightedState.TrySetActive(false);
                        _pressedState.TrySetActive(false);
                        _selectedState.TrySetActive(false);
                        _disabledState.TrySetActive(true);
                    }
                    break;
            }
        }

        #endregion

    }

}
