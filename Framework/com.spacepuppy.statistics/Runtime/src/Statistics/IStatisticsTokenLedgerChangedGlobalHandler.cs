using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.spacepuppy.Statistics
{
    /// <summary>
    /// This is the message dispatched locally on the GameObject the tokenledger service component is attached.
    /// </summary>
    public interface IStatisticsTokenLedgerChangedHandler
    {
        void OnChanged(IStatisticsTokenLedgerService ledger, LedgerChangedEventArgs ev);
    }
    /// <summary>
    /// This is the message that is dispatched globally.
    /// </summary>
    public interface IStatisticsTokenLedgerChangedGlobalHandler
    {
        void OnChanged(IStatisticsTokenLedgerService ledger, LedgerChangedEventArgs ev);
    }
}
