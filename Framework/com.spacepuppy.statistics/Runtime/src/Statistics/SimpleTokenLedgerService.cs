using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Utils;


namespace com.spacepuppy.Statistics
{

    [System.Serializable]
    public class SimpleTokenLedgerService : ServiceComponent<IStatisticsTokenLedgerService>, IStatisticsTokenLedgerService
    {

        #region Fields

        [System.NonSerialized]
        private Ledger _ledger = new Ledger();

        #endregion

        #region CONSTRUCTOR

        public SimpleTokenLedgerService()
            : base(Services.AutoRegisterOption.Register, Services.MultipleServiceResolutionOption.UnregisterSelf, Services.UnregisterResolutionOption.DestroySelf)
        {

        }

        public SimpleTokenLedgerService(Services.AutoRegisterOption autoRegister, Services.MultipleServiceResolutionOption multipleServiceResolution, Services.UnregisterResolutionOption unregisterResolution)
            : base(autoRegister, multipleServiceResolution, unregisterResolution)
        {

        }

        protected override void OnValidAwake()
        {
            base.OnValidAwake();

            _ledger.Changed -= _ledger_Changed;
            _ledger.Changed += _ledger_Changed;
        }

        protected override void OnServiceUnregistered()
        {
            base.OnServiceUnregistered();

            _ledger.Changed -= _ledger_Changed;
        }

        #endregion

        #region Properties

        public Ledger Ledger => _ledger;

        #endregion

        #region Methods

        private void _ledger_Changed(object sender, LedgerChangedEventArgs ev)
        {
            Messaging.Broadcast<IStatisticsTokenLedgerChangedGlobalHandler, LedgerChangedEventArgs>(ev, (o, e) => o.OnChanged(this, e));
        }

        #endregion

        #region IStatisticsTokenLedger Interface

        public virtual void AdjustStat(string stat, double amount, string token = null)
        {
            _ledger.AdjustStat(stat, amount, token);
        }

        public virtual void SetStat(string stat, double amount, string token = null)
        {
            _ledger.SetStat(stat, token, amount, true);
        }

        public virtual double? GetStat(string stat, string token = null)
        {
            return _ledger.GetStat(stat, token);
        }

        public virtual void ResetStats()
        {
            _ledger.Reset();
        }

        public IEnumerable<LedgerStatData> EnumerateStats(string filterstat = null)
        {
            return string.IsNullOrEmpty(filterstat) ? _ledger.GetAllStatAndTokenEntries() : _ledger.GetStatAndTokenEntries(filterstat);
        }

        #endregion

    }

}
