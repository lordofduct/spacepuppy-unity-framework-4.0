using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using com.spacepuppy.Utils;
using com.spacepuppy.Collections;

namespace com.spacepuppy
{

    [Infobox("Sets 'Targets' to the same active/enabled state as the 'Observed Targets' during any 'Observed Targets' enabled or disable events.\r\n\r\nNote - this doesn't start syncing until the first time it has been enabled, the Sync method can be called to premptively sync it.")]
    public sealed class MirrorActiveStateOfTargets : SPComponent
    {

        public enum Modes
        {
            AllOn = 0,
            AnyOn = 1,
            AnyOff = 2,
            AllOff = 3,
        }

        #region Fields

        [SerializeField]
        private Modes _mode;

        [SerializeField, DefaultFromSelf, ReorderableArray]
        private GameObject[] _targets;

        [SerializeField, ReorderableArray]
        private GameObject[] _observedTargets;

        [System.NonSerialized]
        private HashSet<Hook> _activeObservedTargets = new();

        #endregion

        #region CONSTRUCTOR

        protected override void Awake()
        {
            base.Awake();
            this.SyncHooks();
            this.Sync_Imp();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            this.PurgeHooks();
        }

        #endregion

        #region Properties

        public Modes Mode
        {
            get => _mode;
            set
            {
                _mode = value;
#if UNITY_EDITOR
                if (Application.isPlaying)
#endif
                {
                    this.Sync();
                }
            }
        }

        public GameObject[] Targets
        {
            get => _observedTargets;
            set
            {
                _observedTargets = value;
#if UNITY_EDITOR
                if (Application.isPlaying)
#endif
                {
                    this.SyncHooks();
                    this.Sync_Imp();
                }
            }
        }

        #endregion

        #region Methods

        void SyncHooks()
        {
            using (var hash = TempCollection.GetSet<GameObject>())
            {
                if (_activeObservedTargets.Count > 0)
                {
                    using (var lst = TempCollection.GetList<Hook>(_activeObservedTargets))
                    {
                        foreach (var h in lst)
                        {
                            if (!h)
                            {
                                _activeObservedTargets.Remove(h); //already dead
                            }
                            if (!_observedTargets.Contains(h.gameObject))
                            {
                                h.target = null;
                                _activeObservedTargets.Remove(h);
                                Destroy(h);
                            }
                            else
                            {
                                hash.Add(h.gameObject);
                            }
                        }
                    }
                }

                foreach (var t in _observedTargets)
                {
                    if (!t || hash.Contains(t)) continue;

                    hash.Add(t);
                    var h = t.AddComponent<Hook>();
                    h.target = this;
                    _activeObservedTargets.Add(h);
                }
            }
        }

        void PurgeHooks()
        {
            if (_activeObservedTargets.Count == 0) return;

            using (var lst = TempCollection.GetList<Hook>(_activeObservedTargets))
            {
                _activeObservedTargets.Clear();
                foreach (var h in lst)
                {
                    h.target = null;
                    Destroy(h);
                }
            }
        }

        public void Sync()
        {
            if (_observedTargets == null || _observedTargets.Length == 0)
            {
                if (_activeObservedTargets.Count > 0)
                {
                    this.PurgeHooks();
                }
                return;
            }
            else if (_activeObservedTargets.Count != _observedTargets.Length)
            {
                this.SyncHooks();
            }
            this.Sync_Imp();
        }
        void Sync_Imp()
        {
            bool state = this.GetState();
            foreach (var t in _targets)
            {
                t.TrySetActive(state);
            }
        }
        bool GetState()
        {
            switch (_mode)
            {
                case Modes.AllOn:
                    return !_observedTargets.Any(o => !o.IsAliveAndActive());
                case Modes.AnyOn:
                    return _observedTargets.Any(o => o.IsAliveAndActive());
                case Modes.AnyOff:
                    return _observedTargets.Any(o => !o.IsAliveAndActive());
                case Modes.AllOff:
                    return !_observedTargets.Any(o => o.IsAliveAndActive());
                default:
                    return false;
            }
        }

        #endregion

        #region Special Types

        class Hook : MonoBehaviour
        {
            public MirrorActiveStateOfTargets target;

            private void OnEnable()
            {
                if (target) target.Sync();
            }
            private void OnDisable()
            {
                if (target) target.Sync();
            }
            private void OnDestroy()
            {
                if (target && target._activeObservedTargets.Remove(this))
                {
                    target.Sync_Imp();
                }
            }
        }

        #endregion

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                this.SyncHooks();
                this.Sync_Imp();
            }
        }
#endif

    }

}
