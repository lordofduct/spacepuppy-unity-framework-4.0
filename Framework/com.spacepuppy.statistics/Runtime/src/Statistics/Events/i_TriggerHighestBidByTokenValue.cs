using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Events;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Statistics.Events
{

    public class i_TriggerHighestBidByTokenValue : AutoTriggerable
    {

#if UNITY_EDITOR
        public const string PROP_CATEGORY = nameof(_category);
        public const string PROP_TOKEN = nameof(_token);
        public const string PROP_BIDS = nameof(_bids);
        public const string PROP_BIDS_VALUE = nameof(BidData.Value);
        public const string PROP_BIDS_TARGET = nameof(BidData.Target);
#endif

        #region Fields

        [SerializeField]
        private string _category;
        [SerializeField]
        [UnityEngine.Serialization.FormerlySerializedAs("_id")]
        private string _token;

        [SerializeField]
        private List<BidData> _bids = new List<BidData>();

        [System.NonSerialized]
        private bool _bidsClean = false;

        #endregion

        #region Properties

        public string Category { get { return _category; } set { _category = value; } }
        public string Token { get { return _token; } set { _token = value; } }

        public IReadOnlyList<BidData> Bids => _bids;

        #endregion

        #region Methods

        public void ConfigureBids(params BidData[] bids)
        {
            _bids.Clear();
            _bids.AddRange(bids);
            _bidsClean = false;
        }

        #endregion

        #region Triggerable Interface

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            if (_bids == null || _bids.Count == 0) return false;

            var service = Services.Get<IStatisticsTokenLedgerService>();
            if (service == null) return false;

            if (!_bidsClean)
            {
                _bids.Sort((a, b) => a.Value.CompareTo(b.Value));
                _bidsClean = true;
            }

            float fval = (float)service.GetStatOrDefault(_category, _token);
            if (_bids[0].Value > fval) return false; //value is < all bids, quit now

            int imax = _bids.Count - 1;
            for (int i = 1; i <= imax; i++)
            {
                if (_bids[i].Value > fval)
                {
                    //i is > than bid, so i - 1 bid is the highest bid not over
                    EventTriggerEvaluator.Current.TriggerAllOnTarget(_bids[i - 1].Target, null, this, null);
                    return true;
                }
            }

            if (_bids[imax].Value <= fval)
            {
                EventTriggerEvaluator.Current.TriggerAllOnTarget(_bids[imax].Target, null, this, null);
                return true;
            }

            return false;
        }

        #endregion

        #region Special Types

        [System.Serializable]
        public struct BidData
        {
            public float Value;
            public UnityEngine.Object Target;
        }

        #endregion

#if UNITY_EDITOR
        private void OnValidate()
        {
            _bidsClean = false;
        }
#endif

    }

}
