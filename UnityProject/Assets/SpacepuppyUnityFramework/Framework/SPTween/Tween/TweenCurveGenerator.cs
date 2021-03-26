using com.spacepuppy.Dynamic.Accessors;

namespace com.spacepuppy.Tween
{

    public interface ITweenCurveGenerator
    {
        TweenCurve CreateCurve(IMemberAccessor accessor, int option);
        /// <summary>
        /// Return the type of enum the option is, or null if option is unused.
        /// </summary>
        /// <returns></returns>
        System.Type GetOptionEnumType();

        System.Type GetExpectedMemberType(int option);
    }

    public sealed class TweenCurveGenerator : ITweenCurveGenerator
    {

        private System.Func<IMemberAccessor, int, TweenCurve> _callback;
        private System.Type _expectedMemberType;
        private System.Type _optionEnumType;

        public TweenCurveGenerator(System.Func<IMemberAccessor, int, TweenCurve> callback, System.Type expectedMemberType, System.Type optionEnumType = null)
        {
            if (callback == null) throw new System.ArgumentNullException(nameof(callback));
            if (expectedMemberType == null) throw new System.ArgumentNullException(nameof(expectedMemberType));
            if (optionEnumType != null && !optionEnumType.IsEnum) throw new System.ArgumentException("OptionEnumType must be an enum type.", nameof(optionEnumType));
            _callback = callback;
            _expectedMemberType = expectedMemberType;
            _optionEnumType = optionEnumType;
        }

        public TweenCurve CreateCurve(IMemberAccessor accessor, int option)
        {
            return _callback(accessor, option);
        }

        public System.Type GetOptionEnumType()
        {
            return _optionEnumType;
        }

        public System.Type GetExpectedMemberType(int option)
        {
            return _expectedMemberType;
        }

    }

}
