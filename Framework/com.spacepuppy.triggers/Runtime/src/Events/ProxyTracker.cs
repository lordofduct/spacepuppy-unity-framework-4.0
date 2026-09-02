using UnityEngine;
using System.Collections.Generic;

namespace com.spacepuppy.Events
{

    [Infobox("While this is active it is added to the ProxyMediator as a target. A related ProxyTrackerStateMachineDriver will set its state based on if the proxy has targets.")]
    public class ProxyTracker : SPComponent, IMStartOrEnableReceiver
    {

        #region Fields

        [SerializeField]
        private ProxyMediator _proxy;

        #endregion

        #region CONSTRUCTOR

        void IMStartOrEnableReceiver.OnStartOrEnable()
        {
            if (_proxy)
            {
                _proxy.PushProxyTarget(this);
            }
        }

        protected override void OnDisable()
        {
            if (_proxy)
            {
                _proxy.RemoveProxyTarget(this);
            }
            base.OnDisable();
        }

        #endregion

    }

}
