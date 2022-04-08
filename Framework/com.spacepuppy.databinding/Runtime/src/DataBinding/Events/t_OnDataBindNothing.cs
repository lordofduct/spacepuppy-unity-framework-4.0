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
        private SPEvent _onBoundNothing = new SPEvent("OnBoundNothing");

        #endregion

        #region IDataBindingMessageHandler Interface

        int IDataBindingMessageHandler.BindOrder => 0;

        void IDataBindingMessageHandler.Bind(object source, int index)
        {
            if (source == null) _onBoundNothing.ActivateTrigger(this, null);
        }

        #endregion

    }
}
