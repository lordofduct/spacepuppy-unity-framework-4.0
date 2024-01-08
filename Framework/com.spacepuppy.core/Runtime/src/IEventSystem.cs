using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace com.spacepuppy
{

    public interface IEventSystem : IService
    {

        EventSystem GetEventSystem(object context = null) => EventSystem.current;

        bool SetSelectedGameObject(GameObject selected, object context = null)
        {
            var ev = EventSystem.current;
            if (ev == null) return false;

            if (selected != null && ev.currentSelectedGameObject == selected)
            {
                ev.SetSelectedGameObject(null);
                ev.SetSelectedGameObject(selected);
            }
            else
            {
                ev.SetSelectedGameObject(selected);
            }
            return true;
        }

        GameObject GetSelectedGameObject(object context = null)
        {
            var ev = EventSystem.current;
            return ev != null ? ev.currentSelectedGameObject : null;
        }

    }

    internal class BasicCoreEventSystemService : ServiceObject<IEventSystem>, IEventSystem
    {

        #region Static Initializer

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        static void Init()
        {
            if (Services.SPInternal_GetDefaultService<IEventSystem>() == null)
            {
                Services.SPInternal_RegisterDefaultService<IEventSystem>(new BasicCoreEventSystemService());
            }
        }

        #endregion

    }

}
