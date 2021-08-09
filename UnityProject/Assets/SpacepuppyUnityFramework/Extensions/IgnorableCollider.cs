using UnityEngine;

using com.spacepuppy.Utils;

namespace com.spacepuppy
{

    public class IgnorableCollider : SPComponent, IIgnorableCollision
    {

        #region Fields

        [SerializeField]
        [DefaultFromSelf()]
        private Collider _collider;

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

            using(var lst = com.spacepuppy.Collections.TempCollection.GetList<IgnorableCollider>())
            {
                coll.GetComponents<IgnorableCollider>(lst);
                
                if(lst.Count > 0)
                {
                    for(int i = 0; i < lst.Count; i++)
                    {
                        if (lst[i].Collider == coll) return lst[i];
                    }
                }

                var result = coll.AddComponent<IgnorableCollider>();
                result._collider = coll;
                return result;
            }
        }

        #endregion

    }

}