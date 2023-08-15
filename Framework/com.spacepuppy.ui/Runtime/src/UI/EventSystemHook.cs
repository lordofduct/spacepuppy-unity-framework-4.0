using UnityEngine;
using System.Collections.Generic;
using com.spacepuppy.Utils;
using UnityEngine.EventSystems;

namespace com.spacepuppy.UI
{

    public interface ISelectedUIElementChangedGlobalHandler
    {
        void OnSelectedUIElementChanged();
    }

    internal sealed class EventSystemHook : IUpdateable
    {


        static EventSystemHook _instance;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        static void Init()
        {
            if (_instance == null)
            {
                _instance = new EventSystemHook();
            }
        }


        [System.NonSerialized]
        private GameObject _lastSelectedObject;


        private EventSystemHook()
        {
            Messaging.AddHasRegisteredGlobalListenerChangedCallback<ISelectedUIElementChangedGlobalHandler>(OnSelectedUIElementChangedReceiverListChanged);
        }

        void OnSelectedUIElementChangedReceiverListChanged()
        {
            if (Messaging.HasRegisteredGlobalListener<ISelectedUIElementChangedGlobalHandler>())
            {
                GameLoop.LateUpdatePump.Add(this);
            }
            else
            {
                GameLoop.LateUpdatePump.Remove(this);
            }
        }

        void IUpdateable.Update()
        {
            var evtsys = EventSystem.current;
            if (!evtsys) return;

            var curgo = evtsys.currentSelectedGameObject;
            if (curgo != _lastSelectedObject)
            {
                _lastSelectedObject = curgo;
                Messaging.Broadcast<ISelectedUIElementChangedGlobalHandler>(o => o.OnSelectedUIElementChanged());
            }
        }

    }

}
