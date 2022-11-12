using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.spacepuppy.Statistics
{
    public interface IStatisticsTokenLedgerChangedGlobalHandler
    {
        void OnChanged(IStatisticsTokenLedger ledger, LedgerChangedEventArgs ev);
    }
}
