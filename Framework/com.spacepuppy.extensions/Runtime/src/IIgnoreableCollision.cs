using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Utils;

namespace com.spacepuppy
{

    public interface IIgnorableCollision : IGameObjectSource
    {

        void IgnoreCollision(Collider coll, bool ignore);
        void IgnoreCollision(IIgnorableCollision coll, bool ignore);

    }

    /// <summary>
    /// A token for 2 Colliders or IIgnorableCollision objects ignoring one another. 
    /// This also allows you to not have to track ignore relationships. If 2 objects are flagged to ignore in 2 different segments 
    /// of code, this will track them stacking, and won't unignore until BOTH places call to stop the ignoring.
    /// </summary>
    public class CollisionExclusion
    {

        #region Fields

        private IIgnorableCollision _collA;
        private IIgnorableCollision _collB;
        private bool _active;

        #endregion

        #region CONSTRUCTOR

        public CollisionExclusion(Collider a, Collider b)
        {
            if (a == null) throw new System.ArgumentNullException("a");
            if (b == null) throw new System.ArgumentNullException("b");

            _collA = IgnorableCollider.GetIgnorableCollision(a);
            _collB = IgnorableCollider.GetIgnorableCollision(b);
        }

        public CollisionExclusion(IIgnorableCollision a, IIgnorableCollision b)
        {
            if (a == null) throw new System.ArgumentNullException("a");
            if (b == null) throw new System.ArgumentNullException("b");

            _collA = a;
            _collB = b;
        }

        public CollisionExclusion(Collider a, IIgnorableCollision b)
        {
            if (a == null) throw new System.ArgumentNullException("a");
            if (b == null) throw new System.ArgumentNullException("b");

            _collA = IgnorableCollider.GetIgnorableCollision(a);
            _collB = b;
        }

        public CollisionExclusion(IIgnorableCollision a, Collider b)
        {
            if (a == null) throw new System.ArgumentNullException("a");
            if (b == null) throw new System.ArgumentNullException("b");

            _collA = a;
            _collB = IgnorableCollider.GetIgnorableCollision(b);
        }

        ~CollisionExclusion()
        {
            if (GameLoop.ApplicationClosing) return;

            //if (_active) PurgeWhenCan(_collA, _collB);
        }

        #endregion

        #region Properties

        public IIgnorableCollision ColliderA { get { return _collA; } }

        public IIgnorableCollision ColliderB { get { return _collB; } }

        public bool Active { get { return _active; } }

        #endregion

        #region Methods

        public void BeginExclusion()
        {
            if (_active) return;
            if (_collA.IsNullOrDestroyed() || _collB.IsNullOrDestroyed()) throw new System.InvalidOperationException("One or more referenced collider is null or destroyed.");

            var tagA = _collA.gameObject.AddOrGetComponent<CollisionExclussionTag>();
            var tagB = _collB.gameObject.AddOrGetComponent<CollisionExclussionTag>();
            tagA._exclusions.Add(this);
            tagB._exclusions.Add(this);

            _collA.IgnoreCollision(_collB, true);

            _active = true;
        }

        public void EndExclusion()
        {
            _collA?.gameObject?.GetComponent<CollisionExclussionTag>()?._exclusions.Remove(this);
            _collB?.gameObject?.GetComponent<CollisionExclussionTag>()?._exclusions.Remove(this);

            if (!_collA.IsNullOrDestroyed() && !_collB.IsNullOrDestroyed())
            {
                _collA.IgnoreCollision(_collB, false);
            }

            _active = false;
        }

        #endregion

        #region IDisposable Interface

        public void Dispose()
        {
            this.EndExclusion();
        }

        #endregion

        #region Special Types

        private class CollisionExclussionTag : MonoBehaviour
        {

            [System.NonSerialized]
            internal HashSet<CollisionExclusion> _exclusions = new HashSet<CollisionExclusion>();

        }

        #endregion

    }

}
