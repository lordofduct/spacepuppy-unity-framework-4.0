using UnityEngine;

using com.spacepuppy.Utils;
using com.spacepuppy.Dynamic.Accessors;

namespace com.spacepuppy.Tween.Accessors
{

    [CustomTweenMemberAccessor(typeof(Rigidbody), typeof(Vector3), "MovePosition")]
    public class RigidbodyMovePositionAccessor : ITweenMemberAccessorProvider, IMemberAccessor<Vector3>
    {

        #region ITweenMemberAccessor Interface

        string com.spacepuppy.Dynamic.Accessors.IMemberAccessor.GetMemberName()
        {
            return "MovePosition";
        }

        public System.Type GetMemberType()
        {
            return typeof(Vector3);
        }

        public IMemberAccessor GetAccessor(object target, string propName, string args)
        {
            return this;
        }

        object IMemberAccessor.Get(object target)
        {
            return this.Get(target);
        }

        public void Set(object targ, object valueObj)
        {
            this.Set(targ, ConvertUtil.ToVector3(valueObj));
        }

        public Vector3 Get(object target)
        {
            if (target is Rigidbody rb)
            {
                return rb.position;
            }
            return Vector3.zero;
        }

        public void Set(object target, Vector3 value)
        {
            if (target is Rigidbody rb)
            {
                rb.MovePosition(value - rb.position);
            }
        }

        #endregion

    }
}
