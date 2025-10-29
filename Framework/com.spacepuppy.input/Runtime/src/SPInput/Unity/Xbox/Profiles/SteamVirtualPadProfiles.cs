using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.spacepuppy.SPInput.Unity.Xbox.Profiles
{

    [InputProfileDescription("Steam Virtual Pad", TargetPlatform.Windows, Description = "Steam Virtual Pad (Windows)")]
    [InputProfileJoystickName(XboxInputProfile.STEAMVIRTUALPAD)]
    public class SteamVirtualPadWindowsProfile : XboxInputProfile
    {

        public SteamVirtualPadWindowsProfile()
        {
            this.RegisterAxis(XboxInputId.LStickX, SPInputId.Axis1);
            this.RegisterAxis(XboxInputId.LStickY, SPInputId.Axis2, true);
            this.RegisterAxis(XboxInputId.RStickX, SPInputId.Axis4);
            this.RegisterAxis(XboxInputId.RStickY, SPInputId.Axis5, true);
            this.RegisterAxis(XboxInputId.DPadX, SPInputId.Axis6);
            this.RegisterAxis(XboxInputId.DPadY, SPInputId.Axis7);
            this.RegisterTrigger(XboxInputId.LTrigger, SPInputId.Axis9);
            this.RegisterTrigger(XboxInputId.RTrigger, SPInputId.Axis10);

            this.RegisterButton(XboxInputId.A, SPInputId.Button0);
            this.RegisterButton(XboxInputId.B, SPInputId.Button1);
            this.RegisterButton(XboxInputId.X, SPInputId.Button2);
            this.RegisterButton(XboxInputId.Y, SPInputId.Button3);
            this.RegisterButton(XboxInputId.LB, SPInputId.Button4);
            this.RegisterButton(XboxInputId.RB, SPInputId.Button5);
            this.RegisterButton(XboxInputId.Back, SPInputId.Button6);
            this.RegisterButton(XboxInputId.Start, SPInputId.Button7);
            this.RegisterButton(XboxInputId.LStickPress, SPInputId.Button8);
            this.RegisterButton(XboxInputId.RStickPress, SPInputId.Button9);
            this.RegisterAxleButton(XboxInputId.DPadRight, SPInputId.Axis6, AxleValueConsideration.Positive);
            this.RegisterAxleButton(XboxInputId.DPadLeft, SPInputId.Axis6, AxleValueConsideration.Negative);
            this.RegisterAxleButton(XboxInputId.DPadUp, SPInputId.Axis7, AxleValueConsideration.Positive);
            this.RegisterAxleButton(XboxInputId.DPadDown, SPInputId.Axis7, AxleValueConsideration.Negative);
        }

    }

    [InputProfileDescription("Steam Virtual Pad", TargetPlatform.MacOSX, Description = "Steam Virtual Pad (Mac)")]
    [InputProfileJoystickName(XboxInputProfile.STEAMVIRTUALPAD)]
    public class SteamVirtualPadMacProfile : XboxInputProfile
    {

        public SteamVirtualPadMacProfile()
        {
            this.RegisterAxis(XboxInputId.LStickX, SPInputId.Axis1);
            this.RegisterAxis(XboxInputId.LStickY, SPInputId.Axis2, true);
            this.RegisterAxis(XboxInputId.RStickX, SPInputId.Axis3);
            this.RegisterAxis(XboxInputId.RStickY, SPInputId.Axis4, true);
            this.RegisterAxis(XboxInputId.DPadX, SPInputId.Button8, SPInputId.Button7);
            this.RegisterAxis(XboxInputId.DPadY, SPInputId.Button5, SPInputId.Button6);
            this.RegisterTrigger(XboxInputId.LTrigger, SPInputId.Axis5);
            this.RegisterTrigger(XboxInputId.RTrigger, SPInputId.Axis6);

            this.RegisterButton(XboxInputId.A, SPInputId.Button16);
            this.RegisterButton(XboxInputId.B, SPInputId.Button17);
            this.RegisterButton(XboxInputId.X, SPInputId.Button18);
            this.RegisterButton(XboxInputId.Y, SPInputId.Button19);
            this.RegisterButton(XboxInputId.LB, SPInputId.Button13);
            this.RegisterButton(XboxInputId.RB, SPInputId.Button14);
            this.RegisterButton(XboxInputId.Back, SPInputId.Button10);
            this.RegisterButton(XboxInputId.Start, SPInputId.Button9);
            this.RegisterButton(XboxInputId.LStickPress, SPInputId.Button11);
            this.RegisterButton(XboxInputId.RStickPress, SPInputId.Button12);
            this.RegisterButton(XboxInputId.DPadRight, SPInputId.Button8);
            this.RegisterButton(XboxInputId.DPadLeft, SPInputId.Button7);
            this.RegisterButton(XboxInputId.DPadUp, SPInputId.Button5);
            this.RegisterButton(XboxInputId.DPadDown, SPInputId.Button6);
        }

    }

    [InputProfileDescription("Steam Virtual Pad", TargetPlatform.Linux, Description = "Steam Virtual Pad (Linux)")]
    [InputProfileJoystickName(XboxInputProfile.STEAMVIRTUALPAD)]
    public class SteamVirtualPadLinuxProfile : XboxInputProfile
    {

        public SteamVirtualPadLinuxProfile()
        {
            this.RegisterAxis(XboxInputId.LStickX, SPInputId.Axis1);
            this.RegisterAxis(XboxInputId.LStickY, SPInputId.Axis2, true);
            this.RegisterAxis(XboxInputId.RStickX, SPInputId.Axis4);
            this.RegisterAxis(XboxInputId.RStickY, SPInputId.Axis5, true);
            this.RegisterAxis(XboxInputId.DPadX, SPInputId.Axis7);
            this.RegisterAxis(XboxInputId.DPadY, SPInputId.Axis8, true);
            this.RegisterTrigger(XboxInputId.LTrigger, SPInputId.Axis3);
            this.RegisterTrigger(XboxInputId.RTrigger, SPInputId.Axis6);

            this.RegisterButton(XboxInputId.A, SPInputId.Button0);
            this.RegisterButton(XboxInputId.B, SPInputId.Button1);
            this.RegisterButton(XboxInputId.X, SPInputId.Button2);
            this.RegisterButton(XboxInputId.Y, SPInputId.Button3);
            this.RegisterButton(XboxInputId.LB, SPInputId.Button4);
            this.RegisterButton(XboxInputId.RB, SPInputId.Button5);
            this.RegisterButton(XboxInputId.Back, SPInputId.Button6);
            this.RegisterButton(XboxInputId.Start, SPInputId.Button7);
            this.RegisterButton(XboxInputId.LStickPress, SPInputId.Button9);
            this.RegisterButton(XboxInputId.RStickPress, SPInputId.Button10);
            this.RegisterAxleButton(XboxInputId.DPadRight, SPInputId.Axis7, AxleValueConsideration.Positive);
            this.RegisterAxleButton(XboxInputId.DPadLeft, SPInputId.Axis7, AxleValueConsideration.Negative);
            this.RegisterAxleButton(XboxInputId.DPadUp, SPInputId.Axis8, AxleValueConsideration.Negative);
            this.RegisterAxleButton(XboxInputId.DPadDown, SPInputId.Axis8, AxleValueConsideration.Positive);
        }

    }

}
