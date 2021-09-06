using UnityEngine;
using UnityEngine.Animations;
using System.Collections.Generic;
using System.Dynamic;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Mecanim
{

    /// <summary>
    /// Represents a bridge component between IAnimatorStateMachineBridgeBehaviour and the Animator its attached to.
    /// 
    /// Only one bridge should exist on the same entity as the Animator.
    /// </summary>
    public interface IAnimatorStateMachineBridge : IComponent
    {

        Animator Animator { get; }
        RuntimeAnimatorController InitialRuntimeAnimatorController { get; }

    }

    public interface IAnimatorStateMachineBridgeBehaviour
    {
        Animator Animator { get; }
        IAnimatorStateMachineBridge Bridge { get; }
    }

    public static class BridgedStateMachineBehaviourExtensions
    {

        /// <summary>
        /// Should be called during Start from the script acting as the IAnimatorStateMachineBridge.
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="bridge"></param>
        public static void Initialize<T>(this T bridge, Animator animator) where T : IAnimatorStateMachineBridge
        {
            BridgedStateMachineBehaviour.Initialize(animator, bridge);
        }

    }

    /// <summary>
    /// A dummy hook that allows an Animator to behave like a bridge all on its own.
    /// </summary>
    internal class DummyAnimatorStateMachineBridge : MonoBehaviour, IAnimatorStateMachineBridge
    {

        private Animator _animator;
        private RuntimeAnimatorController _initialController;

        #region IAnimatorStateMachineBridge Interface

        Animator IAnimatorStateMachineBridge.Animator => _animator;

        RuntimeAnimatorController IAnimatorStateMachineBridge.InitialRuntimeAnimatorController => _initialController;

        Component IComponent.component => this;

        #endregion

        #region Static Utils

        public static void TryAttach(Animator animator, IAnimatorStateMachineBridge remoteBridge = null)
        {
            if (animator == null) return;

            if(ObjUtil.IsNullOrDestroyed(animator.GetComponent<IAnimatorStateMachineBridge>()))
            {
                var bridge = animator.AddComponent<DummyAnimatorStateMachineBridge>();
                bridge._animator = animator;
                bridge._initialController = animator.runtimeAnimatorController;
            }
        }

        #endregion

    }

    /// <summary>
    /// A cross entity hook that associates a bridge with its Animator so that something is returned if you call GetComponent(IAnimatorStateMachineBridge) on the Animator even though the real bridge is elsewhere on the entity.
    /// </summary>
    internal class CrossEntityAnimatorStateMachineBridge : MonoBehaviour, IAnimatorStateMachineBridge
    {

        private IAnimatorStateMachineBridge _remoteBridge;

        #region IAnimatorStateMachineBridge Interface

        Animator IAnimatorStateMachineBridge.Animator => _remoteBridge.Animator;

        RuntimeAnimatorController IAnimatorStateMachineBridge.InitialRuntimeAnimatorController => _remoteBridge.InitialRuntimeAnimatorController;

        Component IComponent.component => this;

        #endregion

        #region Static Utils

        public static void TryAttach(Animator animator, IAnimatorStateMachineBridge remoteBridge)
        {
            if (animator == null) return;

            if (ObjUtil.IsNullOrDestroyed(animator.GetComponent<IAnimatorStateMachineBridge>()))
            {
                var bridge = animator.AddComponent<CrossEntityAnimatorStateMachineBridge>();
                bridge._remoteBridge = remoteBridge;
            }
        }

        #endregion

    }

    /// <summary>
    /// Unity does this weird thing where even though these events/messages are declared virtually it still calls them reflectively like MonoBehaviour messages.
    /// 
    /// By sealing them in their own class like this it creates a stop point for them. It forces one inheriting from it to use the 4 parameter version, and that version will be the 1st one Unity finds reflectively and therefore will use it. 
    /// 
    /// This facilitates complex implementations like that found in BridgedStateMachineBehaviour.
    /// </summary>
    public abstract class SealedStateMachineBehaviour : StateMachineBehaviour
    {
        public sealed override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }

        public sealed override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }

        public sealed override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }
    }

    public abstract class BridgedStateMachineBehaviour : SealedStateMachineBehaviour, IAnimatorStateMachineBridgeBehaviour
    {

        public enum UpdateTransitionState
        {
            Inactive = 0,
            Entering = 1,
            Active = 2,
            Exiting = 3
        }

        #region Fields

        [System.NonSerialized]
        private Animator _animator;
        [System.NonSerialized]
        private UpdateTransitionState _transitionState;

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Should be called during Start from the script acting as the IAnimatorStateMachineBridge.
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="bridge"></param>
        public static void Initialize(Animator animator, IAnimatorStateMachineBridge bridge)
        {
            if (object.ReferenceEquals(animator, null)) throw new System.ArgumentNullException(nameof(animator));

            CrossEntityAnimatorStateMachineBridge.TryAttach(animator, bridge);

            var behaviours = animator.GetBehaviours<BridgedStateMachineBehaviour>();
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (object.ReferenceEquals(behaviours[i]._animator, null))
                {
                    behaviours[i]._animator = animator;
                    behaviours[i].InternalInitialize(animator, bridge);
                    behaviours[i].OnInitialized();
                }
            }
        }

        protected abstract void InternalInitialize(Animator animator, IAnimatorStateMachineBridge bridge);

        #endregion

        #region Properties

        public UpdateTransitionState TransitionState { get { return _transitionState; } }

        #endregion

        #region Methods

        protected abstract IAnimatorStateMachineBridge GetBridge();

        protected virtual void OnInitialized() { }

        protected virtual void OnEnter(AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller) { }

        protected virtual void OnTransitionToComplete(AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller) { }

        protected virtual void OnUpdate(AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller) { }

        protected virtual void OnTrasitionFromStart(AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller) { }

        protected virtual void OnExit(AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller) { }

        #endregion

        #region Sealed Crap

        public sealed override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
        {
            if (animator != _animator) throw new System.ArgumentException(nameof(OnStateEnter) + " was called by an animator it was not configured for.", nameof(animator));

            _transitionState = UpdateTransitionState.Entering;
            OnEnter(stateInfo, layerIndex, controller);
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

                        OnUpdate(stateInfo, layerIndex, controller);
                    }
                    break;
                case UpdateTransitionState.Active:
                    {
                        if (animator.IsInTransition(layerIndex))
                        {
                            _transitionState = UpdateTransitionState.Exiting;
                            OnTrasitionFromStart(stateInfo, layerIndex, controller);
                        }
                        OnUpdate(stateInfo, layerIndex, controller);
                    }
                    break;
                case UpdateTransitionState.Exiting:
                    {
                        OnUpdate(stateInfo, layerIndex, controller);
                    }
                    break;
            }
        }

        public sealed override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
        {
            if (animator != _animator) throw new System.ArgumentException(nameof(OnStateEnter) + " was called by an animator it was not configured for.", nameof(animator));

            try
            {
                OnExit(stateInfo, layerIndex, controller);
            }
            finally
            {
                _transitionState = UpdateTransitionState.Inactive;
            }
        }

        #endregion

        #region IAnimatorStateMachineBridgeBehaviour Interface

        public Animator Animator { get { return _animator; } }

        public IAnimatorStateMachineBridge Bridge { get { return this.GetBridge(); } }

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

        public new T Bridge { get { return _bridge; } }

        #endregion

        #region Methods

        protected sealed override IAnimatorStateMachineBridge GetBridge()
        {
            return _bridge;
        }

        protected sealed override void InternalInitialize(Animator animator, IAnimatorStateMachineBridge bridge)
        {
            if (!(bridge is T)) throw new System.ArgumentException("Bridge must be of type '" + typeof(T).Name + "'.", nameof(bridge));
            _bridge = bridge as T;
        }

        #endregion

    }

}
