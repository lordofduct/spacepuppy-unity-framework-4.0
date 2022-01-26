using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Utils;
using com.spacepuppy.Events;

namespace com.spacepuppy.DataBinding.Events
{

    public class i_LayoutGroupDataBinder : LayoutGroupDataBinder, ITriggerable
    {

        public bool CanTrigger => this.isActiveAndEnabled;

        public bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            this.StampLayoutGroup();
            return true;
        }
    }

}
