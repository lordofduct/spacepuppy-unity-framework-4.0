using UnityEngine;
using UnityEngine.Animations;
using System.Collections.Generic;

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

}
