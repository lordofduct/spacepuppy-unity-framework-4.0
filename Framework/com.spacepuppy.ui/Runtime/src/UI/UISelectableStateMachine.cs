using UnityEngine;
using UnityEngine.UI;

using com.spacepuppy.Events;
using com.spacepuppy.Utils;

using SelectionState = com.spacepuppy.UI.SelectableOverride.SelectionState;

namespace com.spacepuppy.UI
{

    public class UISelectableStateMachine : SPComponent
    {


        #region Fields

        [SerializeField]
        [DefaultFromSelf]
        private Selectable _target;

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

        [System.NonSerialized]
        private SelectionState _lastKnownState;
        [System.NonSerialized]
        private UpdateHook _updateHook;
        [System.NonSerialized]
        private SPEventTrackedListenerToken _selectableOverrideOnStateChangedToken;

        #endregion

        #region CONSTRUCTOR

        protected override void OnEnable()
        {
            base.OnEnable();

            this.Sync();
            this.TestStartUpdater();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            _selectableOverrideOnStateChangedToken.Dispose();
            _updateHook?.Deinitialize();
        }

        #endregion

        #region Properties

        public Selectable Target
        {
            get => _target;
            set
            {
                if (_target == value) return;
                _target = value;
#if UNITY_EDITOR
                if (Application.isPlaying && this.isActiveAndEnabled) this.TestStartUpdater();
#else
                if (this.isActiveAndEnabled) this.TestStartUpdater();
#endif
            }
        }

        public GameObject NormalState
        {
            get => _normalState;
            set => _normalState = value;
        }

        public GameObject HighlightedState
        {
            get => _highlightedState;
            set => _highlightedState = value;
        }

        public GameObject PressedState
        {
            get => _pressedState;
            set => _pressedState = value;
        }

        public GameObject SelectedState
        {
            get => _selectedState;
            set => _selectedState = value;
        }

        public GameObject DisabledState
        {
            get => _disabledState;
            set => _disabledState = value;
        }

        #endregion

        #region Methods

        public void Sync() => this.Sync(_target ? SelectableOverride.GetCurrentSelectionState(_target) : SelectionState.Normal);
        void Sync(SelectionState state)
        {
            _lastKnownState = state;
            switch (_lastKnownState)
            {
                case SelectionState.Normal:
                    _highlightedState.TrySetActive(false);
                    _pressedState.TrySetActive(false);
                    _selectedState.TrySetActive(false);
                    _disabledState.TrySetActive(false);
                    _normalState.TrySetActive(true);
                    break;
                case SelectionState.Highlighted:
                    _normalState.TrySetActive(false);
                    _pressedState.TrySetActive(false);
                    _selectedState.TrySetActive(false);
                    _disabledState.TrySetActive(false);
                    _highlightedState.TrySetActive(true);
                    break;
                case SelectionState.Pressed:
                    _normalState.TrySetActive(false);
                    _highlightedState.TrySetActive(false);
                    _selectedState.TrySetActive(false);
                    _disabledState.TrySetActive(false);
                    _pressedState.TrySetActive(true);
                    break;
                case SelectionState.Selected:
                    _normalState.TrySetActive(false);
                    _highlightedState.TrySetActive(false);
                    _pressedState.TrySetActive(false);
                    _disabledState.TrySetActive(false);
                    _selectedState.TrySetActive(true);
                    break;
                case SelectionState.Disabled:
                    _normalState.TrySetActive(false);
                    _highlightedState.TrySetActive(false);
                    _pressedState.TrySetActive(false);
                    _selectedState.TrySetActive(false);
                    _disabledState.TrySetActive(true);
                    break;
            }
        }

        private void TestStartUpdater()
        {
            if (_target)
            {
                if (_target.TryGetComponent(out SelectableOverride sover))
                {
                    _updateHook?.Deinitialize();
                    _selectableOverrideOnStateChangedToken.Dispose();
                    _selectableOverrideOnStateChangedToken = sover.OnSelectStateChanged.AddTrackedListener((s, e) =>
                    {
                        this.Sync();
                    });
                }
                else
                {
                    _selectableOverrideOnStateChangedToken.Dispose();
                    (_updateHook ??= new UpdateHook()).Initialize(this, _target);
                }
            }
            else
            {
                _selectableOverrideOnStateChangedToken.Dispose();
                _updateHook?.Deinitialize();
            }
        }

        #endregion

        #region Static Utils

        class UpdateHook : IUpdateable
        {

            public UISelectableStateMachine owner;
            public Selectable target;
            public void Initialize(UISelectableStateMachine owner, Selectable target)
            {
                this.owner = owner;
                this.target = target;
                if (target)
                {
                    GameLoop.TardyUpdatePump.Add(this);
                }
                else
                {
                    GameLoop.TardyUpdatePump.Remove(this);
                }
            }
            public void Deinitialize()
            {
                GameLoop.TardyUpdatePump.Remove(this);
            }

            void IUpdateable.Update()
            {
                if (this.target)
                {
                    var e = SelectableOverride.GetCurrentSelectionState(target);
                    if (e != owner._lastKnownState) owner.Sync(e);
                }
                else
                {
                    this.Deinitialize();
                }
            }

        }

        #endregion

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (this.isActiveAndEnabled && Application.isPlaying)
            {
                this.TestStartUpdater();
            }
        }
#endif

    }

}
