using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.spacepuppy.Collections;
using com.spacepuppy.Tween.Accessors;
using com.spacepuppy.Tween.Curves;
using com.spacepuppy.Utils;
using com.spacepuppy.Dynamic.Accessors;

namespace com.spacepuppy.Tween
{
    public static class TweenCurveFactory
    {

        #region Static Fields

        private static Dictionary<System.Type, CreateCurveFactoryCallback> _memberTypeToCurveType = new Dictionary<System.Type, CreateCurveFactoryCallback>();

        #endregion

        #region Static Constructor

        static TweenCurveFactory()
        {
            RegisterTweenCurveGenerator(typeof(bool), CreateUninitializedBoolMemberCurve);
            RegisterTweenCurveGenerator(typeof(Color32), CreateUninitializedColor32MemberCurve);
            RegisterTweenCurveGenerator(typeof(Color), CreateUninitializedColorMemberCurve);
            RegisterTweenCurveGenerator(typeof(float), CreateUninitializedFloatMemberCurve);
            RegisterTweenCurveGenerator(typeof(double), CreateUninitializedDoubleMemberCurve);
            RegisterTweenCurveGenerator(typeof(decimal), CreateUninitializedDecimalMemberCurve);
            RegisterTweenCurveGenerator(typeof(sbyte), CreateUninitializedSByteMemberCurve);
            RegisterTweenCurveGenerator(typeof(int), CreateUninitializedIntMemberCurve);
            RegisterTweenCurveGenerator(typeof(long), CreateUninitializedLongMemberCurve);
            RegisterTweenCurveGenerator(typeof(byte), CreateUninitializedByteMemberCurve);
            RegisterTweenCurveGenerator(typeof(uint), CreateUninitializedUIntMemberCurve);
            RegisterTweenCurveGenerator(typeof(ulong), CreateUninitializedULongMemberCurve);
            RegisterTweenCurveGenerator(typeof(Vector2), CreateUninitializedVector2MemberCurve);
            RegisterTweenCurveGenerator(typeof(Vector3), CreateUninitializedVector3MemberCurve);
            RegisterTweenCurveGenerator(typeof(Vector4), CreateUninitializedVector4MemberCurve);
            RegisterTweenCurveGenerator(typeof(Quaternion), CreateUninitializedQuaternionMemberCurve);
            RegisterTweenCurveGenerator(typeof(string), CreateUninintializedStringCurve);
            RegisterTweenCurveGenerator(typeof(Rect), CreateUninitializedRectMemberCurve);
            RegisterTweenCurveGenerator(typeof(com.spacepuppy.Geom.Trans), CreateUninitializedTransMemberCurve);
            RegisterTweenCurveGenerator(typeof(INumeric), CreateUninitializedINumericMemberCurve);
            RegisterTweenCurveGenerator(typeof(FollowTargetPositionAccessor), (a, o) => new FollowTargetPositionCurve(a));
        }

        #endregion

        #region MemberCurve Constructors

        public static BoolMemberCurve CreateUninitializedBoolMemberCurve(IMemberAccessor accessor, int option)
        {
            return new BoolMemberCurve(accessor);
        }

        public static Color32MemberCurve CreateUninitializedColor32MemberCurve(IMemberAccessor accessor, int option)
        {
            return new Color32MemberCurve(accessor);
        }

        public static ColorMemberCurve CreateUninitializedColorMemberCurve(IMemberAccessor accessor, int option)
        {
            return new ColorMemberCurve(accessor);
        }

        public static FloatMemberCurve CreateUninitializedFloatMemberCurve(IMemberAccessor accessor, int option)
        {
            return new FloatMemberCurve(accessor);
        }

        public static NumericMemberCurve<double> CreateUninitializedDoubleMemberCurve(IMemberAccessor accessor, int option)
        {
            return new NumericMemberCurve<double>(accessor);
        }

        public static NumericMemberCurve<decimal> CreateUninitializedDecimalMemberCurve(IMemberAccessor accessor, int option)
        {
            return new NumericMemberCurve<decimal>(accessor);
        }

        public static NumericMemberCurve<sbyte> CreateUninitializedSByteMemberCurve(IMemberAccessor accessor, int option)
        {
            return new NumericMemberCurve<sbyte>(accessor);
        }

        public static NumericMemberCurve<int> CreateUninitializedIntMemberCurve(IMemberAccessor accessor, int option)
        {
            return new NumericMemberCurve<int>(accessor);
        }

        public static NumericMemberCurve<long> CreateUninitializedLongMemberCurve(IMemberAccessor accessor, int option)
        {
            return new NumericMemberCurve<long>(accessor);
        }

        public static NumericMemberCurve<byte> CreateUninitializedByteMemberCurve(IMemberAccessor accessor, int option)
        {
            return new NumericMemberCurve<byte>(accessor);
        }

        public static NumericMemberCurve<uint> CreateUninitializedUIntMemberCurve(IMemberAccessor accessor, int option)
        {
            return new NumericMemberCurve<uint>(accessor);
        }

        public static NumericMemberCurve<ulong> CreateUninitializedULongMemberCurve(IMemberAccessor accessor, int option)
        {
            return new NumericMemberCurve<ulong>(accessor);
        }

        public static MemberCurve<Vector2> CreateUninitializedVector2MemberCurve(IMemberAccessor accessor, int option)
        {
            return option == 0 ? (MemberCurve<Vector2>)new Vector2LerpMemberCurve(accessor) : (MemberCurve<Vector2>)new Vector2SlerpMemberCurve(accessor);
        }

        public static MemberCurve<Vector3> CreateUninitializedVector3MemberCurve(IMemberAccessor accessor, int option)
        {
            return option == 0 ? (MemberCurve<Vector3>)new Vector3LerpMemberCurve(accessor) : (MemberCurve<Vector3>)new Vector3SlerpMemberCurve(accessor);
        }

        public static MemberCurve<Vector4> CreateUninitializedVector4MemberCurve(IMemberAccessor accessor, int option)
        {
            return new Vector4MemberCurve(accessor);
        }

        public static QuaternionMemberCurve CreateUninitializedQuaternionMemberCurve(IMemberAccessor accessor, int option)
        {
            return new QuaternionMemberCurve(accessor);
        }

        public static StringCurve CreateUninintializedStringCurve(IMemberAccessor accessor, int option)
        {
            return new StringCurve(accessor);
        }

        public static RectMemberCurve CreateUninitializedRectMemberCurve(IMemberAccessor accessor, int option)
        {
            return new RectMemberCurve(accessor);
        }

        public static TransMemberCurve CreateUninitializedTransMemberCurve(IMemberAccessor accessor, int option)
        {
            return new TransMemberCurve(accessor);
        }

        public static NumericMemberCurve<INumeric> CreateUninitializedINumericMemberCurve(IMemberAccessor accessor, int option)
        {
            return new NumericMemberCurve<INumeric>(accessor);
        }

        #endregion

        #region MemberCurve Lookup

        public static void RegisterTweenCurveGenerator(System.Type associatedType, CreateCurveFactoryCallback callback)
        {
            if (associatedType == null) throw new System.ArgumentNullException(nameof(associatedType));
            if (callback == null) throw new System.ArgumentNullException(nameof(callback));

            _memberTypeToCurveType[associatedType] = callback;
        }

        public static CreateCurveFactoryCallback LookupTweenCurveGenerator(System.Type associatedType)
        {
            if (associatedType == null) throw new System.ArgumentNullException(nameof(associatedType));

            CreateCurveFactoryCallback callback;
            _memberTypeToCurveType.TryGetValue(associatedType, out callback);
            return callback;
        }

        public static CreateCurveFactoryCallback LookupTweenCurveGenerator(IMemberAccessor accessor)
        {
            if (accessor == null) throw new System.ArgumentNullException(nameof(accessor));

            CreateCurveFactoryCallback callback;
            var tp = accessor.GetType();
            if (_memberTypeToCurveType.TryGetValue(tp, out callback)) return callback;

            tp = accessor.GetMemberType();
            if (_memberTypeToCurveType.TryGetValue(tp, out callback)) return callback;

            if (typeof(INumeric).IsAssignableFrom(tp) && _memberTypeToCurveType.TryGetValue(typeof(INumeric), out callback)) return callback;

            return null;
        }

        public static TweenCurve CreateUnInitializedTweenCurve(IMemberAccessor accessor, int option)
        {
            var result = LookupTweenCurveGenerator(accessor)?.Invoke(accessor, option);
            return result ?? throw new System.InvalidOperationException("IMemberAccessor is for a member type that is not supported.");
        }

        #endregion

        #region Accessor Builders

        public static IMemberAccessor GetAccessor(object target, string propName, out System.Type memberType)
        {
            string args = null;
            if (propName != null)
            {
                int fi = propName.IndexOf("(");
                if (fi >= 0)
                {
                    int li = propName.LastIndexOf(")");
                    if (li < fi) li = propName.Length;
                    args = propName.Substring(fi + 1, li - fi - 1);
                    propName = propName.Substring(0, fi);
                }
            }

            ITweenMemberAccessor acc;
            if (CustomTweenMemberAccessorFactory.TryGetMemberAccessor(target, propName, out acc))
            {
                memberType = acc.Init(target, propName, args);
                return acc;
            }

            //return MemberAccessorPool.GetAccessor(target.GetType(), propName, out memberType);
            return MemberAccessorPool.GetDynamicAccessor(target, propName, out memberType);
        }

        public static IMemberAccessor GetAccessor(object target, string propName)
        {
            string args = null;
            if (propName != null)
            {
                int fi = propName.IndexOf("(");
                if (fi >= 0)
                {
                    int li = propName.LastIndexOf(")");
                    if (li < fi) li = propName.Length;
                    args = propName.Substring(fi + 1, li - fi - 1);
                    propName = propName.Substring(0, fi);
                }
            }

            ITweenMemberAccessor acc;
            System.Type memberType;
            if (CustomTweenMemberAccessorFactory.TryGetMemberAccessor(target, propName, out acc))
            {
                memberType = acc.Init(target, propName, args);
                return acc;
            }

            //return MemberAccessorPool.GetAccessor(target.GetType(), propName, out memberType);
            return MemberAccessorPool.GetDynamicAccessor(target, propName, out memberType);
        }

        public static IMemberAccessor<TProp> GetAccessor<T, TProp>(MemberGetter<T, TProp> getter, MemberSetter<T, TProp> setter) where T : class
        {
            return new GetterSetterMemberAccessor<T, TProp>(getter, setter);
        }

        #endregion

        #region MemberCurve Builders

        public static TweenCurve CreateFromTo(IMemberAccessor accessor, Ease ease, float dur, object start, object end, int option)
        {
            var result = CreateUnInitializedTweenCurve(accessor, option);
            (result as ISupportBoxedConfigurableTweenCurve)?.Configure(ease, dur, start, end, option);
            return result;
        }

        public static TweenCurve CreateTo(IMemberAccessor accessor, object target, Ease ease, float dur, object end, int option)
        {
            var result = CreateUnInitializedTweenCurve(accessor, option);
            var start = accessor.Get(target);
            (result as ISupportBoxedConfigurableTweenCurve)?.Configure(ease, dur, start, end, option);
            return result;
        }

        public static TweenCurve CreateFrom(IMemberAccessor accessor, object target, Ease ease, float dur, object start, int option)
        {
            var result = CreateUnInitializedTweenCurve(accessor, option);
            var end = accessor.Get(target);
            (result as ISupportBoxedConfigurableTweenCurve)?.Configure(ease, dur, start, end, option);
            return result;
        }

        public static TweenCurve CreateBy(IMemberAccessor accessor, object target, Ease ease, float dur, object amt, int option)
        {
            var result = CreateUnInitializedTweenCurve(accessor, option);
            var start = accessor.Get(target);
            var end = Sum(start, amt);
            (result as ISupportBoxedConfigurableTweenCurve)?.Configure(ease, dur, start, end, option);
            return result;
        }

        public static TweenCurve CreateRedirectTo(IMemberAccessor accessor, object target, Ease ease, float dur, object start, object end, int option)
        {
            var result = CreateUnInitializedTweenCurve(accessor, option);
            (result as ISupportBoxedConfigurableTweenCurve)?.ConfigureAsRedirectTo(ease, dur, start, end, option);
            return result;
        }

        public static TweenCurve CreateFromTo<TProp>(IMemberAccessor<TProp> accessor, Ease ease, float dur, TProp start, TProp end, int option)
        {
            var result = CreateUnInitializedTweenCurve(accessor, option);
            if (result is MemberCurve<TProp> memcurve)
                memcurve.Configure(ease, dur, start, end, option);
            else if (result is ISupportBoxedConfigurableTweenCurve boxcurve)
                boxcurve.Configure(ease, dur, start, end, option);
            return result;
        }

        public static TweenCurve CreateTo<TProp>(IMemberAccessor<TProp> accessor, object target, Ease ease, float dur, TProp end, int option)
        {
            var result = CreateUnInitializedTweenCurve(accessor, option);
            var start = accessor.Get(target);
            if (result is MemberCurve<TProp> memcurve)
                memcurve.Configure(ease, dur, start, end, option);
            else if (result is ISupportBoxedConfigurableTweenCurve boxcurve)
                boxcurve.Configure(ease, dur, start, end, option);
            return result;
        }

        public static TweenCurve CreateFrom<TProp>(IMemberAccessor<TProp> accessor, object target, Ease ease, float dur, TProp start, int option)
        {
            var result = CreateUnInitializedTweenCurve(accessor, option);
            var end = accessor.Get(target);
            if (result is MemberCurve<TProp> memcurve)
                memcurve.Configure(ease, dur, start, end, option);
            else if (result is ISupportBoxedConfigurableTweenCurve boxcurve)
                boxcurve.Configure(ease, dur, start, end, option);
            return result;
        }

        public static TweenCurve CreateBy<TProp>(IMemberAccessor<TProp> accessor, object target, Ease ease, float dur, TProp amt, int option)
        {
            var result = CreateUnInitializedTweenCurve(accessor, option);
            var start = accessor.Get(target);
            var end = Sum(start, amt);
            if (result is MemberCurve<TProp> memcurve)
                memcurve.Configure(ease, dur, start, end, option);
            else if (result is ISupportBoxedConfigurableTweenCurve boxcurve)
                boxcurve.Configure(ease, dur, start, end, option);
            return result;
        }

        public static TweenCurve CreateRedirectTo<TProp>(IMemberAccessor<TProp> accessor, object target, Ease ease, float dur, TProp start, TProp end, int option)
        {
            var result = CreateUnInitializedTweenCurve(accessor, option);
            if (result is MemberCurve<TProp> memcurve)
                memcurve.ConfigureAsRedirectTo(ease, dur, accessor.Get(target), start, end, option);
            else if (result is ISupportBoxedConfigurableTweenCurve boxcurve)
                boxcurve.ConfigureAsRedirectTo(ease, dur, accessor.Get(target), start, end, option);
            return result;
        }

        #endregion

        #region Utils

        static object TrySum(System.Type tp, object a, object b)
        {
            if (tp == null) return b;

            if (ConvertUtil.IsNumericType(tp))
            {
                return ConvertUtil.ToPrim(ConvertUtil.ToDouble(a) + ConvertUtil.ToDouble(b), tp);
            }
            else if (tp == typeof(Vector2))
            {
                return ConvertUtil.ToVector2(a) + ConvertUtil.ToVector2(b);
            }
            else if (tp == typeof(Vector3))
            {
                return ConvertUtil.ToVector3(a) + ConvertUtil.ToVector3(b);
            }
            else if (tp == typeof(Vector4))
            {
                return ConvertUtil.ToVector4(a) + ConvertUtil.ToVector4(b);
            }
            else if (tp == typeof(Quaternion))
            {
                return ConvertUtil.ToQuaternion(a) * ConvertUtil.ToQuaternion(b);
            }
            else if (tp == typeof(Color))
            {
                return ConvertUtil.ToColor(a) + ConvertUtil.ToColor(b);
            }
            else if (tp == typeof(Color32))
            {
                return ConvertUtil.ToColor32(ConvertUtil.ToColor(a) + ConvertUtil.ToColor(b));
            }
            else if (tp == typeof(Vector2Int))
            {
                return ConvertUtil.ToVector2Int(a) + ConvertUtil.ToVector2Int(b);
            }
            else if (tp == typeof(Vector3Int))
            {
                return ConvertUtil.ToVector3Int(a) + ConvertUtil.ToVector3Int(b);
            }

            return b;
        }

        static TProp Sum<TProp>(TProp a, TProp b)
        {
            var tp = typeof(TProp);

            object result;
            if (ConvertUtil.IsNumericType(tp))
            {
                result = ConvertUtil.ToDouble(a) + ConvertUtil.ToDouble(b);
            }
            else if (tp == typeof(Vector2))
            {
                result = ConvertUtil.ToVector2(a) + ConvertUtil.ToVector2(b);
            }
            else if (tp == typeof(Vector3))
            {
                result = ConvertUtil.ToVector3(a) + ConvertUtil.ToVector3(b);
            }
            else if (tp == typeof(Vector4))
            {
                result = ConvertUtil.ToVector4(a) + ConvertUtil.ToVector4(b);
            }
            else if (tp == typeof(Quaternion))
            {
                result = ConvertUtil.ToQuaternion(a) * ConvertUtil.ToQuaternion(b);
            }
            else if (tp == typeof(Color))
            {
                result = ConvertUtil.ToColor(a) + ConvertUtil.ToColor(b);
            }
            else if (tp == typeof(Color32))
            {
                result = ConvertUtil.ToColor32(ConvertUtil.ToColor(a) + ConvertUtil.ToColor(b));
            }
            else if (tp == typeof(Vector2Int))
            {
                result = ConvertUtil.ToVector2Int(a) + ConvertUtil.ToVector2Int(b);
            }
            else if (tp == typeof(Vector3Int))
            {
                result = ConvertUtil.ToVector3Int(a) + ConvertUtil.ToVector3Int(b);
            }
            else
            {
                return b;
            }

            return ConvertUtil.ToPrim<TProp>(result);
        }

        #endregion

    }

}
