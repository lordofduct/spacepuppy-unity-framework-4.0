using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

using com.spacepuppy.Events;
using com.spacepuppy.Collections;
using com.spacepuppy.Utils;
using UnityEngine.EventSystems;

namespace com.spacepuppy.UI.Events
{

    public class i_SelectNextUIElement : AutoTriggerable
    {

        [SerializeField]
        private MoveDirection _moveDirection = MoveDirection.Down;
        [SerializeField]
        private bool _forceWrap;

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            var evsys = Services.Get<IEventSystem>();
            if (evsys == null) return false;

            var s = ObjUtil.GetAsFromSource<Selectable>(evsys.GetSelectedGameObject(this.gameObject));
            var next = s ? s.FindNextSelectable(_moveDirection, _forceWrap) : null;
            if (next) evsys.SetSelectedGameObject(next.gameObject, next.gameObject);
            return true;
        }

    }

}
