using UnityEngine;
using System.Collections.Generic;

namespace com.spacepuppy.Statistics
{

    public class TokenLedgerCategorySelectorAttribute : PropertyAttribute
    {
        public bool HideCustom;
    }

    public class TokenLedgerCategoryEntrySelectorAttribute : PropertyAttribute
    {

        public string CategoryFilter;
        public bool HideCustom;
        public TokenLedgerCategoryEntrySelectorAttribute(string categoryFilter)
        {
            this.CategoryFilter = categoryFilter;
        }

    }

    [System.Serializable]
    public struct StatId
    {
        private const string TOKEN_SEPERATOR = "*|*";

        public string Stat;
        public string Token;

        public StatId(string stat, string token = null)
        {
            this.Stat = stat;
            this.Token = token;
        }

        public LedgerStatData CreateData(double? value)
        {
            return new LedgerStatData()
            {
                Stat = this.Stat,
                Token = this.Token,
                Value = value
            };
        }

        public override string ToString() => string.IsNullOrEmpty(Token) ? Stat ?? string.Empty : Stat + TOKEN_SEPERATOR + Token;

        public static StatId Parse(string id)
        {
            int index = (id ?? string.Empty).IndexOf(TOKEN_SEPERATOR);
            if (index >= 0)
            {
                return new StatId(id.Substring(0, index), id.Substring(index + TOKEN_SEPERATOR.Length));
            }
            else
            {
                return new StatId(id);
            }
        }

    }

    class StatIdComparer : IEqualityComparer<StatId>
    {
        public static readonly StatIdComparer Default = new StatIdComparer();

        public bool Equals(StatId x, StatId y)
        {
            return ((x.Stat ?? string.Empty) == (y.Stat ?? string.Empty)) && ((x.Token ?? string.Empty) == (y.Token ?? string.Empty));
        }

        public int GetHashCode(StatId obj)
        {
            return (obj.Stat ?? string.Empty).GetHashCode() ^ (obj.Token ?? string.Empty).GetHashCode();
        }
    }

}
