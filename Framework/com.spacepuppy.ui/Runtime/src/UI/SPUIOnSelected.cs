using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Utils;
using com.spacepuppy.Events;

namespace com.spacepuppy.UI
{

    [RequireComponent(typeof(RectTransform))]
    public sealed class SPUIOnSelected : MonoBehaviour, IUIComponent, ISelectHandler, IDeselectHandler, ISelectedUIElementChangedGlobalHandler, IObservableTrigger
    {

        #region Fields

        [SerializeField]
        private bool _treatAsSelectedIfChildSelected;

        [SerializeField]
        private SPEvent _onSelect = new SPEvent("OnSelect");
        [SerializeField]
        private SPEvent _onDeselect = new SPEvent("OnDeselect");

        [System.NonSerialized]
        private bool _selected;

        #endregion

        #region CONSTRUCTOR

        void OnEnable()
        {
            _selected = false;
            this.SyncMessageHandler();
            this.SyncState();
        }

        void OnDisable()
        {
            _selected = false;
            Messaging.UnregisterGlobal<ISelectedUIElementChangedGlobalHandler>(this);
        }

        #endregion

        #region Properties

        public SPEvent OnSelect => _onSelect;

        public SPEvent OnDeselect => _onDeselect;

        #endregion

        #region Methods

        public void SyncState()
        {
            this.SyncState(this.EvaluateIfSelected());
        }

        private void SyncState(bool selected)
        {
            if (selected)
            {
                if (!_selected)
                {
                    _selected = true;
                    _onSelect.ActivateTrigger(this, null);
                }
            }
            else
            {
                if (_selected)
                {
                    _selected = false;
                    _onDeselect.ActivateTrigger(this, null);
                }
            }
        }

        void SyncMessageHandler()
        {
            if (_treatAsSelectedIfChildSelected)
            {
                Messaging.RegisterGlobal<ISelectedUIElementChangedGlobalHandler>(this);
            }
            else
            {
                Messaging.UnregisterGlobal<ISelectedUIElementChangedGlobalHandler>(this);
            }
        }

        bool EvaluateIfSelected()
        {
            var cur = Services.Get<IEventSystem>().GetSelectedGameObject(this.gameObject);
            if (_treatAsSelectedIfChildSelected)
            {
                return cur && (cur == this.gameObject || cur.transform.IsChildOf(this.transform));
            }
            else
            {
                return cur == this.gameObject;
            }
        }

        #endregion

        #region ISelect/DeselectHandler Interface

        void ISelectHandler.OnSelect(BaseEventData eventData)
        {
            if (!this.IsActiveAndEnabled()) return;

            this.SyncState(this.EvaluateIfSelected());
        }

        void IDeselectHandler.OnDeselect(BaseEventData eventData)
        {
            if (!this.IsActiveAndEnabled()) return;

            if (!_treatAsSelectedIfChildSelected)
            {
                this.SyncState(false);
            }
        }

        void ISelectedUIElementChangedGlobalHandler.OnSelectedUIElementChanged(GameObject deselected, GameObject selected)
        {
            if (!_treatAsSelectedIfChildSelected)
            {
                this.SyncMessageHandler();
                return;
            }

            this.SyncState(this.EvaluateIfSelected());
        }

        #endregion

        #region IUIComponent Interface

        public new RectTransform transform => base.transform as RectTransform;

        RectTransform IUIComponent.transform => base.transform as RectTransform;

        Component IComponent.component => this;

        #endregion

        #region IObservableTrigger Interface

        BaseSPEvent[] IObservableTrigger.GetEvents()
        {
            return new BaseSPEvent[] { _onSelect, _onDeselect };
        }

        #endregion

#if UNITY_EDITOR
        void OnValidate()
        {
            if (Application.isPlaying)
            {
                this.SyncMessageHandler();
            }
        }
#endif

    }

}
