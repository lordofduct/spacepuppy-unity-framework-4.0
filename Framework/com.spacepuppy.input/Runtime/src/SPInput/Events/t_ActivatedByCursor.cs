using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Events;
using com.spacepuppy.Utils;

namespace com.spacepuppy.SPInput.Events
{

    [Infobox("If I_ActivateUnderCursor is triggered while the matching cursor is over this target this will trigger. OnActivated occurs if the Tokens match, otherwise OnFailedToActivate is triggered.")]
    public class t_ActivatedByCursor : MonoBehaviour, CursorInputLogic.ICursorActivatedHandler, IObservableTrigger
    {

        #region Fields

        [SerializeField]
        private UnityEngine.Object _token;

        [SerializeField]
        private bool _ignoreTokenMatch = false;

        [SerializeField]
        [SPEvent.Config("token (object)")]
        private SPEvent _onActivated = new SPEvent("OnActivated");

        [SerializeField]
        [SPEvent.Config("token (object)")]
        private SPEvent _onFailedToActivate = new SPEvent("OnFailedToActivate");

        #endregion

        #region ICursorActivatedHandler Interface

        void CursorInputLogic.ICursorActivatedHandler.OnCursorActivated(object sender, ref CursorInputLogic.CursorActivateEventData data)
        {
            if(_ignoreTokenMatch || object.Equals(_token, data.Token))
            {
                data.Use();
                _onActivated.ActivateTrigger(this, _token);
            }
            else
            {
                _onFailedToActivate.ActivateTrigger(this, _token);
            }
        }

        #endregion

        #region IObservableTrigger Interface

        BaseSPEvent[] IObservableTrigger.GetEvents() => new BaseSPEvent[] { _onActivated, _onFailedToActivate };

        #endregion

    }
}
