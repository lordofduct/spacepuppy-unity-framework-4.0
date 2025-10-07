using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

using com.spacepuppy.Events;
using com.spacepuppy.Collections;
using com.spacepuppy.Utils;

namespace com.spacepuppy.UI.Events
{

    public class i_SelectFirstAvailableUIElement : AutoTriggerable
    {

#if UNITY_EDITOR
        public const string PROP_MODE = nameof(_mode);
        public const string PROP_OPTIONS = nameof(_options);
#endif

        public enum Modes
        {
            FirstChild = 0,
            FirstInList = 1,
        }

        #region Fields

        [SerializeField]
        private Modes _mode;
        [SerializeField, ReorderableArray]
        private GameObject[] _options;

        #endregion

        #region Triggerable Interface

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            var service = Services.Get<IEventSystem>();
            if (service == null) return false;

            foreach (var go in _options)
            {
                if (!go.IsAliveAndActive()) continue;

                switch (_mode)
                {
                    case Modes.FirstChild:
                        using (var lst = TempCollection.GetList<Selectable>())
                        {
                            go.GetComponentsInChildren(lst);
                            foreach (var s in lst)
                            {
                                if (s.interactable && s.enabled && s.gameObject.activeInHierarchy)
                                {
                                    service.SetSelectedGameObject(s.gameObject, s.gameObject);
                                    return true;
                                }
                            }
                        }
                        break;
                    case Modes.FirstInList:
                        if (go.TryGetComponent(out Selectable selectable) && selectable.enabled && selectable.interactable)
                        {
                            service.SetSelectedGameObject(go, go);
                        }
                        break;
                    default:
                        break;
                }
            }

            return false;
        }

        #endregion

    }

}
