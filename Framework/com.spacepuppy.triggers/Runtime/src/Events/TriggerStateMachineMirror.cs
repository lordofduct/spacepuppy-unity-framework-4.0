using UnityEngine;
using System.Collections.Generic;
using com.spacepuppy.Events;

namespace com.spacepuppy
{

    [RequireComponent(typeof(i_TriggerStateMachine))]
    public sealed class TriggerStateMachineMirror : SPComponent, IMStartOrEnableReceiver
    {

        public enum Modes
        {
            MirrorByIndex = 0,
            MirrodById = 1,
        }

        #region Fields

        [SerializeField]
        [ForceFromSelf]
        [Tooltip("The state machine that will mirror the source.")]
        private i_TriggerStateMachine _targetStateMachine;

        [SerializeField]
        [Tooltip("The state machine being mirrored.")]
        private i_TriggerStateMachine _sourceStateMachine;

        [SerializeField]
        private Modes _mode;

        [System.NonSerialized]
        private SPEventTrackedListenerToken _onEnterStateHook;

        #endregion

        #region CONSTRUCTOR

        void IMStartOrEnableReceiver.OnStartOrEnable()
        {
            this.Sync();

            _onEnterStateHook.Dispose();
            if (_sourceStateMachine) _onEnterStateHook = _sourceStateMachine.OnEnterState.AddTrackedListener(OnEnterState_TriggerActivated);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            _onEnterStateHook.Dispose();
        }

        #endregion

        #region Properties

        public i_TriggerStateMachine TargetStateMachine
        {
            get => _targetStateMachine;
            set => _targetStateMachine = value != null && value.gameObject == this.gameObject ? value : null;
        }

        public i_TriggerStateMachine SourceStateMachine
        {
            get => _sourceStateMachine;
            set
            {
                if (_sourceStateMachine == value) return;
                _onEnterStateHook.Dispose();

                _sourceStateMachine = value;
                if (_sourceStateMachine && this.isActiveAndEnabled) _onEnterStateHook =  _sourceStateMachine.OnEnterState.AddTrackedListener(OnEnterState_TriggerActivated);
                this.Sync();
            }
        }

        #endregion

        #region Methods

        public void Sync()
        {
            if (_targetStateMachine && _sourceStateMachine)
            {
                switch (_mode)
                {
                    case Modes.MirrorByIndex:
                        _targetStateMachine.GoToState(_sourceStateMachine.CurrentStateIndex ?? -1);
                        break;
                    case Modes.MirrodById:
                        _targetStateMachine.GoToStateById(_sourceStateMachine.CurrentState?.Id);
                        break;
                }
            }
        }

        private void OnEnterState_TriggerActivated(object sender, TempEventArgs e)
        {
            this.Sync();
        }

        #endregion

    }

}
