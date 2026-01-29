using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy.Collections;
using com.spacepuppy.Project;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Geom
{

    /// <summary>
    /// Represents a component that can send either ICompoundTriggerEnterHandler or ICompoundTriggerExitHandler messages.
    /// </summary>
    public interface ICompoundTrigger : IComponent
    {
        bool ContainsActive();
        bool ContainsActive(Collider c);
        bool InMessagePath(GameObject go);
    }

    [System.Serializable]
    public class ICompoundTriggerRef : InterfaceRef<ICompoundTrigger> { }

    public interface ICompoundTriggerEnterHandler
    {
        void OnCompoundTriggerEnter(ICompoundTrigger trigger, Collider other);
    }
    [UnityEngine.Scripting.Preserve]
    class CompoundTriggerEnterHandlerHook : Messaging.SubscribableMessageHook<ICompoundTriggerEnterHandler>, ICompoundTriggerEnterHandler
    {
        void ICompoundTriggerEnterHandler.OnCompoundTriggerEnter(ICompoundTrigger trigger, Collider other) => this.Signal((trigger, other), (o, a) => o.OnCompoundTriggerEnter(a.trigger, a.other));
    }

    public interface ICompoundTriggerExitHandler
    {
        void OnCompoundTriggerExit(ICompoundTrigger trigger, Collider other);
    }
    [UnityEngine.Scripting.Preserve]
    class CompoundTriggerExitHandlerHook : Messaging.SubscribableMessageHook<ICompoundTriggerExitHandler>, ICompoundTriggerExitHandler
    {
        void ICompoundTriggerExitHandler.OnCompoundTriggerExit(ICompoundTrigger trigger, Collider other) => this.Signal((trigger, other), (o, a) => o.OnCompoundTriggerExit(a.trigger, a.other));
    }

    public interface ICompoundTriggerStayHandler
    {
        void OnCompoundTriggerStay(ICompoundTrigger trigger, Collider other);
    }
    [UnityEngine.Scripting.Preserve]
    public class CompoundTriggerStayHandlerHook : Messaging.SubscribableMessageHook<ICompoundTriggerStayHandler>, ICompoundTriggerStayHandler
    {
        void ICompoundTriggerStayHandler.OnCompoundTriggerStay(ICompoundTrigger trigger, Collider other) => this.Signal((trigger, other), (o, a) => o.OnCompoundTriggerStay(a.trigger, a.other));
    }

    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ExpectsCompoundTriggerAttribute : ComponentHeaderAttribute
    {
        public string CustomMessage;
        public System.Type RestrictType;

        public ExpectsCompoundTriggerAttribute()
        {
            this.CustomMessage = null;
        }

        public ExpectsCompoundTriggerAttribute(string customMessage)
        {
            this.CustomMessage = customMessage;
        }
    }

    [Infobox("Colliders on or in this GameObject are grouped together and treated as a single collider signaling with the ICompoundTriggerXHandler message.")]
    public class CompoundTrigger : SPComponent, ICompoundTrigger
    {

        protected System.EventHandler _activeTargetsChanged;
        public event System.EventHandler ActiveTargetsChanged
        {
            add { _activeTargetsChanged += value; }
            remove { _activeTargetsChanged -= value; }
        }

        [System.Flags]
        public enum ConfigurationOptions
        {
            SendMessageToOtherCollider = 1,
            HandleColliderStayEvent = 2,
            ForceTriggerExitOnDisable = 4
        }

        #region Fields

        [SerializeField]
        private EventActivatorMaskRef _mask = new EventActivatorMaskRef();

        [SerializeField]
        [EnumFlags]
        private ConfigurationOptions _configuration;

        protected readonly Dictionary<Collider, CompoundTriggerMember> _colliders = new Dictionary<Collider, CompoundTriggerMember>();
        protected readonly HashSet<Collider> _active = new HashSet<Collider>();

        private int _lastStayFrame = -1;
        private HashSet<Collider> _signaled;

        [SerializeField]
        [DisableOnPlay]
        [DisplayFlat(DisplayBox = true)]
        [UnityEngine.Serialization.FormerlySerializedAs("_onEnterMessageSetting")]
        protected Messaging.MessageSendCommand _messageSettings = new Messaging.MessageSendCommand()
        {
            SendMethod = Messaging.MessageSendMethod.Signal,
            IncludeDisabledComponents = false,
            IncludeInactiveObjects = false,
        };

        [SerializeField]
#if UNITY_EDITOR
        [DisplayIf(nameof(SendMessageToOtherCollider))]
#endif
        [DisableOnPlay]
        [DisplayFlat(DisplayBox = true)]
        protected Messaging.MessageSendCommand _otherColliderMessageSettings = new Messaging.MessageSendCommand()
        {
            SendMethod = Messaging.MessageSendMethod.Signal,
            IncludeDisabledComponents = false,
            IncludeInactiveObjects = false,
        };

        protected bool _isDirty;

        private ActiveColliderCollection _activeCollidersWrapper;

        #endregion

        #region CONSTRUCTOR

        protected override void OnEnable()
        {
            this.SyncTriggers();
            base.OnEnable();

            if (this.started)
            {
                this.ResyncStateAndSignalMessageIfNecessary();
            }
        }

        protected override void Start()
        {
            this.SyncTriggers();
            base.Start();

            this.ResyncStateAndSignalMessageIfNecessary();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (_active.Count > 0 && (_configuration & ConfigurationOptions.ForceTriggerExitOnDisable) != 0 && !GameLoop.ApplicationClosing)
            {
                this.HandleSignalingExitOnDisable();
            }
            _active.Clear();
            _signaled?.Clear();
            _isDirty = false;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Is only accurate once started.
        /// </summary>
        public int MemberColliderCount => _colliders.Count;

        public IEventActivatorMask Mask
        {
            get => _mask.Value;
            set => _mask.Value = value;
        }

        /// <summary>
        /// Set the configuration of the trigger. 
        /// Modifying this after the component has started and is active 
        /// may require a call to 'SyncTriggers' or toggling the 'enabled' property. 
        /// </summary>
        public ConfigurationOptions Configuration
        {
            get => _configuration;
            set => _configuration = value;
        }

        public Messaging.MessageSendCommand MessageSettings
        {
            get => _messageSettings;
            set => _messageSettings = value;
        }

        public Messaging.MessageSendCommand OtherColliderMessageSettings
        {
            get => _otherColliderMessageSettings;
            set => _otherColliderMessageSettings = value;
        }

        public ActiveColliderCollection ActiveColliders => (_activeCollidersWrapper ??= new(this));

        #endregion

        #region Methods

        public void SyncTriggers()
        {
            bool handleColliderStayEvent = (_configuration & ConfigurationOptions.HandleColliderStayEvent) != 0;
            using (var lst = TempCollection.GetList<Collider>())
            {
                this.GetComponentsInChildren<Collider>(true, lst);

                //purge entries if necessary
                if (_colliders.Count > 0)
                {
                    using (var purge = TempCollection.GetList<Collider>())
                    {
                        var ed = _colliders.GetEnumerator();
                        while (ed.MoveNext())
                        {
                            if (!ObjUtil.IsObjectAlive(ed.Current.Key) || ed.Current.Value == null || !lst.Contains(ed.Current.Key)
                                || (handleColliderStayEvent && !(ed.Current.Value is CompoundTriggerStayMember))
                                || (!handleColliderStayEvent && (ed.Current.Value is CompoundTriggerStayMember)))
                            {
                                purge.Add(ed.Current.Key);
                                ObjUtil.SmartDestroy(ed.Current.Value);
                            }
                        }
                        if (purge.Count > 0)
                        {
                            var e = purge.GetEnumerator();
                            while (e.MoveNext())
                            {
                                _colliders.Remove(e.Current);
                            }
                        }
                    }
                }

                //fill unknowns
                if (lst.Count > 0)
                {
                    var e = lst.GetEnumerator();
                    while (e.MoveNext())
                    {
                        if (!_colliders.ContainsKey(e.Current))
                        {
                            CompoundTriggerMember m = handleColliderStayEvent ? e.Current.AddComponent<CompoundTriggerStayMember>() : e.Current.AddComponent<CompoundTriggerMember>();
                            m.Init(this, e.Current);
                            _colliders.Add(e.Current, m);
                        }
                    }
                }
            }
        }

        public CompoundTriggerMember GetMember(Collider collider) => collider && _colliders.TryGetValue(collider, out var m) ? m : null;

        /// <summary>
        /// The colliders that make up this compoundtrigger
        /// </summary>
        /// <returns></returns>
        public IReadOnlyCollection<CompoundTriggerMember> GetMembers() => _colliders.Values;

        /// <summary>
        /// The colliders that make up this compoundtrigger
        /// </summary>
        /// <returns></returns>
        public IReadOnlyCollection<Collider> GetMemberColliders() => _colliders.Keys;

        /// <summary>
        /// The 'other' colliders that are currently inside this compoundtrigger
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Collider> GetActiveColliders()
        {
            if (_isDirty) this.CleanActive();
            if (_active.Count == 0) return Enumerable.Empty<Collider>();

            return _active.Where(this.ValidateEntryOrSetDirty);
        }

        public bool Contains(Vector3 position)
        {
            Vector3 p;
            foreach (var c in _colliders.Keys)
            {
                if (c.enabled && c.gameObject.activeInHierarchy)
                {
                    p = c.ClosestPoint(position);
                    if (Vector3.SqrMagnitude(p - position) < MathUtil.EPSILON_SQR)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool ContainsActive()
        {
            if (_active.Count == 0) return false;

            var e = _active.GetEnumerator();
            try
            {
                while (e.MoveNext())
                {
                    if (this.ValidateEntryOrSetDirty(e.Current))
                    {
                        return true;
                    }
                }
            }
            finally
            {
                if (_isDirty) this.CleanActive();
            }

            return false;
        }

        public bool ContainsActive(Collider c) => c != null && _active.Contains(c);

        public int GetActiveColliders(ICollection<Collider> output)
        {
            if (_active.Count == 0) return 0;

            var e = _active.GetEnumerator();
            int cnt = 0;
            try
            {
                while (e.MoveNext())
                {
                    if (this.ValidateEntryOrSetDirty(e.Current))
                    {
                        cnt++;
                        output.Add(e.Current);
                    }
                }
            }
            finally
            {
                if (_isDirty) this.CleanActive();
            }
            return cnt;
        }

        public IEnumerable<T> FilterActiveColliders<T>() where T : class
        {
            if (_active.Count == 0) yield break;

            var e = _active.GetEnumerator();
            while (e.MoveNext())
            {
                if (!this.ValidateEntryOrSetDirty(e.Current)) continue;
                if (ObjUtil.GetAsFromSource(e.Current, out T o)) yield return o;
            }
        }

        public IEnumerable<T> FilterActiveColliders<T>(System.Func<T, bool> filter) where T : class
        {
            if (_active.Count == 0) yield break;

            var e = _active.GetEnumerator();
            while (e.MoveNext())
            {
                if (!this.ValidateEntryOrSetDirty(e.Current)) continue;
                if (ObjUtil.GetAsFromSource(e.Current, out T o) && (filter?.Invoke(o) ?? true)) yield return o;
            }
        }

        public IEnumerable<T> FilterActiveColliders<T>(System.Func<T, bool> filter, System.Func<Collider, T> cast) where T : class
        {
            if (_active.Count == 0) yield break;

            var e = _active.GetEnumerator();
            if (cast != null)
            {
                while (e.MoveNext())
                {
                    if (!this.ValidateEntryOrSetDirty(e.Current)) continue;
                    var o = cast(e.Current);
                    if (o != null && (filter?.Invoke(o) ?? true)) yield return o;
                }
            }
            else
            {
                while (e.MoveNext())
                {
                    if (!this.ValidateEntryOrSetDirty(e.Current)) continue;
                    if (ObjUtil.GetAsFromSource(e.Current, out T o) && (filter?.Invoke(o) ?? true)) yield return o;
                }
            }
        }

        public int FilterActiveColliders<T>(ICollection<T> output) where T : class
        {
            if (_active.Count == 0) return 0;

            var e = _active.GetEnumerator();
            int cnt = 0;
            try
            {
                while (e.MoveNext())
                {
                    if (this.ValidateEntryOrSetDirty(e.Current) && ObjUtil.GetAsFromSource(e.Current, out T o))
                    {
                        cnt++;
                        output.Add(o);
                    }
                }
            }
            finally
            {
                if (_isDirty) this.CleanActive();
            }
            return cnt;
        }

        public int FilterActiveColliders<T>(ICollection<T> output, System.Func<T, bool> filter) where T : class
        {
            if (_active.Count == 0) return 0;

            var e = _active.GetEnumerator();
            int cnt = 0;
            try
            {
                while (e.MoveNext())
                {
                    if (this.ValidateEntryOrSetDirty(e.Current) && ObjUtil.GetAsFromSource(e.Current, out T o) && (filter?.Invoke(o) ?? true))
                    {
                        cnt++;
                        output.Add(o);
                    }
                }
            }
            finally
            {
                if (_isDirty) this.CleanActive();
            }
            return cnt;
        }

        public int FilterActiveColliders<T>(ICollection<T> output, System.Func<T, bool> filter, System.Func<Collider, T> cast) where T : class
        {
            if (_active.Count == 0) return 0;

            var e = _active.GetEnumerator();
            int cnt = 0;
            try
            {
                while (e.MoveNext())
                {
                    if (!this.ValidateEntryOrSetDirty(e.Current)) continue;

                    var o = cast != null ? cast(e.Current) : ObjUtil.GetAsFromSource<T>(e.Current);
                    if (o != null && (filter?.Invoke(o) ?? true))
                    {
                        cnt++;
                        output.Add(o);
                    }
                }
            }
            finally
            {
                if (_isDirty) this.CleanActive();
            }
            return cnt;
        }

        protected bool AnyRelatedColliderOverlaps(Collider c, out CompoundTriggerMember member)
        {
            var e = _colliders.GetEnumerator();
            while (e.MoveNext())
            {
                if (e.Current.Value.ActiveRaw.Contains(c))
                {
                    member = e.Current.Value;
                    return true;
                }
            }
            member = null;
            return false;
        }

        protected virtual void SignalTriggerEnter(CompoundTriggerMember member, Collider other)
        {
            if (this.isActiveAndEnabled && (_mask.Value?.Intersects(other) ?? true) && _active.Add(other))
            {
                _activeTargetsChanged?.Invoke(this, System.EventArgs.Empty);
                _messageSettings.Send(this.gameObject, (this, other), OnEnterFunctor);
                if ((_configuration & ConfigurationOptions.SendMessageToOtherCollider) != 0) _otherColliderMessageSettings.Send(other.gameObject, (this, member.Collider), OnEnterFunctor);
            }
        }

        protected virtual void SignalTriggerExit(CompoundTriggerMember member, Collider other)
        {
            if (!this.isActiveAndEnabled) return;
            if (this.AnyRelatedColliderOverlaps(other, out _)) return;

            if (_active.Remove(other))
            {
                _activeTargetsChanged?.Invoke(this, System.EventArgs.Empty);
                _messageSettings.Send(this.gameObject, (this, other), OnExitFunctor);
                if ((_configuration & ConfigurationOptions.SendMessageToOtherCollider) != 0) _otherColliderMessageSettings.Send(other.gameObject, (this, member.Collider), OnExitFunctor);
            }
        }

        protected virtual void HandleSignalingExitOnDisable()
        {
            foreach (var other in _active)
            {
                if (!other) continue;
                _messageSettings.Send(this.gameObject, (this, other), OnExitFunctor);
                if ((_configuration & ConfigurationOptions.SendMessageToOtherCollider) != 0)
                {
                    CompoundTriggerMember member;
                    Collider membercoll;
                    if (this.AnyRelatedColliderOverlaps(other, out member)) membercoll = member.Collider;
                    else membercoll = _colliders.Keys.FirstOrDefault(c => c.enabled && c.gameObject.activeInHierarchy);
                    _otherColliderMessageSettings.Send(other.gameObject, (this, membercoll), OnExitFunctor);
                }
            }
            _active.Clear();
            _activeTargetsChanged?.Invoke(this, System.EventArgs.Empty);
        }

        protected virtual void SignalTriggerStay(CompoundTriggerStayMember member, Collider other)
        {
            if (!this.isActiveAndEnabled || !_active.Contains(other)) return;

            if (_signaled == null) _signaled = new HashSet<Collider>();

            if (GameLoop.FixedFrameCount != _lastStayFrame)
            {
                _lastStayFrame = GameLoop.FixedFrameCount;
                _signaled.Clear();
            }

            if (_signaled.Add(other))
            {
                _messageSettings.Send(this.gameObject, (this, other), OnStayFunctor);
                if ((_configuration & ConfigurationOptions.SendMessageToOtherCollider) != 0) _otherColliderMessageSettings.Send(other.gameObject, (this, member.Collider), OnStayFunctor);
            }
        }

        private void ResyncStateAndSignalMessageIfNecessary()
        {
            _active.Clear();
            var ed = _colliders.GetEnumerator();
            while (ed.MoveNext())
            {
                ed.Current.Value.CleanActive();
                if (ed.Current.Value.ActiveRaw.Count > 0)
                {
                    foreach (var a in ed.Current.Value.ActiveRaw)
                    {
                        this.SignalTriggerEnter(ed.Current.Value, a);
                    }
                }
            }
        }

        protected virtual void CleanActive()
        {
            if (_active.Count > 0)
            {
                _active.RemoveWhere(o => !ObjUtil.IsObjectAlive(o) || !o.IsActiveAndEnabled());
            }

            foreach (var pair in _colliders)
            {
                pair.Value.CleanActive();
            }
            _isDirty = false;
        }

        protected bool ValidateEntryOrSetDirty(Collider o)
        {
            if (ObjUtil.IsObjectAlive(o) && o.IsActiveAndEnabled())
            {
                return true;
            }
            else
            {
                _isDirty = true;
                return false;
            }
        }

        #endregion

        #region ICompoundTrigger Interface

        public bool InMessagePath(GameObject go)
        {
            return _messageSettings.IsInMessagePath(this.gameObject, go);
        }

        #endregion

        #region Special Types

        public class CompoundTriggerMember : MonoBehaviour
        {

            [System.NonSerialized]
            private protected CompoundTrigger _owner;
            [System.NonSerialized]
            private protected Collider _collider;
            [System.NonSerialized]
            private protected HashSet<Collider> _active = new HashSet<Collider>();

            public CompoundTrigger Owner
            {
                get { return _owner; }
            }

            public Collider Collider
            {
                get { return _collider; }
            }

            public IReadOnlyCollection<Collider> Active => _active;
            protected internal HashSet<Collider> ActiveRaw => _active;

            internal void Init(CompoundTrigger owner, Collider collider)
            {
                _owner = owner;
                _collider = collider;
            }

            public void CleanActive()
            {
                if (_active.Count > 0)
                {
                    _active.RemoveWhere(o => !ObjUtil.IsObjectAlive(o) || !o.IsActiveAndEnabled());
                }
            }

            private void OnDisable()
            {
                _active.Clear();
            }

            private void OnTriggerEnter(Collider other)
            {
                if (!this.isActiveAndEnabled) return;
                if (_active.Add(other))
                {
                    if (_owner != null) _owner.SignalTriggerEnter(this, other);
                }
            }

            private void OnTriggerExit(Collider other)
            {
                if (!this.isActiveAndEnabled) return;
                if (_active.Remove(other))
                {
                    if (_owner != null) _owner.SignalTriggerExit(this, other);
                }
            }

        }

        protected class CompoundTriggerStayMember : CompoundTriggerMember
        {

            private void OnTriggerStay(Collider other)
            {
                if (!this.isActiveAndEnabled) return;

                if (_active.Contains(other))
                {
                    if (_owner != null) _owner.SignalTriggerStay(this, other);
                }
            }

        }

        public class ActiveColliderCollection : IEnumerable<Collider>
        {

            private CompoundTrigger _owner;
            internal ActiveColliderCollection(CompoundTrigger owner)
            {
                _owner = owner;
            }

            public int Count => _owner._active.Count;

            public bool Contains(Collider item)
            {
                if (!item) return false;
                if (_owner._isDirty) _owner.CleanActive();
                return _owner._active.Contains(item);
            }
            public void CopyTo(Collider[] array, int arrayIndex)
            {
                var e = new ActiveColliderEnumerator(_owner);
                while (e.MoveNext() && arrayIndex < array.Length)
                {
                    array[arrayIndex] = e.Current;
                    arrayIndex++;
                }
            }

            public ActiveColliderEnumerator GetEnumerator() => new ActiveColliderEnumerator(_owner);
            IEnumerator<Collider> IEnumerable<Collider>.GetEnumerator() => new ActiveColliderEnumerator(_owner);
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => new ActiveColliderEnumerator(_owner);

        }

        public struct ActiveColliderEnumerator : IEnumerator<Collider>
        {
            private CompoundTrigger _owner;
            private HashSet<Collider>.Enumerator _e;
            internal ActiveColliderEnumerator(CompoundTrigger owner)
            {
                _owner = owner;
                if (_owner._isDirty) _owner.CleanActive();
                _e = owner._active.GetEnumerator();
            }

            public Collider Current => _e.Current;
            object System.Collections.IEnumerator.Current => _e.Current;

            public void Dispose() => _e.Dispose();
            public bool MoveNext()
            {
                while (_e.MoveNext())
                {
                    if (_owner.ValidateEntryOrSetDirty(_e.Current)) return true;
                }
                return false;
            }
            void System.Collections.IEnumerator.Reset() => (_e as System.Collections.IEnumerator).Reset();

        }

        #endregion

        #region Messages

        public static ICompoundTrigger FindCompoundTriggerWithTarget(GameObject go)
        {
            using (var lst = TempCollection.GetList<ICompoundTrigger>())
            {
                var entity = SPEntity.Pool.GetFromSource(go);
                if (entity)
                {
                    entity.GetComponentsInChildren(lst);
                    foreach (var t in lst)
                    {
                        if (t.InMessagePath(go)) return t;
                    }
                }
                else
                {
                    go.GetComponentsInChildren(lst);
                    foreach (var t in lst)
                    {
                        if (t.InMessagePath(go)) return t;
                    }
                }

                lst.Clear();
                go.GetComponentsInParent(true, lst);
                foreach (var t in lst)
                {
                    if (t.InMessagePath(go)) return t;
                }
            }

            return null;
        }

        public static T FindCompoundTriggerWithTarget<T>(GameObject go) where T : class, ICompoundTrigger
        {
            using (var lst = TempCollection.GetList<T>())
            {
                var entity = SPEntity.Pool.GetFromSource(go);
                if (entity)
                {
                    entity.GetComponentsInChildren(lst);
                    foreach (var t in lst)
                    {
                        if (t.InMessagePath(go)) return t;
                    }
                }
                else
                {
                    go.GetComponentsInChildren(lst);
                    foreach (var t in lst)
                    {
                        if (t.InMessagePath(go)) return t;
                    }
                }

                lst.Clear();
                go.GetComponentsInParent(true, lst);
                foreach (var t in lst)
                {
                    if (t.InMessagePath(go)) return t;
                }
            }

            return null;
        }

        public static ICompoundTrigger FindCompoundTriggerWithTarget(GameObject go, System.Type restrictType)
        {
            if (restrictType == null) return FindCompoundTriggerWithTarget(go);

            using (var lst = TempCollection.GetList<ICompoundTrigger>())
            {
                var entity = SPEntity.Pool.GetFromSource(go);
                if (entity)
                {
                    entity.GetComponentsInChildren(lst);
                    foreach (var t in lst)
                    {
                        if (restrictType.IsInstanceOfType(t) && t.InMessagePath(go)) return t;
                    }
                }
                else
                {
                    go.GetComponentsInChildren(lst);
                    foreach (var t in lst)
                    {
                        if (restrictType.IsInstanceOfType(t) && t.InMessagePath(go)) return t;
                    }
                }

                lst.Clear();
                go.GetComponentsInParent(true, lst);
                foreach (var t in lst)
                {
                    if (restrictType.IsInstanceOfType(t) && t.InMessagePath(go)) return t;
                }
            }

            return null;
        }

        public static readonly System.Action<ICompoundTriggerEnterHandler, (ICompoundTrigger, Collider)> OnEnterFunctor = (x, y) => x.OnCompoundTriggerEnter(y.Item1, y.Item2);
        public static readonly System.Action<ICompoundTriggerExitHandler, (ICompoundTrigger, Collider)> OnExitFunctor = (x, y) => x.OnCompoundTriggerExit(y.Item1, y.Item2);
        public static readonly System.Action<ICompoundTriggerStayHandler, (ICompoundTrigger, Collider)> OnStayFunctor = (x, y) => x.OnCompoundTriggerStay(y.Item1, y.Item2);

        #endregion

#if UNITY_EDITOR
        private bool SendMessageToOtherCollider => (_configuration & ConfigurationOptions.SendMessageToOtherCollider) != 0;
#endif

    }

}
