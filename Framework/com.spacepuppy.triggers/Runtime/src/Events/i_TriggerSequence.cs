#pragma warning disable 0649 // variable declared but not used.

using UnityEngine;

using com.spacepuppy.Utils;

namespace com.spacepuppy.Events
{

    public sealed class i_TriggerSequence : AutoTriggerable, IObservableTrigger
    {

        #region Fields

        [SerializeField()]
        private MathUtil.WrapMode _wrapMode;

        [SerializeField()]
        private SPEvent _trigger = new SPEvent("Trigger");

        [SerializeField()]
        private bool _passAlongTriggerArg;

        [SerializeField]
        [MinRange(0)]
        private int _currentIndex = 0;

        #endregion

        #region CONSTRUCTOR

        #endregion

        #region Properties

        public MathUtil.WrapMode Wrap
        {
            get { return _wrapMode; }
            set { _wrapMode = value; }
        }

        public SPEvent TriggerSequence
        {
            get
            {
                return _trigger;
            }
        }

        public bool PassAlongTriggerArg
        {
            get { return _passAlongTriggerArg; }
            set { _passAlongTriggerArg = value; }
        }

        public int CurrentIndex
        {
            get { return _currentIndex; }
            set { _currentIndex = value; }
        }

        public int CurrentIndexNormalized
        {
            get => MathUtil.WrapIndex(_wrapMode, _currentIndex, _trigger.TargetCount);
        }

        #endregion

        #region Methods

        public void Reset()
        {
            _currentIndex = 0;
        }

        #endregion

        #region ITriggerableMechanism Interface

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            int i = this.CurrentIndexNormalized;
            _currentIndex++;
            _trigger.ActivateTriggerAt(i, this, _passAlongTriggerArg ? arg : null);
            return true;
        }

        #endregion

        #region IObservableTrigger Interface

        BaseSPEvent[] IObservableTrigger.GetEvents()
        {
            return new BaseSPEvent[] { _trigger };
        }

        #endregion

    }

}
