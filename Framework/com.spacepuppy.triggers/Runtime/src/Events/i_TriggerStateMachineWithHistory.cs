using UnityEngine;

using com.spacepuppy.Collections;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Events
{

    public class i_TriggerStateMachineWithHistory : i_TriggerStateMachine
    {

        public const int MIN_HISTORICAL_COUNT = 4;

        #region Fields

        [SerializeField]
        [Min(MIN_HISTORICAL_COUNT)]
        private int _maxHistory = 16;
        [SerializeField]
        [Min(MIN_HISTORICAL_COUNT)]
        private int _maxCheckpoints = 16;

        [System.NonSerialized]
        private FiniteDeque<int> _history;
        [System.NonSerialized]
        private FiniteDeque<int> _checkpoints;

        [System.NonSerialized]
        private bool _goingToHistoryState;

        #endregion

        #region CONSTRUCTOR

        protected override void Awake()
        {
            base.Awake();

            _history = new FiniteDeque<int>(_maxHistory > MIN_HISTORICAL_COUNT ? _maxHistory : MIN_HISTORICAL_COUNT)
            {
                EnumeratesAsStack = true,
            };
            _checkpoints = new FiniteDeque<int>(_maxCheckpoints > MIN_HISTORICAL_COUNT ? _maxCheckpoints : MIN_HISTORICAL_COUNT)
            {
                EnumeratesAsStack = true,
            };
            this.StateChanged += this.I_TriggerStateMachineWithHistory_StateChanged;
        }

        #endregion

        #region Properties

        public IIndexedEnumerable<int> History => _history;

        public IIndexedEnumerable<int> Checkpoints => _checkpoints;

        [ShowNonSerializedProperty("Last Active State")]
        public int? LastActiveState => _history.Count > 0 ? _history.PeekPop() : (int?)null;

        [ShowNonSerializedProperty("Last Checkpoint")]
        public int? LastCheckpoint => _checkpoints.Count > 0 ? _checkpoints.PeekPop() : (int?)null;

        #endregion

        #region Methods

        public void GoToPreviouslyActiveState()
        {
            if (_history.Count > 0)
            {
                try
                {
                    _goingToHistoryState = true; //avoid signaling a new history entry in the override below
                    this.GoToState(_history.Pop());
                }
                finally
                {
                    _goingToHistoryState = false;
                }
            }
        }

        public void JumpToPreviouslyActiveState(int count)
        {
            if (count == 0 || _history.Count == 0) return;

            int index = this.CurrentStateIndex ?? 0;
            while (_history.Count > 0 && count > 0)
            {
                index = _history.Pop();
                count--;
            }

            try
            {
                _goingToHistoryState = true; //avoid signaling a new history entry in the override below
                this.GoToState(index);
            }
            finally
            {
                _goingToHistoryState = false;
            }
        }

        public void RegisterCheckpoint(int index)
        {
            _checkpoints.Push(index);
        }

        public void RegisterCheckpointById(string id)
        {
            _checkpoints.Push(this.States.FindIndex(id));
        }

        public void RegisterCurrentAsCheckpoint()
        {
            if (this.CurrentStateIndex != null) _checkpoints.Push(this.CurrentStateIndex.Value);
        }

        public void GoToPreviousCheckpoint()
        {
            if (_checkpoints.Count > 0)
            {
                this.GoToState(_checkpoints.Pop());
            }
        }

        public void JumpToPreviousCheckpoint(int count)
        {
            if (count == 0 || _checkpoints.Count == 0) return;

            int index = this.CurrentStateIndex ?? 0;
            while (_checkpoints.Count > 0 && count > 0)
            {
                index = _checkpoints.Pop();
                count--;
            }
            this.GoToState(index);
        }

        private void I_TriggerStateMachineWithHistory_StateChanged(object sender, System.EventArgs e)
        {
            if (!_goingToHistoryState && this.States.LastStateIndex != null)
            {
                _history.Push(this.States.LastStateIndex.Value);
            }
        }

        #endregion

    }
}
