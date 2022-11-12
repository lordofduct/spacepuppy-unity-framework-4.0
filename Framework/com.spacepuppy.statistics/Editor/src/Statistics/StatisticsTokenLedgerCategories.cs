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
        public string[] Entries { get; set; }
        public bool Permanent { get; set; }
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
            return Categories.FirstOrDefault(o => o.Name == name)?.Entries ?? Enumerable.Empty<string>();
        }


    }

}
