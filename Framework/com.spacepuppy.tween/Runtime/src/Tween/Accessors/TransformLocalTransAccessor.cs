using UnityEngine;

using com.spacepuppy.Geom;
using com.spacepuppy.Utils;
using com.spacepuppy.Dynamic.Accessors;

namespace com.spacepuppy.Tween.Accessors
{

    [CustomTweenMemberAccessor(typeof(GameObject), typeof(Trans), "*LocalTrans")]
    [CustomTweenMemberAccessor(typeof(Component), typeof(Trans), "*LocalTrans")]
    [CustomTweenMemberAccessor(typeof(IGameObjectSource), typeof(Trans), "*LocalTrans")]
    public class TransformLocalTransAccessor : ITweenMemberAccessorProvider, IMemberAccessor<Trans>
    {
        private static readonly TransformLocalTransAccessor IncludeScaleAccessor = new TransformLocalTransAccessor() { _includeScale = true };

        private bool _includeScale;

        #region ITweenMemberAccessor Interface

        string com.spacepuppy.Dynamic.Accessors.IMemberAccessor.GetMemberName()
        {
            return "*LocalTrans";
        }

        public System.Type GetMemberType()
        {
            return typeof(Trans);
        }

        public IMemberAccessor GetAccessor(object target, string propName, string args)
        {
            if (ConvertUtil.ToBool(args))
            {
                return IncludeScaleAccessor;
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
            this.Set(targ, Trans.Massage(valueObj));
        }

        public Trans Get(object target)
        {
            var trans = GameObjectUtil.GetTransformFromSource(target);
            if (trans != null)
            {
                return Trans.GetLocal(trans);
            }
            return Trans.Identity;
        }

        public void Set(object target, Trans value)
        {
            var trans = GameObjectUtil.GetTransformFromSource(target);
            if (trans != null)
            {
                value.SetToLocal(trans, _includeScale);
            }
        }

        #endregion

    }
}
