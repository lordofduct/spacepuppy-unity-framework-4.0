using UnityEngine;
using System.Collections.Generic;
using com.spacepuppy.Utils;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
using System;
using com.spacepuppy.Collections;

namespace com.spacepuppy.UI
{

    public interface ISelectedUIElementChangedGlobalHandler
    {
        void OnSelectedUIElementChanged(GameObject deselected, GameObject selected);
    }

    [MBubbledOnSelected]
    public interface IMBubbledOnSelected : IAutoMixinDecorator, IEventfulComponent
    {
        void OnBubbledSelected(GameObject deselected, GameObject selected);
    }
    internal class MBubbledOnSelectedAttribute : StatelessAutoMixinConfigAttribute
    {

        public static int ActiveCount { get; private set; }

        protected override void OnAutoCreated(object obj, Type mixinType)
        {
            var c = obj as IEventfulComponent;
            if (c == null) return;

            c.OnEnabled += (s,e) =>
            {
                ActiveCount++;
                if (ActiveCount == 1)
                {
                    EventSystemHook.Instance?.SyncIfShouldListenForUIElementChanged();
                }
            };
            c.OnDisabled += (s, e) =>
            {
                ActiveCount--;
                if (ActiveCount <= 0)
                {
                    ActiveCount = 0;
                    EventSystemHook.Instance?.SyncIfShouldListenForUIElementChanged();
                }
            };
        }
    }

    [MBubbledOnDeselected]
    public interface IMBubbledOnDeselected : IAutoMixinDecorator, IEventfulComponent
    {
        void OnBubbledDeselected(GameObject deselected, GameObject selected);
    }
    internal class MBubbledOnDeselectedAttribute : StatelessAutoMixinConfigAttribute
    {

        public static int ActiveCount { get; private set; }

        protected override void OnAutoCreated(object obj, Type mixinType)
        {
            var c = obj as IEventfulComponent;
            if (c == null) return;

            c.OnEnabled += (s, e) =>
            {
                ActiveCount++;
                if (ActiveCount == 1)
                {
                    EventSystemHook.Instance?.SyncIfShouldListenForUIElementChanged();
                }
            };
            c.OnDisabled += (s, e) =>
            {
                ActiveCount--;
                if (ActiveCount <= 0)
                {
                    ActiveCount = 0;
                    EventSystemHook.Instance?.SyncIfShouldListenForUIElementChanged();
                }
            };
        }
    }

    internal sealed class EventSystemHook : IUpdateable
    {


        static EventSystemHook _instance;
        public static EventSystemHook Instance => _instance;
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
            Messaging.AddHasRegisteredGlobalListenerChangedCallback<ISelectedUIElementChangedGlobalHandler>(SyncIfShouldListenForUIElementChanged);
            SyncIfShouldListenForUIElementChanged();
        }

        public void SyncIfShouldListenForUIElementChanged()
        {
            if (Messaging.HasRegisteredGlobalListener<ISelectedUIElementChangedGlobalHandler>() || 
                MBubbledOnSelectedAttribute.ActiveCount > 0 || 
                MBubbledOnDeselectedAttribute.ActiveCount > 0)
            {
                if (!GameLoop.LateUpdatePump.Contains(this))
                {
                    //sync _lastSelectedObject if we're starting a new update cycle
                    _lastSelectedObject = EventSystem.current ? EventSystem.current.currentSelectedGameObject : null;
                    GameLoop.LateUpdatePump.Add(this);
                }
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
                var last = _lastSelectedObject;
                _lastSelectedObject = curgo;

                System.ValueTuple<GameObject, GameObject> args = (last, curgo);
                if (last) Messaging.SignalUpwards<IMBubbledOnDeselected, System.ValueTuple<GameObject, GameObject>>(last, args, (o, a) => o.OnBubbledDeselected(a.Item1, a.Item2));
                if (curgo) Messaging.SignalUpwards<IMBubbledOnSelected, System.ValueTuple<GameObject, GameObject>>(curgo, args, (o, a) => o.OnBubbledSelected(a.Item1, a.Item2));
                Messaging.Broadcast<ISelectedUIElementChangedGlobalHandler, System.ValueTuple<GameObject, GameObject>>(args, (o, a) => o.OnSelectedUIElementChanged(a.Item1, a.Item2));
            }
        }

    }

}
