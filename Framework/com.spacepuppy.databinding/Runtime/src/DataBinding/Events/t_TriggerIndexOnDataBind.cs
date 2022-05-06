using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.DataBinding;
using com.spacepuppy.Events;
using com.spacepuppy.Utils;

namespace com.spacepuppy.DataBinding.Events
{
    public class t_TriggerIndexOnDataBind : SPComponent, IDataBindingMessageHandler, IObservableTrigger
    {
        #region Fields

        [SerializeField]
        private int _bindOrder;

        [SerializeField]
        private MathUtil.WrapMode _wrapMode = MathUtil.WrapMode.Loop;
        [SerializeField]
        [DisplayName("On Bound By Index (Triggered by index modulo length)")]
        private SPEvent _onBoundByIndex = new SPEvent("OnBoundByIndex");

        #endregion

        #region Properties

        public int BindOrder
        {
            get => _bindOrder;
            set => _bindOrder = value;
        }

        public SPEvent OnBoundByIndex => _onBoundByIndex;

        #endregion

        #region IDataBindingContext Interface

        int IDataBindingMessageHandler.BindOrder => _bindOrder;

        void IDataBindingMessageHandler.Bind(object source, int index)
        {
            if (_onBoundByIndex.HasReceivers)
            {
                int i = MathUtil.WrapIndex(_wrapMode, index, _onBoundByIndex.TargetCount);
                _onBoundByIndex.ActivateTriggerAt(i, this, source);
            }
        }

        #endregion

        #region IObservableTrigger Interface

        BaseSPEvent[] IObservableTrigger.GetEvents()
        {
            return new BaseSPEvent[] { _onBoundByIndex };
        }

        #endregion

    }
}
