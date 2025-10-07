using UnityEngine;

using com.spacepuppy.Utils;
using com.spacepuppy.Dynamic.Accessors;

namespace com.spacepuppy.Tween.Accessors
{

    [CustomTweenMemberAccessor(typeof(GameObject), typeof(Vector3), FollowTargetPositionAccessor.PROP_NAME)]
    [CustomTweenMemberAccessor(typeof(Component), typeof(Vector3), FollowTargetPositionAccessor.PROP_NAME)]
    [CustomTweenMemberAccessor(typeof(IGameObjectSource), typeof(Vector3), FollowTargetPositionAccessor.PROP_NAME)]
    [CustomTweenMemberAccessor(typeof(Rigidbody), typeof(Vector3), FollowTargetPositionAccessor.PROP_NAME)]
    public class FollowTargetPositionAccessor : ITweenMemberAccessorProvider, IMemberAccessor<Vector3>
    {

        public const string PROP_NAME = "*Follow";

        string com.spacepuppy.Dynamic.Accessors.IMemberAccessor.GetMemberName()
        {
            return PROP_NAME;
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
            var t = GameObjectUtil.GetTransformFromSource(target);
            if (t != null)
            {
                return t.position;
            }
            return Vector3.zero;
        }


        public void Set(object targ, Vector3 value)
        {
            if (targ is Rigidbody rb)
            {
                rb.MovePosition(value - rb.position);
            }
            else
            {
                var trans = GameObjectUtil.GetTransformFromSource(targ);
                if (trans == null) return;

                rb = trans.GetComponent<Rigidbody>();
                if (rb != null && !rb.isKinematic)
                {
                    var dp = value - rb.position;
#if UNITY_2023_3_OR_NEWER
                    rb.linearVelocity = dp / Time.fixedDeltaTime;
#else
                    rb.velocity = dp / Time.fixedDeltaTime;
#endif
                    return;
                }

                //just update the position
                trans.position = value;
            }
        }


    }

}
