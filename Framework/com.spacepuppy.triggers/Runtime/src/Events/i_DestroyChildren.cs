using UnityEngine;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Events
{

    public class i_DestroyChildren : AutoTriggerable
    {

        #region Fields

        [SerializeField()]
        [TriggerableTargetObject.Config(typeof(Transform))]
        private TriggerableTargetObject _target = new TriggerableTargetObject();

        [SerializeField()]
        private SPTimePeriod _delay = 0f;

        [SerializeField]
        private bool _invokeGuaranteed;

        #endregion

        #region Properties

        public TriggerableTargetObject Target
        {
            get { return _target; }
        }

        public SPTimePeriod Delay
        {
            get { return _delay; }
            set { _delay = value; }
        }

        public bool InvokeGuaranteed
        {
            get => _invokeGuaranteed;
            set => _invokeGuaranteed = value;
        }

        #endregion

        #region ITriggerableMechanism Interface

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            var targ = this._target.GetTarget<Transform>(arg);
            if (targ == null) return false;

            if (_delay.Seconds > 0f)
            {
                if (_invokeGuaranteed)
                {
                    this.InvokeGuaranteed(() =>
                    {
                        foreach (Transform child in targ)
                        {
                            ObjUtil.SmartDestroy(child.gameObject);
                        }
                    }, _delay.Seconds, _delay.TimeSupplier);
                }
                else
                {
                    this.Invoke(() =>
                    {
                        foreach (Transform child in targ)
                        {
                            ObjUtil.SmartDestroy(child.gameObject);
                        }
                    }, _delay.Seconds, _delay.TimeSupplier, RadicalCoroutineDisableMode.CancelOnDisable);
                }
            }
            else
            {
                foreach(Transform child in targ)
                {
                    ObjUtil.SmartDestroy(child.gameObject);
                }
            }

            return true;
        }

        #endregion

    }

}
