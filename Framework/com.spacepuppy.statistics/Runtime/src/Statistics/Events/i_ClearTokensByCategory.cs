using UnityEngine;
using com.spacepuppy;
using com.spacepuppy.Events;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Statistics.Events
{

    public class i_ClearTokensByCategory : AutoTriggerable
    {

        #region Fields

        [SerializeField, TokenLedgerCategorySelector]
        private string _category;

        #endregion

        #region Properties

        public string Category
        {
            get => _category;
            set => _category = value;
        }

        #endregion

        #region Trigger Interface

        public override bool CanTrigger => base.CanTrigger && !string.IsNullOrEmpty(_category);

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            var service = Services.Get<IStatisticsTokenLedgerService>();
            if (service == null) return false;

            service.ClearStat(_category);
            return true;
        }

        #endregion

    }

}
