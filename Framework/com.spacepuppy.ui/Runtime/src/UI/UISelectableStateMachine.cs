using UnityEngine;
using UnityEngine.UI;
using System.Reflection;

using com.spacepuppy.Utils;

namespace com.spacepuppy.UI
{

    public class UISelectableStateMachine : SPComponent
    {

        enum SelectionState
        {
            Normal,
            Highlighted,
            Pressed,
            Selected,
            Disabled,
        }


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
        private Coroutine _routine;

        #endregion

        #region CONSTRUCTOR

        protected override void OnEnable()
        {
            base.OnEnable();

            this.TestStartRoutine();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (_routine != null)
            {
                StopCoroutine(_routine);
                _routine = null;
            }
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
                if (this.isActiveAndEnabled) this.TestStartRoutine();
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

        public void Sync() => this.Sync( _target ? SelectableProtectedHook.GetCurrentSelectionState(_target) : SelectionState.Normal);
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

        private void TestStartRoutine()
        {
            if (_target)
            {
                if (_routine != null) StopCoroutine(_routine);
                _routine = this.StartCoroutine(this.UpdateRoutine(_target));
            }
            else if (_routine != null)
            {
                StopCoroutine(_routine);
                _routine = null;
            }
        }

        private System.Collections.IEnumerator UpdateRoutine(Selectable selectable)
        {
            if (!selectable) yield break;

            var del = SelectableProtectedHook.CreateCurrentSelectionStateGetterDelegate(selectable);
            SelectionState e;
            while (_target)
            {
                e = (SelectionState)del();
                if (e != _lastKnownState) this.Sync(e);
                yield return null;
            }

            _routine = null;
        }

        #endregion

        #region Static Utils

        class SelectableProtectedHook : Selectable
        {
            const string PROP_CURRENTSELECTIONSTATE = nameof(SelectableProtectedHook.currentSelectionState);
            static readonly PropertyInfo CurrentSelectoinStatePropertyInfo = typeof(Selectable).GetProperty(PROP_CURRENTSELECTIONSTATE, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.GetProperty);

            public static com.spacepuppy.UI.UISelectableStateMachine.SelectionState GetCurrentSelectionState(Selectable selectable) => (com.spacepuppy.UI.UISelectableStateMachine.SelectionState)((int)CurrentSelectoinStatePropertyInfo.GetValue(selectable));

            public static System.Func<int> CreateCurrentSelectionStateGetterDelegate(Selectable selectable)
            {
                return CurrentSelectoinStatePropertyInfo.GetMethod.CreateDelegate(typeof(System.Func<int>), selectable) as System.Func<int>;
            }

        }

        #endregion

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (this.isActiveAndEnabled && Application.isPlaying)
            {
                this.TestStartRoutine();
            }
        }
#endif

    }

}
