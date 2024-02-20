using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using com.spacepuppy.Statistics;

namespace com.spacepuppy
{
    public interface IStatisticsTokenLedger
    {
        bool TryGetStat(StatId stat, out double? value);
        void SetStat(StatId stat, double amount);
        void AdjustStat(StatId stat, double amount);
        void ClearStat(StatId stat);
        void ResetStats();

        IEnumerable<LedgerStatData> EnumerateStats(string filterstat = null);
    }

    public interface IStatisticsTokenLedgerService : IStatisticsTokenLedger, IService
    {

    }

}
