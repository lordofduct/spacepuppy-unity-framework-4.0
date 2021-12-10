using UnityEngine;

using com.spacepuppy.Utils;

namespace com.spacepuppy
{

    [RequireComponent(typeof(Rigidbody))]
    public class IgnorableRigidbody : SPComponent, IIgnorableCollision
    {

        #region Fields

        [System.NonSerialized()]
        private Collider[] _colliders; //consider making this configurable

        [System.NonSerialized()]
        private Rigidbody _rigidbody;

        #endregion

        #region CONSTRUCTOR

        protected override void Awake()
        {
            base.Awake();

            _rigidbody = this.GetComponent<Rigidbody>();
            _colliders = this.GetComponentsInChildren<Collider>();
        }

        #endregion

        #region IIgnorableCollision Interface

        public void IgnoreCollision(IIgnorableCollision coll, bool ignore)
        {
            if (coll == null) return;
            if (_colliders == null) return;

            for (int i = 0; i < _colliders.Length; i++)
            {
                if (_colliders[i] != null) coll.IgnoreCollision(_colliders[i], ignore);
            }
        }

        public void IgnoreCollision(Collider coll, bool ignore)
        {
            if (coll == null) return;
            if (_colliders == null) return;

            for (int i = 0; i < _colliders.Length; i++)
            {
                if (_colliders[i] != null) Physics.IgnoreCollision(_colliders[i], coll, ignore);
            }
        }

        #endregion

        #region Static Utils

        public static IgnorableRigidbody GetIgnorableCollision(Rigidbody rb)
        {
            if (rb == null) return null;

            return rb.AddOrGetComponent<IgnorableRigidbody>();
        }

        #endregion

    }

}