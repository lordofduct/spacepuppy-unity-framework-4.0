using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Events;
using com.spacepuppy.Project;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Events
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

        [SerializeField, ForceFromSelf, Tooltip("The state machine that will mirror the source.")]
        private InterfaceRef<IStateMachine> _targetStateMachine = new();

        [SerializeField, Tooltip("The state machine being mirrored.")]
        private InterfaceRef<IReadOnlyStateMachine> _sourceStateMachine = new();

        [SerializeField]
        private Modes _mode;

        [System.NonSerialized]
        private TrackedEventHandlerToken _eventHook;

        #endregion

        #region CONSTRUCTOR

        void IMStartOrEnableReceiver.OnStartOrEnable()
        {
            this.Sync();

            _eventHook.Dispose();
            if (this.SourceStateMachine.IsAlive()) _eventHook = this.SourceStateMachine.StateChanged_ref().AddTrackedListener(OnEnterState_TriggerActivated);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            _eventHook.Dispose();
        }

        #endregion

        #region Properties

        public IStateMachine TargetStateMachine
        {
            get => _targetStateMachine.Value;
            set => _targetStateMachine.Value = GameObjectUtil.GetGameObjectFromSource(value) == this.gameObject ? value : null;
        }

        public IReadOnlyStateMachine SourceStateMachine
        {
            get => _sourceStateMachine.Value;
            set
            {
                if (_sourceStateMachine.Value == value) return;
                _eventHook.Dispose();

                _sourceStateMachine.Value = value;
#if UNITY_EDITOR
                if (Application.isPlaying && _sourceStateMachine.Value.IsAlive() && this.isActiveAndEnabled) _eventHook = _sourceStateMachine.Value.StateChanged_ref().AddTrackedListener(OnEnterState_TriggerActivated);
#else
                if (_sourceStateMachine.Value.IsAlive() && this.isActiveAndEnabled) _eventHook = _sourceStateMachine.Value.StateChanged_ref().AddTrackedListener(OnEnterState_TriggerActivated);
#endif
                this.Sync();
            }
        }

        #endregion

        #region Methods

        public void Sync()
        {
            if (this.TargetStateMachine.IsAlive() && this.SourceStateMachine.IsAlive())
            {
                switch (_mode)
                {
                    case Modes.MirrorByIndex:
                        this.TargetStateMachine.GoToState(this.SourceStateMachine.CurrentStateIndex ?? -1);
                        break;
                    case Modes.MirrodById:
                        this.TargetStateMachine.GoToStateById(this.SourceStateMachine.CurrentStateId);
                        break;
                }
            }
        }

        private void OnEnterState_TriggerActivated(object sender, System.EventArgs e)
        {
            this.Sync();
        }

        #endregion

#if UNITY_EDITOR
        void OnValidate()
        {
            if (Application.isPlaying)
            {
                _eventHook.Dispose();
                if (this.SourceStateMachine.IsAlive() && this.isActiveAndEnabled) _eventHook = this.SourceStateMachine.StateChanged_ref().AddTrackedListener(OnEnterState_TriggerActivated);
            }
        }
#endif

    }

}
