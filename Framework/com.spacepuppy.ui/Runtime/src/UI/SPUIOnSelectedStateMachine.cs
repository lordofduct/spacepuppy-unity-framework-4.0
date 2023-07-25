using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Utils;
using com.spacepuppy.Events;

namespace com.spacepuppy.UI
{

    public sealed class SPUIOnSelectedStateMachine : SPComponent, ISelectHandler, IDeselectHandler, IMStartOrEnableReceiver
    {

        #region Fields

        [SerializeField]
        private GameObject _unselectedState;
        [SerializeField]
        private GameObject _selectedState;

        #endregion

        #region CONSTRUCTOR

        void IMStartOrEnableReceiver.OnStartOrEnable()
        {
            this.SyncState();
        }

        #endregion

        #region Properties

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

        #endregion

    }

}
