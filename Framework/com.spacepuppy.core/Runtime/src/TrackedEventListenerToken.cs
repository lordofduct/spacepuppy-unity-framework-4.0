using System;

namespace com.spacepuppy
{

    public struct TrackedEventListenerToken : IDisposable
    {

        private EventHandler _handler;
        private Action<EventHandler> _unregistercallback;

        public TrackedEventListenerToken(EventHandler handler, Action<EventHandler> unregistercallback)
        {
            if (handler != null)
            {
                _handler = handler;
                _unregistercallback = unregistercallback;
            }
            else
            {
                _handler = null;
                _unregistercallback = null;
            }
        }

        public void Dispose()
        {
            _unregistercallback?.Invoke(_handler);
            _handler = null;
            _unregistercallback = null;
        }

    }

    public struct TrackedEventListenerToken<T> : IDisposable where T : EventArgs
    {

        private EventHandler<T> _handler;
        private Action<EventHandler<T>> _unregistercallback;

        public TrackedEventListenerToken(EventHandler<T> handler, Action<System.EventHandler<T>> unregistercallback)
        {
            if (handler != null)
            {
                _handler = handler;
                _unregistercallback = unregistercallback;
            }
            else
            {
                _handler = null;
                _unregistercallback = null;
            }
        }

        public void Dispose()
        {
            _unregistercallback?.Invoke(_handler);
            _handler = null;
            _unregistercallback = null;
        }

    }

}
