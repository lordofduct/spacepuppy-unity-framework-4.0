using UnityEngine;
using UnityEngine.Animations;
using System.Collections.Generic;

using com.spacepuppy.Utils;
using System.Linq;
using com.spacepuppy.Mecanim.Behaviours;

namespace com.spacepuppy.Mecanim
{

    public interface IBridgedStateMachineBehaviour
    {

        bool Initialized { get; }
        Animator Animator { get; }
        IAnimatorStateMachineBridge Bridge { get; }

        void Initialize(IAnimatorStateMachineBridge bridge);

    }

    public static class BridgedStateMachineBehaviourExtensions
    {

        /// <summary>
        /// Should be called during Start from the script acting as the IAnimatorStateMachineBridge.
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="bridge"></param>
        public static void InitializeBridge<T>(this T bridge) where T : IAnimatorStateMachineBridge
        {
            var behaviours = bridge.Animator.GetBehaviours<StateMachineBehaviour>();

            //initialize substate bridge container if necessary
            if (behaviours.OfType<a_SubStateBridge>().Any())
            {
                try
                {
                    bridge.SyncSubStateBridges();
                }
                catch (System.Exception ex)
                {
                    Debug.LogException(ex);
                }
            }

            //now initialize and bridges
            foreach (var b in behaviours)
            {
                if (b is IBridgedStateMachineBehaviour ab)
                {
                    try
                    {
                        ab.Initialize(bridge);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
            }
        }

        /// <summary>
        /// If a change was made to the SubStateBridgeContainer child hierarchy, call this to sync any state bridge's.
        /// </summary>
        public static void SyncSubStateBridges(this IAnimatorStateMachineBridge bridge)
        {
            bridge?.SubStateBridgeContainer?.AddOrGetComponent<AnimatorSubStateBridgeContainer>()?.SyncSubStateBridges();
        }

        public static IEnumerable<AnimatorSubStateBridge> GetSubStateBridges(this IAnimatorStateMachineBridge bridge)
        {
            return bridge?.SubStateBridgeContainer?.GetComponent<AnimatorSubStateBridgeContainer>()?.GetSubStateBridges() ?? Enumerable.Empty<AnimatorSubStateBridge>();
        }

        public static IEnumerable<AnimatorSubStateBridge> GetActiveSubStateBridges(this IAnimatorStateMachineBridge bridge)
        {
            return bridge?.SubStateBridgeContainer?.GetComponent<AnimatorSubStateBridgeContainer>()?.GetActiveSubStateBridges() ?? Enumerable.Empty<AnimatorSubStateBridge>();
        }

    }

    public abstract class BridgedStateMachineBehaviour : StateMachineBehaviour, IBridgedStateMachineBehaviour
    {

        #region Fields

        [System.NonSerialized]
        private IAnimatorStateMachineBridge _bridge;

        #endregion

        #region Methods

        protected virtual void OnInitialized() { }

        #endregion

        #region IAnimatorStateMachineBridgeBehaviour Interface

        public bool Initialized { get { return _bridge != null; } }

        public Animator Animator => _bridge?.Animator;

        public IAnimatorStateMachineBridge Bridge => _bridge;

        void IBridgedStateMachineBehaviour.Initialize(IAnimatorStateMachineBridge bridge)
        {
            if (bridge == null) throw new System.ArgumentNullException(nameof(bridge));
            _bridge = bridge;
            this.OnInitialized();
        }

        #endregion

    }

    public class BridgedStateMachineBehaviour<T> : StateMachineBehaviour, IBridgedStateMachineBehaviour where T : class, IAnimatorStateMachineBridge
    {

        #region Fields

        [System.NonSerialized]
        private T _bridge;

        #endregion

        #region Methods

        protected virtual void OnInitialized() { }

        #endregion

        #region IAnimatorStateMachineBridgeBehaviour Interface

        public bool Initialized { get { return _bridge != null; } }

        public Animator Animator => _bridge?.Animator;

        public T Bridge => _bridge;
        IAnimatorStateMachineBridge IBridgedStateMachineBehaviour.Bridge => _bridge;

        void IBridgedStateMachineBehaviour.Initialize(IAnimatorStateMachineBridge bridge)
        {
            if (bridge == null) throw new System.ArgumentNullException(nameof(bridge));
            if (!(bridge is T)) throw new System.InvalidOperationException("AnimatorStateMachineBridgeBehaviour that expects a bridge of type '" + typeof(T).Name + "' was initialized by a bridge of type '" + bridge.GetType().Name + "'.");

            _bridge = bridge as T;
            this.OnInitialized();
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

    /// <summary>
    /// A statemachine behaviour that always has update called on it and can provide information about the transition state. 
    /// Only inherit from this if your behaviour relies on the 'Update' method.
    /// </summary>
    public abstract class ActiveStateMachineBehaviour : SealedStateMachineBehaviour
    {

        #region Fields

        [System.NonSerialized]
        private StateMachineBehaviourUpdateTransitionState _transitionState;

        #endregion

        #region CONSTRUCTOR

        #endregion

        #region Properties

        public StateMachineBehaviourUpdateTransitionState TransitionState { get { return _transitionState; } }

        #endregion

        #region Methods

        protected virtual void OnEnter(AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller) { }

        protected virtual void OnTransitionToComplete(AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller) { }

        protected virtual void OnUpdate(AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller) { }

        protected virtual void OnTrasitionFromStart(AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller) { }

        protected virtual void OnExit(AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller) { }

        #endregion

        #region Sealed Crap

        public sealed override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
        {
            _transitionState = StateMachineBehaviourUpdateTransitionState.Entering;
            OnEnter(stateInfo, layerIndex, controller);
        }

        public sealed override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
        {
            if (!animator.gameObject.activeSelf)
                return;

            switch (_transitionState)
            {
                case StateMachineBehaviourUpdateTransitionState.Inactive:
                    {
                        //NOTE - OnStateUpdate should never be called without first calling OnStateEnter meaning this state chould never occur. 
                        //This would only occur if Unity screwed up. Do nothing for now, but in the future we may want to resolve this odd behaviour if we find it happens.
                        const string MSG = "Spacepuppy Developer Note - " + nameof(ActiveBridgedStateMachineBehaviour) + " entered an unexpected state, has the Unity StateMachineBehaviour API changed?";
                        Debug.LogWarning(MSG);
                    }
                    break;
                case StateMachineBehaviourUpdateTransitionState.Entering:
                    {
                        if (!animator.IsInTransition(layerIndex))
                        {
                            _transitionState = StateMachineBehaviourUpdateTransitionState.Active;
                            OnTransitionToComplete(stateInfo, layerIndex, controller);
                        }
                        else if (animator.GetCurrentAnimatorStateInfo(layerIndex).fullPathHash == stateInfo.fullPathHash)
                        {
                            _transitionState = StateMachineBehaviourUpdateTransitionState.Active;
                            OnTransitionToComplete(stateInfo, layerIndex, controller);
                            _transitionState = StateMachineBehaviourUpdateTransitionState.Exiting;
                            OnTrasitionFromStart(stateInfo, layerIndex, controller);
                        }

                        OnUpdate(stateInfo, layerIndex, controller);
                    }
                    break;
                case StateMachineBehaviourUpdateTransitionState.Active:
                    {
                        if (animator.IsInTransition(layerIndex))
                        {
                            _transitionState = StateMachineBehaviourUpdateTransitionState.Exiting;
                            OnTrasitionFromStart(stateInfo, layerIndex, controller);
                        }
                        OnUpdate(stateInfo, layerIndex, controller);
                    }
                    break;
                case StateMachineBehaviourUpdateTransitionState.Exiting:
                    {
                        OnUpdate(stateInfo, layerIndex, controller);
                    }
                    break;
            }
        }

        public sealed override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
        {
            try
            {
                OnExit(stateInfo, layerIndex, controller);
            }
            finally
            {
                _transitionState = StateMachineBehaviourUpdateTransitionState.Inactive;
            }
        }

        #endregion

    }

    /// <summary>
    /// A statemachine behaviour that always has update called on it and can provide information about the transition state. 
    /// Only inherit from this if your behaviour relies on the 'Update' method.
    /// </summary>
    public abstract class ActiveBridgedStateMachineBehaviour : ActiveStateMachineBehaviour, IBridgedStateMachineBehaviour
    {

        #region Fields

        private IAnimatorStateMachineBridge _bridge;

        #endregion

        #region Methods

        protected virtual void OnInitialized() { }

        #endregion

        #region IAnimatorStateMachineBridgeBehaviour Interface

        public bool Initialized { get { return _bridge != null; } }

        public Animator Animator { get { return _bridge?.Animator; } }

        public IAnimatorStateMachineBridge Bridge { get { return _bridge; } }

        void IBridgedStateMachineBehaviour.Initialize(IAnimatorStateMachineBridge bridge)
        {
            if (bridge == null) throw new System.ArgumentNullException(nameof(bridge));
            _bridge = bridge;
            this.OnInitialized();
        }

        #endregion

    }

    /// <summary>
    /// A statemachine behaviour that always has update called on it and can provide information about the transition state. 
    /// Only inherit from this if your behaviour relies on the 'Update' method.
    /// </summary>
    public abstract class ActiveBridgedStateMachineBehaviour<T> : ActiveStateMachineBehaviour, IBridgedStateMachineBehaviour where T : class, IAnimatorStateMachineBridge
    {

        #region Fields

        [System.NonSerialized]
        private T _bridge;

        #endregion

        #region Methods

        protected virtual void OnInitialized() { }

        #endregion

        #region IAnimatorStateMachineBridgeBehaviour Interface

        public bool Initialized { get { return _bridge != null; } }

        public Animator Animator => _bridge?.Animator;

        public T Bridge => _bridge;
        IAnimatorStateMachineBridge IBridgedStateMachineBehaviour.Bridge => _bridge;

        void IBridgedStateMachineBehaviour.Initialize(IAnimatorStateMachineBridge bridge)
        {
            if (bridge == null) throw new System.ArgumentNullException(nameof(bridge));
            if (!(bridge is T)) throw new System.InvalidOperationException("AnimatorStateMachineBridgeBehaviour that expects a bridge of type '" + typeof(T).Name + "' was initialized by a bridge of type '" + bridge.GetType().Name + "'.");

            _bridge = bridge as T;
            this.OnInitialized();
        }

        #endregion

    }

}
