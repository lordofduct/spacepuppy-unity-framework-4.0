using UnityEngine;

namespace com.spacepuppy.Events
{

    public class i_SetParent : AutoTriggerable
    {

        #region Fields

        [SerializeField]
        [TriggerableTargetObject.Config(typeof(Transform))]
        private TriggerableTargetObject _child = new TriggerableTargetObject();
        [SerializeField]
        [TriggerableTargetObject.Config(typeof(Transform))]
        private TriggerableTargetObject _parent = new TriggerableTargetObject();

        [SerializeField]
        private bool _worldPositionStays = true;

        #endregion

        #region Properties

        public TriggerableTargetObject Child => _child;

        public TriggerableTargetObject Parent => _parent;

        #endregion

        #region Methods

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            var child = _child.GetTarget<Transform>(arg);
            var parent = _parent.GetTarget<Transform>(arg);

            if (child == null) return false;
            child.SetParent(parent, _worldPositionStays);
            return true;
        }

        #endregion

    }

}
