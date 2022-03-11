using UnityEngine;
using System.Collections.Generic;

namespace com.spacepuppy.Events
{
    public class t_PhaseEvaluator : SPComponent
    {

        #region Fields

        [SerializeField]
        private float _currentValue;
        [SerializeField]
        private float _min = float.NegativeInfinity;
        [SerializeField]
        private float _max = float.PositiveInfinity;

        [SerializeField]
        private List<PhaseData> _phases = new List<PhaseData>();

        #endregion

        #region Properties

        public float CurrentValue
        {
            get => _currentValue;
            set
            {
                this.SetCurrentValue(value);
            }
        }

        public IList<PhaseData> Phases => _phases;

        #endregion

        #region Methods

        public void ResetCurrentValue(float value = 0f)
        {
            _currentValue = Mathf.Clamp(value, _min, _max);
        }

        public void SetCurrentValue(float value)
        {
            this.EvaluateTransitions(_currentValue, _currentValue = Mathf.Clamp(value, _min, _max));
        }

        protected virtual void EvaluateTransitions(float oldvalue, float newvalue)
        {
            foreach (var phase in _phases)
            {
                switch (phase.Direction)
                {
                    case Direction.Up:
                        if(phase.Value > oldvalue && phase.Value <= newvalue)
                        {
                            EventTriggerEvaluator.Current.TriggerAllOnTarget(phase.Target, null, this, null);
                        }
                        break;
                    case Direction.Down:
                        if(phase.Value < oldvalue && phase.Value >= newvalue)
                        {
                            EventTriggerEvaluator.Current.TriggerAllOnTarget(phase.Target, null, this, null);
                        }
                        break;
                    case Direction.Any:
                        if ((phase.Value > oldvalue && phase.Value <= newvalue) || (phase.Value < oldvalue && phase.Value >= newvalue))
                        {
                            EventTriggerEvaluator.Current.TriggerAllOnTarget(phase.Target, null, this, null);
                        }
                        break;
                }
            }
        }

        #endregion

        #region Special Types

        public enum Direction
        {
            Up = 0,
            Down = 1,
            Any = 2,
        }

        [System.Serializable]
        public struct PhaseData
        {
            public Direction Direction;
            public float Value;
            public UnityEngine.Object Target;
        }

#if UNITY_EDITOR
        public static class EditorHelper
        {

            public static void EvaluateTransitions(t_PhaseEvaluator target, float oldvalue, float newvalue)
            {
                if (target == null) return;
                target.EvaluateTransitions(oldvalue, newvalue);
            }

        }
#endif

        #endregion

    }
}
