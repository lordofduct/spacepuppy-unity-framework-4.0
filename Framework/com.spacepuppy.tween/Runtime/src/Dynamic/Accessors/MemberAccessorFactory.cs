using System;
using System.Collections.Generic;
using System.Reflection;

using com.spacepuppy.Collections;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Dynamic.Accessors
{

    public class MemberAccessorFactory
    {

        #region Fields

        private LifetimeSet<MemberInfo> _pool = new LifetimeSet<MemberInfo>();

        #endregion

        #region Properties

        /// <summary>
        /// Max age of a cached member accessor.
        /// </summary>
        public TimeSpan MemberAccessMaxLifetime { get; set; }

        public bool PreferBasicMemberAccessor { get; set; }

        public bool ResolveIDynamicContracts { get; set; }

        #endregion

        #region Methods

        public virtual void RegisterPerminentlyCachedAccessor(MemberInfo memberInfo, IMemberAccessor accessor)
        {
            if (accessor == null) throw new ArgumentNullException(nameof(accessor));
            _pool.Add(memberInfo, accessor, TimeSpan.FromTicks(long.MaxValue));
        }

        public virtual void Purge()
        {
            _pool.Clear();
        }

        public virtual IMemberAccessor GetAccessor(object target, string memberName)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            if (this.ResolveIDynamicContracts && target is IDynamic dyntarget)
            {
                var info = dyntarget.GetMember(memberName, false);
                if (info != null)
                {
                    //we don't cache dynamic accessors since 2 different IDynamic's could have the same memberName but different returntypes. Would need to cache on both and that's cumbersome just to save a little gc
                    return new DynamicMemberAccessor(memberName, DynamicUtil.GetReturnType(info));
                }
            }

            return GetAccessor(target.GetType(), memberName);
        }

        public virtual IMemberAccessor GetAccessor(Type targetType, string memberName)
        {
            if (targetType == null) throw new System.ArgumentNullException(nameof(targetType));

            const MemberTypes MASK_MEMBERTYPES = MemberTypes.Field | MemberTypes.Property;
            const BindingFlags MASK_BINDINGS = BindingFlags.Public | BindingFlags.Instance;

            if (memberName.IndexOf('.') >= 0)
            {
                var arr = memberName.Split('.');
                using (var chainBuilder = TempCollection.GetQueue<IMemberAccessor>())
                {
                    for (int i = 0; i < arr.Length; i++)
                    {
                        var matches = targetType.GetMember(arr[i],
                                                           MASK_MEMBERTYPES,
                                                           MASK_BINDINGS);
                        if (matches == null || matches.Length == 0)
                            throw new ArgumentException(string.Format("Member \"{0}\" does not exist for type {1}.", memberName, targetType));

                        targetType = DynamicUtil.GetReturnType(matches[0]);
                        chainBuilder.Enqueue(GetAccessor(matches[0]));
                    }

                    //the currentObjectType value will be the type effectively being manipulated
                    IMemberAccessor accessor = chainBuilder.Dequeue();
                    while (chainBuilder.Count > 0)
                    {
                        accessor = new ChainingAccessor(chainBuilder.Dequeue(), accessor);
                    }
                    return accessor;
                }
            }
            else
            {
                var matches = targetType.GetMember(memberName,
                                                   MASK_MEMBERTYPES,
                                                   MASK_BINDINGS);
                if (matches == null || matches.Length == 0)
                    throw new ArgumentException(string.Format("Member \"{0}\" does not exist for type {1}.", memberName, targetType));

                return GetAccessor(matches[0]);
            }
        }

        public virtual IMemberAccessor GetAccessor(MemberInfo memberInfo)
        {
            if (memberInfo == null) throw new System.ArgumentNullException(nameof(memberInfo));

            object tag;
            if (_pool.TryGetTag(memberInfo, out tag))
            {
                return tag as IMemberAccessor;
            }
            else
            {
                IMemberAccessor result;
                if (PreferBasicMemberAccessor)
                {
                    if (memberInfo is PropertyInfo || memberInfo is FieldInfo)
                    {
                        result = new BasicMemberAccessor(memberInfo);
                    }
                    else
                    {
                        throw new System.ArgumentException("MemberInfo must be either a PropertyInfo or a FieldInfo.");
                    }
                }
                else
                {
                    if (memberInfo is PropertyInfo)
                    {
                        result = new PropertyAccessor(memberInfo as PropertyInfo);
                    }
                    else if (memberInfo is FieldInfo)
                    {
                        result = new FieldAccessor(memberInfo as FieldInfo);
                    }
                    else
                    {
                        throw new System.ArgumentException("MemberInfo must be either a PropertyInfo or a FieldInfo.");
                    }
                }

                _pool.Add(memberInfo, result, this.MemberAccessMaxLifetime);
                return result;
            }
        }

        #endregion

    }

}
