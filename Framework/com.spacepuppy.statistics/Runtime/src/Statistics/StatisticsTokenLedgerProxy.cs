using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Statistics
{

    [UnityEngine.Scripting.Preserve]
    [CreateAssetMenu(fileName = "StatisticsTokenLedgerProxy", menuName = "Spacepuppy/Proxy/StatisticsTokenLedgerProxy")]
    public class StatisticsTokenLedgerProxy : ScriptableObject, com.spacepuppy.Dynamic.IDynamic
    {

        #region Fields

        [SerializeField]
        private string _category;

        #endregion

        #region IDynamic Interface

        public bool SetValue(string sMemberName, object value, params object[] index)
        {
            if (string.IsNullOrEmpty(sMemberName)) return false;
            Services.Get<IStatisticsTokenLedger>()?.SetStat(_category, ConvertUtil.ToDouble(value), sMemberName);
            return true;
        }

        public bool TryGetValue(string sMemberName, out object result, params object[] args)
        {
            result = null;
            if (string.IsNullOrEmpty(sMemberName)) return false;

            var ledger = Services.Get<IStatisticsTokenLedger>();
            if (ledger == null) return false;

            result = ledger.GetStat(_category, sMemberName);
            return true;
        }

        public System.Reflection.MemberInfo GetMember(string sMemberName, bool includeNonPublic)
        {
            if (string.IsNullOrEmpty(sMemberName)) throw new System.ArgumentException("member name must not be blank", nameof(sMemberName));
            return new com.spacepuppy.Dynamic.DynamicPropertyInfo(sMemberName, typeof(double));
        }

#if UNITY_EDITOR
        private com.spacepuppy.Dynamic.TypeAccessWrapper _klass = new spacepuppy.Dynamic.TypeAccessWrapper(TypeUtil.FindType("com.spacepuppyeditor.Statistics.StatisticsTokenLedgerCategories"));

        public IEnumerable<string> GetMemberNames(bool includeNonPublic)
        {
            return _klass.CallStaticMethod("GetTokenCategoryEntryValues", typeof(System.Func<string, IEnumerable<string>>), _category) as IEnumerable<string>;
        }

        public IEnumerable<System.Reflection.MemberInfo> GetMembers(bool includeNonPublic)
        {
            foreach (var sname in GetMemberNames(includeNonPublic))
            {
                yield return new com.spacepuppy.Dynamic.DynamicPropertyInfo(sname, typeof(StatisticsTokenLedgerProxy), typeof(double));
            }
        }
#else
        public IEnumerable<string> GetMemberNames(bool includeNonPublic)
        {
            return Enumerable.Empty<string>();
        }

        public IEnumerable<System.Reflection.MemberInfo> GetMembers(bool includeNonPublic)
        {
            return Enumerable.Empty<MemberInfo>();
        }
#endif

        public bool HasMember(string sMemberName, bool includeNonPublic)
        {
            return !string.IsNullOrEmpty(sMemberName);
        }

        public object InvokeMethod(string sMemberName, params object[] args)
        {
            throw new System.NotSupportedException();
        }

#endregion

    }

}
