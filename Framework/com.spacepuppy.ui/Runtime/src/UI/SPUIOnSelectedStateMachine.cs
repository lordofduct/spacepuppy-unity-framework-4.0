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
    public sealed class SPUIOnSelectedStateMachine : SPComponent, IUIComponent, ISelectHandler, IDeselectHandler, ISelectedUIElementChangedGlobalHandler, IMStartOrEnableReceiver
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
            this.SyncState(this.EvaluateIfSelected());
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

        void ISelectedUIElementChangedGlobalHandler.OnSelectedUIElementChanged()
        {
            this.SyncState(this.EvaluateIfSelected());
        }

        #endregion

        #region IUIComponent Interface

        public new RectTransform transform => base.transform as RectTransform;

        RectTransform IUIComponent.transform => base.transform as RectTransform;

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
