using UnityEngine;
using System.Collections.Generic;
using com.spacepuppy.Events;

namespace com.spacepuppy.debugconsole
{

    [Infobox("Closes the currently open 'ActiveConsole' if it exists.")]
    public class i_CloseDebugConsole : AutoTriggerable
    {

        #region Triggerable Interface

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            SPDebugConsole.ActiveConsole?.Close();
            return true;
        }

        #endregion

    }

}
