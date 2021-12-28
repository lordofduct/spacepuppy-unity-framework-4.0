#pragma warning disable 0649 // variable declared but not used.

using UnityEngine;
using System.Linq;

using com.spacepuppy.Utils;

namespace com.spacepuppy.Events
{

    public class i_TriggerOnEval : AutoTriggerable, IObservableTrigger
    {

        #region Fields

        [SerializeField]
        private VariantReference _input = new VariantReference();

        [SerializeField()]
        [ReorderableArray]
        private ConditionBlock[] _conditions;
        [SerializeField]
        private SPEvent _elseCondition = new SPEvent("Else Condition");

        [SerializeField()]
        private bool _passAlongTriggerArg;

        [SerializeField()]
        private SPTimePeriod _delay = 0f;

        #endregion

        #region Properties

        public bool PassAlongTriggerArg
        {
            get { return _passAlongTriggerArg; }
            set { _passAlongTriggerArg = value; }
        }

        public SPTimePeriod Delay
        {
            get { return _delay; }
            set { _delay = value; }
        }

        #endregion

        #region ITriggerableMechanism Interface

        public override bool CanTrigger => base.CanTrigger && _conditions != null && _conditions.Length > 0;

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            double value = _input.DoubleValue;

            if (!this._passAlongTriggerArg) arg = null;
            foreach (var c in _conditions)
            {
                if (c.Compare(value))
                {
                    if (_delay.Seconds > 0f)
                    {
                        this.InvokeGuaranteed(() =>
                        {
                            c.Trigger.ActivateTrigger(this, arg);
                        }, _delay.Seconds, _delay.TimeSupplier);
                    }
                    else
                    {
                        c.Trigger.ActivateTrigger(this, arg);
                    }
                    return true;
                }
            }

            //if we reached here, it's else
            if (_delay.Seconds > 0f)
            {
                this.InvokeGuaranteed(() =>
                {
                    _elseCondition.ActivateTrigger(this, arg);
                }, _delay.Seconds, _delay.TimeSupplier);
            }
            else
            {
                _elseCondition.ActivateTrigger(this, arg);
            }
            return true;
        }

        #endregion

        #region Special Types

        [System.Serializable()]
        public class ConditionBlock
        {

            [SerializeField()]
            private ComparisonOperator _operator;
            [SerializeField]
            private double _value;
            [SerializeField()]
            private SPEvent _trigger = new SPEvent();

            public SPEvent Trigger
            {
                get { return _trigger; }
            }

            public bool Compare(double input)
            {
                return CompareUtil.Compare(_operator, input, _value);
            }

        }

        #endregion

        #region IObservableTrigger Interface

        BaseSPEvent[] IObservableTrigger.GetEvents()
        {
            return (from c in _conditions select c.Trigger).Append(_elseCondition).ToArray();
        }

        #endregion

    }

}
