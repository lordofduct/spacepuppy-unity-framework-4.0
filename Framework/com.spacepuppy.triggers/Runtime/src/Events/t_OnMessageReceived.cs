using com.spacepuppy.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.spacepuppy.Events
{
    public sealed class t_OnMessageReceived : SPComponent, IMessageMediatorReceiver, IObservableTrigger
    {

        #region Fields

        [SerializeField]
        private MessageMediator _message;

        [SerializeField]
        private SPEvent _onReceived = new SPEvent("OnReceived");

        [System.NonSerialized]
        private MessageMediator.RegistrationToken _registerToken;

        #endregion

        #region CONSTRUCTOR

        protected override void OnEnable()
        {
            base.OnEnable();

            this.RegisterSelf();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            _registerToken.Unregister(true);
        }

        #endregion

        #region Properties

        public MessageMediator Message
        {
            get { return _message; }
            set
            {
                if(_message != value)
                {
                    _message = value;
                    _registerToken.Unregister(true);
                    if (this.enabled) this.RegisterSelf();
                }
            }
        }

        public SPEvent OnRecieved
        {
            get { return _onReceived; }
        }

        #endregion

        #region Methods

        private void RegisterSelf()
        {
            if (_message != null)
            {
                _registerToken = _message.RegisterListener(this);
            }
        }

        #endregion

        #region IMessageMediatorReceiver Interface

        void IMessageMediatorReceiver.OnMessageTokenSignaled(MessageMediator token, object arg)
        {
            if (token != _message) return;

            _onReceived.ActivateTrigger(token, arg);
        }

        #endregion

        #region IObserverableTrigger Interface

        BaseSPEvent[] IObservableTrigger.GetEvents()
        {
            return new BaseSPEvent[] { _onReceived };
        }

        #endregion

        #region UNITY_EDITOR

        void OnValidate()
        {
            if(Application.isPlaying)
            {
                _registerToken.Unregister(true);
                if (this.enabled) this.RegisterSelf();
            }
        }

        #endregion

    }
}
