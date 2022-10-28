using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Utils;
using com.spacepuppy.Dynamic;

namespace com.spacepuppy.DataBinding
{

    public interface ISourceBindingProtocol
    {

        /// <summary>
        /// DataBindingContext will attempt to coerce the object into this type before calling GetValue.
        /// </summary>
        System.Type PreferredSourceType { get; }

        /// <summary>
        /// Returns a list of the keys that the protocol definitely supports. This is used to facilitate the ditor for the various ContentBinders.
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetDefinedKeys(DataBindingContext context);

        /// <summary>
        /// Return a value from a source for a key.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        object GetValue(DataBindingContext context, object source, string key);

    }

    public class StandardBindingProtocol : ISourceBindingProtocol
    {

        public static readonly StandardBindingProtocol Default = new StandardBindingProtocol();

        public virtual System.Type PreferredSourceType => null;

        public virtual IEnumerable<string> GetDefinedKeys(DataBindingContext context)
        {
            if (context.ConfiguredDataSource == null) return Enumerable.Empty<string>();

            if (context.RespectProxySources && context.ConfiguredDataSource is IProxy proxy)
            {
                return DynamicUtil.GetMembersFromType(proxy.GetTargetType() ?? typeof(object), false, System.Reflection.MemberTypes.Field | System.Reflection.MemberTypes.Property)
                                  .Where(o => IsAcceptableMemberType(DynamicUtil.GetReturnType(o)))
                                  .Select(o => o.Name);
            }
            else if (!context.RespectDataProviderAsSource && context.ConfiguredDataSource is IDataProvider dp)
            {
                return DynamicUtil.GetMembersFromType(dp.ElementType ?? typeof(object), false, System.Reflection.MemberTypes.Field | System.Reflection.MemberTypes.Property)
                                  .Where(o => IsAcceptableMemberType(DynamicUtil.GetReturnType(o)))
                                  .Select(o => o.Name);
            }
            else if (context.ConfiguredDataSource != null)
            {
                return DynamicUtil.GetMembers(context.ConfiguredDataSource, false, System.Reflection.MemberTypes.Field | System.Reflection.MemberTypes.Property)
                                  .Where(o => IsAcceptableMemberType(DynamicUtil.GetReturnType(o)))
                                  .Select(o => o.Name);
            }
            else
            {
                return Enumerable.Empty<string>();
            }
        }

        public virtual object GetValue(DataBindingContext context, object source, string key)
        {
            if (source is System.Collections.IDictionary dict && dict.Contains(key))
            {
                return dict[key];
            }
            else
            {
                return DynamicUtil.GetValue(source, key);
            }
        }

        protected virtual bool IsAcceptableKeyType(System.Type tp)
        {
            return IsAcceptableMemberType(tp);
        }

        #region Static Utils

        public static bool IsAcceptableMemberType(System.Type tp)
        {
            return VariantReference.AcceptableSerializableType(tp) || TypeUtil.IsType(tp, _acceptableTypes);
        }

#if SP_ADDRESSABLES
        private static readonly System.Type[] _acceptableTypes = new System.Type[] { typeof(UnityEngine.AddressableAssets.AssetReference), typeof(System.DateTime), typeof(System.DateTime?), typeof(System.TimeSpan), typeof(System.TimeSpan?), typeof(Sprite), typeof(System.IConvertible) };
#else
        private static readonly System.Type[] _acceptableTypes = new System.Type[] { typeof(System.DateTime), typeof(System.DateTime?), typeof(System.TimeSpan), typeof(System.TimeSpan?), typeof(Sprite), typeof(System.IConvertible) };
#endif

        #endregion

    }

    [System.Serializable]
    public class SourceBindingProtocol : StandardBindingProtocol
    {

        #region Fields

        [SerializeField]
        private TypeReference _sourceType;

        #endregion

        #region Properties

        /// <summary>
        /// The type expected when binding, this is not enforced particularly. It is used to coerce the bound source to the type expected by the binders, otherwise the property is just reflected.
        /// </summary>
        public System.Type SourceType
        {
            get => _sourceType.Type;
            set => _sourceType.Type = value;
        }

        #endregion

        #region Methods

        public override System.Type PreferredSourceType => this.SourceType;

        public override IEnumerable<string> GetDefinedKeys(DataBindingContext context)
        {
            var sourcetype = _sourceType.Type;
            if (sourcetype == null) return base.GetDefinedKeys(context);

            return DynamicUtil.GetMembersFromType(sourcetype, false, System.Reflection.MemberTypes.Field | System.Reflection.MemberTypes.Property)
                              .Where(o => this.IsAcceptableKeyType(DynamicUtil.GetReturnType(o)))
                              .Select(o => o.Name);
        }

        #endregion

    }

}
