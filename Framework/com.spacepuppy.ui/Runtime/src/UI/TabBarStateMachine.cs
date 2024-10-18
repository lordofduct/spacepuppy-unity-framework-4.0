using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy.Project;
using com.spacepuppy.Utils;

namespace com.spacepuppy.UI
{

#if UNITY_EDITOR
    [InsertButton("Configure Forwards", nameof(TabBarStateMachine.ConfigureForwards), SupportsMultiObjectEditing = false, RecordUndo = true, ValidateShowCallback = nameof(TabBarStateMachine.ValidateShowConfigurationButtons))]
    [InsertButton("Configure Reverse", nameof(TabBarStateMachine.ConfigureReverse), SupportsMultiObjectEditing = false, RecordUndo = true, ValidateShowCallback = nameof(TabBarStateMachine.ValidateShowConfigurationButtons))]
#endif
    public sealed class TabBarStateMachine : SPComponent, IMStartOrEnableReceiver
    {

        #region Fields

        [SerializeField, DefaultFromSelf]
        private InterfaceRef<IStateMachine> _stateMachine = new();
        [SerializeField, ReorderableArray]
        private List<ButtonRef> _buttons = new();

        [SerializeField]
        private InterfaceRefOrPicker<IMode> _mode = new (ByIndex.Default);

        [SerializeField, Tooltip("If the button for the currently selected state has a 'SelectableOverride' on it, it'll be flagged as highlighted.")]
        private bool _tryHighlightSelectedTabButton;

        [System.NonSerialized]
        private List<ButtonRef.TrackedListenerToken> _buttonOnClickHooks = new();
        [System.NonSerialized]
        private TrackedEventListenerToken _stateMachineStateChangedHook;

        #endregion

        #region CONSTRUCTOR

        void IMStartOrEnableReceiver.OnStartOrEnable()
        {
            this.Reinitialize();
        }

        protected override void OnDisable()
        {
            this.RemoveTrackedListeners();
            base.OnDisable();
        }

        #endregion

        #region Properties

        public IStateMachine StateMachine
        {
            get => _stateMachine.Value;
            set => _stateMachine.Value = value;
        }

        public List<ButtonRef> Buttons => _buttons;

        public IMode Mode
        {
            get => _mode.Value;
            set => _mode.Value = value;
        }

        public bool TryHighlightSelectedTabButton
        {
            get => _tryHighlightSelectedTabButton;
            set => _tryHighlightSelectedTabButton = value;
        }

        #endregion

        #region Methods

        public void Reinitialize()
        {
            this.RemoveTrackedListeners();
            foreach (var btn in _buttons)
            {
                _buttonOnClickHooks.Add(btn.AddTrackedListener(this.btn_OnClick));
            }
            if (this.StateMachine != null)
            {
                _stateMachineStateChangedHook = this.StateMachine.AddTrackedStateChangedListener(stateMachine_StateChanged);
            }
            if (_tryHighlightSelectedTabButton)
            {
                this.Mode?.SyncHighlighted(this);
            }
        }

        void RemoveTrackedListeners()
        {
            _stateMachineStateChangedHook.Dispose();
            foreach (var d in _buttonOnClickHooks)
            {
                d.Dispose();
            }
            _buttonOnClickHooks.Clear();
        }

        void btn_OnClick(object sender, System.EventArgs e)
        {
            if (sender is Selectable btn)
            {
                this.Mode?.GoToState(this, btn);
            }
        }

        void stateMachine_StateChanged(object sender, System.EventArgs e)
        {
            if (_tryHighlightSelectedTabButton)
            {
                this.Mode?.SyncHighlighted(this);
            }
        }

        #endregion

        #region Special Types

        public interface IMode
        {
            void GoToState(TabBarStateMachine tabbar, Selectable btn);
            void SyncHighlighted(TabBarStateMachine tabbar);
        }

        [System.Serializable]
        public class ByIndex : IMode
        {
            public static readonly ByIndex Default = new();

            public void GoToState(TabBarStateMachine tabbar, Selectable btn)
            {
                if (tabbar == null || tabbar.StateMachine.IsNullOrDestroyed()) return;

                int i = tabbar.Buttons.IndexOf(btn, (o, a) => object.ReferenceEquals(a, o.Button));
                if (i >= 0)
                {
                    tabbar.StateMachine?.GoToState(i);
                }
            }

            public void SyncHighlighted(TabBarStateMachine tabbar)
            {
                if (tabbar == null || tabbar.StateMachine.IsNullOrDestroyed()) return;

                int current = tabbar.StateMachine.CurrentStateIndex ?? -1;
                for (int i = 0; i < tabbar.Buttons.Count; i++)
                {
                    if (tabbar.Buttons[i].Button && tabbar.Buttons[i].Button.TryGetComponent(out SelectableOverride h))
                    {
                        h.OverrideHighlighted = (current == i);
                    }
                }
            }

        }

        [System.Serializable]
        public class ByButtonName : IMode
        {
            public static readonly ByButtonName Default = new();

            public void GoToState(TabBarStateMachine tabbar, Selectable btn)
            {
                if (tabbar == null || tabbar.StateMachine.IsNullOrDestroyed()) return;

                if (btn) tabbar.StateMachine?.GoToStateById(btn.name);
            }

            public void SyncHighlighted(TabBarStateMachine tabbar)
            {
                if (tabbar == null || tabbar.StateMachine.IsNullOrDestroyed()) return;

                string current = tabbar.StateMachine.CurrentStateId;
                for (int i = 0; i < tabbar.Buttons.Count; i++)
                {
                    if (tabbar.Buttons[i].Button && tabbar.Buttons[i].Button.TryGetComponent(out SelectableOverride h))
                    {
                        h.OverrideHighlighted = h.CompareName(current);
                    }
                }
            }

        }

        #endregion

        #region Editor Configuration Helper Methods
#if UNITY_EDITOR
        bool ValidateShowConfigurationButtons() => _buttons.Count == 0;
        void ConfigureForwards()
        {
            if (Application.isPlaying) return;

            _buttons.Clear();
            var btns = this.transform.EnumerateImmediateChildren()
                           .Select(t => t.GetComponentsInChildren<Selectable>().FirstOrDefault(o => (o is Button || o is SPUIButton) && o.enabled && o.interactable))
                           .Where(o => o != null);
            _buttons.AddRange(btns.Select(o => new ButtonRef(o)));
        }
        void ConfigureReverse()
        {
            if (Application.isPlaying) return;

            _buttons.Clear();
            var btns = this.transform.EnumerateImmediateChildren()
                           .Select(t => t.GetComponentsInChildren<Selectable>().FirstOrDefault(o => (o is Button || o is SPUIButton) && o.enabled && o.interactable))
                           .Where(o => o != null);
            _buttons.AddRange(btns.Reverse().Select(o => new ButtonRef(o)));
        }
#endif
        #endregion

    }

}
