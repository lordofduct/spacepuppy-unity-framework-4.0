using UnityEngine;
using com.spacepuppy.Events;

namespace com.spacepuppy
{

    /// <summary>
    /// Facilitates sending events between assets/prefabs. By relying on a ScriptableObject to mediate an event between 2 other assets 
    /// you can have communication between those assets setup at dev time. For example communicating an event between 2 prefabs, or a prefab 
    /// and a scene, or any other similar situation. 
    /// 
    /// To use:
    /// Create a ProxyMediator as an asset and name it something unique.
    /// Any SPEvent/Trigger can target this asset just by dragging it into the target field.
    /// Now any script can accept a ProxyMediator and listen for the 'OnTriggered' event to receive a signal that the mediator had been triggered elsewhere.
    /// You can also attach a T_OnProxyMediatorTriggered, and drag the ProxyMediator asset in question into the 'Mediator' field. This T_ will fire when the mediator is triggered elsewhere.
    /// </summary>
    [CreateAssetMenu(fileName = "ProxyMediator", menuName = "Spacepuppy/ProxyMediator")]
    public class ProxyMediator : ScriptableObject, ITriggerable
    {

        public System.EventHandler OnTriggered;

        private RadicalWaitHandle _handle;

        #region Methods

        public IRadicalWaitHandle WaitForNextTrigger()
        {
            if (_handle == null)
            {
                _handle = RadicalWaitHandle.Create();
            }
            return _handle;
        }

        public void Trigger()
        {
            var h = _handle;
            _handle = null;

            this.OnTriggered?.Invoke(this, System.EventArgs.Empty);
            if (h != null)
            {
                h.SignalComplete();
            }
        }

        #endregion

        #region ITriggerableMechanism Interface

        bool ITriggerable.CanTrigger
        {
            get
            {
                return true;
            }
        }

        int ITriggerable.Order
        {
            get
            {
                return 0;
            }
        }

        bool ITriggerable.Trigger(object sender, object arg)
        {
            this.Trigger();
            return true;
        }

        #endregion

    }

}
