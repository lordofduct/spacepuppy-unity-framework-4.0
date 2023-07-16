using UnityEngine;

using com.spacepuppy.Geom;
using com.spacepuppy.Utils;
using com.spacepuppy.Dynamic.Accessors;

namespace com.spacepuppy.Tween.Accessors
{

    [CustomTweenMemberAccessor(typeof(GameObject), typeof(Vector3), "*relativePosition")]
    [CustomTweenMemberAccessor(typeof(Transform), typeof(Vector3), "*relativePosition")]
    [CustomTweenMemberAccessor(typeof(IGameObjectSource), typeof(Vector3), "*relativePosition")]
    public class TransformRelativePositionAccessor : ITweenMemberAccessorProvider, IMemberAccessor<Vector3>
    {

        private Trans _initialTrans = Trans.Identity;

        #region ITweenMemberAccessor Interface

        string com.spacepuppy.Dynamic.Accessors.IMemberAccessor.GetMemberName()
        {
            return "*relativePosition";
        }

        public System.Type GetMemberType()
        {
            return typeof(Vector3);
        }

        public IMemberAccessor GetAccessor(object target, string propName, string args)
        {
            var trans = GameObjectUtil.GetTransformFromSource(target);
            if (trans != null)
            {
                return new TransformRelativePositionAccessor()
                {
                    _initialTrans = Trans.GetGlobal(trans)
                };
            }
            else
            {
                return this;
            }
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
            var trans = GameObjectUtil.GetTransformFromSource(target);
            if (trans != null)
            {
                return _initialTrans.InverseTransformPoint(trans.position);
            }

            return Vector3.zero;
        }

        public void Set(object target, Vector3 value)
        {
            var trans = GameObjectUtil.GetTransformFromSource(target);
            if (trans != null)
            {
                trans.position = _initialTrans.TransformPoint(value);
            }
        }

        #endregion

    }
}
