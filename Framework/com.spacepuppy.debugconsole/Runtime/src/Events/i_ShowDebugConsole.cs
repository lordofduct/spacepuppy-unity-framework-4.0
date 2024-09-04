using UnityEngine;
using System.Collections.Generic;
using com.spacepuppy.Events;

namespace com.spacepuppy.debugconsole
{

    [Infobox("Shows a debug console and sets it to 'ActiveConsole'.")]
    public class i_ShowDebugConsole : AutoTriggerable
    {

        #region Triggerable Interface

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            SPDebugConsole.ShowDebugConsole();
            return true;
        }

        #endregion

    }

}
