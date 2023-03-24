using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using com.spacepuppy.Statistics;

namespace com.spacepuppy
{
    public interface IStatisticsTokenLedger
    {
        double? GetStat(string stat, string token = null);
        void SetStat(string stat, double amount, string token = null);
        void AdjustStat(string stat, double amount, string token = null);
        void ResetStats();

        IEnumerable<LedgerStatData> EnumerateStats(string filterstat = null);
    }

    public interface IStatisticsTokenLedgerService : IStatisticsTokenLedger, IService
    {

    }

}
