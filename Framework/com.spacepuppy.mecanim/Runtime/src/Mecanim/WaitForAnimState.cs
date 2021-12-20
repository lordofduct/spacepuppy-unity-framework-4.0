using UnityEngine;

namespace com.spacepuppy.Mecanim
{

    public sealed class WaitForAnimState : IRadicalEnumerator, IPooledYieldInstruction
    {

        private enum Mode
        {
            Exit = 0,
            StartExit = 1,
            Active = 2,

            WaitFirstThenExit = 4,
            WaitFirstThenStartExit = 5
        }

        #region Fields

        private Animator _animator;
        private int _layerIndex;
        private int _hash;
        private string _stateName;
        private KeepWaitingCallback _mode;

        private bool _isComplete;

        #endregion

        #region CONSTRUCTOR

        private WaitForAnimState()
        {
            //block constructor
        }

        #endregion

        #region Properties

        public Animator Animator => _animator;

        public int LayerIndex => _layerIndex;

        #endregion

        #region IRadicalEnumerator Interface

        bool IRadicalYieldInstruction.IsComplete => _isComplete;

        object System.Collections.IEnumerator.Current => null;


        bool IRadicalYieldInstruction.Tick(out object yieldObject)
        {
            yieldObject = null;
            if (_animator == null) return false;

            _isComplete = (_mode?.Invoke(this) ?? false);
            return !_isComplete;
        }

        bool System.Collections.IEnumerator.MoveNext()
        {
            if (_animator == null) return false;

            _isComplete = (_mode?.Invoke(this) ?? false);
            return !_isComplete;
        }

        void System.Collections.IEnumerator.Reset()
        {
            //do nothing
        }

        #endregion

        #region Factory

        private static com.spacepuppy.Collections.ObjectCachePool<WaitForAnimState> _pool = new com.spacepuppy.Collections.ObjectCachePool<WaitForAnimState>(-1, () => new WaitForAnimState());

        public static WaitForAnimState WaitForCurrentStateExit(Animator animator, int layerIndex)
        {
            var state = animator.GetCurrentAnimatorStateInfo(layerIndex);
            var result = _pool.GetInstance();
            result._animator = animator;
            result._layerIndex = layerIndex;
            result._hash = state.fullPathHash;
            result._stateName = null;
            result._mode = Callback_WaitForExitByHash;
            return result;
        }

        public static WaitForAnimState WaitForStateEnter(Animator animator, string stateName, int layerIndex, SPTimePeriod timeout)
        {
            var result = _pool.GetInstance();
            result._animator = animator;
            result._layerIndex = layerIndex;
            result._hash = 0;
            result._stateName = stateName;
            result._mode = Callback_WaitForEnterByName;
            return result;
        }

        public static WaitForAnimState WaitForStateExit(Animator animator, string stateName, int layerIndex)
        {
            var result = _pool.GetInstance();
            result._animator = animator;
            result._layerIndex = layerIndex;
            result._hash = 0;
            result._stateName = stateName;
            result._mode = Callback_WaitForExitByName;
            return result;
        }

        /// <summary>
        /// Waits first for the state to enter since it can take a couple frames after calling Play/CrossFade before the state actually activates. Then it waits for the exit.
        /// 
        /// Note - if the state is not entered within 4 frames, this exits to avoid getting stuck forever.
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="stateName"></param>
        /// <param name="layerIndex"></param>
        /// <returns></returns>
        public static WaitForAnimState WaitForStateExit_PostPlay(Animator animator, string stateName, int layerIndex)
        {
            var result = _pool.GetInstance();
            result._animator = animator;
            result._layerIndex = layerIndex;
            result._hash = 0;
            result._stateName = stateName;
            result._mode = Callback_WaitForExitByName_PostPlay;
            return result;
        }

        public static WaitForAnimState WaitFor(Animator animator, string stateName, int layerIndex, KeepWaitingCallback callback)
        {
            var result = _pool.GetInstance();
            result._animator = animator;
            result._layerIndex = layerIndex;
            result._hash = 0;
            result._stateName = stateName;
            result._mode = callback;
            return result;
        }

        #endregion

        #region IPooledYieldInstruction Interface

        void System.IDisposable.Dispose()
        {
            _animator = null;
            _layerIndex = 0;
            _hash = 0;
            _stateName = null;
            _mode = null;
            _isComplete = false;
            _pool.Release(this);
        }

        #endregion

        #region Delegate Pointers

        public delegate bool KeepWaitingCallback(WaitForAnimState state);

        private static KeepWaitingCallback _waitForExitByHash;
        public static KeepWaitingCallback Callback_WaitForExitByHash => _waitForExitByHash ?? (_waitForExitByHash = (s) => s._animator.GetCurrentAnimatorStateInfo(s._layerIndex).fullPathHash != s._hash);

        private static KeepWaitingCallback _waitForExitByName;
        public static KeepWaitingCallback Callback_WaitForExitByName => _waitForExitByName ?? (_waitForExitByName = (s) => !s._animator.GetCurrentAnimatorStateIs(s._stateName, s._layerIndex));

        private static KeepWaitingCallback _waitForEnterByHash;
        public static KeepWaitingCallback Callback_WaitForEnterByHash => _waitForEnterByHash ?? (_waitForEnterByHash = (s) => s._animator.GetCurrentAnimatorStateInfo(s._layerIndex).fullPathHash == s._hash);

        private static KeepWaitingCallback _waitForEnterByName;
        public static KeepWaitingCallback Callback_WaitForEnterByName => _waitForEnterByName ?? (_waitForEnterByName = (s) => s._animator.GetCurrentAnimatorStateIs(s._stateName, s._layerIndex));

        private static KeepWaitingCallback _waitForExitByName_PostPlay;
        public static KeepWaitingCallback Callback_WaitForExitByName_PostPlay => _waitForExitByName_PostPlay ?? (_waitForExitByName_PostPlay = (s) =>
        {
            AnimatorStateInfo info;
            int li = s._layerIndex;
            if (s._animator.GetCurrentAnimatorStateIs(s._stateName, ref li, out info))
            {
                s._stateName = null;
                s._hash = info.fullPathHash;
                s._layerIndex = li;
                s._mode = Callback_WaitForExitByHash;
                return false;
            }
            else if(s._animator.GetNextAnimatorStateIs(s._stateName, ref li, out info))
            {
                //this happens if we're in a "cross-fade"
                s._layerIndex = li;
                return false;
            }
            else
            {
                s._hash++;
                return s._hash > 4;
            }
        });

        #endregion

    }

}
