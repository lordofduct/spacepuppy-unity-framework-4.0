using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Utils;

namespace com.spacepuppy.Events
{

    public static class SPEventUtils
    {

        public static T GetTarget<T>(object configuredTarg, object arg) where T : class
        {
            if(!ObjUtil.IsNullOrDestroyed(configuredTarg))
            {
                return ObjUtil.GetAsFromSource<T>(configuredTarg);
            }
            else
            {
                return ObjUtil.GetAsFromSource<T>(arg);
            }
        }

    }

}
