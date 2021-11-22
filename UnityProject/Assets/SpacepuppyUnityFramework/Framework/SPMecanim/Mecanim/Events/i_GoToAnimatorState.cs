using UnityEngine;
using UnityEngine.Serialization;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Events;
using com.spacepuppy.Utils;
using System.Linq;

namespace com.spacepuppy.Mecanim.Events
{

    public class i_GoToAnimatorState : AutoTriggerable
    {

        #region Fields

        [SerializeField]
        [TriggerableTargetObject.Config(typeof(Animator))]
        private TriggerableTargetObject _target;

        [SerializeField]
        private string _stateName;
        [SerializeField]
        [Tooltip("Layer to target, -1 to select first state of any layer (or if only 1 layer).")]
        private int _layer = -1;
        [SerializeField]
        private float _normalizedTime = float.NegativeInfinity;

        [SerializeField]
        private SPEvent _onStateExit;

        #endregion

        #region Properties

        public TriggerableTargetObject Target { get { return _target; } }

        public string StateName { get { return _stateName; } set { _stateName = value; } }

        public int Layer { get { return _layer; } set { _layer = value; } }

        public float NormalizedTime { get { return _normalizedTime; } set { _normalizedTime = value; } }

        #endregion

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            var targ = _target.GetTarget<Animator>(arg);
            if (targ == null) return false;

            targ.Play(_stateName, _layer, _normalizedTime);
            if(_onStateExit.HasReceivers)
            {
                GameLoop.Hook.StartRadicalCoroutine(this.DoWait(targ, _stateName, _layer));
            }
            return true;
        }

        private System.Collections.IEnumerator DoWait(Animator animator, string stateName, int layerIndex)
        {
            yield return WaitForAnimState.WaitForStateExit_PostPlay(animator, stateName, layerIndex);
            _onStateExit.ActivateTrigger(this, null);
        }

    }

}
