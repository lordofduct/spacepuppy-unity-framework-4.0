using UnityEngine;
using System.Collections.Generic;

namespace com.spacepuppy.Statistics
{

    public static class ExtensionMethods
    {

        public static double? GetStat(this IStatisticsTokenLedger ledger, string stat, string token = null) => ledger?.GetStat(new StatId(stat, token));
        public static void SetStat(this IStatisticsTokenLedger ledger, string stat, double amount, string token = null) => ledger?.SetStat(new StatId(stat, token), amount);
        public static void AdjustStat(this IStatisticsTokenLedger ledger, string stat, double amount, string token = null) => ledger?.AdjustStat(new StatId(stat, token), amount);

        public static double GetStatOrDefault(this IStatisticsTokenLedgerService ledger, string stat, string token = null) => ledger?.GetStat(stat, token) ?? 0d;
        public static double GetStatOrDefault(this IStatisticsTokenLedgerService ledger, StatId stat) => ledger?.GetStat(stat) ?? 0d;

        public static bool GetStatAsBool(this IStatisticsTokenLedgerService ledger, string stat, string token = null) => ledger?.GetStat(stat, token) != 0f;
        public static bool GetStatAsBool(this IStatisticsTokenLedgerService ledger, StatId stat) => ledger?.GetStat(stat) != 0f;

    }
}
