using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Events;
using com.spacepuppy.Motor;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Motor.Events
{

    public class i_ApplyForce : Triggerable
    {

        #region Fields

        [SerializeField()]
        [UnityEngine.Serialization.FormerlySerializedAs("TargetObject")]
        [TriggerableTargetObject.Config(typeof(GameObject))]
        private TriggerableTargetObject _target = new TriggerableTargetObject();
        [SerializeField()]
        [UnityEngine.Serialization.FormerlySerializedAs("Force")]
        private ConfigurableForce _force = new ConfigurableForce();
        [SerializeField()]
        private bool _targetEntireEntity = true;

        [SerializeField]
        private SPTimePeriod _delay;

        [SerializeField]
        private bool _invokeGuaranteed;

        #endregion

        #region Properties

        public TriggerableTargetObject Target
        {
            get { return _target; }
        }

        public ConfigurableForce Force
        {
            get { return _force; }
        }

        public bool TargetEntireEntity
        {
            get { return _targetEntireEntity; }
            set { _targetEntireEntity = value; }
        }

        public SPTimePeriod Delay
        {
            get => _delay;
            set => _delay = value;
        }

        public bool InvokeGuaranteed
        {
            get => _invokeGuaranteed;
            set => _invokeGuaranteed = value;
        }

        #endregion

        #region Methods

        private void DoApplyForce(GameObject targ)
        {
            IMotor controller;
            if (targ.GetComponentInChildren<IMotor>(out controller))
            {
                //controller.AddForce(this.Force.GetForce(this.transform), this.Force.ForceMode);
                this.Force.ApplyForce(this.transform, controller);
                return;
            }
            Rigidbody body;
            if (targ.GetComponentInChildren<Rigidbody>(out body))
            {
                this.Force.ApplyForce(this.transform, body);
                return;
            }
        }

        #endregion

        #region TriggerableMechanism Interface

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            var targ = _target.GetTarget<GameObject>(arg);
            if (targ == null) return false;
            if (_targetEntireEntity) targ = GameObjectUtil.FindRoot(targ);


            if (_delay.Seconds > 0f)
            {
                if (_invokeGuaranteed)
                {
                    this.InvokeGuaranteed(() =>
                    {
                        this.DoApplyForce(targ);
                    }, _delay.Seconds, _delay.TimeSupplier);
                }
                else
                {
                    this.Invoke(() =>
                    {
                        this.DoApplyForce(targ);
                    }, _delay.Seconds, _delay.TimeSupplier, RadicalCoroutineDisableMode.CancelOnDisable);
                }
            }
            else
            {
                this.DoApplyForce(targ);
            }
            return true;
        }

        #endregion

    }

}