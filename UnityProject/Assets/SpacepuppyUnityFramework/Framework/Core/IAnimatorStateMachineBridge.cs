using UnityEngine;
using UnityEngine.Animations;
using System.Collections.Generic;


namespace com.spacepuppy
{

    public interface IAnimatorStateMachineBridge : IComponent
    {

    }

    public static class BridgedStateMachineBehaviourExtensions
    {

        public static void Initialize<T>(this T bridge, Animator animator) where T : IAnimatorStateMachineBridge
        {
            BridgedStateMachineBehaviour.Initialize(animator, bridge);
        }

    }

    public abstract class BridgedStateMachineBehaviour : StateMachineBehaviour
    {

        public enum UpdateTransitionState
        {
            Inactive = 0,
            Entering = 1,
            Active = 2,
            Exiting = 3
        }

        #region Fields

        private Animator _animator;

        private UpdateTransitionState _transitionState;

        #endregion

        #region CONSTRUCTOR

        public static void Initialize(Animator animator, IAnimatorStateMachineBridge bridge)
        {
            var behaviours = animator.GetBehaviours<BridgedStateMachineBehaviour>();

            for (int i = 0; i < behaviours.Length; i++)
            {
                behaviours[i]._animator = animator;
                behaviours[i].InternalInitialize(animator, bridge);
                behaviours[i].OnStart();
            }
        }

        protected abstract void InternalInitialize(Animator animator, System.Object bridge);

        #endregion

        #region Properties

        public Animator Animator { get { return _animator; } }

        public UpdateTransitionState TransitionState { get { return _transitionState; } }

        #endregion

        #region Methods

        protected virtual void OnStart() { }

        protected virtual void OnStateEnter(AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller) { }

        protected virtual void OnTransitionToComplete(AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller) { }

        protected virtual void OnStateUpdate(AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller) { }

        protected virtual void OnTrasitionFromStart(AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller) { }

        protected virtual void OnStateExit(AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller) { }

        #endregion

        #region Sealed Crap

        public sealed override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }

        public sealed override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }

        public sealed override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }

        public sealed override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
        {
            if (animator != _animator) throw new System.ArgumentException(nameof(OnStateEnter) + " was called by an animator it was not configured for.", nameof(animator));

            _transitionState = UpdateTransitionState.Entering;
            OnStateEnter(stateInfo, layerIndex, controller);
        }

        public sealed override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
        {
            if (animator != _animator) throw new System.ArgumentException(nameof(OnStateEnter) + " was called by an animator it was not configured for.", nameof(animator));

            if (!animator.gameObject.activeSelf)
                return;

            switch (_transitionState)
            {
                case UpdateTransitionState.Inactive:
                    {
                        //NOTE - OnStateUpdate should never be called without first calling OnStateEnter meaning this state chould never occur. 
                        //This would only occur if Unity screwed up. Do nothing for now, but in the future we may want to resolve this odd behaviour if we find it happens.
                        const string MSG = "Spacepuppy Developer Note - " + nameof(BridgedStateMachineBehaviour) + " entered an unexpected state, has the Unity StateMachineBehaviour API changed?";
                        Debug.LogWarning(MSG);
                    }
                    break;
                case UpdateTransitionState.Entering:
                    {
                        if (!animator.IsInTransition(layerIndex))
                        {
                            _transitionState = UpdateTransitionState.Active;
                            OnTransitionToComplete(stateInfo, layerIndex, controller);
                        }
                        else if (animator.GetCurrentAnimatorStateInfo(layerIndex).fullPathHash == stateInfo.fullPathHash)
                        {
                            _transitionState = UpdateTransitionState.Active;
                            OnTransitionToComplete(stateInfo, layerIndex, controller);
                            _transitionState = UpdateTransitionState.Exiting;
                            OnTrasitionFromStart(stateInfo, layerIndex, controller);
                        }

                        OnStateUpdate(stateInfo, layerIndex, controller);
                    }
                    break;
                case UpdateTransitionState.Active:
                    {
                        if (animator.IsInTransition(layerIndex))
                        {
                            _transitionState = UpdateTransitionState.Exiting;
                            OnTrasitionFromStart(stateInfo, layerIndex, controller);
                        }
                        OnStateUpdate(stateInfo, layerIndex, controller);
                    }
                    break;
                case UpdateTransitionState.Exiting:
                    {
                        OnStateUpdate(stateInfo, layerIndex, controller);
                    }
                    break;
            }
        }

        public sealed override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
        {
            if (animator != _animator) throw new System.ArgumentException(nameof(OnStateEnter) + " was called by an animator it was not configured for.", nameof(animator));

            try
            {
                OnStateExit(stateInfo, layerIndex, controller);
            }
            finally
            {
                _transitionState = UpdateTransitionState.Inactive;
            }
        }

        #endregion

    }

    public abstract class BridgedStateMachineBehaviour<T> : BridgedStateMachineBehaviour where T : class, IAnimatorStateMachineBridge
    {

        #region Fields

        private T _bridge;

        #endregion

        #region CONSTRUCTOR

        #endregion

        #region Properties

        public T Bridge { get { return _bridge; } }

        #endregion

        #region Methods

        protected sealed override void InternalInitialize(Animator animator, object bridge)
        {
            if (!(bridge is T)) throw new System.ArgumentException("Bridge must be of type '" + typeof(T).Name + "'.", nameof(bridge));
            _bridge = bridge as T;
        }

        #endregion

    }

}
