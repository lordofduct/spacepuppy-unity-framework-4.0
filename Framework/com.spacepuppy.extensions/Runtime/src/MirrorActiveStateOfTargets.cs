using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using com.spacepuppy.Utils;
using com.spacepuppy.Collections;

namespace com.spacepuppy
{

    [Infobox("Mirrors enable/disable from 'Observed Targets' onto two lists:\n - TargetsMatch: set to SAME state\n - TargetsInvert: set to OPPOSITE state\n\nNote: Syncing begins once this component is enabled. Call Sync() to pre-sync.")]
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

        // Applies the computed state as-is
        [SerializeField, ReorderableArray]
        private GameObject[] _targets; // Matches state

        // Applies the inverse of the computed state
        [SerializeField, ReorderableArray]
        private GameObject[] _targetsInverted; // Inverts state

        [SerializeField, ReorderableArray]
        private GameObject[] _observedTargets;

        [System.NonSerialized]
        private HashSet<Hook> _activeObservedTargets = new();

        [System.NonSerialized]
        private bool _scheduled;

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
            get => _targets;
            set
            {
                _targets = value;
            }
        }

        public GameObject[] TargetsInverted
        {
            get => _targetsInverted;
            set
            {
                _targetsInverted = value;
            }
        }

        public GameObject[] ObservedTargets
        {
            get => _observedTargets;
            set
            {
                _observedTargets = value;
            }
        }

        #endregion

        #region Methods

        void SyncHooks()
        {
            try
            {
                _scheduled = true;
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
                                    _activeObservedTargets.Remove(h); // already dead
                                    continue;
                                }

                                if (_observedTargets == null || !_observedTargets.Contains(h.gameObject))
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

                    if (_observedTargets != null)
                    {
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
            }
            finally
            {
                _scheduled = false;
            }
        }

        void PurgeHooks()
        {
            if (_activeObservedTargets.Count == 0) return;

            try
            {
                _scheduled = true;
                using (var lst = TempCollection.GetList<Hook>(_activeObservedTargets))
                {
                    _activeObservedTargets.Clear();
                    foreach (var h in lst)
                    {
                        if (!h) continue;
                        h.target = null;
                        Destroy(h);
                    }
                }
            }
            finally
            {
                _scheduled = false;
            }
        }

        [InsertButton("Sync", RuntimeOnly = true)]
        public void Sync()
        {
            if (_observedTargets == null || _observedTargets.Length == 0)
            {
                if (_activeObservedTargets.Count > 0)
                {
                    this.PurgeHooks();
                }
            }
            else if (_activeObservedTargets.Count != _observedTargets.Length ||
                     !ArrayUtil.SimilarTo(_activeObservedTargets.Select(o => o.gameObject), _observedTargets))
            {
                this.SyncHooks();
            }

            this.Sync_Imp();
        }

        void Sync_Imp()
        {
            bool state = this.GetState();

            if (_targets != null)
            {
                foreach (var t in _targets)
                {
                    t.TrySetActive(state);
                }
            }

            if (_targetsInverted != null)
            {
                foreach (var t in _targetsInverted)
                {
                    t.TrySetActive(!state);
                }
            }
        }

        bool GetState()
        {
            if (_observedTargets == null || _observedTargets.Length == 0) return false;

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

        void ScheduleSync()
        {
            if (!_scheduled && this)
            {
                _scheduled = true;
                GameLoop.LateUpdateHandle.BeginInvoke(() =>
                {
                    _scheduled = false;
                    this.Sync();
                });
            }
        }

        #endregion

        #region Special Types

        class Hook : MonoBehaviour
        {
            public MirrorActiveStateOfTargets target;

            private void OnEnable()
            {
                if (target) target.ScheduleSync();
            }
            private void OnDisable()
            {
                if (target) target.ScheduleSync();
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
