using UnityEngine;
using UnityEngine.Events;

namespace com.spacepuppy
{

    public struct TrackableUnityEvent : ITrackableEvent<UnityAction>
    {
        private UnityEvent _event;
        public TrackableUnityEvent(UnityEvent ev) { this._event = ev; }
        public void AddListener(UnityAction listener) => _event?.AddListener(listener);
        public void RemoveListener(UnityAction listener) => _event?.RemoveListener(listener);
        public UnityEventTrackedListenerToken AddTrackedListener(UnityAction listener) => UnityEventTrackedListenerToken.Create(_event, listener);
        public static implicit operator TrackableUnityEvent(UnityEvent ev) => new() { _event = ev };
    }
    public struct UnityEventTrackedListenerToken : ITrackedListenerToken<UnityAction>
    {

        private UnityEvent _event;
        private UnityAction _listener;

        TrackableUnityEvent Target => new TrackableUnityEvent(_event);
        ITrackableEvent<UnityAction> ITrackedListenerToken<UnityAction>.Target => new TrackableUnityEvent(_event);
        UnityAction ITrackedListenerToken<UnityAction>.Listener => _listener;

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

    public struct TrackableUnityEvent<T0> : ITrackableEvent<UnityAction<T0>>
    {
        private UnityEvent<T0> _event;
        public TrackableUnityEvent(UnityEvent<T0> ev) { this._event = ev; }
        public void AddListener(UnityAction<T0> listener) => _event?.AddListener(listener);
        public void RemoveListener(UnityAction<T0> listener) => _event?.RemoveListener(listener);
        public UnityEventTrackedListenerToken<T0> AddTrackedListener(UnityAction<T0> listener) => UnityEventTrackedListenerToken<T0>.Create(_event, listener);
        public static implicit operator TrackableUnityEvent<T0>(UnityEvent<T0> ev) => new() { _event = ev };
    }
    public struct UnityEventTrackedListenerToken<T0> : ITrackedListenerToken<UnityAction<T0>>
    {

        private UnityEvent<T0> _event;
        private UnityAction<T0> _listener;

        TrackableUnityEvent<T0> Target => new TrackableUnityEvent<T0>(_event);
        ITrackableEvent<UnityAction<T0>> ITrackedListenerToken<UnityAction<T0>>.Target => new TrackableUnityEvent<T0>(_event);
        UnityAction<T0> ITrackedListenerToken<UnityAction<T0>>.Listener => _listener;

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

    public struct TrackableUnityEvent<T0, T1> : ITrackableEvent<UnityAction<T0, T1>>
    {
        private UnityEvent<T0, T1> _event;
        public TrackableUnityEvent(UnityEvent<T0, T1> ev) { this._event = ev; }
        public void AddListener(UnityAction<T0, T1> listener) => _event?.AddListener(listener);
        public void RemoveListener(UnityAction<T0, T1> listener) => _event?.RemoveListener(listener);
        public UnityEventTrackedListenerToken<T0, T1> AddTrackedListener(UnityAction<T0, T1> listener) => UnityEventTrackedListenerToken<T0, T1>.Create(_event, listener);
        public static implicit operator TrackableUnityEvent<T0, T1>(UnityEvent<T0, T1> ev) => new() { _event = ev };
    }
    public struct UnityEventTrackedListenerToken<T0, T1> : ITrackedListenerToken<UnityAction<T0, T1>>
    {

        private UnityEvent<T0, T1> _event;
        private UnityAction<T0, T1> _listener;

        TrackableUnityEvent<T0, T1> Target => new TrackableUnityEvent<T0, T1>(_event);
        ITrackableEvent<UnityAction<T0, T1>> ITrackedListenerToken<UnityAction<T0, T1>>.Target => new TrackableUnityEvent<T0, T1>(_event);
        UnityAction<T0, T1> ITrackedListenerToken<UnityAction<T0, T1>>.Listener => _listener;

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

    public struct TrackableUnityEvent<T0, T1, T2> : ITrackableEvent<UnityAction<T0, T1, T2>>
    {
        private UnityEvent<T0, T1, T2> _event;
        public TrackableUnityEvent(UnityEvent<T0, T1, T2> ev) { this._event = ev; }
        public void AddListener(UnityAction<T0, T1, T2> listener) => _event?.AddListener(listener);
        public void RemoveListener(UnityAction<T0, T1, T2> listener) => _event?.RemoveListener(listener);
        public UnityEventTrackedListenerToken<T0, T1, T2> AddTrackedListener(UnityAction<T0, T1, T2> listener) => UnityEventTrackedListenerToken<T0, T1, T2>.Create(_event, listener);
        public static implicit operator TrackableUnityEvent<T0, T1, T2>(UnityEvent<T0, T1, T2> ev) => new() { _event = ev };
    }
    public struct UnityEventTrackedListenerToken<T0, T1, T2> : ITrackedListenerToken<UnityAction<T0, T1, T2>>
    {

        private UnityEvent<T0, T1, T2> _event;
        private UnityAction<T0, T1, T2> _listener;

        TrackableUnityEvent<T0, T1, T2> Target => new TrackableUnityEvent<T0, T1, T2>(_event);
        ITrackableEvent<UnityAction<T0, T1, T2>> ITrackedListenerToken<UnityAction<T0, T1, T2>>.Target => new TrackableUnityEvent<T0, T1, T2>(_event);
        UnityAction<T0, T1, T2> ITrackedListenerToken<UnityAction<T0, T1, T2>>.Listener => _listener;

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

    public struct TrackableUnityEvent<T0, T1, T2, T3> : ITrackableEvent<UnityAction<T0, T1, T2, T3>>
    {
        private UnityEvent<T0, T1, T2, T3> _event;
        public TrackableUnityEvent(UnityEvent<T0, T1, T2, T3> ev) { this._event = ev; }
        public void AddListener(UnityAction<T0, T1, T2, T3> listener) => _event?.AddListener(listener);
        public void RemoveListener(UnityAction<T0, T1, T2, T3> listener) => _event?.RemoveListener(listener);
        public UnityEventTrackedListenerToken<T0, T1, T2, T3> AddTrackedListener(UnityAction<T0, T1, T2, T3> listener) => UnityEventTrackedListenerToken<T0, T1, T2, T3>.Create(_event, listener);
        public static implicit operator TrackableUnityEvent<T0, T1, T2, T3>(UnityEvent<T0, T1, T2, T3> ev) => new() { _event = ev };
    }
    public struct UnityEventTrackedListenerToken<T0, T1, T2, T3> : ITrackedListenerToken<UnityAction<T0, T1, T2, T3>>
    {

        private UnityEvent<T0, T1, T2, T3> _event;
        private UnityAction<T0, T1, T2, T3> _listener;

        TrackableUnityEvent<T0, T1, T2, T3> Target => new TrackableUnityEvent<T0, T1, T2, T3>(_event);
        ITrackableEvent<UnityAction<T0, T1, T2, T3>> ITrackedListenerToken<UnityAction<T0, T1, T2, T3>>.Target => new TrackableUnityEvent<T0, T1, T2, T3>(_event);
        UnityAction<T0, T1, T2, T3> ITrackedListenerToken<UnityAction<T0, T1, T2, T3>>.Listener => _listener;

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

    public static partial class SPExtensions
    {
        public static UnityEventTrackedListenerToken AddTrackedListener(this UnityEvent ev, UnityAction action) => UnityEventTrackedListenerToken.Create(ev, action);
        public static UnityEventTrackedListenerToken<T0> AddTrackedListener<T0>(this UnityEvent<T0> ev, UnityAction<T0> action) => UnityEventTrackedListenerToken<T0>.Create(ev, action);
        public static UnityEventTrackedListenerToken<T0, T1> AddTrackedListener<T0, T1>(this UnityEvent<T0, T1> ev, UnityAction<T0, T1> action) => UnityEventTrackedListenerToken<T0, T1>.Create(ev, action);
        public static UnityEventTrackedListenerToken<T0, T1, T2> AddTrackedListener<T0, T1, T2>(this UnityEvent<T0, T1, T2> ev, UnityAction<T0, T1, T2> action) => UnityEventTrackedListenerToken<T0, T1, T2>.Create(ev, action);
        public static UnityEventTrackedListenerToken<T0, T1, T2, T3> AddTrackedListener<T0, T1, T2, T3>(this UnityEvent<T0, T1, T2, T3> ev, UnityAction<T0, T1, T2, T3> action) => UnityEventTrackedListenerToken<T0, T1, T2, T3>.Create(ev, action);
    }

}
