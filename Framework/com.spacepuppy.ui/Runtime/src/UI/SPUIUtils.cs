using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

namespace com.spacepuppy.UI
{

    public static class SPUIUtils
    {

        public static Selectable FindNextSelectable(this Selectable selectable, MoveDirection moveDirection, bool forceWrap)
        {
            if (!selectable) return null;

            if (forceWrap)
            {
                var navcache = selectable.navigation;
                try
                {
                    var nav = selectable.navigation;
                    nav.wrapAround = true;
                    switch (nav.mode)
                    {
                        case Navigation.Mode.Automatic:
                        case Navigation.Mode.Explicit:
                            nav.mode = (moveDirection == MoveDirection.Up || moveDirection == MoveDirection.Down) ? Navigation.Mode.Vertical : Navigation.Mode.Horizontal;
                            break;
                    }
                    selectable.navigation = nav;

                    switch (moveDirection)
                    {
                        case MoveDirection.Left:
                            return selectable.FindSelectableOnLeft();
                        case MoveDirection.Up:
                            return selectable.FindSelectableOnUp();
                        case MoveDirection.Right:
                            return selectable.FindSelectableOnRight();
                        case MoveDirection.Down:
                            return selectable.FindSelectableOnDown();
                        default:
                            return null;
                    }
                }
                finally
                {
                    selectable.navigation = navcache;
                }
            }
            else
            {
                switch (moveDirection)
                {
                    case MoveDirection.Left:
                        return selectable.FindSelectableOnLeft();
                    case MoveDirection.Up:
                        return selectable.FindSelectableOnUp();
                    case MoveDirection.Right:
                        return selectable.FindSelectableOnRight();
                    case MoveDirection.Down:
                        return selectable.FindSelectableOnDown();
                    default:
                        return null;
                }
            }
        }

    }

}
