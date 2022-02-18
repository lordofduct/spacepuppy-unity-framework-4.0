using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Events;
using com.spacepuppy.Utils;
using com.spacepuppy.SPInput;
using System.Linq;

namespace com.spacepuppy.SPInput.Events
{

    [Infobox("When triggered it will find a target under every cursor that passes 'Pointer Filter'. If any one of those cursors has an activateable target underneath it (see: t_ActivatedByCursor) that has a matching 'Mediator', 'On Success' will be triggered. Otherwise 'On Failure' will be triggered.")]
    public class i_ActivateUnderCursor : Triggerable
    {

        #region Fields

        [SerializeField]
        private PointerFilter _pointerFilter;
        [SerializeField]
        private UnityEngine.Object _token;
        [SerializeField]
        [Tooltip("Ignores if the UI is blocking the cursor, you likely want this true since this is generally used in tandem with a drag which means UI elements are inherently blocking.")]
        private bool _ignoreIfCursorBlocked = true;

        [SerializeField]
        private SPEvent _onSuccess = new SPEvent("OnSuccess");

        [SerializeField]
        private SPEvent _onFailure = new SPEvent("OnFailure");

        #endregion

        #region Properties

        public PointerFilter PointerFilter
        {
            get => _pointerFilter;
            set => _pointerFilter = value;
        }

        #endregion

        #region Triggerable Interface

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            IEnumerable<CursorInputLogic> e = CursorInputLogic.Pool;
            if (_pointerFilter != null) e = e.Where(c => _pointerFilter.IsValid(c));

            int passcount = 0;
            foreach (var cursor in e)
            {
                var data = cursor.ActivateCursor(_token, _ignoreIfCursorBlocked);
                passcount += data.UseCount;
            }

            if (passcount > 0)
            {
                _onSuccess.ActivateTrigger(this, null);
                return true;
            }
            else
            {
                _onFailure.ActivateTrigger(this, null);
                return false;
            }
        }

        #endregion

    }
}
