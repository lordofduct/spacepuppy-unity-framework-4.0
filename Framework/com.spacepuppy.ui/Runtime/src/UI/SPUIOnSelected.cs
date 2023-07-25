using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Utils;
using com.spacepuppy.Events;

namespace com.spacepuppy.UI
{

    public sealed class SPUIOnSelected : MonoBehaviour, ISelectHandler, IDeselectHandler, IObservableTrigger
    {

        #region Fields

        [SerializeField]
        private SPEvent _onSelect = new SPEvent("OnSelect");
        [SerializeField]
        private SPEvent _onDeselect = new SPEvent("OnDeselect");

        #endregion

        #region Properties

        public SPEvent OnSelect => _onSelect;

        public SPEvent OnDeselect => _onDeselect;

        #endregion

        #region ISelect/DeselectHandler Interface

        void ISelectHandler.OnSelect(BaseEventData eventData)
        {
            if (!this.IsActiveAndEnabled()) return;

            _onSelect.ActivateTrigger(this, null);
        }

        void IDeselectHandler.OnDeselect(BaseEventData eventData)
        {
            if (!this.IsActiveAndEnabled()) return;

            _onDeselect.ActivateTrigger(this, null);
        }

        #endregion

        #region IObservableTrigger Interface

        BaseSPEvent[] IObservableTrigger.GetEvents()
        {
            return new BaseSPEvent[] { _onSelect, _onDeselect };
        }

        #endregion

    }

}
