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
        private InterfaceRef<IStateMachine> _sourceStateMachine = new();

        [SerializeField]
        private Modes _mode;

        [System.NonSerialized]
        private TrackedEventListenerToken _onStateChangedHook;

        #endregion

        #region CONSTRUCTOR

        void IMStartOrEnableReceiver.OnStartOrEnable()
        {
            this.Sync();

            _onStateChangedHook.Dispose();
            if (this.SourceStateMachine.IsAlive()) _onStateChangedHook = this.SourceStateMachine.AddTrackedStateChangedListener(OnEnterState_TriggerActivated);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            _onStateChangedHook.Dispose();
        }

        #endregion

        #region Properties

        public IStateMachine TargetStateMachine
        {
            get => _targetStateMachine.Value;
            set => _targetStateMachine.Value = GameObjectUtil.GetGameObjectFromSource(value) == this.gameObject ? value : null;
        }

        public IStateMachine SourceStateMachine
        {
            get => _sourceStateMachine.Value;
            set
            {
                if (_sourceStateMachine.Value == value) return;
                _onStateChangedHook.Dispose();

                _sourceStateMachine.Value = value;
                if (_sourceStateMachine.Value.IsAlive() && this.isActiveAndEnabled) _onStateChangedHook =  _sourceStateMachine.Value.AddTrackedStateChangedListener(OnEnterState_TriggerActivated);
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

    }

}
