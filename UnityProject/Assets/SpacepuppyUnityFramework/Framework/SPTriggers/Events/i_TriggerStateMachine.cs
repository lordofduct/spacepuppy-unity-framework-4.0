
using UnityEngine;

using com.spacepuppy.Utils;

namespace com.spacepuppy.Events
{

    [Infobox("Triggering this forwards the trigger down to the current state using that state's configuration.")]
    public sealed class i_TriggerStateMachine : Triggerable
    {

        public enum WrapMode
        {
            Clamp = 0,
            Loop = 1,
        }

        #region Fields

        [SerializeField]
        private SPEvent _states;

        [SerializeField]
        private int _initialState;

        [Space(10)]
        [SerializeField]
        private SPEvent _onStateChanged;

        [System.NonSerialized]
        private int _currentState;

        #endregion

        #region CONSTRUCTOR

        protected override void Start()
        {
            base.Start();

            this.GoToState(_initialState);
        }

        #endregion

        #region Properties

        public int CurrentStateIndex
        {
            get { return _currentState; }
        }

        public EventTriggerTarget CurrentState
        {
            get
            {
                if (_currentState < 0 || _currentState >= _states.Targets.Count) return null;
                return _states.Targets[_currentState];
            }
        }

        public SPEvent States => _states;

        public SPEvent OnStateChanged => _onStateChanged;

        #endregion

        #region Methods

        public void GoToState(int index)
        {
            bool signal = (_currentState != index);

            _currentState = index;
            for(int i = 0; i < _states.TargetCount; i++)
            {
                var go = GameObjectUtil.GetGameObjectFromSource(_states.Targets[i].Target, true);
                if (go != null) go.SetActive(i == _currentState);
            }

            if(signal && _onStateChanged.HasReceivers)
            {
                _onStateChanged.ActivateTrigger(this, null);
            }
        }

        public void GoToNextState(WrapMode mode = WrapMode.Loop)
        {
            switch(mode)
            {
                case WrapMode.Loop:
                    this.GoToState(MathUtil.Wrap(_currentState + 1, _states.TargetCount));
                    break;
                case WrapMode.Clamp:
                default:
                    this.GoToState(Mathf.Clamp(_currentState + 1, 0, _states.TargetCount - 1));
                    break;
            }
        }

        public void GoToPreviousState(WrapMode mode = WrapMode.Loop)
        {
            switch (mode)
            {
                case WrapMode.Loop:
                    this.GoToState(MathUtil.Wrap(_currentState - 1, _states.TargetCount));
                    break;
                case WrapMode.Clamp:
                default:
                    this.GoToState(Mathf.Clamp(_currentState - 1, 0, _states.TargetCount - 1));
                    break;
            }
        }

        #endregion

        #region ITriggerable Interface

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            _states.ActivateTriggerAt(_currentState, this, null);
            return true;
        }

        #endregion

    }

}
