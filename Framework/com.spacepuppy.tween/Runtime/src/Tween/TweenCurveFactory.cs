using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy.Collections;
using com.spacepuppy.Dynamic;
using com.spacepuppy.Dynamic.Accessors;
using com.spacepuppy.Tween.Accessors;
using com.spacepuppy.Tween.Curves;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Tween
{
    public class TweenCurveFactory
    {

        #region MemberCurve Static Constructors

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
            switch ((VectorTweenOptions)option)
            {
                case VectorTweenOptions.Lerp:
                    return new Vector2LerpMemberCurve(accessor);
                case VectorTweenOptions.Slerp:
                    return new Vector2SlerpMemberCurve(accessor);
                case VectorTweenOptions.ScaleIn:
                    return new Vector2ScaleInMemberCurve(accessor);
                default:
                    return new Vector2LerpMemberCurve(accessor);
            }
        }

        public static MemberCurve<Vector3> CreateUninitializedVector3MemberCurve(IMemberAccessor accessor, int option)
        {
            switch((VectorTweenOptions)option)
            {
                case VectorTweenOptions.Lerp:
                    return new Vector3LerpMemberCurve(accessor);
                case VectorTweenOptions.Slerp:
                    return new Vector3SlerpMemberCurve(accessor);
                case VectorTweenOptions.ScaleIn:
                    return new Vector3ScaleInMemberCurve(accessor);
                default:
                    return new Vector3LerpMemberCurve(accessor);
            }
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

        public static FollowTargetPositionCurve CreateUninitializeFollowTargetPositionCurve(IMemberAccessor accessor, int option)
        {
            return new FollowTargetPositionCurve(accessor);
        }

        #endregion

        #region Fields

        private TweenMemberAccessorFactory _accessorFactory = new TweenMemberAccessorFactory();
        private ListDictionary<string, SpecialNameGeneratorInfo> _memberTypeNamePairToCurveType = new ListDictionary<string, SpecialNameGeneratorInfo>();
        private Dictionary<System.Type, ITweenCurveGenerator> _memberTypeToCurveType = new Dictionary<System.Type, ITweenCurveGenerator>();

        #endregion

        #region Constructor

        public TweenCurveFactory(bool donotRegisterDefaultGenerators = false)
        {
            _accessorFactory.MemberAccessMaxLifetime = System.TimeSpan.FromMinutes(5);
            _accessorFactory.ResolveIDynamicContracts = true;
            if (!donotRegisterDefaultGenerators)
            {
                this.Reset();
            }
        }

        #endregion

        #region Properties

        public TweenMemberAccessorFactory AccessorFactory { get { return _accessorFactory; } }

        #endregion

        #region Configuration

        public void Reset(bool donotRegisterDefaultGenerators = false)
        {
            _accessorFactory.Purge();
            _accessorFactory.MemberAccessMaxLifetime = System.TimeSpan.FromMinutes(5);
            _accessorFactory.ResolveIDynamicContracts = true;
            _memberTypeNamePairToCurveType.Clear();
            _memberTypeToCurveType.Clear();

            if (!donotRegisterDefaultGenerators)
            {
                //register fast accessors
                FindAccessor.RegisterFastAccessorsWithTweenCurveFactory(_accessorFactory);

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
                RegisterTweenCurveGenerator(typeof(object), FollowTargetPositionAccessor.PROP_NAME, new TweenCurveGenerator(CreateUninitializeFollowTargetPositionCurve, typeof(Transform)));

                //register special prop name associated accessors
                var groups = CustomTweenMemberAccessorAttribute.FindCustomTweenMemberAccessorProviderTypes()
                                                               .GroupBy(o => new { o.HandledPropName, o.HandledTargetType });
                foreach (var grp in groups)
                {
                    foreach (var attrib in grp.OrderByDescending(o => o.priority))
                    {
                        try
                        {
                            var acc = System.Activator.CreateInstance(attrib.DeclaringType) as ITweenMemberAccessorProvider;
                            if (acc != null)
                            {
                                _accessorFactory.RegisterMemberAccessorProvider(attrib.HandledTargetType, attrib.HandledPropName, acc);
                                break;
                            }
                        }
                        catch { }
                    }
                }
            }
        }

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
        public void RegisterTweenCurveGenerator(System.Type targetType, string memberName, ITweenCurveGenerator generator)
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
                for (int i = 0; i < lst.Count; i++)
                {
                    if (lst[i].TargetType == info.TargetType)
                    {
                        lst[i] = info;
                        return;
                    }

                    if (TypeUtil.IsType(info.TargetType, lst[i].TargetType))
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

        public void RegisterTweenCurveGenerator(System.Type memberType, ITweenCurveGenerator generator)
        {
            if (memberType == null) throw new System.ArgumentNullException(nameof(memberType));
            if (generator == null) throw new System.ArgumentNullException(nameof(generator));

            _memberTypeToCurveType[memberType] = generator;
        }

        #endregion

        #region MemberCurve Lookup

        public ITweenCurveGenerator LookupTweenCurveGenerator(System.Type targetType, string memberName, System.Type memberType)
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

        public ITweenCurveGenerator LookupTweenCurveGenerator(System.Type targetType, IMemberAccessor accessor)
        {
            if (accessor == null) throw new System.ArgumentNullException(nameof(accessor));

            return LookupTweenCurveGenerator(targetType, accessor.GetMemberName(), accessor.GetMemberType());
        }

        public TweenCurve CreateUnInitializedTweenCurve(System.Type targetType, IMemberAccessor accessor, int option)
        {
            var result = LookupTweenCurveGenerator(targetType, accessor)?.CreateCurve(accessor, option);
            return result ?? throw new System.InvalidOperationException("IMemberAccessor is for a member type that is not supported.");
        }

        #endregion

        #region Accessor Builders

        public IMemberAccessor GetAccessor(object target, string propName)
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

            return _accessorFactory.GetAccessor(target, propName, args);
        }

        #endregion

        #region MemberCurve Builders

        public TweenCurve CreateFromTo(object target, IMemberAccessor accessor, Ease ease, float dur, object start, object end, int option)
        {
            var result = CreateUnInitializedTweenCurve(target?.GetType(), accessor, option);
            (result as ISupportBoxedConfigurableTweenCurve)?.Configure(ease, dur, start, end, option);
            return result;
        }

        public TweenCurve CreateTo(object target, IMemberAccessor accessor, Ease ease, float dur, object end, int option)
        {
            var result = CreateUnInitializedTweenCurve(target?.GetType(), accessor, option);
            var start = accessor.Get(target);
            (result as ISupportBoxedConfigurableTweenCurve)?.Configure(ease, dur, start, end, option);
            return result;
        }

        public TweenCurve CreateFrom(object target, IMemberAccessor accessor, Ease ease, float dur, object start, int option)
        {
            var result = CreateUnInitializedTweenCurve(target?.GetType(), accessor, option);
            var end = accessor.Get(target);
            (result as ISupportBoxedConfigurableTweenCurve)?.Configure(ease, dur, start, end, option);
            return result;
        }

        public TweenCurve CreateBy(object target, IMemberAccessor accessor, Ease ease, float dur, object amt, int option)
        {
            var result = CreateUnInitializedTweenCurve(target?.GetType(), accessor, option);
            var start = accessor.Get(target);
            var end = TrySum(accessor.GetMemberType(), start, amt);
            (result as ISupportBoxedConfigurableTweenCurve)?.Configure(ease, dur, start, end, option);
            return result;
        }

        public TweenCurve CreateRedirectTo(object target, IMemberAccessor accessor, Ease ease, float dur, object start, object end, int option)
        {
            var result = CreateUnInitializedTweenCurve(target?.GetType(), accessor, option);
            (result as ISupportBoxedConfigurableTweenCurve)?.ConfigureAsRedirectTo(ease, dur, accessor.Get(target), start, end, option);
            return result;
        }

        public TweenCurve CreateFromTo<TProp>(object target, IMemberAccessor<TProp> accessor, Ease ease, float dur, TProp start, TProp end, int option)
        {
            var result = CreateUnInitializedTweenCurve(target?.GetType(), accessor, option);
            if (result is MemberCurve<TProp> memcurve)
                memcurve.Configure(ease, dur, start, end, option);
            else if (result is ISupportBoxedConfigurableTweenCurve boxcurve)
                boxcurve.Configure(ease, dur, start, end, option);
            return result;
        }

        public TweenCurve CreateTo<TProp>(object target, IMemberAccessor<TProp> accessor, Ease ease, float dur, TProp end, int option)
        {
            var result = CreateUnInitializedTweenCurve(target?.GetType(), accessor, option);
            var start = accessor.Get(target);
            if (result is MemberCurve<TProp> memcurve)
                memcurve.Configure(ease, dur, start, end, option);
            else if (result is ISupportBoxedConfigurableTweenCurve boxcurve)
                boxcurve.Configure(ease, dur, start, end, option);
            return result;
        }

        public TweenCurve CreateFrom<TProp>(object target, IMemberAccessor<TProp> accessor, Ease ease, float dur, TProp start, int option)
        {
            var result = CreateUnInitializedTweenCurve(target?.GetType(), accessor, option);
            var end = accessor.Get(target);
            if (result is MemberCurve<TProp> memcurve)
                memcurve.Configure(ease, dur, start, end, option);
            else if (result is ISupportBoxedConfigurableTweenCurve boxcurve)
                boxcurve.Configure(ease, dur, start, end, option);
            return result;
        }

        public TweenCurve CreateBy<TProp>(object target, IMemberAccessor<TProp> accessor, Ease ease, float dur, TProp amt, int option)
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

        public TweenCurve CreateRedirectTo<TProp>(object target, IMemberAccessor<TProp> accessor, Ease ease, float dur, TProp start, TProp end, int option)
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
                result = TrySum(a?.GetType() ?? b?.GetType(), a, b);
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

        public struct SpecialNameAccessorInfo
        {
            public System.Type TargetType;
            public string MemberName;
            public ITweenMemberAccessorProvider Provider;
        }

        public class TweenMemberAccessorFactory : MemberAccessorFactory
        {

            private ListDictionary<string, SpecialNameAccessorInfo> _specialMemberNameTable = new ListDictionary<string, SpecialNameAccessorInfo>();

            public override void Purge()
            {
                _specialMemberNameTable.Clear();
                base.Purge();
            }

            public void RegisterMemberAccessorProvider(System.Type targetType, string memberName, ITweenMemberAccessorProvider provider)
            {
                if (memberName == null) throw new System.ArgumentNullException(nameof(memberName)); //can't be null, that's reserved for the 'general' type registering
                if (provider == null) throw new System.ArgumentNullException(nameof(provider));

                var info = new SpecialNameAccessorInfo()
                {
                    TargetType = targetType ?? typeof(object),
                    MemberName = memberName,
                    Provider = provider
                };

                //the memberName is not present on the type, so we're going to register it as a special name
                IList<SpecialNameAccessorInfo> lst;
                if (_specialMemberNameTable.Lists.TryGetList(memberName, out lst))
                {
                    for (int i = 0; i < lst.Count; i++)
                    {
                        if (lst[i].TargetType == info.TargetType)
                        {
                            lst[i] = info;
                            return;
                        }

                        if (TypeUtil.IsType(info.TargetType, lst[i].TargetType))
                        {
                            lst.Insert(i, info);
                            return;
                        }
                    }

                    lst.Add(info);
                }
                else
                {
                    _specialMemberNameTable.Add(memberName, info);
                }
            }

            public IMemberAccessor GetAccessor(object target, string memberName, string args)
            {
                IList<SpecialNameAccessorInfo> lst;
                if (_specialMemberNameTable.Lists.TryGetList(memberName, out lst))
                {
                    for (int i = 0; i < lst.Count; i++)
                    {
                        if (lst[i].TargetType.IsInstanceOfType(target))
                        {
                            return lst[i].Provider.GetAccessor(target, memberName, args);
                        }
                    }
                }
                return base.GetAccessor(target, memberName);
            }
            public override IMemberAccessor GetAccessor(object target, string memberName)
            {
                IList<SpecialNameAccessorInfo> lst;
                if (_specialMemberNameTable.Lists.TryGetList(memberName, out lst))
                {
                    for (int i = 0; i < lst.Count; i++)
                    {
                        if (lst[i].TargetType.IsInstanceOfType(target))
                        {
                            return lst[i].Provider.GetAccessor(target, memberName, null);
                        }
                    }
                }
                return base.GetAccessor(target, memberName);
            }

            public bool TryGetMemberAccessorInfoByType(System.Type tp, string name, out SpecialNameAccessorInfo data)
            {
                if (tp == null)
                {
                    data = default(SpecialNameAccessorInfo);
                    return false;
                }

                IList<SpecialNameAccessorInfo> lst;
                if (_specialMemberNameTable.Lists.TryGetList(name, out lst))
                {
                    SpecialNameAccessorInfo d2;
                    int cnt = lst.Count;
                    for (int i = 0; i < cnt; i++)
                    {
                        d2 = lst[i];
                        if (d2.TargetType.IsAssignableFrom(tp))
                        {
                            data = d2;
                            return true;
                        }
                    }
                }

                data = default(SpecialNameAccessorInfo);
                return false;
            }


            public string[] GetCustomAccessorIds(System.Type tp, System.Func<SpecialNameAccessorInfo, bool> predicate = null)
            {
                if (tp == null) throw new System.ArgumentNullException(nameof(tp));

                using (var set = TempCollection.GetSet<string>())
                {
                    var e = _specialMemberNameTable.GetEnumerator();
                    while (e.MoveNext())
                    {
                        var lst = e.Current.Value;
                        for (int i = 0; i < lst.Count; i++)
                        {
                            if (lst[i].TargetType.IsAssignableFrom(tp) &&
                                (predicate == null || predicate(lst[i])))
                            {
                                set.Add(e.Current.Key);
                                break;
                            }
                        }
                    }

                    return set.ToArray();
                }
            }

        }

        #endregion

    }

}
