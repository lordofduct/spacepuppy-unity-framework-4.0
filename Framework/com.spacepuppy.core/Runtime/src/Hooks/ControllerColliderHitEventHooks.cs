using UnityEngine;

namespace com.spacepuppy.Hooks
{

    public delegate void OnControllerColliderHitCallback(object sender, ControllerColliderHit hit);

    public sealed class ControllerColliderHitEventHooks : MonoBehaviour
    {

        private OnControllerColliderHitCallback _onControllerColliderHit;
        public event OnControllerColliderHitCallback ControllerColliderHit
        {
            add
            {
                _onControllerColliderHit += value;
                if (!this.enabled) this.enabled = true;
            }
            remove
            {
                _onControllerColliderHit -= value;
                if (_onControllerColliderHit == null) this.enabled = false;
            }
        }

        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            _onControllerColliderHit?.Invoke(this, hit);
        }

        private void OnDestroy()
        {
            _onControllerColliderHit = null;
        }

    }
}
