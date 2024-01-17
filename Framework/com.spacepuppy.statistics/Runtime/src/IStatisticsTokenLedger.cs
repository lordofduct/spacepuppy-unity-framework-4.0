using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using com.spacepuppy.Statistics;

namespace com.spacepuppy
{
    public interface IStatisticsTokenLedger
    {
        double? GetStat(StatId stat);
        void SetStat(StatId stat, double amount);
        void AdjustStat(StatId stat, double amount);
        void ResetStats();

        IEnumerable<LedgerStatData> EnumerateStats(string filterstat = null);
    }

    public interface IStatisticsTokenLedgerService : IStatisticsTokenLedger, IService
    {

    }

}
