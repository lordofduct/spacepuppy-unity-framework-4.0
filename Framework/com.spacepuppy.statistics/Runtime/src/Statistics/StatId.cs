using UnityEngine;
using System.Collections.Generic;
using System;
using System.Drawing;

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
    public struct StatId : IEquatable<StatId>
    {
        private const string LEGACY_SEPERATOR = "*|*";
        private const char SEPERATOR = '|';

        public string Category;
        public string Token;
        /// <summary>
        /// Normally left blank, this is used to represent meta data used by custom IStatModifiers.
        /// </summary>
        public string MetaData;

        public bool IsMeta => !string.IsNullOrEmpty(MetaData);

        public StatId(string category)
        {
            this.Category = category;
            this.Token = null;
            this.MetaData = null;
        }
        public StatId(string category, string token)
        {
            this.Category = category;
            this.Token = token;
            this.MetaData = null;
        }
        public StatId(string category, string token, string metadata)
        {
            this.Category = category;
            this.Token = token;
            this.MetaData = metadata;
        }


        public LedgerStatData CreateData(double? value)
        {
            return new LedgerStatData()
            {
                Stat = this.Category,
                Token = this.Token,
                MetaData = this.MetaData,
                Value = value
            };
        }

        public StatId GetMeta(string metadata)
        {
            return new StatId(Category, Token, metadata);
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(MetaData))
            {
                return $"{Category}{SEPERATOR}{Token}{SEPERATOR}{MetaData}";
            }
            if (!string.IsNullOrEmpty(Token))
            {
                return $"{Category}{SEPERATOR}{Token}";
            }
            else
            {
                return Category ?? string.Empty;
            }
        }

        public override int GetHashCode() => StatIdComparer.Default.GetHashCode(this);

        public override bool Equals(object obj)
        {
            if (obj is StatId other)
            {
                return StatIdComparer.Default.Equals(this, other);
            }
            else
            {
                return false;
            }
        }

        public bool Equals(StatId other) => StatIdComparer.Default.Equals(this, other);

        public static StatId Parse(string id)
        {
            if (string.IsNullOrEmpty(id)) return default;

            //this is to support older serialized data
            int index = id.IndexOf(LEGACY_SEPERATOR);
            if (index >= 0) return new StatId(id.Substring(0, index), id.Substring(index + LEGACY_SEPERATOR.Length));

            index = id.IndexOf(SEPERATOR);
            if (index >= 0)
            {
                string cat = id.Substring(0, index);
                string token = id.Substring(index + 1);
                string aux = null;
                index = token.IndexOf(SEPERATOR);
                if (index >= 0)
                {
                    token = token.Substring(0, index);
                    aux = token.Substring(index + 1);
                }
                return new StatId(cat, token, aux);
            }

            return new StatId(id);
        }

        public static bool operator ==(StatId a, StatId b) => StatIdComparer.Default.Equals(a, b);
        public static bool operator !=(StatId a, StatId b) => !StatIdComparer.Default.Equals(a, b);

    }

    class StatIdComparer : IEqualityComparer<StatId>
    {
        public static readonly StatIdComparer Default = new StatIdComparer();

        public bool Equals(StatId x, StatId y)
        {
            return (string.IsNullOrEmpty(x.Category) ? string.IsNullOrEmpty(y.Category) : x.Category.Equals(y.Category)) &&
                   (string.IsNullOrEmpty(x.Token) ? string.IsNullOrEmpty(y.Token) : x.Token.Equals(y.Token)) &&
                   (string.IsNullOrEmpty(x.MetaData) ? string.IsNullOrEmpty(y.MetaData) : x.MetaData.Equals(y.MetaData));
        }

        public int GetHashCode(StatId obj)
        {
            return System.HashCode.Combine(obj.Category ?? string.Empty, obj.Token ?? string.Empty, obj.MetaData ?? string.Empty);
        }
    }

    class StatIdCategoryComparer : IEqualityComparer<StatId>
    {
        public static readonly StatIdCategoryComparer Default = new StatIdCategoryComparer();

        public bool Equals(StatId x, StatId y)
        {
            return (string.IsNullOrEmpty(x.Category) ? string.IsNullOrEmpty(y.Category) : x.Category.Equals(y.Category));
        }

        public int GetHashCode(StatId obj)
        {
            return (obj.Category ?? string.Empty).GetHashCode();
        }
    }

}
