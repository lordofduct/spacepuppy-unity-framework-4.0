using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Project;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Events
{

    [RequireComponent(typeof(i_TriggerStateMachine))]
    public sealed class TriggerStateMachineProxyLink : SPComponent, IMStartOrEnableReceiver
    {

        #region Fields

        [SerializeField, ForceFromSelf]
        private InterfaceRef<IStateMachine> _stateMachine = new();

        [SerializeField, ReorderableArray]
        private List<ProxyLink> _links = new List<ProxyLink>();

        [System.NonSerialized]
        private List<TrackedEventListenerToken<TempEventArgs>> _eventHooks = new List<TrackedEventListenerToken<TempEventArgs>>();

        #endregion

        #region CONSTRUCTOR

        void IMStartOrEnableReceiver.OnStartOrEnable()
        {
            if (_stateMachine.Value == null) _stateMachine.Value = this.GetComponent<IStateMachine>();
            for (int i = 0; i < _links.Count; i++)
            {
                if (_links[i].Proxy) _eventHooks.Add(_links[i].Proxy.AddTrackedOnTriggeredListener(Proxy_OnTriggered));
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (_eventHooks.Count > 0)
            {
                foreach (var hook in _eventHooks)
                {
                    hook.Dispose();
                }
                _eventHooks.Clear();
            }
        }

        #endregion

        #region Properties

        public IStateMachine StateMachine
        {
            get => _stateMachine.Value;
            set => _stateMachine.Value = value;
        }

        public IReadOnlyList<ProxyLink> Links => _links; //TODO - for now this is read-only so, need to implement a simple way to update listeners at runtime efficiently

        #endregion

        #region Methods

        private void Proxy_OnTriggered(object sender, TempEventArgs e)
        {
            var m = sender as ProxyMediator;
            if (m == null || this.StateMachine.IsNullOrDestroyed()) return;

            bool found = false;
            if (this.isActiveAndEnabled)
            {
                foreach (var link in _links)
                {
                    if (link.Proxy == m)
                    {
                        found = true;
                        this.StateMachine.GoToStateById(link.Id);
                        break;
                    }
                }
            }

            if (!found)
            {
                m.OnTriggered -= Proxy_OnTriggered;
            }
        }

        #endregion

        #region Special Types

        [System.Serializable]
        public struct ProxyLink
        {
            public string Id;
            public ProxyMediator Proxy;
        }

        #endregion

    }

}
