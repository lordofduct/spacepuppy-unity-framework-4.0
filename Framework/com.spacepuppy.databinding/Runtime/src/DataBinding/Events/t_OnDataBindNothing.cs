using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Events;
using com.spacepuppy.Utils;

namespace com.spacepuppy.DataBinding.Events
{
    public class t_OnDataBindNothing : SPComponent, IDataBindingMessageHandler
    {

        #region Fields

        [SerializeField]
        private int _bindOrder;

        [SerializeField]
        private SPEvent _onBoundNothing = new SPEvent("OnBoundNothing");

        [SerializeField]
        [SPEvent.Config("source (object)")]
        private SPEvent _onBoundSomething = new SPEvent("OnBoundSomething");

        #endregion

        #region Properties

        public int BindOrder
        {
            get => _bindOrder;
            set => _bindOrder = value;
        }

        public SPEvent OnBoundNothing => _onBoundNothing;

        public SPEvent OnBoundSomething => _onBoundSomething;

        #endregion

        #region IDataBindingMessageHandler Interface

        int IDataBindingMessageHandler.BindOrder => _bindOrder;

        void IDataBindingMessageHandler.Bind(object source, int index)
        {
            if (source == null)
            {
                _onBoundNothing.ActivateTrigger(this, null);
            }
            else
            {
                _onBoundSomething.ActivateTrigger(this, source);
            }
        }

        #endregion

    }
}
