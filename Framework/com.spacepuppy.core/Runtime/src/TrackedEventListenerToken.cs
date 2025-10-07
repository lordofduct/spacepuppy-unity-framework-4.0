using System;
using System.Collections.Generic;

using com.spacepuppy.Utils;


namespace com.spacepuppy
{

    public static partial class SPExtensions
    {

        public static TrackedListenerToken<T> AsTrackedListenerToken<T>(this ITrackedListenerToken<T> token) where T : System.Delegate
        {
            return token != null ? new TrackedListenerToken<T>(token.Target, token.Listener) : default;
        }

        public static EventHandlerRef GetEventRef(this object sender, string name)
        {
            if (sender == null) return default;

            var info = sender.GetType().GetEvent(name);
            if (info == null) return default;

            return EventHandlerRef.Create(l => info.AddEventHandler(sender, l), l => info.RemoveEventHandler(sender, l));
        }

        public static TrackedEventHandlerToken AddTrackedListener(this System.Reflection.EventInfo info, object sender, System.EventHandler listener)
        {
            if (info == null) return default;

            return EventHandlerRef.Create(l => info.AddEventHandler(sender, l), l => info.RemoveEventHandler(sender, l)).AddTrackedListener(listener);
        }

    }


    public interface ITrackableEvent<T> where T : System.Delegate
    {
        void AddListener(T listener);
        void RemoveListener(T listener);

        ITrackedListenerToken<T> AddTrackedListener(T listener) => TrackedListenerToken<T>.AddListener(this, listener);
    }

    public struct EventHandlerRef : ITrackableEvent<System.EventHandler>
    {

        private object source;
        private System.Delegate addcallback;
        private System.Delegate removecallback;

        public EventHandlerRef(System.Action<System.EventHandler> add, System.Action<System.EventHandler> remove)
        {
            this.source = null;
            this.addcallback = add;
            this.removecallback = remove;
        }

        public bool IsValid => addcallback != null && removecallback != null;

        public void AddListener(System.EventHandler listener)
        {
            if (addcallback == null) return;
            if (addcallback is System.Action<System.EventHandler> simple)
            {
                simple.Invoke(listener);
            }
            else
            {
                var arr = ArrayUtil.Temp<object>(source, listener);
                try
                {
                    addcallback.DynamicInvoke(arr);
                }
                catch (System.Exception ex)
                {
                    UnityEngine.Debug.LogException(ex);
                }
                finally
                {
                    ArrayUtil.ReleaseTemp(arr);
                }
            }
        }
        public void RemoveListener(System.EventHandler listener)
        {
            if (removecallback == null) return;
            if (removecallback is System.Action<System.EventHandler> simple)
            {
                simple.Invoke(listener);
            }
            else
            {
                var arr = ArrayUtil.Temp<object>(source, listener);
                try
                {
                    removecallback.DynamicInvoke(arr);
                }
                catch (System.Exception ex)
                {
                    UnityEngine.Debug.LogException(ex);
                }
                finally
                {
                    ArrayUtil.ReleaseTemp(arr);
                }
            }
        }
        public TrackedEventHandlerToken AddTrackedListener(System.EventHandler listener) => TrackedEventHandlerToken.AddListener(this, listener);



        public static EventHandlerRef Create(System.Action<System.EventHandler> add, System.Action<System.EventHandler> remove)
        {
            return new()
            {
                source = null,
                addcallback = add,
                removecallback = remove,
            };
        }

        public static EventHandlerRef Create<TSource>(TSource source, System.Action<TSource, System.EventHandler> add, System.Action<TSource, System.EventHandler> remove) where TSource : class
        {
            return new()
            {
                source = source,
                addcallback = add,
                removecallback = remove,
            };
        }

    }

    public struct EventHandlerRef<T> : ITrackableEvent<System.EventHandler<T>> where T : System.EventArgs
    {

        private object source;
        private System.Delegate addcallback;
        private System.Delegate removecallback;

        public EventHandlerRef(System.Action<System.EventHandler<T>> add, System.Action<System.EventHandler<T>> remove)
        {
            this.source = null;
            this.addcallback = add;
            this.removecallback = remove;
        }

        public bool IsValid => addcallback != null && removecallback != null;

        public void AddListener(System.EventHandler<T> listener)
        {
            if (addcallback == null) return;
            if (addcallback is System.Action<System.EventHandler<T>> simple)
            {
                simple.Invoke(listener);
            }
            else
            {
                var arr = ArrayUtil.Temp<object>(source, listener);
                try
                {
                    addcallback.DynamicInvoke(arr);
                }
                catch (System.Exception ex)
                {
                    UnityEngine.Debug.LogException(ex);
                }
                finally
                {
                    ArrayUtil.ReleaseTemp(arr);
                }
            }
        }
        public void RemoveListener(System.EventHandler<T> listener)
        {
            if (removecallback == null) return;
            if (removecallback is System.Action<System.EventHandler<T>> simple)
            {
                simple.Invoke(listener);
            }
            else
            {
                var arr = ArrayUtil.Temp<object>(source, listener);
                try
                {
                    removecallback.DynamicInvoke(arr);
                }
                catch (System.Exception ex)
                {
                    UnityEngine.Debug.LogException(ex);
                }
                finally
                {
                    ArrayUtil.ReleaseTemp(arr);
                }
            }
        }
        public TrackedEventHandlerToken<T> AddTrackedListener(System.EventHandler<T> listener) => TrackedEventHandlerToken<T>.AddListener(this, listener);


        public static EventHandlerRef<T> Create(System.Action<System.EventHandler<T>> add, System.Action<System.EventHandler<T>> remove)
        {
            return new()
            {
                source = null,
                addcallback = add,
                removecallback = remove,
            };
        }

        public static EventHandlerRef<T> Create<TSource>(TSource source, System.Action<TSource, System.EventHandler<T>> add, System.Action<TSource, System.EventHandler<T>> remove) where TSource : class
        {
            return new()
            {
                source = source,
                addcallback = add,
                removecallback = remove,
            };
        }

    }

    public struct DelegateRef<T> : ITrackableEvent<T> where T : System.Delegate
    {

        private object source;
        private System.Delegate addcallback;
        private System.Delegate removecallback;

        public DelegateRef(System.Action<T> add, System.Action<T> remove)
        {
            this.source = null;
            this.addcallback = add;
            this.removecallback = remove;
        }

        public bool IsValid => addcallback != null && removecallback != null;

        public void AddListener(T listener)
        {
            if (addcallback == null) return;
            if (addcallback is System.Action<T> simple)
            {
                simple.Invoke(listener);
            }
            else
            {
                var arr = ArrayUtil.Temp<object>(source, listener);
                try
                {
                    addcallback.DynamicInvoke(arr);
                }
                catch (System.Exception ex)
                {
                    UnityEngine.Debug.LogException(ex);
                }
                finally
                {
                    ArrayUtil.ReleaseTemp(arr);
                }
            }
        }
        public void RemoveListener(T listener)
        {
            if (removecallback == null) return;
            if (removecallback is System.Action<T> simple)
            {
                simple.Invoke(listener);
            }
            else
            {
                var arr = ArrayUtil.Temp<object>(source, listener);
                try
                {
                    removecallback.DynamicInvoke(arr);
                }
                catch (System.Exception ex)
                {
                    UnityEngine.Debug.LogException(ex);
                }
                finally
                {
                    ArrayUtil.ReleaseTemp(arr);
                }
            }
        }
        public TrackedListenerToken<T> AddTrackedListener(T listener) => TrackedListenerToken<T>.AddListener(this, listener);

        public static DelegateRef<T> Create(System.Action<T> add, System.Action<T> remove)
        {
            return new()
            {
                source = null,
                addcallback = add,
                removecallback = remove,
            };
        }

        public static DelegateRef<T> Create<TSource>(TSource source, System.Action<TSource, T> add, System.Action<TSource, T> remove) where TSource : class
        {
            return new()
            {
                source = source,
                addcallback = add,
                removecallback = remove,
            };
        }

    }



    public interface ITrackedListenerToken<T> : System.IDisposable where T : System.Delegate
    {
        ITrackableEvent<T> Target { get; }
        T Listener { get; }
    }

    /// <summary>
    /// Generalized listener token.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct TrackedListenerToken<T> : ITrackedListenerToken<T> where T : System.Delegate
    {

        private ITrackableEvent<T> _target;
        private T _listener;

        public TrackedListenerToken(ITrackableEvent<T> target, T listener)
        {
            if (target == null) throw new System.ArgumentNullException(nameof(target));
            this._target = target;
            this._listener = listener;
        }

        public ITrackableEvent<T> Target => _target;
        public T Listener => _listener;

        public void Dispose()
        {
            if (_target != null && _listener != null)
            {
                _target?.RemoveListener(_listener);
                _target = null;
                _listener = null;
            }
        }

        public static TrackedListenerToken<T> Create(ITrackableEvent<T> target, T listener)
        {
            if (target == null) throw new System.ArgumentNullException(nameof(target));
            if (listener == null) return default;
            return new()
            {
                _target = target,
                _listener = listener
            };
        }

        public static TrackedListenerToken<T> AddListener(ITrackableEvent<T> target, T listener)
        {
            if (target == null) throw new System.ArgumentNullException(nameof(target));
            if (listener == null) return default;
            target.AddListener(listener);
            return new()
            {
                _target = target,
                _listener = listener
            };
        }

    }


    public struct TrackedEventHandlerToken : ITrackedListenerToken<System.EventHandler>
    {

        public EventHandlerRef target;
        private System.EventHandler _listener;

        ITrackableEvent<EventHandler> ITrackedListenerToken<System.EventHandler>.Target => target;
        System.EventHandler ITrackedListenerToken<System.EventHandler>.Listener => _listener;

        public void Dispose()
        {
            if (_listener != null)
            {
                target.RemoveListener(_listener);
                target = default;
                _listener = null;
            }
        }

        public static TrackedEventHandlerToken Create(EventHandlerRef target, System.EventHandler listener)
        {
            if (listener == null) return default;
            return new()
            {
                target = target,
                _listener = listener
            };
        }

        public static TrackedEventHandlerToken AddListener(EventHandlerRef target, System.EventHandler listener)
        {
            if (listener == null) return default;
            target.AddListener(listener);
            return new()
            {
                target = target,
                _listener = listener
            };
        }

        public static implicit operator TrackedListenerToken<System.EventHandler>(TrackedEventHandlerToken token)
        {
            return new TrackedListenerToken<System.EventHandler>(token.target, token._listener);
        }

    }

    public struct TrackedEventHandlerToken<T> : ITrackedListenerToken<System.EventHandler<T>> where T : System.EventArgs
    {

        public EventHandlerRef<T> target;
        private System.EventHandler<T> _listener;

        ITrackableEvent<EventHandler<T>> ITrackedListenerToken<System.EventHandler<T>>.Target => target;
        System.EventHandler<T> ITrackedListenerToken<System.EventHandler<T>>.Listener => _listener;

        public void Dispose()
        {
            if (_listener != null)
            {
                target.RemoveListener(_listener);
                target = default;
                _listener = null;
            }
        }

        public static TrackedEventHandlerToken<T> Create(EventHandlerRef<T> target, System.EventHandler<T> listener)
        {
            if (listener == null) return default;
            return new()
            {
                target = target,
                _listener = listener
            };
        }

        public static TrackedEventHandlerToken<T> AddListener(EventHandlerRef<T> target, System.EventHandler<T> listener)
        {
            if (listener == null) return default;
            target.AddListener(listener);
            return new()
            {
                target = target,
                _listener = listener
            };
        }

        public static implicit operator TrackedListenerToken<System.EventHandler<T>>(TrackedEventHandlerToken<T> token)
        {
            return new TrackedListenerToken<System.EventHandler<T>>(token.target, token._listener);
        }

    }

    /// <summary>
    /// Effectively a linked-list of ITrackedListenerToken to allow referencing multiple tokens in one field. 
    /// </summary>
    /// <remarks>
    /// While this technically supports any IDisposable, it's intended for ITrackedListenerToken. 
    /// You 'can' use it for other disposable contracts, but the consequences of doing so hinge on the IDisposable 
    /// in question.
    /// </remarks>
    public struct MultiTrackedListenerToken : System.IDisposable
    {

        private System.IDisposable _this;
        private System.IDisposable _next;

        public void Add(System.IDisposable next)
        {
            if (this._this == null)
            {
                if (next is MultiTrackedListenerToken t)
                {
                    _this = t._this;
                    _next = t._next;
                }
                else
                {
                    _this = next;
                }
            }
            else if (_next == null)
            {
                _next = next;
                return;
            }
            else if (next is MultiTrackedListenerToken nt)
            {
                if (nt._this == null)
                {
                    //do nothing
                }
                else if (nt._next == null)
                {
                    var copy = this;
                    _this = nt._this;
                    _next = copy;
                }
                else
                {
                    //walk next and append it to this
                    var a = nt._this;
                    var b = nt._next;

                    while (a != null)
                    {
                        var copy = this;
                        _this = a;
                        _next = copy;

                        if (b is MultiTrackedListenerToken t)
                        {
                            b = t._next;
                            a = t._this;
                        }
                        else
                        {
                            a = b;
                            b = null;
                        }
                    }
                }
            }
            else
            {
                var copy = this;
                _this = next;
                _next = copy;
            }
        }

        public void AddRange(IEnumerable<System.IDisposable> e)
        {
            if (e == null) return;

            foreach (var d in e)
            {
                if (d == null) continue;
                this.Add(d);
            }
        }

        public void Dispose()
        {
            var a = _this;
            var b = _next;
            _this = null;
            _next = null;

            while (a != null)
            {
                try
                {
                    a?.Dispose();
                }
                catch (System.Exception ex)
                {
                    //if we have an exception stop walking and set ourselves to the failed position in the linked list
                    _this = a;
                    _next = b;
                    throw ex;
                }

                if (b is MultiTrackedListenerToken t)
                {
                    b = t._next;
                    a = t._this;
                }
                else
                {
                    a = b;
                    b = null;
                }
            }
        }

        public static MultiTrackedListenerToken operator +(MultiTrackedListenerToken @this, System.IDisposable next)
        {
            if (@this._this == null)
            {
                if (next is MultiTrackedListenerToken t)
                {
                    return t;
                }
                else
                {
                    @this._this = next;
                    return @this;
                }
            }
            else if (@this._next == null)
            {
                @this._next = next;
                return @this;
            }
            else if (next is MultiTrackedListenerToken nt)
            {
                if (nt._this == null)
                {
                    return @this;
                }
                else if (nt._next == null)
                {
                    nt._next = @this;
                    return nt;
                }
                else
                {
                    //walk next and append it to this
                    var a = nt._this;
                    var b = nt._next;

                    while (a != null)
                    {
                        @this = new MultiTrackedListenerToken()
                        {
                            _this = a,
                            _next = @this,
                        };

                        if (b is MultiTrackedListenerToken t)
                        {
                            b = t._next;
                            a = t._this;
                        }
                        else
                        {
                            a = b;
                            b = null;
                        }
                    }
                    return @this;
                }
            }
            else
            {
                return new MultiTrackedListenerToken()
                {
                    _this = next,
                    _next = @this,
                };
            }
        }

    }

}
