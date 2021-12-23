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

        #endregion

        #region CONSTRUCTOR

        protected override void Awake()
        {
            base.Awake();

            _history = new FiniteDeque<int>(_maxHistory > MIN_HISTORICAL_COUNT ? _maxHistory : MIN_HISTORICAL_COUNT);
            _checkpoints = new FiniteDeque<int>(_maxCheckpoints > MIN_HISTORICAL_COUNT ? _maxCheckpoints : MIN_HISTORICAL_COUNT);
        }

        #endregion

        #region Properties

        public System.Collections.Generic.IEnumerable<int> History => _history;

        public System.Collections.Generic.IEnumerable<int> Checkpoints => _checkpoints;

        [ShowNonSerializedProperty("Last Active State")]
        public int? LastActiveState => _history.Count > 1 ? _history.PeekPop() : (int?)null;

        [ShowNonSerializedProperty("Last Checkpoint")]
        public int? LastCheckpoint => _checkpoints.Count > 0 ? _checkpoints.PeekPop() : (int?)null;

        #endregion

        #region Methods

        public void GoToPreviouslyActiveState()
        {
            if (_history.Count > 1)
            {
                base.GoToState(_history.Pop()); //call base to avoid signaling a new history entry in the override below
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
            base.GoToState(index); //call base to avoid signaling a new history entry in the override below
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
            if(_checkpoints.Count > 0)
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

        public override void GoToState(int index)
        {
            if (this.CurrentStateIndex != null) _history.Push(this.CurrentStateIndex.Value);
            base.GoToState(index);
        }

        #endregion

    }
}
