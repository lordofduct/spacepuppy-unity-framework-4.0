using UnityEngine;
using UnityEngine.Animations;
using System.Collections.Generic;

using com.spacepuppy.Collections;
using com.spacepuppy.Events;
using com.spacepuppy.Utils;
using System.Linq;

namespace com.spacepuppy.Mecanim
{

    [Infobox("State Id must match the state name exactly including case.\r\n\r\nYou can target specific layers and sub states by formatting the name as:\r\nLayerName.StateName\r\nLayerName.SubStateMachineName.StateName\r\nOr even further nesting.\r\n\r\nNote - if any changes are made to StateId at runtime; SyncSubStateBridges must be called to take effect.")]
    public class AnimatorSubStateBridge : SPComponent
    {

        #region Fields

        [SerializeField]
        private string _stateId;

        [SerializeField]
        [Tooltip("Enter/Exit events with trigger even if this is disabled.")]
        private bool _triggerEventsWhenDisabled;

        [SerializeField]
        [SPEvent.Config("animator (Animator)")]
        private SPEvent _onStateEnter = new SPEvent("OnStateEnter");

        [SerializeField]
        [SPEvent.Config("animator (Animator)")]
        private SPEvent _onStateExit = new SPEvent("OnStateExit");

        [System.NonSerialized]
        private int _hash;
        [System.NonSerialized]
        private bool _complexId;

        [System.NonSerialized]
        private Messaging.MessageToken<ISubStateBridgeMessageHandler> _messageToken;
        [System.NonSerialized]
        private Animator _animator;
        [System.NonSerialized]
        private AnimatorStateInfo _stateInfo;
        [System.NonSerialized]
        private int _layerIndex;
        [System.NonSerialized]
        private System.Action<ISubStateBridgeMessageHandler> _onStateEnterCallback;
        [System.NonSerialized]
        private System.Action<ISubStateBridgeMessageHandler> _onStateExitCallback;

        #endregion

        #region CONSTRUCTOR

        protected override void Awake()
        {
            base.Awake();

            this.Sync();
        }

        #endregion

        #region Properties

        public string StateId => _stateId;

        public bool TriggerEventsWhenDisabled => _triggerEventsWhenDisabled;

        public SPEvent OnStateEnter {  get { return _onStateEnter; } }

        public SPEvent OnStateExit { get { return _onStateExit; } }

        [ShowNonSerializedProperty("State Id Hash")]
        public int StateIdHash { get { return _hash; } }

        public bool StateIdIsPath { get { return _complexId; } }

        #endregion

        #region Methods

        internal void Sync()
        {
            _hash = Animator.StringToHash(_stateId ?? string.Empty);
            _complexId = _stateId?.Contains('.') ?? false;
            _messageToken = Messaging.CreateBroadcastToken<ISubStateBridgeMessageHandler>(this.gameObject);
        }

        protected internal virtual void SignalStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (!_triggerEventsWhenDisabled && !this.isActiveAndEnabled) return;

            _animator = animator;
            _stateInfo = stateInfo;
            _layerIndex = layerIndex;

            if (_onStateEnter.HasReceivers) _onStateEnter.ActivateTrigger(this, _animator);

            if(_messageToken.Count > 0)
            {
                if (_onStateEnterCallback == null) _onStateEnterCallback = (o) => o.OnStateEnter(_animator, _stateInfo, _layerIndex);
                _messageToken.Invoke(_onStateEnterCallback);
            }
        }

        protected internal virtual void SignalStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (!_triggerEventsWhenDisabled && !this.isActiveAndEnabled) return;

            if (_onStateExit.HasReceivers) _onStateExit.ActivateTrigger(this, _animator);

            if (_messageToken.Count > 0)
            {
                if (_onStateExitCallback == null) _onStateExitCallback = (o) => o.OnStateExit(_animator, _stateInfo, _layerIndex);
                _messageToken.Invoke(_onStateExitCallback);
            }
        }

        #endregion

    }

    internal sealed class AnimatorSubStateBridgeContainer : SPComponent
    {

        #region Fields

        private AnimatorSubStateBridge[] _bridges;
        private Dictionary<int, int> _hashToStartIndex = new Dictionary<int, int>();
        private HashSet<AnimatorSubStateBridge> _activeStateBridges = new HashSet<AnimatorSubStateBridge>(ObjectInstanceIDEqualityComparer<AnimatorSubStateBridge>.Default);

        private bool _signaling;
        private bool _attemptedSync;

        #endregion

        #region Public Methods

        public IEnumerable<AnimatorSubStateBridge> GetSubStateBridges()
        {
            return _bridges;
        }

        public IEnumerable<AnimatorSubStateBridge> GetActiveSubStateBridges()
        {
            return _activeStateBridges;
        }

        #endregion

        #region Internal Methods

        internal void SyncSubStateBridges()
        {
            if (this == null) return;

            if (_signaling)
            {
                _attemptedSync = true;
                return;
            }

            _attemptedSync = false;
            _bridges = this.GetComponentsInChildren<AnimatorSubStateBridge>(true);
            foreach (var bridge in _bridges) bridge.Sync();
            System.Array.Sort(_bridges, AnimatorSubStateBridgeHashComparer.Default);

            _hashToStartIndex.Clear();
            if(_bridges.Length > 0)
            {
                int hash = _bridges[0].StateIdHash;
                _hashToStartIndex[hash] = 0;
                for(int i = 1; i < _bridges.Length; i++)
                {
                    if (_bridges[i].StateIdHash != hash)
                    {
                        hash = _bridges[i].StateIdHash;
                        _hashToStartIndex[hash] = i;
                    }
                }
            }

            using (var lst = TempCollection.GetList<AnimatorSubStateBridge>())
            {
                foreach (var t in _activeStateBridges)
                {
                    if (!_bridges.Contains(t))
                    {
                        lst.Add(t);
                    }
                }

                foreach (var t in lst)
                {
                    _activeStateBridges.Remove(t);
                }
            }

            this.enabled = _bridges.Length > 0;
        }


        internal void SignalEnterState(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (this == null) return;

            _signaling = true;
            try
            {
                int index;
                int hash;

                //signal shortNameHash matches
                hash = stateInfo.shortNameHash;
                if (_hashToStartIndex.TryGetValue(hash, out index))
                {
                    while (index >= 0 && index < _bridges.Length && _bridges[index].StateIdHash == hash)
                    {
                        if (!_bridges[index].StateIdIsPath && _activeStateBridges.Add(_bridges[index]))
                        {
                            _bridges[index].SignalStateEnter(animator, stateInfo, layerIndex);
                        }
                        index++;
                    }
                }

                //signal fullPathHash matches
                hash = stateInfo.fullPathHash;
                if (_hashToStartIndex.TryGetValue(hash, out index))
                {
                    while (index >= 0 && index < _bridges.Length && _bridges[index].StateIdHash == hash)
                    {
                        if (_bridges[index].StateIdIsPath && _activeStateBridges.Add(_bridges[index]))
                        {
                            _bridges[index].SignalStateEnter(animator, stateInfo, layerIndex);
                        }
                        index++;
                    }
                }
            }
            finally
            {
                _signaling = false;
                if(_attemptedSync)
                {
                    this.SyncSubStateBridges();
                }
            }
        }

        internal void SignalExitState(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (this == null) return;

            _signaling = true;
            try
            {
                int index;
                int hash;

                //signal shortNameHash matches
                hash = stateInfo.shortNameHash;
                if (_hashToStartIndex.TryGetValue(hash, out index))
                {
                    while (index >= 0 && index < _bridges.Length && _bridges[index].StateIdHash == hash)
                    {
                        if (_activeStateBridges.Contains(_bridges[index]) && !_bridges[index].StateIdIsPath)
                        {
                            try
                            {
                                _bridges[index].SignalStateExit(animator, stateInfo, layerIndex);
                            }
                            finally
                            {
                                _activeStateBridges.Remove(_bridges[index]);
                            }
                        }
                        index++;
                    }
                }

                //signal fullPathHash matches
                hash = stateInfo.fullPathHash;
                if (_hashToStartIndex.TryGetValue(hash, out index))
                {
                    while (index >= 0 && index < _bridges.Length && _bridges[index].StateIdHash == hash)
                    {
                        if (_activeStateBridges.Contains(_bridges[index]) && _bridges[index].StateIdIsPath)
                        {
                            try
                            {
                                _bridges[index].SignalStateExit(animator, stateInfo, layerIndex);
                            }
                            finally
                            {
                                _activeStateBridges.Remove(_bridges[index]);
                            }
                        }
                        index++;
                    }
                }
            }
            finally
            {
                _signaling = false;
                if (_attemptedSync)
                {
                    this.SyncSubStateBridges();
                }
            }
        }

        #endregion

        #region Special Types

        private class AnimatorSubStateBridgeHashComparer : IComparer<AnimatorSubStateBridge>
        {
            public static readonly AnimatorSubStateBridgeHashComparer Default = new AnimatorSubStateBridgeHashComparer();

            public int Compare(AnimatorSubStateBridge x, AnimatorSubStateBridge y)
            {
                return x.StateIdHash.CompareTo(y.StateIdHash);
            }
        }

        #endregion

    }

}
