
using com.spacepuppy.Utils;
using com.spacepuppy.Dynamic.Accessors;

namespace com.spacepuppy.Tween.Accessors
{

    [CustomTweenMemberAccessor(typeof(ITimeSupplier), typeof(float), "Scale")]
    public class TimeScaleMemberAccessor : ITweenMemberAccessorProvider, IMemberAccessor<float>
    {

        public const string DEFAULT_TIMESCALE_ID = "SPTween.TimeScale";

        #region Fields

        private string _id = DEFAULT_TIMESCALE_ID;

        #endregion

        #region ITweenMemberAccessor Interface

        string com.spacepuppy.Dynamic.Accessors.IMemberAccessor.GetMemberName()
        {
            return "Scale";
        }

        public System.Type GetMemberType()
        {
            return typeof(float);
        }

        public IMemberAccessor GetAccessor(object target, string propName, string args)
        {
            if (string.IsNullOrEmpty(args) || args == DEFAULT_TIMESCALE_ID)
            {
                return this;
            }
            else
            {
                return new TimeScaleMemberAccessor()
                {
                    _id = args
                };
            }
        }

        object IMemberAccessor.Get(object target)
        {
            return this.Get(target);
        }

        public void Set(object targ, object valueObj)
        {
            this.Set(targ, ConvertUtil.ToSingle(valueObj));
        }

        public float Get(object target)
        {
            if ((target is IScalableTimeSupplier supplier) && supplier.HasScale(_id))
            {
                return supplier.GetScale(_id);
            }
            return 1f;
        }

        public void Set(object target, float value)
        {
            var supplier = target as IScalableTimeSupplier;
            if (supplier != null)
            {
                supplier.SetScale(_id, value);
            }
        }

        #endregion

    }

}
