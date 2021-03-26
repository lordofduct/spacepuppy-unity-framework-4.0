using UnityEngine;

using com.spacepuppy.Geom;
using com.spacepuppy.Utils;
using com.spacepuppy.Dynamic.Accessors;

namespace com.spacepuppy.Tween.Accessors
{

    [CustomTweenMemberAccessor(typeof(GameObject), typeof(Trans), "*GlobalTrans")]
    [CustomTweenMemberAccessor(typeof(Component), typeof(Trans), "*GlobalTrans")]
    [CustomTweenMemberAccessor(typeof(IGameObjectSource), typeof(Trans), "*GlobalTrans")]
    [CustomTweenMemberAccessor(typeof(GameObject), typeof(Transform), "*GlobalTransform")]
    [CustomTweenMemberAccessor(typeof(Component), typeof(Transform), "*GlobalTransform")]
    [CustomTweenMemberAccessor(typeof(IGameObjectSource), typeof(Transform), "*GlobalTransform")]
    public class TransformGlobalTransAccessor : ITweenMemberAccessor, IMemberAccessor<Trans>
    {

        private bool _includeScale;

        #region ITweenMemberAccessor Interface

        string com.spacepuppy.Dynamic.Accessors.IMemberAccessor.GetMemberName()
        {
            return "*GlobalTransform";
        }

        public System.Type GetMemberType()
        {
            return typeof(Trans);
        }

        public System.Type Init(object target, string propName, string args)
        {
            _includeScale = ConvertUtil.ToBool(args);

            return typeof(Trans);
        }

        object IMemberAccessor.Get(object target)
        {
            return this.Get(target);
        }

        public void Set(object targ, object valueObj)
        {
            this.Set(targ, Trans.Massage(valueObj));
        }

        public Trans Get(object target)
        {
            var trans = GameObjectUtil.GetTransformFromSource(target);
            if (trans != null)
            {
                return Trans.GetGlobal(trans);
            }
            return Trans.Identity;
        }

        public void Set(object target, Trans value)
        {
            var trans = GameObjectUtil.GetTransformFromSource(target);
            if (trans != null)
            {
                value.SetToGlobal(trans, _includeScale);
            }
        }

        #endregion

    }
    
}
