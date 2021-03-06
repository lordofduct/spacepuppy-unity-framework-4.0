using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.spacepuppy.SPInput.Unity.Xbox.Profiles
{

    /// <remarks>
    /// This layout is seen to be a member of other libraries like InControl. Including layout 'just-in-case'.
    /// </remarks>
    [InputProfileDescription("Shenghic PS3 Controller", TargetPlatform.Windows, Description = "Shenghic PS3 Controller (Windows)")]
    [InputProfileJoystickName("PLAYSTATION(R)3Conteroller")]
    public class PS3ShenghicWinProfile : XboxInputProfile
    {
        
        public PS3ShenghicWinProfile()
        {
            this.RegisterAxis(XboxInputId.LStickX, SPInputId.Axis1);
            this.RegisterAxis(XboxInputId.LStickY, SPInputId.Axis2, true);
            this.RegisterAxis(XboxInputId.RStickX, SPInputId.Axis3);
            this.RegisterAxis(XboxInputId.RStickY, SPInputId.Axis4, true);
            this.RegisterAxis(XboxInputId.DPadX, SPInputId.Button5, SPInputId.Button7);
            this.RegisterAxis(XboxInputId.DPadY, SPInputId.Button4, SPInputId.Button6);
            this.RegisterTrigger(XboxInputId.LTrigger, SPInputId.Button8);
            this.RegisterTrigger(XboxInputId.RTrigger, SPInputId.Button9);
            

            this.RegisterButton(XboxInputId.A, SPInputId.Button14); //X
            this.RegisterButton(XboxInputId.B, SPInputId.Button13); //O
            this.RegisterButton(XboxInputId.X, SPInputId.Button15); //Sqr
            this.RegisterButton(XboxInputId.Y, SPInputId.Button12); //Tri
            this.RegisterButton(XboxInputId.LB, SPInputId.Button10); //L1
            this.RegisterButton(XboxInputId.RB, SPInputId.Button11); //R1
            this.RegisterButton(XboxInputId.Back, SPInputId.Button3); //Share - no idea why, but sources say that select/start are the same button id
            this.RegisterButton(XboxInputId.Start, SPInputId.Button3); //Options
            //this.RegisterButton(XboxInputId.LStickPress, SPInputId.Button9);
            //this.RegisterButton(XboxInputId.RStickPress, SPInputId.Button10);
            this.RegisterButton(XboxInputId.DPadRight, SPInputId.Button5);
            this.RegisterButton(XboxInputId.DPadLeft, SPInputId.Button7);
            this.RegisterButton(XboxInputId.DPadUp, SPInputId.Button4);
            this.RegisterButton(XboxInputId.DPadDown, SPInputId.Button6);
        }
        
    }

}
