using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy.Collections;
using com.spacepuppy.Utils;
using UnityEngine.Scripting;

namespace com.spacepuppy.Geom
{

    /// <summary>
    /// Represents a component that can send either ICompoundTriggerEnterHandler or ICompoundTriggerExitHandler messages.
    /// </summary>
    public interface ICompoundTrigger : IComponent
    {
        bool InMessagePath(GameObject go);
    }

    public interface ICompoundTriggerEnterHandler
    {
        void OnCompoundTriggerEnter(ICompoundTrigger trigger, Collider other);
    }
    [Preserve]
    class CompoundTriggerEnterHandlerHook : Messaging.SubscribableMessageHook<ICompoundTriggerEnterHandler>, ICompoundTriggerEnterHandler
    {
        void ICompoundTriggerEnterHandler.OnCompoundTriggerEnter(ICompoundTrigger trigger, Collider other) => this.Signal((trigger, other), (o, a) => o.OnCompoundTriggerEnter(a.trigger, a.other));
    }

    public interface ICompoundTriggerExitHandler
    {
        void OnCompoundTriggerExit(ICompoundTrigger trigger, Collider other);
    }
    [Preserve]
    class CompoundTriggerExitHandlerHook : Messaging.SubscribableMessageHook<ICompoundTriggerExitHandler>, ICompoundTriggerExitHandler
    {
        void ICompoundTriggerExitHandler.OnCompoundTriggerExit(ICompoundTrigger trigger, Collider other) => this.Signal((trigger, other), (o, a) => o.OnCompoundTriggerExit(a.trigger, a.other));
    }

    public interface ICompoundTriggerStayHandler
    {
        void OnCompoundTriggerStay(ICompoundTrigger trigger, Collider other);
    }
    [Preserve]
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

        #region Fields

        [SerializeField]
        private EventActivatorMaskRef _mask = new EventActivatorMaskRef();

        [SerializeField]
        private bool _sendMessageToOtherCollider;

        [SerializeField]
        private bool _handleColliderStayEvent;

        private Dictionary<Collider, CompoundTriggerMember> _colliders = new Dictionary<Collider, CompoundTriggerMember>();
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
        [DisplayIf(nameof(SendMessageToOtherCollider))]
        [DisableOnPlay]
        [DisplayFlat(DisplayBox = true)]
        protected Messaging.MessageSendCommand _otherColliderMessageSettings = new Messaging.MessageSendCommand()
        {
            SendMethod = Messaging.MessageSendMethod.Signal,
            IncludeDisabledComponents = false,
            IncludeInactiveObjects = false,
        };

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

            _active.Clear();
        }

        #endregion

        #region Properties

        public IEventActivatorMask Mask
        {
            get => _mask.Value;
            set => _mask.Value = value;
        }

        public bool SendMessageToOtherCollider
        {
            get => _sendMessageToOtherCollider;
            set => _sendMessageToOtherCollider = value;
        }

        /// <summary>
        /// Flag if the stay event should also be raised. 
        /// </summary>
        /// <remarks>
        /// Do not toggle this frequently as it causes the creation/destruction 
        /// of MonoBehaviours which can lead to poor performance.
        /// </remarks>
        public bool HandleColliderStayEvent
        {
            get => _handleColliderStayEvent;
            set
            {
                if (_handleColliderStayEvent == value) return;
                _handleColliderStayEvent = value;
                if (this.isActiveAndEnabled) this.SyncTriggers();
            }
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

        #endregion

        #region Methods

        public void SyncTriggers()
        {
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
                                || (_handleColliderStayEvent && !(ed.Current.Value is CompoundTriggerStayMember))
                                || (!_handleColliderStayEvent && (ed.Current.Value is CompoundTriggerStayMember)))
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
                            CompoundTriggerMember m = _handleColliderStayEvent ? e.Current.AddComponent<CompoundTriggerStayMember>() : e.Current.AddComponent<CompoundTriggerMember>();
                            m.Init(this, e.Current);
                            _colliders.Add(e.Current, m);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The colliders that make up this compoundtrigger
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Collider> GetMemberColliders() => _colliders.Keys;

        /// <summary>
        /// The 'other' colliders that are currently inside this compoundtrigger
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Collider> GetActiveColliders() => _active.Where(o => o != null);

        public bool ContainsActive(Collider c) => c != null && _active.Contains(c);

        public int GetActiveColliders(ICollection<Collider> output)
        {
            if (_active.Count == 0) return 0;

            var e = _active.GetEnumerator();
            int cnt = 0;
            bool doclean = false;
            try
            {
                while (e.MoveNext())
                {
                    if (!ObjUtil.IsObjectAlive(e.Current))
                    {
                        doclean = true;
                        continue;
                    }

                    cnt++;
                    output.Add(e.Current);
                }
            }
            finally
            {
                if (doclean) this.CleanActive();
            }
            return cnt;
        }

        protected bool AnyRelatedColliderOverlaps(Collider c)
        {
            var e = _colliders.GetEnumerator();
            while (e.MoveNext())
            {
                if (e.Current.Value.Active.Contains(c)) return true;
            }
            return false;
        }

        protected virtual void SignalTriggerEnter(CompoundTriggerMember member, Collider other)
        {
            if (this.isActiveAndEnabled && (_mask.Value?.Intersects(other) ?? true) && _active.Add(other))
            {
                _messageSettings.Send(this.gameObject, (this, other), OnEnterFunctor);
                _otherColliderMessageSettings.Send(other.gameObject, (this, member.Collider), OnEnterFunctor);
            }
        }

        protected virtual void SignalTriggerExit(CompoundTriggerMember member, Collider other)
        {
            if (!this.isActiveAndEnabled) return;
            if (this.AnyRelatedColliderOverlaps(other)) return;

            if (_active.Remove(other))
            {
                _messageSettings.Send(this.gameObject, (this, other), OnExitFunctor);
                _otherColliderMessageSettings.Send(other.gameObject, (this, member.Collider), OnExitFunctor);
            }
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
                _otherColliderMessageSettings.Send(other.gameObject, (this, member.Collider), OnStayFunctor);
            }
        }

        private void ResyncStateAndSignalMessageIfNecessary()
        {
            _active.Clear();
            var ed = _colliders.GetEnumerator();
            while (ed.MoveNext())
            {
                ed.Current.Value.CleanActive();
                if (ed.Current.Value.Active.Count > 0)
                {
                    foreach (var a in ed.Current.Value.Active)
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
        }

        #endregion

        #region ICompoundTrigger Interface

        public bool InMessagePath(GameObject go)
        {
            return _messageSettings.IsInMessagePath(this.gameObject, go);
        }

        #endregion

        #region Special Types

        protected class CompoundTriggerMember : MonoBehaviour
        {

            [System.NonSerialized]
            private protected CompoundTrigger _owner;
            [System.NonSerialized]
            private protected Collider _collider;
            [System.NonSerialized]
            private protected HashSet<Collider> _active = new HashSet<Collider>();

            internal CompoundTrigger Owner
            {
                get { return _owner; }
            }

            internal Collider Collider
            {
                get { return _collider; }
            }

            internal HashSet<Collider> Active
            {
                get { return _active; }
            }

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

    }

}
