using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Events;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Statistics.Events
{

    public class i_CheckToken : AutoTriggerable, IObservableTrigger
    {

        [SerializeField]
        private string _category;
        [SerializeField]
        [UnityEngine.Serialization.FormerlySerializedAs("_id")]
        private string _token;
        [SerializeField]
        private double _value;
        [SerializeField]
        private ComparisonOperator _comparison = ComparisonOperator.Equal;

        [SerializeField]
        [SPEvent.Config("daisy chained object (object)")]
        private SPEvent _onSuccess = new SPEvent("OnSuccess");
        [SerializeField]
        [SPEvent.Config("daisy chained object (object)")]
        private SPEvent _onFailure = new SPEvent("OnFailure");

        #region Properties

        public string Category { get { return _category; } set { _category = value; } }
        public string Token { get { return _token; } set { _token = value; } }
        public double Value { get { return _value; } set { _value = value; } }
        public ComparisonOperator Comparison { get { return _comparison; } set { _comparison = value; } }

        public SPEvent OnSuccess { get { return _onSuccess; } }
        public SPEvent OnFailure { get { return _onFailure; } }

        #endregion

        #region Trigger Interface

        public override bool CanTrigger => base.CanTrigger && !string.IsNullOrEmpty(_category) && !string.IsNullOrEmpty(_token);

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            var service = Services.Get<IStatisticsTokenLedgerService>();
            if (service == null) return false;

            var val = service.GetStatOrDefault(_category, _token);
            if (CompareUtil.Compare(_comparison, val, _value))
            {
                if (_onSuccess.HasReceivers) _onSuccess.ActivateTrigger(this, arg);
            }
            else
            {
                if (_onFailure.HasReceivers) _onFailure.ActivateTrigger(this, arg);
            }
            return true;
        }

        #endregion

        #region IObservableTrigger Interface

        BaseSPEvent[] IObservableTrigger.GetEvents()
        {
            return new BaseSPEvent[] { _onSuccess, _onFailure };
        }

        #endregion

    }

}