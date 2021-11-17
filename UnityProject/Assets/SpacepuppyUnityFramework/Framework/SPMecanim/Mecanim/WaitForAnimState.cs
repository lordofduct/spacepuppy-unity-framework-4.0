using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.spacepuppy
{

    public sealed class WaitForAnimState : CustomYieldInstruction, IRadicalEnumerator, IPooledYieldInstruction
    {

        private enum Mode
        {
            Exit = 0,
            StartExit = 1,
            Active = 2,
        }

        #region Fields

        private Animator _animator;
        private int _layerIndex;
        private AnimatorStateInfo _state;
        private Mode _mode;

        #endregion

        #region CONSTRUCTOR

        private WaitForAnimState()
        {
            //block constructor
        }

        #endregion

        #region CustomYieldInstruction Interface

        public override bool keepWaiting
        {
            get
            {
                if (_animator == null) return false;

                switch (_mode)
                {
                    case Mode.Exit:
                        return _animator.GetCurrentAnimatorStateInfo(_layerIndex).fullPathHash != _state.fullPathHash;
                    case Mode.StartExit:
                        return _animator.IsInTransition(_layerIndex) && _animator.GetNextAnimatorStateInfo(_layerIndex).fullPathHash != _state.fullPathHash;
                    case Mode.Active:
                        return _animator.GetCurrentAnimatorStateInfo(_layerIndex).fullPathHash == _state.fullPathHash;
                    default:
                        return false;
                }

            }
        }

        #endregion

        #region IRadicalEnumerator Interface

        bool IRadicalYieldInstruction.IsComplete => !this.keepWaiting;

        bool IRadicalYieldInstruction.Tick(out object yieldObject)
        {
            yieldObject = null;
            return this.keepWaiting;
        }

        #endregion

        #region Factory

        private static com.spacepuppy.Collections.ObjectCachePool<WaitForAnimState> _pool = new com.spacepuppy.Collections.ObjectCachePool<WaitForAnimState>(-1, () => new WaitForAnimState());

        public WaitForAnimState WaitForCurrentStateExit(Animator animator, int layerIndex)
        {
            var state = animator.GetCurrentAnimatorStateInfo(layerIndex);
            var result = _pool.GetInstance();
            result._animator = animator;
            result._layerIndex = layerIndex;
            result._state = state;
            result._mode = Mode.Exit;
            return result;
        }

        public static WaitForAnimState WaitForCurrentStateExitTransitionBegin(Animator animator, int layerIndex)
        {
            var state = animator.GetCurrentAnimatorStateInfo(layerIndex);
            var result = _pool.GetInstance();
            result._animator = animator;
            result._layerIndex = layerIndex;
            result._state = state;
            result._mode = Mode.StartExit;
            return result;
        }

        public static WaitForAnimState WaitUntilEnterState(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var result = _pool.GetInstance();
            result._animator = animator;
            result._layerIndex = layerIndex;
            result._state = stateInfo;
            result._mode = Mode.Active;
            return result;
        }

        #endregion

        #region IPooledYieldInstruction Interface

        void System.IDisposable.Dispose()
        {
            _animator = null;
            _layerIndex = 0;
            _state = default(AnimatorStateInfo);
            _mode = Mode.Exit;
            _pool.Release(this);
        }

        #endregion

    }

}
