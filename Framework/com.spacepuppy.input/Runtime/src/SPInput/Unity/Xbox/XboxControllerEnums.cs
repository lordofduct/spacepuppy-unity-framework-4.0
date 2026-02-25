using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.spacepuppy.SPInput.Unity.Xbox
{

    public enum XboxInputId
    {
        LStickX,
        LStickY,
        RStickX,
        RStickY,
        DPadX,
        DPadY,
        LTrigger,
        RTrigger,
        A,
        B,
        X,
        Y,
        LB,
        RB,
        Start,
        Back,
        LStickPress,
        RStickPress,
        DPadUp,
        DPadRight,
        DPadDown,
        DPadLeft,


        // - TODO - consider adding this, requires finding inputs on all platforms.
        // Xbox and Steampads have been determined for windows as button 10, and button 8 on linux. Macos 'might' be button 15?
        // Thing is that not all gamepads have a home button AND steam and the OS really like to steal that button for its own use. 
        //Home,
    }

}
