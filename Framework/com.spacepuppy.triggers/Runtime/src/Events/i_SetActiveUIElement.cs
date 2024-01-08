#pragma warning disable 0649 // variable declared but not used.

using com.spacepuppy.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

namespace com.spacepuppy.Events
{

    [System.Obsolete("Use i_SetSelectedUIElement")]
    public class i_SetActiveUIElement : AutoTriggerable
    {

        #region Fields

        [SerializeField]
        private GameObject _element;

        #endregion

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            return Services.Get<IEventSystem>()?.SetSelectedGameObject(_element) ?? false;
        }

    }

}
