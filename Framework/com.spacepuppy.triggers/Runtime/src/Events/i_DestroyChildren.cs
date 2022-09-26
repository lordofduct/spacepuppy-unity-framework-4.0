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

        #endregion

        #region ITriggerableMechanism Interface

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            var targ = this._target.GetTarget<Transform>(arg);
            if (targ == null) return false;

            if (_delay.Seconds > 0f)
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
