using UnityEngine;

using com.spacepuppy.Utils;

namespace com.spacepuppy
{

    public class IgnorableCollider : SPComponent, IIgnorableCollision
    {

        #region Fields

        private Collider _collider;

        #endregion

        #region CONSTRUCTOR

        protected override void Awake()
        {
            base.Awake();

            _collider = this.GetComponent<Collider>();
        }

        #endregion

        #region Properties

        public Collider Collider { get { return _collider; } }

        #endregion

        #region IIgnorableCollision Interface

        public void IgnoreCollision(Collider coll, bool ignore)
        {
            if (_collider == null || coll == null || _collider == coll) return;

            Physics.IgnoreCollision(_collider, coll, ignore);
        }

        public void IgnoreCollision(IIgnorableCollision coll, bool ignore)
        {
            if (_collider == null || coll == null || object.ReferenceEquals(this, coll)) return;

            coll.IgnoreCollision(_collider, ignore);
        }

        #endregion

        #region Static Interface

        public static IgnorableCollider GetIgnorableCollision(Collider coll)
        {
            if (coll == null) return null;

            return coll.AddOrGetComponent<IgnorableCollider>();
        }

        #endregion

    }

}