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
        private StateCollection _states = new StateCollection();

        [SerializeField]
        private SPEvent _onStateChanged = new SPEvent("OnStateChanged");

        [System.NonSerialized]
        private int? _currentState = null;

        #endregion

        #region CONSTRUCTOR

        protected override void Start()
        {
            base.Start();

            if (_currentState == null)
            {
                this.GoToState(_initialState);
            }
        }

        #endregion

        #region Properties


        [ShowNonSerializedProperty("Current State")]
        public int? CurrentStateIndex => _currentState;

        public StateInfo? CurrentState
        {
            get
            {
                if (_currentState == null || _currentState < 0 || _currentState >= _states.Count) return null;
                return _states[_currentState.Value];
            }
        }

        public StateCollection States => _states;

        public SPEvent OnStateChanged => _onStateChanged;

        #endregion

        #region Methods

        public virtual void GoToState(int index)
        {
            bool signal = (_currentState != index);

            _currentState = index;
            var currentGo = index >= 0 && index < _states.Count ? GameObjectUtil.GetGameObjectFromSource(_states[index].Target) : null;
            for (int i = 0; i < _states.Count; i++)
            {
                var go = GameObjectUtil.GetGameObjectFromSource(_states[i].Target, true);
                if (go) go.SetActive(i == _currentState || go == currentGo);
            }

            if (signal && _onStateChanged.HasReceivers)
            {
                _onStateChanged.ActivateTrigger(this, null);
            }
        }

        public void GoToStateById(string id)
        {
            this.GoToState(_states.FindIndex(id));
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

        #region ITriggerable Interface

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            if(_currentState != null && _currentState >= 0 && _currentState < _states.Count)
            {
                _states.ActivateTriggerAt(_currentState.Value, this, null);
            }
            return true;
        }

        #endregion

        #region IObservableTrigger Interface

        BaseSPEvent[] IObservableTrigger.GetEvents()
        {
            return new BaseSPEvent[] { _onStateChanged };
        }

        #endregion

        #region Special Types

        [System.Serializable]
        public class StateCollection : IList<StateInfo>
        {

            #region Fields

            [SerializeField]
            [UnityEngine.Serialization.FormerlySerializedAs("_targets")]
            private List<StateInfo> _states = new List<StateInfo>();

            private const int MIN_SIZE_TOSEARCH = 8;
            [System.NonSerialized]
            private Dictionary<string, int> _idToIndex;

            #endregion

            #region Properties

            public int Count => _states.Count;

            bool ICollection<StateInfo>.IsReadOnly => false;

            public StateInfo this[int index]
            {
                get => _states[index];
                set {
                    _states[index] = value;
                    _idToIndex = null;
                }
            }

            #endregion

            #region Methods

            public void ActivateTriggerAt(int index, object sender, object arg)
            {
                if (index < 0 || index >= _states.Count) return;

                EventTriggerEvaluator.Current.TriggerAllOnTarget(_states[index].Target, sender, arg);
            }

            public StateInfo? Find(string id)
            {
                if(_states.Count < MIN_SIZE_TOSEARCH)
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
                for(int i = _states.Count - 1; i >= 0; i--)
                {
                    dict[_states[i].Id] = i;
                }
                _idToIndex = dict;
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
                if( _states.Remove(item))
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

        }

        #endregion

    }

}
