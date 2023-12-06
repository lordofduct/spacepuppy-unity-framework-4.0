using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.SPInput;

namespace com.spacepuppy
{

    public interface IInputManager : IService, IEnumerable<IInputDevice>
    {

        int Count { get; }
        IInputDevice this[string id] { get; }
        IInputDevice Main { get; }

        bool TryGetDevice(string id, out IInputDevice device);

    }

    public static class IInputManagerExtensions
    {

        public static IInputDevice GetDevice(this IInputManager manager, string id)
        {
            if (manager != null && manager.TryGetDevice(id, out IInputDevice result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        public static T GetDevice<T>(this IInputManager manager, string id) where T : class, IInputDevice
        {
            return manager?.GetDevice(id) as T;
        }

    }

}
