﻿using UnityEngine;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Events
{

    public class i_Destroy : AutoTriggerable
    {

        #region Fields

        [SerializeField()]
        [TriggerableTargetObject.Config(typeof(UnityEngine.Object))]
        private TriggerableTargetObject _target = new TriggerableTargetObject();

        [SerializeField]
        private bool _destroyRoot;

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

            var targ = this._target.GetTarget<UnityEngine.Object>(arg);
            if (_destroyRoot && GameObjectUtil.IsGameObjectSource(targ)) targ = GameObjectUtil.FindRoot(GameObjectUtil.GetGameObjectFromSource(targ));
            if (targ == null) return false;

            if (_delay.Seconds > 0f)
            {
                if (_invokeGuaranteed)
                {
                    this.InvokeGuaranteed(() =>
                    {
                        ObjUtil.SmartDestroy(targ);
                    }, _delay.Seconds, _delay.TimeSupplier);
                }
                else
                {
                    this.Invoke(() =>
                    {
                        ObjUtil.SmartDestroy(targ);
                    }, _delay.Seconds, _delay.TimeSupplier, RadicalCoroutineDisableMode.CancelOnDisable);
                }
            }
            else
            {
                ObjUtil.SmartDestroy(targ);
            }

            return true;
        }

        #endregion

    }

}
