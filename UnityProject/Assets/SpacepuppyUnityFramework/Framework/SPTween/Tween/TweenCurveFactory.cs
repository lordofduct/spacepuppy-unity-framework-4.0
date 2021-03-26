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

        private static ListDictionary<string, SpecialNameGeneratorInfo> _memberTypeNamePairToCurveType = new ListDictionary<string, SpecialNameGeneratorInfo>();
        private static Dictionary<System.Type, ITweenCurveGenerator> _memberTypeToCurveType = new Dictionary<System.Type, ITweenCurveGenerator>();

        #endregion

        #region Static Constructor

        static TweenCurveFactory()
        {
            //register member type generators
            RegisterTweenCurveGenerator(typeof(bool), new TweenCurveGenerator(CreateUninitializedBoolMemberCurve, typeof(bool)));
            RegisterTweenCurveGenerator(typeof(Color32), new TweenCurveGenerator(CreateUninitializedColor32MemberCurve, typeof(Color32)));
            RegisterTweenCurveGenerator(typeof(Color), new TweenCurveGenerator(CreateUninitializedColorMemberCurve, typeof(Color), typeof(VectorTweenOptions)));
            RegisterTweenCurveGenerator(typeof(float), new TweenCurveGenerator(CreateUninitializedFloatMemberCurve, typeof(float)));
            RegisterTweenCurveGenerator(typeof(double), new TweenCurveGenerator(CreateUninitializedDoubleMemberCurve, typeof(double)));
            RegisterTweenCurveGenerator(typeof(decimal), new TweenCurveGenerator(CreateUninitializedDecimalMemberCurve, typeof(decimal)));
            RegisterTweenCurveGenerator(typeof(sbyte), new TweenCurveGenerator(CreateUninitializedSByteMemberCurve, typeof(sbyte)));
            RegisterTweenCurveGenerator(typeof(int), new TweenCurveGenerator(CreateUninitializedIntMemberCurve, typeof(int)));
            RegisterTweenCurveGenerator(typeof(long), new TweenCurveGenerator(CreateUninitializedLongMemberCurve, typeof(long)));
            RegisterTweenCurveGenerator(typeof(byte), new TweenCurveGenerator(CreateUninitializedByteMemberCurve, typeof(byte)));
            RegisterTweenCurveGenerator(typeof(uint), new TweenCurveGenerator(CreateUninitializedUIntMemberCurve, typeof(uint)));
            RegisterTweenCurveGenerator(typeof(ulong), new TweenCurveGenerator(CreateUninitializedULongMemberCurve, typeof(ulong)));
            RegisterTweenCurveGenerator(typeof(Vector2), new TweenCurveGenerator(CreateUninitializedVector2MemberCurve, typeof(Vector2), typeof(VectorTweenOptions)));
            RegisterTweenCurveGenerator(typeof(Vector3), new TweenCurveGenerator(CreateUninitializedVector3MemberCurve, typeof(Vector3), typeof(VectorTweenOptions)));
            RegisterTweenCurveGenerator(typeof(Vector4), new TweenCurveGenerator(CreateUninitializedVector4MemberCurve, typeof(Vector4)));
            RegisterTweenCurveGenerator(typeof(Quaternion), new QuaternionMemberCurveGenerator());
            RegisterTweenCurveGenerator(typeof(string), new TweenCurveGenerator(CreateUninintializedStringCurve, typeof(string), typeof(StringTweenStyle)));
            RegisterTweenCurveGenerator(typeof(Rect), new TweenCurveGenerator(CreateUninitializedRectMemberCurve, typeof(Rect)));
            RegisterTweenCurveGenerator(typeof(com.spacepuppy.Geom.Trans), new TweenCurveGenerator(CreateUninitializedTransMemberCurve, typeof(com.spacepuppy.Geom.Trans), typeof(VectorTweenOptions)));
            RegisterTweenCurveGenerator(typeof(INumeric), new TweenCurveGenerator(CreateUninitializedINumericMemberCurve, typeof(INumeric)));

            //register special prop name associated generators
            RegisterTweenCurveGenerator(typeof(object), FollowTargetPositionAccessor.PROP_NAME, new TweenCurveGenerator((a, o) => new FollowTargetPositionCurve(a), typeof(Transform)));
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

        /// <summary>
        /// Register an ITweenCurveGenerator for a specific property name. 
        /// </summary>
        /// <remarks>
        /// This is generally used for special CustomTweenMemberAccessors who generally prefix their property name with an *. 
        /// For example the '*Follow' for the FollowTargetPositionAccessor and FollowTargetPositionCurve register here with '*Follow'. 
        /// If you register with a common property name like say 'position' this will override ANY usage of that property by string. 
        /// This generally is not desired. 
        /// </remarks>
        /// <param name="targetType">The targetType with the property by name, this can be an inherited type including System.Object (if left null, object is selected)</param>
        /// <param name="memberName">The name of the member.</param>
        /// <param name="generator">The generator that will be used to create the curve.</param>
        public static void RegisterTweenCurveGenerator(System.Type targetType, string memberName, ITweenCurveGenerator generator)
        {
            if (memberName == null) throw new System.ArgumentNullException(nameof(memberName)); //can't be null, that's reserved for the 'general' type registering
            if (generator == null) throw new System.ArgumentNullException(nameof(generator));

            var info = new SpecialNameGeneratorInfo()
            {
                TargetType = targetType ?? typeof(object),
                MemberName = memberName,
                Generator = generator
            };

            IList<SpecialNameGeneratorInfo> lst;
            if (_memberTypeNamePairToCurveType.Lists.TryGetList(memberName, out lst))
            {
                for(int i = 0; i < lst.Count; i++)
                {
                    if (lst[i].TargetType == info.TargetType)
                    {
                        lst[i] = info;
                        return;
                    }

                    if(TypeUtil.IsType(info.TargetType, lst[i].TargetType))
                    {
                        lst.Insert(i, info);
                        return;
                    }
                }

                lst.Add(info);
            }
            else
            {
                _memberTypeNamePairToCurveType.Add(memberName, info);
            }
        }

        public static void RegisterTweenCurveGenerator(System.Type memberType, ITweenCurveGenerator generator)
        {
            if (memberType == null) throw new System.ArgumentNullException(nameof(memberType));
            if (generator == null) throw new System.ArgumentNullException(nameof(generator));

            _memberTypeToCurveType[memberType] = generator;
        }

        public static ITweenCurveGenerator LookupTweenCurveGenerator(System.Type targetType, string memberName, System.Type memberType)
        {
            ITweenCurveGenerator callback;
            if (memberName != null)
            {
                IList<SpecialNameGeneratorInfo> lst;
                if(_memberTypeNamePairToCurveType.Lists.TryGetList(memberName, out lst))
                {
                    if (targetType == null) targetType = typeof(object);

                    for (int i = 0; i < lst.Count; i++)
                    {
                        if (TypeUtil.IsType(targetType, lst[i].TargetType)) return lst[i].Generator;
                    }
                }
            }

            if (memberType != null)
            {
                if (_memberTypeToCurveType.TryGetValue(memberType, out callback))
                    return callback;

                if (typeof(INumeric).IsAssignableFrom(memberType) && memberType != typeof(INumeric))
                    return LookupTweenCurveGenerator(targetType, memberName, typeof(INumeric));
            }

            return null;
        }

        public static ITweenCurveGenerator LookupTweenCurveGenerator(System.Type targetType, IMemberAccessor accessor)
        {
            if (accessor == null) throw new System.ArgumentNullException(nameof(accessor));

            return LookupTweenCurveGenerator(targetType, accessor.GetMemberName(), accessor.GetMemberType());
        }

        public static TweenCurve CreateUnInitializedTweenCurve(System.Type targetType, IMemberAccessor accessor, int option)
        {
            var result = LookupTweenCurveGenerator(targetType, accessor)?.CreateCurve(accessor, option);
            return result ?? throw new System.InvalidOperationException("IMemberAccessor is for a member type that is not supported.");
        }

        #endregion

        #region Accessor Builders

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

        public static TweenCurve CreateFromTo(object target, IMemberAccessor accessor, Ease ease, float dur, object start, object end, int option)
        {
            var result = CreateUnInitializedTweenCurve(target?.GetType(), accessor, option);
            (result as ISupportBoxedConfigurableTweenCurve)?.Configure(ease, dur, start, end, option);
            return result;
        }

        public static TweenCurve CreateTo(object target, IMemberAccessor accessor, Ease ease, float dur, object end, int option)
        {
            var result = CreateUnInitializedTweenCurve(target?.GetType(), accessor, option);
            var start = accessor.Get(target);
            (result as ISupportBoxedConfigurableTweenCurve)?.Configure(ease, dur, start, end, option);
            return result;
        }

        public static TweenCurve CreateFrom(object target, IMemberAccessor accessor, Ease ease, float dur, object start, int option)
        {
            var result = CreateUnInitializedTweenCurve(target?.GetType(), accessor, option);
            var end = accessor.Get(target);
            (result as ISupportBoxedConfigurableTweenCurve)?.Configure(ease, dur, start, end, option);
            return result;
        }

        public static TweenCurve CreateBy(object target, IMemberAccessor accessor, Ease ease, float dur, object amt, int option)
        {
            var result = CreateUnInitializedTweenCurve(target?.GetType(), accessor, option);
            var start = accessor.Get(target);
            var end = Sum(start, amt);
            (result as ISupportBoxedConfigurableTweenCurve)?.Configure(ease, dur, start, end, option);
            return result;
        }

        public static TweenCurve CreateRedirectTo(object target, IMemberAccessor accessor, Ease ease, float dur, object start, object end, int option)
        {
            var result = CreateUnInitializedTweenCurve(target?.GetType(), accessor, option);
            (result as ISupportBoxedConfigurableTweenCurve)?.ConfigureAsRedirectTo(ease, dur, start, end, option);
            return result;
        }

        public static TweenCurve CreateFromTo<TProp>(object target, IMemberAccessor<TProp> accessor, Ease ease, float dur, TProp start, TProp end, int option)
        {
            var result = CreateUnInitializedTweenCurve(target?.GetType(), accessor, option);
            if (result is MemberCurve<TProp> memcurve)
                memcurve.Configure(ease, dur, start, end, option);
            else if (result is ISupportBoxedConfigurableTweenCurve boxcurve)
                boxcurve.Configure(ease, dur, start, end, option);
            return result;
        }

        public static TweenCurve CreateTo<TProp>(object target, IMemberAccessor<TProp> accessor, Ease ease, float dur, TProp end, int option)
        {
            var result = CreateUnInitializedTweenCurve(target?.GetType(), accessor, option);
            var start = accessor.Get(target);
            if (result is MemberCurve<TProp> memcurve)
                memcurve.Configure(ease, dur, start, end, option);
            else if (result is ISupportBoxedConfigurableTweenCurve boxcurve)
                boxcurve.Configure(ease, dur, start, end, option);
            return result;
        }

        public static TweenCurve CreateFrom<TProp>(object target, IMemberAccessor<TProp> accessor, Ease ease, float dur, TProp start, int option)
        {
            var result = CreateUnInitializedTweenCurve(target?.GetType(), accessor, option);
            var end = accessor.Get(target);
            if (result is MemberCurve<TProp> memcurve)
                memcurve.Configure(ease, dur, start, end, option);
            else if (result is ISupportBoxedConfigurableTweenCurve boxcurve)
                boxcurve.Configure(ease, dur, start, end, option);
            return result;
        }

        public static TweenCurve CreateBy<TProp>(object target, IMemberAccessor<TProp> accessor, Ease ease, float dur, TProp amt, int option)
        {
            var result = CreateUnInitializedTweenCurve(target?.GetType(), accessor, option);
            var start = accessor.Get(target);
            var end = Sum(start, amt);
            if (result is MemberCurve<TProp> memcurve)
                memcurve.Configure(ease, dur, start, end, option);
            else if (result is ISupportBoxedConfigurableTweenCurve boxcurve)
                boxcurve.Configure(ease, dur, start, end, option);
            return result;
        }

        public static TweenCurve CreateRedirectTo<TProp>(object target, IMemberAccessor<TProp> accessor, Ease ease, float dur, TProp start, TProp end, int option)
        {
            var result = CreateUnInitializedTweenCurve(target?.GetType(), accessor, option);
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


        #region Special Types

        private struct SpecialNameGeneratorInfo
        {
            public System.Type TargetType;
            public string MemberName;
            public ITweenCurveGenerator Generator;
        }

        #endregion

    }

}
