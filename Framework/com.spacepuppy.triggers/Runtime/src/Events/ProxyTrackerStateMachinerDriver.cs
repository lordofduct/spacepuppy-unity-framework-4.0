using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy.Events;
using com.spacepuppy.Project;

namespace com.spacepuppy.Events
{

    [RequireComponent(typeof(i_TriggerStateMachine))]
    public class ProxyTrackerStateMachinerDriver : SPComponent, IMStartOrEnableReceiver, i_TriggerStateMachine.IStateDriverComponent
    {

        #region Fields

        [SerializeField, ForceFromSelf]
        private InterfaceRef<IStateMachine> _stateMachine = new();

        [SerializeField]
        private ProxyMediator _proxy;

        [SerializeField]
        private bool _gotoStateByIndex;

        [SerializeField, Tooltip("ProxyMediators can technically have any targets via i_SyncProxyMediator, if this is true then we only consider those targets that are ProxyTrackers.")]
        private bool _considerProxyTrackerTargetsExclusively;

        [SerializeField]
        private bool _allowUserDefinedStates;


        [System.NonSerialized]
        private TrackedEventHandlerToken _token;
        [System.NonSerialized]
        private bool _dirty;

        #endregion

        #region CONSTRUCTOR

        void IMStartOrEnableReceiver.OnStartOrEnable()
        {
            _dirty = true;
            this.Sync();
        }

        protected override void OnDisable()
        {
            _token.Dispose();
            base.OnDisable();
        }

        #endregion

        #region Properties

        public IStateMachine StateMachine
        {
            get => _stateMachine.Value;
            set => _stateMachine.Value = value;
        }

        public ProxyMediator Proxy
        {
            get => _proxy;
            set
            {
                _proxy = value;
                _dirty = true;
            }
        }

        public bool GoToStateByIndex
        {
            get => _gotoStateByIndex;
            set => _gotoStateByIndex = value;
        }

        public bool ConsiderProxyTrackerTargetsExclusively
        {
            get => _considerProxyTrackerTargetsExclusively;
            set => _considerProxyTrackerTargetsExclusively = value;
        }

        public bool AllowUserDefinedStates
        {
            get => _allowUserDefinedStates;
            set => _allowUserDefinedStates = value;
        }

        #endregion

        #region IStateDriverComponent Interface

        i_TriggerStateMachine i_TriggerStateMachine.IStateDriverComponent.StateMachine => _stateMachine.Value as i_TriggerStateMachine;

        IEnumerable<string> i_TriggerStateMachine.IStateDriverComponent.GetStateIds() => new string[] { "Inactive", "Active" };

        bool i_TriggerStateMachine.IStateDriverComponent.AllowUserDefinedStates => _allowUserDefinedStates;

        public void Sync()
        {
            if (_dirty)
            {
                _dirty = false;
                _token.Dispose();
                if (_proxy) _token = _proxy.OnTargetsChanged_ref().AddTrackedListener((s, e) => this.Sync());
            }

            bool isactive = false;
            if (_proxy)
            {
                if (_considerProxyTrackerTargetsExclusively)
                {
                    isactive = _proxy.GetProxyTargets()?.OfType<ProxyTracker>().Any() ?? false;
                }
                else
                {
                    isactive = _proxy.GetProxyTargets()?.Any() ?? false;
                }
            }

            if (_gotoStateByIndex)
            {
                _stateMachine.Value?.GoToState(isactive ? 1 : 0);
            }
            else
            {
                _stateMachine.Value?.GoToStateById(isactive ? "Active" : "Inactive");
            }
        }

        #endregion

    }

}
