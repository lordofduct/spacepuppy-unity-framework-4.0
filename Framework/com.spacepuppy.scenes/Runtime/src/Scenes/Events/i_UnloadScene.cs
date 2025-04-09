using UnityEngine;
using System.Collections.Generic;
using com.spacepuppy.Events;

namespace com.spacepuppy.Scenes.Events
{

    [Infobox("This unloads the scene this GameObject is located in.\r\nYou probably shouldn't use this unless you know multiple scenes are loaded simultaneously.")]
    public class i_UnloadScene : AutoTriggerable
    {

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            var sc = this.gameObject.scene;
            if (sc.IsValid())
            {
                var manager = Services.Get<ISceneManager>() ?? InternalSceneManager.Instance;
                manager.UnloadScene(sc);
            }
            return true;
        }

    }

}
