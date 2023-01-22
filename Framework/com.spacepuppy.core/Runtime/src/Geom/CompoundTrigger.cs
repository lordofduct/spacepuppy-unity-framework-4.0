using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy.Collections;
using com.spacepuppy.Utils;

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

    public interface ICompoundTriggerExitHandler
    {
        void OnCompoundTriggerExit(ICompoundTrigger trigger, Collider other);
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
    public class CompoundTrigger : SPComponent, IMStartOrEnableReceiver, ICompoundTrigger
    {

        #region Fields

        private Dictionary<Collider, CompoundTriggerMember> _colliders = new Dictionary<Collider, CompoundTriggerMember>();
        protected readonly HashSet<Collider> _active = new HashSet<Collider>();

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

        #endregion

        #region CONSTRUCTOR

        void IMStartOrEnableReceiver.OnStartOrEnable() => this.OnStartOrEnable();

        protected virtual void OnStartOrEnable()
        {
            this.SyncTriggers();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            this.PurgeActiveState();
        }

        #endregion

        #region Properties

        public Messaging.MessageSendCommand MessageSettings
        {
            get => _messageSettings;
            set => _messageSettings = value;
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
                            if (!ObjUtil.IsObjectAlive(ed.Current.Key) || ed.Current.Value == null || !lst.Contains(ed.Current.Key))
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
                            var m = e.Current.AddComponent<CompoundTriggerMember>();
                            m.Init(this, e.Current);
                            _colliders.Add(e.Current, m);
                        }
                    }
                }
            }
        }

        public bool ContainsActive(Collider c) => c != null && _active.Contains(c);

        public Collider[] GetActiveColliders()
        {
            return _active.Count > 0 ? _active.ToArray() : ArrayUtil.Empty<Collider>();
        }

        public int GetActiveColliders(ICollection<Collider> output)
        {
            int cnt = _active.Count;
            if (cnt == 0) return 0;

            var e = _active.GetEnumerator();
            while (e.MoveNext())
            {
                output.Add(e.Current);
            }
            return cnt;
        }

        /// <summary>
        /// Dumps state of what member colliders are intersected currently. 
        /// Caution using this, any member collider currently intersected will not refire OnTriggerEnter. 
        /// This is usually reserved for OnDisable to revert state.
        /// </summary>
        public void PurgeActiveState()
        {
            _active.Clear();
            var e = _colliders.GetEnumerator();
            while (e.MoveNext())
            {
                e.Current.Value.Active.Clear();
            }
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
            if (_active.Add(other))
            {
                _messageSettings.Send(this.gameObject, (this, other), OnEnterFunctor);
            }
        }

        protected virtual void SignalTriggerExit(CompoundTriggerMember member, Collider other)
        {
            if (this.AnyRelatedColliderOverlaps(other)) return;

            if (_active.Remove(other))
            {
                _messageSettings.Send(this.gameObject, (this, other), OnExitFunctor);
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
            private CompoundTrigger _owner;
            [System.NonSerialized]
            private Collider _collider;
            [System.NonSerialized]
            private HashSet<Collider> _active;

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
                _active = new HashSet<Collider>();
            }

            private void OnTriggerEnter(Collider other)
            {
                if (_active.Add(other))
                {
                    if (_owner != null) _owner.SignalTriggerEnter(this, other);
                }
            }

            private void OnTriggerExit(Collider other)
            {
                if (_active.Remove(other))
                {
                    if (_owner != null) _owner.SignalTriggerExit(this, other);
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

        #endregion

    }

}
