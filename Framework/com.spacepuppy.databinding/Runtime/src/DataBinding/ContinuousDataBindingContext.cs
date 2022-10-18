using UnityEngine;
using System.Collections.Generic;

namespace com.spacepuppy.DataBinding
{

    [Infobox("This databinding context will repeatedly keep polling the databinding source at the frequency configured. This can be costly to perform so use sparingly.", MessageType = InfoBoxMessageType.Warning)]
    public class ContinuousDataBindingContext : DataBindingContext
    {

        #region Fields

        [SerializeField]
        private SPTimePeriod _continuousBindingFrequency = 0f;

        [System.NonSerialized]
        private double _lastUpdateTime;
        [System.NonSerialized]
        private int? _bindIndex;

        #endregion

        #region Properties

        public SPTimePeriod ContinuousBindingFrequency
        {
            get => _continuousBindingFrequency;
            set => _continuousBindingFrequency = value;
        }

        #endregion

        #region Methods

        public override void Bind(object source, int index)
        {
            base.Bind(source, index);

            _bindIndex = index;
            _lastUpdateTime = _continuousBindingFrequency.TimeSupplierOrDefault.TotalPrecise;
        }

        public void HaultContinousBinding()
        {
            _bindIndex = null;
        }

        private void Update()
        {
            if (_bindIndex == null) return;

            if ((_continuousBindingFrequency.TimeSupplierOrDefault.TotalPrecise - _lastUpdateTime) > _continuousBindingFrequency.Seconds)
            {
                this.Bind(this.DataSource, _bindIndex.Value);
            }
        }

        #endregion

    }

}
