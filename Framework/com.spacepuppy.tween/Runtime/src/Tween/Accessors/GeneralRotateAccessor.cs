using UnityEngine;

using com.spacepuppy.Utils;
using com.spacepuppy.Dynamic.Accessors;

namespace com.spacepuppy.Tween.Accessors
{

    [CustomTweenMemberAccessor(typeof(GameObject), typeof(Quaternion), "*Rotate")]
    [CustomTweenMemberAccessor(typeof(Component), typeof(Quaternion), "*Rotate")]
    [CustomTweenMemberAccessor(typeof(IGameObjectSource), typeof(Quaternion), "*Rotate")]
    [CustomTweenMemberAccessor(typeof(Rigidbody), typeof(Quaternion), "*Rotate")]
    public class GeneralRotateAccessor : ITweenMemberAccessorProvider, IMemberAccessor<Quaternion>
    {

        #region ImplicitCurve Interface

        string com.spacepuppy.Dynamic.Accessors.IMemberAccessor.GetMemberName()
        {
            return "*Rotate";
        }

        public System.Type GetMemberType()
        {
            return typeof(Quaternion);
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
            this.Set(targ, QuaternionUtil.MassageAsQuaternion(valueObj));
        }

        public Quaternion Get(object target)
        {
            var t = GameObjectUtil.GetTransformFromSource(target);
            if (t != null)
            {
                return t.rotation;
            }
            return Quaternion.identity;
        }

        public void Set(object targ, Quaternion value)
        {
            if (targ is Rigidbody rb)
            {
                rb.MoveRotation(QuaternionUtil.FromToRotation(rb.rotation, value));
            }
            else
            {
                var trans = GameObjectUtil.GetTransformFromSource(targ);
                if (trans == null) return;

                rb = trans.GetComponent<Rigidbody>();
                if (rb != null && !rb.isKinematic)
                {
                    rb.MoveRotation(QuaternionUtil.FromToRotation(rb.rotation, value));
                    return;
                }

                //just update the rotation
                trans.rotation = value;
            }
        }

        #endregion

    }
}
