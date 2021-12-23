#pragma warning disable 0649 // variable declared but not used.
using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Collections;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Events
{

    public class t_StateMachine : SPComponent, IRadicalEnumerable<t_StateMachine.State>, IObservableTrigger
    {

        #region Fields

        [SerializeField]
        [ReorderableArray(DrawElementAtBottom = true, ChildPropertyToDrawAsElementLabel = "_name")]
        private State[] _states = ArrayUtil.Empty<State>();

        [SerializeField]
        private int _initialState;

        [SerializeField]
        [Tooltip("When starting should the initial states 'OnEnterState' event be fired?")]
        private bool _notifyFirstStateOnStart;

        [Space(10)]
        [SerializeField]
        private SPEvent _onStateChanged = new SPEvent("OnStateChanged");

        [System.NonSerialized]
        private State _currentState;

        #endregion

        #region CONSTRUCTOR

        protected override void Start()
        {
            base.Start();

            _currentState = (_initialState >= 0 && _initialState < _states.Length) ? _states[_initialState] : null;
            if (_notifyFirstStateOnStart && _currentState != null)
            {
                _currentState.OnEnterState.ActivateTrigger(this, null);
                this.OnStateChanged();
            }
        }

        #endregion

        #region Properties

        public int NumOfStates
        {
            get { return _states.Length; }
        }

        public State Current
        {
            get { return _currentState; }
        }

        public string CurrentStateName
        {
            get { return _currentState != null ? _currentState.Name : null; }
        }

        public int CurrentStateIndex
        {
            get { return System.Array.IndexOf(_states, _currentState); }
        }

        public State this[int index]
        {
            get
            {
                if (index < 0 || index >= _states.Length) throw new System.IndexOutOfRangeException();
                return _states[index];
            }
        }

        #endregion

        #region Methods

        public int IndexOf(State state)
        {
            return System.Array.IndexOf(_states, state);
        }

        public bool Contains(State state)
        {
            return System.Array.IndexOf(_states, state) >= 0;
        }

        public State ChangeState(int index)
        {
            if (index < 0 || index >= _states.Length) throw new System.IndexOutOfRangeException();

            var s = _states[index];
            if (_currentState == s) return s;

            var lastState = _currentState;
            _currentState = s;
            if (lastState != null) lastState.OnExitState.ActivateTrigger(this, null);
            if (_currentState != null) _currentState.OnEnterState.ActivateTrigger(this, null);

            this.OnStateChanged();
            return _currentState;
        }

        #endregion

        #region StateMachine Interface
        
        protected virtual void OnStateChanged()
        {
            if (_onStateChanged.HasReceivers) _onStateChanged.ActivateTrigger(this, null);
        }

        public bool Contains(string state)
        {
            foreach (var s in _states)
            {
                if (string.Equals(s.Name, state)) return true;
            }

            return false;
        }

        public string ChangeState(string state)
        {
            if (_currentState != null && _currentState.Name == state) return state;

            foreach (var s in _states)
            {
                if (string.Equals(s.Name, state))
                {
                    var lastState = _currentState;
                    _currentState = s;
                    if (lastState != null) lastState.OnExitState.ActivateTrigger(this, null);
                    _currentState.OnEnterState.ActivateTrigger(this, null);

                    this.OnStateChanged();

                    return state;
                }
            }

            return null;
        }
        
        #endregion

        #region IEnumerable Interface

        public IEnumerator<State> GetEnumerator()
        {
            return (_states as IList<State>).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public int Enumerate(ICollection<State> coll)
        {
            for(int i = 0; i < _states.Length; i++)
            {
                coll.Add(_states[i]);
            }
            return _states.Length;
        }

        public int Enumerate(System.Action<State> callback)
        {
            if (callback == null) return 0;

            int cnt = 0;
            for(int i = 0; i < _states.Length; i++)
            {
                callback(_states[i]);
                cnt++;
            }
            return cnt;
        }

        #endregion

        #region IObserverableTrigger Interface

        BaseSPEvent[] IObservableTrigger.GetEvents()
        {
            return new BaseSPEvent[] { _onStateChanged };
        }

        #endregion

        #region Special Types

        [System.Serializable]
        public class State
        {
            [SerializeField]
            private string _name;
            [SerializeField]
            private SPEvent _onEnterState = new SPEvent("OnEnterState");
            [SerializeField]
            private SPEvent _onExitState = new SPEvent("OnExitState");

            public string Name
            {
                get { return _name; }
            }

            public SPEvent OnEnterState
            {
                get { return _onEnterState; }
            }

            public SPEvent OnExitState
            {
                get { return _onExitState; }
            }

        }

        #endregion

    }

}
