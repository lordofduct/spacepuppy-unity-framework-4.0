using com.spacepuppy.Events;
using MacFsWatcher;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace com.spacepuppy
{

    public struct UnityEventTrackedListenerToken : System.IDisposable
    {

        private UnityEvent _event;
        private UnityAction _listener;

        public void Dispose()
        {
            if (_event != null && _listener != null)
            {
                _event.RemoveListener(_listener);
            }
            _event = null;
            _listener = null;
        }

        internal static UnityEventTrackedListenerToken Create(UnityEvent ev, UnityAction handler)
        {
            if (ev == null || handler == null) return default;
            ev.AddListener(handler);
            return new UnityEventTrackedListenerToken()
            {
                _event = ev,
                _listener = handler
            };
        }

    }

    public struct UnityEventTrackedListenerToken<T0> : System.IDisposable
    {

        private UnityEvent<T0> _event;
        private UnityAction<T0> _listener;

        public void Dispose()
        {
            if (_event != null && _listener != null)
            {
                _event.RemoveListener(_listener);
            }
            _event = null;
            _listener = null;
        }

        internal static UnityEventTrackedListenerToken<T0> Create(UnityEvent<T0> ev, UnityAction<T0> handler)
        {
            if (ev == null || handler == null) return default;
            ev.AddListener(handler);
            return new UnityEventTrackedListenerToken<T0>()
            {
                _event = ev,
                _listener = handler
            };
        }

    }

    public struct UnityEventTrackedListenerToken<T0, T1> : System.IDisposable
    {

        private UnityEvent<T0, T1> _event;
        private UnityAction<T0, T1> _listener;

        public void Dispose()
        {
            if (_event != null && _listener != null)
            {
                _event.RemoveListener(_listener);
            }
            _event = null;
            _listener = null;
        }

        internal static UnityEventTrackedListenerToken<T0, T1> Create(UnityEvent<T0, T1> ev, UnityAction<T0, T1> handler)
        {
            if (ev == null || handler == null) return default;
            ev.AddListener(handler);
            return new UnityEventTrackedListenerToken<T0, T1>()
            {
                _event = ev,
                _listener = handler
            };
        }

    }

    public struct UnityEventTrackedListenerToken<T0, T1, T2> : System.IDisposable
    {

        private UnityEvent<T0, T1, T2> _event;
        private UnityAction<T0, T1, T2> _listener;

        public void Dispose()
        {
            if (_event != null && _listener != null)
            {
                _event.RemoveListener(_listener);
            }
            _event = null;
            _listener = null;
        }

        internal static UnityEventTrackedListenerToken<T0, T1, T2> Create(UnityEvent<T0, T1, T2> ev, UnityAction<T0, T1, T2> handler)
        {
            if (ev == null || handler == null) return default;
            ev.AddListener(handler);
            return new UnityEventTrackedListenerToken<T0, T1, T2>()
            {
                _event = ev,
                _listener = handler
            };
        }

    }

    public struct UnityEventTrackedListenerToken<T0, T1, T2, T3> : System.IDisposable
    {

        private UnityEvent<T0, T1, T2, T3> _event;
        private UnityAction<T0, T1, T2, T3> _listener;

        public void Dispose()
        {
            if (_event != null && _listener != null)
            {
                _event.RemoveListener(_listener);
            }
            _event = null;
            _listener = null;
        }

        internal static UnityEventTrackedListenerToken<T0, T1, T2, T3> Create(UnityEvent<T0, T1, T2, T3> ev, UnityAction<T0, T1, T2, T3> handler)
        {
            if (ev == null || handler == null) return default;
            ev.AddListener(handler);
            return new UnityEventTrackedListenerToken<T0, T1, T2, T3>()
            {
                _event = ev,
                _listener = handler
            };
        }

    }

    public static class UnityEventExtensions
    {
        public static UnityEventTrackedListenerToken AddTrackedListener(this UnityEvent ev, UnityAction action) => UnityEventTrackedListenerToken.Create(ev, action);
        public static UnityEventTrackedListenerToken<T0> AddTrackedListener<T0>(this UnityEvent<T0> ev, UnityAction<T0> action) => UnityEventTrackedListenerToken<T0>.Create(ev, action);
        public static UnityEventTrackedListenerToken<T0, T1> AddTrackedListener<T0, T1>(this UnityEvent<T0, T1> ev, UnityAction<T0, T1> action) => UnityEventTrackedListenerToken<T0, T1>.Create(ev, action);
        public static UnityEventTrackedListenerToken<T0, T1, T2> AddTrackedListener<T0, T1, T2>(this UnityEvent<T0, T1, T2> ev, UnityAction<T0, T1, T2> action) => UnityEventTrackedListenerToken<T0, T1, T2>.Create(ev, action);
        public static UnityEventTrackedListenerToken<T0, T1, T2, T3> AddTrackedListener<T0, T1, T2, T3>(this UnityEvent<T0, T1, T2, T3> ev, UnityAction<T0, T1, T2, T3> action) => UnityEventTrackedListenerToken<T0, T1, T2, T3>.Create(ev, action);
    }

}
