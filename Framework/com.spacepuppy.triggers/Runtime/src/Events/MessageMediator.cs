using UnityEngine;
using System.Collections.Generic;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Events
{

    public interface IMessageMediatorReceiver
    {
        void OnMessageTokenSignaled(MessageMediator token, object arg);
    }

    [CreateAssetMenu(fileName = "MessageMediator", menuName = "Spacepuppy/MessageMediator")]
    public class MessageMediator : ScriptableObject, INameable, ITriggerable
    {

        #region Fields

        [SerializeField]
        private int _order;

        private HashSet<IMessageMediatorReceiver> _receivers = new HashSet<IMessageMediatorReceiver>();
        private System.Action<IMessageMediatorReceiver, object> _functor;

        #endregion

        #region CONSTRUCTOR

        public MessageMediator()
        {
            _nameCache = new NameCache.UnityObjectNameCache(this);
        }

        #endregion

        #region Methods

        public RegistrationToken RegisterListener(IMessageMediatorReceiver receiver)
        {
            _receivers.Add(receiver);
            return new RegistrationToken(this, receiver);
        }

        public void UnregisterListener(IMessageMediatorReceiver receiver)
        {
            _receivers.Remove(receiver);
        }

        #endregion

        #region INameable Interface

        private NameCache.UnityObjectNameCache _nameCache;
        public new string name
        {
            get { return _nameCache.Name; }
            set { _nameCache.Name = value; }
        }
        string INameable.Name
        {
            get { return _nameCache.Name; }
            set { _nameCache.Name = value; }
        }

        public bool CompareName(string nm)
        {
            return _nameCache.CompareName(nm);
        }
        void INameable.SetDirty()
        {
            _nameCache.SetDirty();
        }

        #endregion

        #region ITriggerable Interface

        public int Order { get { return _order; } }

        public bool CanTrigger { get { return this != null; } }

        public bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            var e = _receivers.GetEnumerator();
            while (e.MoveNext())
            {
                e.Current.OnMessageTokenSignaled(this, arg);
            }

            if (_functor == null)
            {
                _functor = (t, a) =>
                {
                    if (!_receivers.Contains(t)) t.OnMessageTokenSignaled(this, a);
                };
            }
            Messaging.Broadcast<IMessageMediatorReceiver, object>(arg, _functor);
            return true;
        }

        #endregion

        #region Special Types

        public struct RegistrationToken
        {
            private MessageMediator _mediator;
            private IMessageMediatorReceiver _receiver;

            internal RegistrationToken(MessageMediator mediator, IMessageMediatorReceiver receiver)
            {
                _mediator = mediator;
                _receiver = receiver;
            }

            public bool Disposed
            {
                get { return _receiver == null; }
            }

            public void Unregister(bool dispose = false)
            {
                if (!object.ReferenceEquals(_mediator, null) && !object.ReferenceEquals(_receiver, null))
                {
                    _mediator.UnregisterListener(_receiver);
                }

                if(dispose)
                {
                    this.Dispose();
                }
            }

            public void Dispose()
            {
                _mediator = null;
                _receiver = null;
            }

        }

        #endregion

    }

}
