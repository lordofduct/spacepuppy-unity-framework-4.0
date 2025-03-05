using UnityEngine;

namespace com.spacepuppy.Events
{

    [System.Serializable()]
    public class ObservableTargetData
    {

        public System.EventHandler<TempEventArgs> TriggerActivated;

        #region Fields

        [SerializeField]
        [TypeRestriction(typeof(IObservableTrigger))]
        private UnityEngine.Object _target;

        [SerializeField]
        private int _triggerIndex;


        [System.NonSerialized]
        private bool _initialized;
        [System.NonSerialized]
        private SPEventTrackedListenerToken _eventHook;
        
        #endregion

        #region Properties

        /// <summary>
        /// The object being observed.
        /// 
        /// If you set this after hijacking/adding a handler. You may need to call Init and recall 'BeginHijack'.
        /// </summary>
        public IObservableTrigger Target
        {
            get { return _target as IObservableTrigger; }
            set
            {
                var targ = value as UnityEngine.Object;
                if (targ == _target) return;

                this.DeInit();
                _target = targ;
                _initialized = false;
            }
        }

        public int TriggerIndex
        {
            get { return _triggerIndex; }
            set { _triggerIndex = value; }
        }

        public BaseSPEvent TargetEvent
        {
            get
            {
                if (!_initialized) this.Init();
                return _eventHook.SPEvent;
            }
        }

        #endregion

        #region Methods

        public void Init()
        {
            if (_initialized) return;

            _initialized = true;
            if (_triggerIndex >= 0 && _target is IObservableTrigger)
            {
                var arr = (_target as IObservableTrigger).GetEvents();
                if (arr != null && _triggerIndex < arr.Length)
                {
                    _eventHook.SPEvent?.EndHijack(this);
                    _eventHook.Dispose();
                    if (arr[_triggerIndex] != null) _eventHook = arr[_triggerIndex].AddTrackedListener(this.OnTriggerActivated);
                }
            }
        }

        public void DeInit()
        {
            _eventHook.SPEvent?.EndHijack(this);
            _eventHook.Dispose();
            _initialized = false;
        }

        public bool BeginHijack()
        {
            if (!_initialized) this.Init();

            if (_eventHook.SPEvent == null) return false;

            _eventHook.SPEvent.BeginHijack(this);
            return true;
        }

        public void EndHijack()
        {
            _eventHook.SPEvent?.EndHijack(this);
        }

        private void OnTriggerActivated(object sender, TempEventArgs e)
        {
            this.TriggerActivated?.Invoke(sender, e);
        }

        #endregion

    }

}
