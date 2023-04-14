using UnityEngine;
using System.Collections.Generic;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Events
{

    [RequireComponent(typeof(i_TriggerStateMachine))]
    public sealed class TriggerStateMachineProxyLink : SPComponent, IMStartOrEnableReceiver
    {

        #region Fields

        [SerializeField]
        [ForceFromSelf]
        private i_TriggerStateMachine _stateMachine;

        [SerializeField]
        [ReorderableArray]
        private List<ProxyLink> _links = new List<ProxyLink>();

        #endregion

        #region CONSTRUCTOR

        void IMStartOrEnableReceiver.OnStartOrEnable()
        {
            if (!_stateMachine) _stateMachine = this.GetComponent<i_TriggerStateMachine>();
            for (int i = 0; i < _links.Count; i++)
            {
                if (_links[i].Proxy) _links[i].Proxy.OnTriggered += Proxy_OnTriggered;
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            for (int i = 0; i < _links.Count; i++)
            {
                if (_links[i].Proxy) _links[i].Proxy.OnTriggered -= Proxy_OnTriggered;
            }
        }

        #endregion

        #region Methods

        private void Proxy_OnTriggered(object sender, TempEventArgs e)
        {
            var m = sender as ProxyMediator;
            if (m == null || !_stateMachine) return;

            bool found = false;
            if (this.isActiveAndEnabled)
            {
                foreach (var link in _links)
                {
                    if (link.Proxy == m)
                    {
                        found = true;
                        _stateMachine.GoToStateById(link.Id);
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
