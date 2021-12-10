using UnityEngine;
using System.Collections.Generic;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Mecanim
{

    /// <summary>
    /// Similar to SPEvent but instead performs various Animator related functionality. 
    /// This should only ever be used from within a IAnimatorStateMachineBridgeBehaviour, 
    /// if the version of ActivateTrigger that takes an 'Animator' is called it may need to hunt down an associated IAnimatorStateMachineBridge
    /// to support the 'PurgeAnimatorOverride' mode. If one is not found, it will do nothing.
    /// You should put a infobox in the component to warn about this.
    /// </summary>
    [System.Serializable]
    public class SPAnimatorStateMachineEvent
    {

        #region Events

        private System.EventHandler<TempEventArgs> _triggerActivated;
        public event System.EventHandler<TempEventArgs> TriggerActivated
        {
            add
            {
                _triggerActivated += value;
            }
            remove
            {
                _triggerActivated -= value;
            }
        }
        protected virtual void OnTriggerActivated(object sender, object arg)
        {
            if (_triggerActivated != null)
            {
                var e = TempEventArgs.Create(arg);
                var d = _triggerActivated;
                d(sender, e);
                TempEventArgs.Release(e);
            }
        }

        #endregion

        #region Fields

        [SerializeField]
        private List<AnimatorTriggerTarget> _animatorTargets;

        #endregion

        #region CONSTRUCTOR

        public SPAnimatorStateMachineEvent()
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns true if TargetCount > 0 or there are event receivers attached. 
        /// </summary>
        public bool HasReceivers
        {
            get { return _triggerActivated != null || _animatorTargets.Count > 0; }
        }

        #endregion

        #region Methods

        public void ActivateTrigger(Animator animator, object arg)
        {
            foreach (var obj in _animatorTargets)
            {
                obj.ActivateTrigger(animator, arg);
            }
            OnTriggerActivated(animator, arg);
        }

        public virtual void ActivateTrigger(IBridgedStateMachineBehaviour smb, object arg)
        {
            foreach (var obj in _animatorTargets)
            {
                obj.ActivateTrigger(smb, arg);
            }
            OnTriggerActivated(smb, arg);
        }

        #endregion

        #region Special Types

        [System.Serializable]
        public class AnimatorTriggerTarget
        {

            [SerializeField]
            private AnimatorTriggerAction _action;
            [SerializeField]
            private string _id;
            [SerializeField]
            private float _value;
            [SerializeField]
            private UnityEngine.Object _objectRef;

            public void ActivateTrigger(Animator animator, object arg)
            {
                if (animator == null) return;

                switch (_action)
                {
                    case AnimatorTriggerAction.SetTrigger:
                        animator.SetTrigger(_id);
                        break;
                    case AnimatorTriggerAction.ResetTrigger:
                        animator.ResetTrigger(_id);
                        break;
                    case AnimatorTriggerAction.SetBool:
                        animator.SetBool(_id, _value != 0f);
                        break;
                    case AnimatorTriggerAction.SetInt:
                        animator.SetInteger(_id, Mathf.RoundToInt(_value));
                        break;
                    case AnimatorTriggerAction.SetFloat:
                        animator.SetFloat(_id, _value);
                        break;
                    case AnimatorTriggerAction.OverrideAnimatorController:
                        MecanimExtensions.StackOverrideGeneralized(animator, _objectRef, _id, false);
                        break;
                    case AnimatorTriggerAction.PurgeAnimatorOverride:
                        animator.RemoveOverride(_id);
                        break;
                    case AnimatorTriggerAction.TriggerAllOnTarget:
                        com.spacepuppy.Events.EventTriggerEvaluator.Current.TriggerAllOnTarget(_objectRef, animator, arg);
                        break;
                }
            }

            public void ActivateTrigger(IBridgedStateMachineBehaviour smb, object arg)
            {
                if (smb == null) throw new System.ArgumentNullException(nameof(smb));

                var animator = smb.Animator;
                if (animator == null) return;

                switch (_action)
                {
                    case AnimatorTriggerAction.SetTrigger:
                        animator.SetTrigger(_id);
                        break;
                    case AnimatorTriggerAction.ResetTrigger:
                        animator.ResetTrigger(_id);
                        break;
                    case AnimatorTriggerAction.SetBool:
                        animator.SetBool(_id, _value != 0f);
                        break;
                    case AnimatorTriggerAction.SetInt:
                        animator.SetInteger(_id, Mathf.RoundToInt(_value));
                        break;
                    case AnimatorTriggerAction.SetFloat:
                        animator.SetFloat(_id, _value);
                        break;
                    case AnimatorTriggerAction.OverrideAnimatorController:
                        MecanimExtensions.StackOverrideGeneralized(animator, _objectRef, _id, false);
                        break;
                    case AnimatorTriggerAction.PurgeAnimatorOverride:
                        animator.RemoveOverride(_id);
                        break;
                    case AnimatorTriggerAction.TriggerAllOnTarget:
                        com.spacepuppy.Events.EventTriggerEvaluator.Current.TriggerAllOnTarget(_objectRef, animator, arg);
                        break;
                }
            }

        }

        public enum AnimatorTriggerAction
        {
            SetTrigger = 0,
            ResetTrigger = 1,
            SetBool = 2,
            SetInt = 3,
            SetFloat = 4,
            OverrideAnimatorController = 5,
            PurgeAnimatorOverride = 6, //only supported from within a IAnimatorStateMachineBridgeBehaviour
            TriggerAllOnTarget = 7,
        }

        #endregion

    }

}
