using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Utils;
using System.ComponentModel;

namespace com.spacepuppy.Events
{

    [Infobox("Triggering this forwards the trigger down to the current state using that state's configuration.")]
    public class i_TriggerStateMachine : Triggerable, IObservableTrigger
    {

        public enum WrapMode
        {
            Clamp = 0,
            Loop = 1,
        }

        #region Fields

        [SerializeField]
        [Min(-1)]
        private int _initialState;

        [SerializeField]
        private bool _returnToInitialStateOnEnable;

        [SerializeField]
        private StateCollection _states = new StateCollection();

        [SerializeField]
        private SPEvent _onExitState = new SPEvent("OnExitState");

        [SerializeField]
        [UnityEngine.Serialization.FormerlySerializedAs("_onStateChanged")]
        private SPEvent _onEnterState = new SPEvent("OnEnterState");

        #endregion

        #region CONSTRUCTOR

        protected override void Awake()
        {
            _states.ExitingState += (s, e) => _onExitState.ActivateTrigger(this, null);
            _states.EnteringState += (s, e) => _onEnterState.ActivateTrigger(this, null);
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();

            if (_states.CurrentState == null || _returnToInitialStateOnEnable)
            {
                _states.GoToState(_initialState);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (_returnToInitialStateOnEnable && this.started)
            {
                _states.GoToState(_initialState);
            }
        }

        #endregion

        #region Properties


        [ShowNonSerializedProperty("Current State")]
        public int? CurrentStateIndex => _states.CurrentStateIndex;

        public StateInfo? CurrentState => _states.CurrentState;

        public StateCollection States => _states;

        public SPEvent OnExitState => _onExitState;

        public SPEvent OnEnterState => _onEnterState;

        #endregion

        #region Methods

        public virtual void GoToState(int index)
        {
            _states.GoToState(index);
        }

        public void GoToStateById(string id)
        {
            _states.GoToStateById(id);
        }

        public void GoToNextState(WrapMode mode = WrapMode.Loop)
        {
            _states.GoToNextState(mode);
        }

        public void GoToPreviousState(WrapMode mode = WrapMode.Loop)
        {
            _states.GoToPreviousState(mode);
        }

        #endregion

        #region ITriggerable Interface

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            var stateindex = _states.CurrentStateIndex;
            if (stateindex != null && stateindex >= 0 && stateindex < _states.Count)
            {
                _states.ActivateTriggerAt(stateindex.Value, this, null);
            }
            return true;
        }

        #endregion

        #region IObservableTrigger Interface

        BaseSPEvent[] IObservableTrigger.GetEvents()
        {
            return new BaseSPEvent[] { _onExitState, _onEnterState };
        }

        #endregion

        #region Special Types

        [System.Serializable]
        public class StateCollection : IList<StateInfo>
        {

            public event System.EventHandler ExitingState;
            public event System.EventHandler EnteringState;

            #region Fields

            [SerializeField]
            [UnityEngine.Serialization.FormerlySerializedAs("_targets")]
            private List<StateInfo> _states = new List<StateInfo>();

            private const int MIN_SIZE_TOSEARCH = 8;
            [System.NonSerialized]
            private Dictionary<string, int> _idToIndex;

            [System.NonSerialized]
            private int? _currentState = null;

            #endregion

            #region Properties

            public int Count => _states.Count;

            bool ICollection<StateInfo>.IsReadOnly => false;

            public StateInfo this[int index]
            {
                get => _states[index];
                set
                {
                    _states[index] = value;
                    _idToIndex = null;
                }
            }

            public int? CurrentStateIndex => _currentState;

            public StateInfo? CurrentState
            {
                get
                {
                    if (_currentState == null || _currentState < 0 || _currentState >= _states.Count) return null;
                    return _states[_currentState.Value];
                }
            }

            #endregion

            #region Methods

            public void ActivateTriggerAt(int index, object sender, object arg)
            {
                if (index < 0 || index >= _states.Count) return;

                EventTriggerEvaluator.Current.TriggerAllOnTarget(_states[index].Target, arg, sender, arg);
            }

            public StateInfo? Find(string id)
            {
                if (id == null) return null;

                if (_states.Count < MIN_SIZE_TOSEARCH)
                {
                    for (int i = 0; i < _states.Count; i++)
                    {
                        if (string.Equals(_states[i].Id, id)) return _states[i];
                    }
                }
                else
                {
                    if (_idToIndex == null) this.SanitizeLookup();

                    int index;
                    if (!_idToIndex.TryGetValue(id, out index)) return null;

                    return _states[index];
                }
                return null;
            }

            public int FindIndex(string id)
            {
                if (id == null) return -1;

                if (_states.Count < MIN_SIZE_TOSEARCH)
                {
                    for (int i = 0; i < _states.Count; i++)
                    {
                        if (string.Equals(_states[i].Id, id)) return i;
                    }
                }
                else
                {
                    if (_idToIndex == null) this.SanitizeLookup();

                    int index;
                    if (!_idToIndex.TryGetValue(id, out index)) index = -1;
                    return index;
                }
                return -1;
            }

            private void SanitizeLookup()
            {
                var dict = new Dictionary<string, int>();
                for (int i = _states.Count - 1; i >= 0; i--)
                {
                    dict[_states[i].Id] = i;
                }
                _idToIndex = dict;
            }

            public virtual void GoToState(int index)
            {
                bool signal = (_currentState != index);

                if (signal && _currentState >= 0)
                {
                    this.ExitingState?.Invoke(this, System.EventArgs.Empty);
                }

                //first disable, then enable, this way you can use the OnDisable and OnEnable of the states to perform actions predictably
                _currentState = index;
                var currentGo = index >= 0 && index < _states.Count ? GameObjectUtil.GetGameObjectFromSource(_states[index].Target, true) : null;
                for (int i = 0; i < _states.Count; i++)
                {
                    var go = GameObjectUtil.GetGameObjectFromSource(_states[i].Target, true);
                    if (go && i != _currentState && go != currentGo) go.SetActive(false);
                }
                if (currentGo) currentGo.SetActive(true);

                if (signal)
                {
                    this.EnteringState?.Invoke(this, System.EventArgs.Empty);
                }
            }

            public void GoToStateById(string id)
            {
                this.GoToState(this.FindIndex(id));
            }

            public void GoToNextState(WrapMode mode = WrapMode.Loop)
            {
                switch (mode)
                {
                    case WrapMode.Loop:
                        this.GoToState(MathUtil.Wrap((_currentState ?? -1) + 1, _states.Count));
                        break;
                    case WrapMode.Clamp:
                    default:
                        this.GoToState(Mathf.Clamp((_currentState ?? -1) + 1, 0, _states.Count - 1));
                        break;
                }
            }

            public void GoToPreviousState(WrapMode mode = WrapMode.Loop)
            {
                switch (mode)
                {
                    case WrapMode.Loop:
                        this.GoToState(MathUtil.Wrap((_currentState ?? 1) - 1, _states.Count));
                        break;
                    case WrapMode.Clamp:
                    default:
                        this.GoToState(Mathf.Clamp((_currentState ?? _states.Count) - 1, 0, _states.Count - 1));
                        break;
                }
            }

            #endregion

            #region List Interface

            public void Add(StateInfo state)
            {
                _states.Add(state);
                _idToIndex = null;
            }

            public int IndexOf(StateInfo item)
            {
                return _states.IndexOf(item);
            }

            public void Insert(int index, StateInfo item)
            {
                _states.Insert(index, item);
                _idToIndex = null;
            }

            public void RemoveAt(int index)
            {
                _states.RemoveAt(index);
                _idToIndex = null;
            }

            public void Clear()
            {
                _states.Clear();
                _idToIndex = null;
            }

            public bool Contains(StateInfo item)
            {
                return _states.Contains(item);
            }

            public void CopyTo(StateInfo[] array, int arrayIndex)
            {
                _states.CopyTo(array, arrayIndex);
            }

            public bool Remove(StateInfo item)
            {
                if (_states.Remove(item))
                {
                    _idToIndex = null;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public List<StateInfo>.Enumerator GetEnumerator()
            {
                return _states.GetEnumerator();
            }

            IEnumerator<StateInfo> IEnumerable<StateInfo>.GetEnumerator()
            {
                return _states.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return _states.GetEnumerator();
            }

            #endregion

        }

        [System.Serializable]
        public struct StateInfo
        {

            #region Fields

            [SerializeField]
            private string _id;

            [SerializeField()]
            [UnityEngine.Serialization.FormerlySerializedAs("_triggerable")]
            private UnityEngine.Object _target;

            #endregion

            #region Properties

            public string Id
            {
                get => _id;
                set => _id = value;
            }

            public UnityEngine.Object Target
            {
                get => _target;
                set => _target = value;
            }

            #endregion

            #region Methods

            public override string ToString()
            {
                return _id;
            }

            #endregion

        }

        #endregion

    }

}
