using UnityEngine;

namespace com.spacepuppy.Hooks
{

    public delegate void OnCollisionCallback(GameObject sender, Collision collision);

    public delegate void OnTriggerCallback(GameObject sender, Collider otherCollider);

    public delegate void OnStrikeCallback(GameObject sender, Collider otherCollider);

    public class CollisionHooks : MonoBehaviour
    {
        private OnCollisionCallback _onEnter;
        private OnCollisionCallback _onExit;
        private OnCollisionCallback _onStay;


        public event OnCollisionCallback OnEnter
        {
            add
            {
                _onEnter += value;
                if (!this.enabled) this.enabled = true;
            }
            remove
            {
                _onEnter -= value;
                if (_onEnter == null && _onStay == null && _onExit == null) this.enabled = false;
            }
        }
        public event OnCollisionCallback OnExit
        {
            add
            {
                _onExit += value;
                if (!this.enabled) this.enabled = true;
            }
            remove
            {
                _onExit -= value;
                if (_onEnter == null && _onStay == null && _onExit == null) this.enabled = false;
            }
        }

        public event OnCollisionCallback OnStay
        {
            add
            {
                _onStay += value;
                if (!this.enabled) this.enabled = true;
            }
            remove
            {
                _onStay -= value;
                if (_onEnter == null && _onStay == null && _onExit == null) this.enabled = false;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!this.isActiveAndEnabled) return;

            if (_onEnter != null) _onEnter(this.gameObject, collision);
        }

        private void OnCollisionStay(Collision collision)
        {
            if (!this.isActiveAndEnabled) return;

            if (_onStay != null) _onStay(this.gameObject, collision);
        }

        private void OnCollisionExit(Collision collision)
        {
            if (!this.isActiveAndEnabled) return;

            if (_onExit != null) _onExit(this.gameObject, collision);
        }

        private void OnDestroy()
        {
            _onEnter = null;
            _onExit = null;
            _onStay = null;
        }
    }

}
