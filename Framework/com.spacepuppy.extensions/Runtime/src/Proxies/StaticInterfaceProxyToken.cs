using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.spacepuppy.Dynamic;
using com.spacepuppy.Utils;

namespace com.spacepuppy
{

    [CreateAssetMenu(fileName = "MemberProxy", menuName = "Spacepuppy/Proxy/StaticInterfaceProxy")]
    public class StaticInterfaceProxyToken : ScriptableObject, IDynamic
    {

        #region Fields

        [SerializeField]
        private TypeReference _type = new TypeReference();

        #endregion

        #region Properties

        public System.Type Type
        {
            get => _type.Type;
            set => _type.Type = value;
        }

        #endregion

        #region IDynamic Interface

        public MemberInfo GetMember(string sMemberName, bool includeNonPublic)
        {
            var flags = BindingFlags.Static | BindingFlags.Public;
            if (includeNonPublic) flags |= BindingFlags.NonPublic;
            return _type.Type?.GetMember(sMemberName, flags).FirstOrDefault();
        }

        public IEnumerable<string> GetMemberNames(bool includeNonPublic)
        {
            var flags = BindingFlags.Static | BindingFlags.Public;
            if (includeNonPublic) flags |= BindingFlags.NonPublic;
            return _type.Type?.GetMembers(flags).Select(o => o.Name) ?? Enumerable.Empty<string>();
        }

        public IEnumerable<MemberInfo> GetMembers(bool includeNonPublic)
        {
            var flags = BindingFlags.Static | BindingFlags.Public;
            if (includeNonPublic) flags |= BindingFlags.NonPublic;
            return _type.Type?.GetMembers(flags) ?? Enumerable.Empty<MemberInfo>();
        }

        public bool HasMember(string sMemberName, bool includeNonPublic)
        {
            return this.GetMember(sMemberName, includeNonPublic) != null;
        }

        public object InvokeMethod(string sMemberName, params object[] args)
        {
            try
            {
                var m = this.GetMember(sMemberName, true);
                switch(m)
                {
                    case MethodInfo meth:
                        return meth.Invoke(null, args);
                    default:
                        return null;
                }
            }
            catch
            {
                return null;
            }
        }

        public bool SetValue(string sMemberName, object value, params object[] index)
        {
            try
            {
                var m = this.GetMember(sMemberName, true);
                switch (m)
                {
                    case MethodInfo meth:
                        meth.Invoke(null, new object[] { value });
                        return true;
                    case PropertyInfo prop:
                        prop.SetValue(null, value, index);
                        return true;
                    case FieldInfo field:
                        field.SetValue(null, value);
                        return true;
                    default:
                        return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool TryGetValue(string sMemberName, out object result, params object[] args)
        {
            result = null;
            try
            {
                var m = this.GetMember(sMemberName, true);
                switch (m)
                {
                    case MethodInfo meth:
                        result = meth.Invoke(null, args);
                        return true;
                    case PropertyInfo prop:
                        result = prop.GetValue(null, args);
                        return true;
                    case FieldInfo field:
                        result = field.GetValue(null);
                        return true;
                    default:
                        return false;
                }
            }
            catch
            {
                return false;
            }
        }

        #endregion

    }

}
