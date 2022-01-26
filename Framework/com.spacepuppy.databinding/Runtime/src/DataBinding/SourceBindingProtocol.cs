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

        IEnumerable<string> GetDefinedKeys();
        object GetValue(object source, string key);

    }

    public class StandardBindingProtocol : ISourceBindingProtocol
    {

        public static readonly StandardBindingProtocol Default = new StandardBindingProtocol();

        public virtual IEnumerable<string> GetDefinedKeys()
        {
            return Enumerable.Empty<string>();
        }

        public virtual object GetValue(object source, string key)
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

        public override IEnumerable<string> GetDefinedKeys()
        {
            var sourcetype = _sourceType.Type;
            if (sourcetype == null) return base.GetDefinedKeys();

            return DynamicUtil.GetMembersFromType(sourcetype, false, System.Reflection.MemberTypes.Field | System.Reflection.MemberTypes.Property)
                              .Where(o => this.IsAcceptableKeyType(DynamicUtil.GetReturnType(o)))
                              .Select(o => o.Name);
        }

        protected virtual bool IsAcceptableKeyType(System.Type tp)
        {
            return IsAcceptableMemberType(tp);
        }

        #endregion

        #region Static Utils

        public static bool IsAcceptableMemberType(System.Type tp)
        {
            return VariantReference.AcceptableSerializableType(tp) || TypeUtil.IsType(tp, _acceptableTypes);
        }

#if SP_ADDRESSABLES
        private static readonly System.Type[] _acceptableTypes = new System.Type[] { typeof(UnityEngine.AddressableAssets.AssetReference), typeof(System.DateTime), typeof(System.DateTime?), typeof(Sprite) };
#else
        private static readonly System.Type[] _acceptableTypes = new System.Type[] { typeof(System.DateTime), typeof(System.DateTime?), typeof(Sprite) };
#endif

        #endregion

    }
}
