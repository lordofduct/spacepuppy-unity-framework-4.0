#pragma warning disable 0649 // variable declared but not used.

using UnityEngine;

using com.spacepuppy.Events;
using com.spacepuppy.Tween;

namespace com.spacepuppy.Tween.Events
{

    public class i_KillTween : AutoTriggerable
    {

        #region Properties

        [SerializeField()]
        [SelectableObject()]
        private UnityEngine.Object _target;

        [SerializeField()]
        [UnityEngine.Serialization.FormerlySerializedAs("_tweenToken")]
        [Tooltip("Leave blank to kill all associated with target.")]
        private string _killToken;

        #endregion

        #region ITriggerable Interface

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            if (string.IsNullOrEmpty(_killToken))
                SPTween.KillAll(_target);
            else
                SPTween.KillAll(_target, _killToken);

            return true;
        }

        #endregion

    }

}
