using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using com.spacepuppy.Utils;

namespace com.spacepuppyeditor.Statistics
{

    public class TokenLedgerCategory
    {
        public string Name { get; set; }
        public System.Type DataStore { get; set; }
        public bool Permanent { get; set; }

        private string[] _entries = ArrayUtil.Empty<string>();
        public object Entries
        {
            get => _entries;
            set
            {
                switch(value)
                {
                    case string str:
                        _entries = new string[] { str };
                        break;
                    case string[] arr:
                        _entries = arr;
                        break;
                    case IEnumerable<string> e:
                        _entries = e.ToArray();
                        break;
                    case System.Type tp:
                        _entries = tp.IsEnum ? System.Enum.GetNames(tp) : new string[] { tp.Name };
                        break;
                    case System.Delegate d:
                        try
                        {
                            this.Entries = d.DynamicInvoke();
                        }
                        catch( System.Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                        break;
                    default:
                        //unsupported
                        _entries = ArrayUtil.Empty<string>();
                        break;
                }
            }
        }
        public string[] EntriesArray => _entries;

    }

    public class StatisticsTokenLedgerCategories
    {

        private static TokenLedgerCategory[] _categories;
        public static TokenLedgerCategory[] Categories
        {
            get => _categories ?? ArrayUtil.Empty<TokenLedgerCategory>();
            protected set => _categories = value;
        }

        public static bool IndexInRange(int index) => index >= 0 && index < Categories.Length;

        public static int FindIndexOfCategory(string name)
        {
            for (int i = 0; i < Categories.Length; i++)
            {
                if (Categories[i].Name == name) return i;
            }
            return -1;
        }

        public static IEnumerable<string> GetTokenCategoryEntryValues(string name)
        {
            return Categories.FirstOrDefault(o => o.Name == name)?.EntriesArray ?? Enumerable.Empty<string>();
        }


    }

}
