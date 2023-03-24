using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Utils;
using com.spacepuppy.Events;

namespace com.spacepuppy.Statistics.Events
{

    [Infobox("Note thaat even when filtering for a specific category/id, this will still fire if the ledger is cleared causing a 'multiple changed' event.")]
    public sealed class t_OnTokenLedgerValueChange : SPComponent, IObservableTrigger, IMStartOrEnableReceiver, IStatisticsTokenLedgerChangedGlobalHandler
    {

#if UNITY_EDITOR
        public const string PROP_HITFILTER = nameof(_hitFilter);
        public const string PROP_CATEGORY = nameof(_category);
        public const string PROP_TOKEN = nameof(_token);
#endif

        public enum HitFilterOptions
        {
            Any = -1,
            Direct = 0,
            DirectOrMulti = 1,
            Category = 2,
            CategoryOrMulti = 3,
            MultiOnly = 4,
        }

        #region Fields

        [SerializeField]
        private HitFilterOptions _hitFilter;
        [SerializeField]
        private string _category;
        [SerializeField]
        [UnityEngine.Serialization.FormerlySerializedAs("_id")]
        private string _token;

        [SerializeField]
        private SPEvent _onChanged = new SPEvent("OnChanged");

        #endregion

        #region CONSTRUCTOR

        void IMStartOrEnableReceiver.OnStartOrEnable()
        {
            Messaging.RegisterGlobal<IStatisticsTokenLedgerChangedGlobalHandler>(this);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            Messaging.UnregisterGlobal<IStatisticsTokenLedgerChangedGlobalHandler>(this);
        }

        #endregion

        #region Properties

        public HitFilterOptions HitFilter
        {
            get => _hitFilter;
            set => _hitFilter = value;
        }

        public string Category
        {
            get => _category;
            set => _category = value;
        }

        public string Token
        {
            get => _token;
            set => _token = value;
        }

        public SPEvent OnChanged => _onChanged;

        #endregion

        #region ITokenLedgerChangedGlobalHandler Interface

        void IStatisticsTokenLedgerChangedGlobalHandler.OnChanged(IStatisticsTokenLedgerService ledger, LedgerChangedEventArgs ev)
        {
            if (!_onChanged.HasReceivers) return;

            switch(_hitFilter)
            {
                case HitFilterOptions.Any:
                    _onChanged.ActivateTrigger(this, null);
                    break;
                case HitFilterOptions.Direct:
                    if (string.Equals(ev.Stat, _category) && string.Equals(ev.Token, _token))
                    {
                        _onChanged.ActivateTrigger(this, null);
                    }
                    break;
                case HitFilterOptions.DirectOrMulti:
                    if (ev.MultipleChanged || (string.Equals(ev.Stat, _category) && string.Equals(ev.Token, _token)))
                    {
                        _onChanged.ActivateTrigger(this, null);
                    }
                    break;
                case HitFilterOptions.Category:
                    if (string.Equals(ev.Stat, _category))
                    {
                        _onChanged.ActivateTrigger(this, null);
                    }
                    break;
                case HitFilterOptions.CategoryOrMulti:
                    if (ev.MultipleChanged || string.Equals(ev.Stat, _category))
                    {
                        _onChanged.ActivateTrigger(this, null);
                    }
                    break;
                case HitFilterOptions.MultiOnly:
                    if (ev.MultipleChanged)
                    {
                        _onChanged.ActivateTrigger(this, null);
                    }
                    break;
            }
        }

        #endregion

        #region IObservableTrigger Interface

        BaseSPEvent[] IObservableTrigger.GetEvents() => new BaseSPEvent[] { _onChanged };

        #endregion

    }
}
