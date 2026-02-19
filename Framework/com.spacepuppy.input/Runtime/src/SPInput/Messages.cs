using UnityEngine;
using System.Collections.Generic;

namespace com.spacepuppy.SPInput
{

    public interface IJoystickHotpluggedGlobalHandler
    {
        void OnJoystickHotplugged(IInputManager inputmanager);
    }

}
