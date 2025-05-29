using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using com.spacepuppy.Utils;

namespace com.spacepuppy.UI
{

    public interface ISelectedUIElementChangedGlobalHandler
    {
        void OnSelectedUIElementChanged();
    }

    [AutoInitMixin]
    public interface IMBubbledOnSelected : IMixin, IEventfulComponent
    {

        sealed void OnInitMixin()
        {
            this.OnEnabled += (s, e) =>
            {
                StandardSelectedUIElementChangedProcessor.IncrementSelectionBubblingListenerCount();
            };
            this.OnDisabled += (s, e) =>
            {
                StandardSelectedUIElementChangedProcessor.DecrementSelectionBubblingListenerCount();
            };
        }

        void OnBubbledSelected(GameObject deselected, GameObject selected);
    }

    [AutoInitMixin]
    public interface IMBubbledOnDeselected : IMixin, IEventfulComponent
    {

        sealed void OnInitMixin()
        {
            this.OnEnabled += (s, e) =>
            {
                StandardSelectedUIElementChangedProcessor.IncrementSelectionBubblingListenerCount();
            };
            this.OnDisabled += (s, e) =>
            {
                StandardSelectedUIElementChangedProcessor.DecrementSelectionBubblingListenerCount();
            };
        }

        void OnBubbledDeselected(GameObject deselected, GameObject selected);
    }
    
    public sealed class StandardSelectedUIElementChangedProcessor : ISPDisposable, IUpdateable
    {

        [System.NonSerialized]
        private GameObject _lastSelectedObject;

        public bool AttemptActivate()
        {
            if (this.IsDisposed) return false;

            if (Messaging.HasRegisteredGlobalListener<ISelectedUIElementChangedGlobalHandler>() ||
                StandardSelectedUIElementChangedProcessor.SelectedBubblingListenerCount > 0)
            {
                if (!GameLoop.TardyUpdatePump.Contains(this))
                {
                    //sync _lastSelectedObject if we're starting a new update cycle
                    _lastSelectedObject = EventSystem.current ? EventSystem.current.currentSelectedGameObject : null;
                    GameLoop.TardyUpdatePump.Add(this);
                }
                return true;
            }
            else
            {
                GameLoop.TardyUpdatePump.Remove(this);
                return false;
            }
        }

        public void Deactivate()
        {
            _lastSelectedObject = null;
            GameLoop.TardyUpdatePump.Remove(this);
        }

        void IUpdateable.Update()
        {
            if (this.IsDisposed)
            {
                GameLoop.TardyUpdatePump.Remove(this);
                return;
            }

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
                Messaging.Broadcast<ISelectedUIElementChangedGlobalHandler>(o => o.OnSelectedUIElementChanged());
            }
        }

        #region ISPDisposable Interface

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            this.IsDisposed = true;
            this.Deactivate();
        }

        #endregion

        #region Static Utils

        public static event System.Action SelectedBubblingListenerCountChanged;
        public static int SelectedBubblingListenerCount { get; private set; }
        internal static void IncrementSelectionBubblingListenerCount()
        {
            SelectedBubblingListenerCount++;
            if (SelectedBubblingListenerCount == 1)
            {
                SelectedBubblingListenerCountChanged?.Invoke();
            }
        }
        internal static void DecrementSelectionBubblingListenerCount()
        {
            SelectedBubblingListenerCount++;
            if (SelectedBubblingListenerCount <= 0)
            {
                SelectedBubblingListenerCount = 0;
                SelectedBubblingListenerCountChanged?.Invoke();
            }
        }

        #endregion

    }

}
