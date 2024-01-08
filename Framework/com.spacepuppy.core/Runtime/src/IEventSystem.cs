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

        /// <summary>
        /// The hash changes any time 'currentSelectedGameObject' would return a new value. 
        /// By default this should be if 'currentSelectedGameObject' is has been modified, 
        /// but in a multiplayer setup this behaviour may be more complicated. 
        /// </summary>
        /// <returns></returns>
        int GetSelectionStateHash() => EventSystem.current && EventSystem.current.currentSelectedGameObject ? EventSystem.current.currentSelectedGameObject.GetInstanceID() : 0;

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
