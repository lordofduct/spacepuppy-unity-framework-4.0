using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Utils;
using com.spacepuppy.Events;

namespace com.spacepuppy.UI
{

    public sealed class SPUIOnSelectedStateMachine : SPComponent, ISelectHandler, IDeselectHandler, ISelectedUIElementChangedGlobalHandler, IMStartOrEnableReceiver
    {

        #region Fields

        [SerializeField]
        private bool _treatAsSelectedIfChildSelected;

        [SerializeField]
        private GameObject _unselectedState;
        [SerializeField]
        private GameObject _selectedState;

        #endregion

        #region CONSTRUCTOR

        void IMStartOrEnableReceiver.OnStartOrEnable()
        {
            this.SyncMessageHandler();
            this.SyncState();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            Messaging.UnregisterGlobal<ISelectedUIElementChangedGlobalHandler>(this);
        }

        #endregion

        #region Properties

        public bool TreatAsSelectedIfChildSelected
        {
            get => _treatAsSelectedIfChildSelected;
            set
            {
                _treatAsSelectedIfChildSelected = value;
#if UNITY_EDITOR
                if (Application.isPlaying && this.isActiveAndEnabled)
#else
                if (this.isActiveAndEnabled)
#endif
                {
                    this.SyncMessageHandler();
                }
            }
        }

        public GameObject UnselectedState
        {
            get => _unselectedState;
            set => _selectedState = value;
        }

        public GameObject SelectedState
        {
            get => _selectedState;
            set => _selectedState = value;
        }

        #endregion

        #region Methods

        public void SyncState()
        {
            this.SyncState(EventSystem.current && EventSystem.current.currentSelectedGameObject == this.gameObject);
        }

        private void SyncState(bool selected)
        {
            if (selected)
            {
                if (_unselectedState) _unselectedState.SetActive(false);
                if (_selectedState) _selectedState.SetActive(true);
            }
            else
            {
                if (_selectedState) _selectedState.SetActive(false);
                if (_unselectedState) _unselectedState.SetActive(true);
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

        #endregion

        #region ISelect/DeselectHandler Interface

        void ISelectHandler.OnSelect(BaseEventData eventData)
        {
            if (!this.IsActiveAndEnabled()) return;

            this.SyncState(true);
        }

        void IDeselectHandler.OnDeselect(BaseEventData eventData)
        {
            if (!this.IsActiveAndEnabled()) return;

            this.SyncState(false);
        }

        void ISelectedUIElementChangedGlobalHandler.OnSelectedUIElementChanged()
        {
            var cur = EventSystem.current.currentSelectedGameObject;
            if (cur && cur != this.gameObject)
            {
                this.SyncState(_treatAsSelectedIfChildSelected && cur.transform.IsChildOf(this.transform));
            }
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
