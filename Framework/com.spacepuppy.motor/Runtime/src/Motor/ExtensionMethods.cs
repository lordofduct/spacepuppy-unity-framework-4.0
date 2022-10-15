using UnityEngine;
using System.Collections.Generic;

namespace com.spacepuppy.Motor
{
    public static class ExtensionMethods
    {

        public static IForceReceiver GetForceReceiver(this GameObject go)
        {
            var fr = go.GetComponent<IForceReceiver>();
            if (fr != null) return fr;

            var rb = go.GetComponent<Rigidbody>();
            if (rb) return RigidbodyForceReceiver.Get(rb);

            return null;
        }

    }
}
