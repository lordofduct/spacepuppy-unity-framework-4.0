using UnityEngine;
using UnityEngine.Serialization;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Events;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Mecanim.Events
{

    public class i_OverrideAnimator : AutoTriggerable
    {

        #region Fields

        [SerializeField]
        [TriggerableTargetObject.Config(typeof(Animator))]
        private TriggerableTargetObject _target;

        [SerializeField]
        private UnityEngine.Object _overrides;
        [SerializeField]
        private bool _treatUnconfiguredEntriesAsValidEntries;

        [SerializeField]
        [Tooltip("The token used to identify the layer to purge.")]
        private string _token;

        #endregion

        #region Properties

        public TriggerableTargetObject Target { get { return _target; } }

        public UnityEngine.Object Overrides => _overrides;

        public bool TreatUnconfiguredEntriesAsValidEntries => _treatUnconfiguredEntriesAsValidEntries;

        #endregion

        #region Methods

        public void SetOverrides(AnimatorOverrideController controller, bool treatUnconfiguredEntriesAsValidEntries = false)
        {
            _overrides = controller;
            _treatUnconfiguredEntriesAsValidEntries = treatUnconfiguredEntriesAsValidEntries;
        }

        public void SetOverrides(IAnimatorOverrideSource source)
        {
            _overrides = source as UnityEngine.Object;
            _treatUnconfiguredEntriesAsValidEntries = false;
        }

        #endregion

        #region Triggerable Interface

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            var targ = _target.GetTarget<Animator>(arg);
            if (targ == null) return false;

            MecanimExtensions.StackOverrideGeneralized(targ, _overrides, _token, false);
            return true;
        }

        #endregion

    }

}
