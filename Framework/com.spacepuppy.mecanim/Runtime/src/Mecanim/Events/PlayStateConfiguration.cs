using UnityEngine;
using System.Collections.Generic;

namespace com.spacepuppy.Mecanim.Events
{

    [System.Serializable]
    public class PlayStateConfiguration
    {
        #region Fields

        [SerializeField]
        private string _stateName;
        [SerializeField]
        [Tooltip("Layer to target, -1 to select first state of any layer (or if only 1 layer).")]
        private int _layer = -1;

        [SerializeField]
        private float _crossFadeDur = float.NegativeInfinity;
        [SerializeField]
        private float _startOffset = float.NegativeInfinity;
        [SerializeField]
        [Tooltip("When setting cross-fade or start offset, should the time be done in fixed time, or normalized time. Fixed time refers to real seconds, where as normalized is relative to the duration of the animation where 0 = start, and 1 = end.")]
        private bool _useFixedTime;

        [SerializeField]
        [Tooltip("If left blank the state to play is considered the 'final state'.")]
        private string _finalState;
        [SerializeField]
        [Tooltip("Timeout period to wait before considering the 'final state' non-existent and exiting abruptly.")]
        private SPTimePeriod _finalStateTimeout = new SPTimePeriod(60f);

        #endregion

        #region CONSTRUCTOR

        public PlayStateConfiguration()
        {
            //parameterless constructor for serialization construction
        }

        public PlayStateConfiguration(string stateName, int layer = -1)
        {
            _stateName = stateName;
            _layer = layer;
        }

        #endregion

        #region Properties

        public string StateName { get { return _stateName; } set { _stateName = value; } }

        public int Layer { get { return _layer; } set { _layer = Mathf.Max(-1, value); } }

        public float CrossFadeDuration { get { return _crossFadeDur; } set { _crossFadeDur = value >= 0 ? value : float.NegativeInfinity; } }

        public float StartOffset { get { return _startOffset; } set { _startOffset = value >= 0 ? value : float.NegativeInfinity; } }

        public bool UseFixedTime { get { return _useFixedTime; } set { _useFixedTime = value; } }

        public string FinalState { get { return _finalState; } set { _finalState = value; } }

        public SPTimePeriod FinalStateTimeout { get { return _finalStateTimeout; } set { _finalStateTimeout = value; } }

        #endregion

        #region Methods

        public void Play(Animator animator, System.Action onexit = null)
        {
            if (_crossFadeDur > 0f)
            {
                if (_useFixedTime)
                {
                    animator.CrossFadeInFixedTime(_stateName, _crossFadeDur, _layer, _startOffset);
                }
                else
                {
                    animator.CrossFade(_stateName, _crossFadeDur, _layer, _startOffset);
                }
            }
            else
            {
                if (_useFixedTime)
                {
                    animator.PlayInFixedTime(_stateName, _layer, _startOffset);
                }
                else
                {
                    animator.Play(_stateName, _layer, _startOffset);
                }
            }
        }

        #endregion

    }

}
