using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using com.spacepuppy.Utils;

namespace com.spacepuppy.UI
{

    public class ToggleIsOnStateMachine : SPComponent
    {

        #region Fields

        [SerializeField]
        [DefaultFromSelf]
        private Toggle _target;

        [SerializeField]
        private GameObject _onState;
        [SerializeField]
        private GameObject _offState;

        [System.NonSerialized]
        private UnityEventTrackedListenerToken<bool> _onValueChangedHook;

        #endregion

        #region CONSTRUCTOR

        protected override void OnEnable()
        {
            base.OnEnable();

            this.Sync();
            this.SyncListeners();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            _onValueChangedHook.Dispose();
        }

        #endregion

        #region Properties

        public Toggle Target
        {
            get => _target;
            set
            {
                if (_target == value) return;
                _target = value;
                if (this.isActiveAndEnabled && Application.isPlaying) this.SyncListeners();
            }
        }

        public GameObject OnState
        {
            get => _onState;
            set => _onState = value;
        }

        public GameObject OffState
        {
            get => _offState;
            set => _offState = value;
        }

        #endregion

        #region Methods

        public void Sync()
        {
            if (_target && _target.isOn)
            {
                _offState.TrySetActive(false);
                _onState.TrySetActive(true);
            }
            else
            {
                _onState.TrySetActive(false);
                _offState.TrySetActive(true);
            }
        }

        void SyncListeners()
        {
            _onValueChangedHook.Dispose();
            if (_target) _onValueChangedHook = _target.onValueChanged.AddTrackedListener(b => this.Sync());
        }

        #endregion

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (this.isActiveAndEnabled && Application.isPlaying)
            {
                this.SyncListeners();
            }
        }
#endif

    }

}
