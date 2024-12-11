using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Events;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Statistics.Events
{

    public sealed class EffectTokenWhileActive : SPComponent, IMStartOrEnableReceiver
    {

        #region Fields

        [SerializeField]
        private StatId _stat;

        [SerializeField]
        private int _value;

        [System.NonSerialized]
        private System.ValueTuple<StatId, int>? _effectiveValue;

        #endregion

        #region CONSTRUCTOR

        void IMStartOrEnableReceiver.OnStartOrEnable()
        {
            if (_effectiveValue == null && Services.Get(out IStatisticsTokenLedgerService ledger))
            {
                _effectiveValue = (_stat, _value);
                ledger.AdjustStat(_stat, _value);
            }
        }

        protected override void OnDisable()
        {
            if (_effectiveValue != null)
            {
                if (Services.Get(out IStatisticsTokenLedgerService ledger))
                {
                    ledger.AdjustStat(_effectiveValue.Value.Item1, -_effectiveValue.Value.Item2);
                }
                _effectiveValue = null;
            }
            base.OnDisable();
        }

        #endregion

        #region CONSTRUCTOR

        public StatId Stat
        {
            get => _stat;
            set => _stat = value;
        }

        public int Value
        {
            get => _value;
            set => _value = value;
        }

        #endregion

        #region Methods

        public void Resync()
        {
            if (this.isActiveAndEnabled && _effectiveValue != null && Services.Get(out IStatisticsTokenLedgerService ledger))
            {
                ledger.AdjustStat(_effectiveValue.Value.Item1, -_effectiveValue.Value.Item2);
                _effectiveValue = (_stat, _value);
                ledger.AdjustStat(_stat, _value);
            }
        }

        public void ChangeEffectiveValue(int value)
        {
            if (_value == value) return;

            _value = value;
            this.Resync();
        }

        #endregion

    }

}
