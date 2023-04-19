using UnityEngine;
using System.Collections.Generic;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Events
{
    public class i_SetLayer : AutoTriggerable
    {

        #region Fields

        [SerializeField]
        [RespectsIProxy]
        [TypeRestriction(typeof(GameObject), AllowProxy = true)]
        private UnityEngine.Object _target;

        [SerializeField]
        [LayerSelector]
        private int _layer;

        [SerializeField]
        private bool _recursive;

        #endregion

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            var go = GameObjectUtil.GetGameObjectFromSource(_target, true);
            if (go)
            {
                go.ChangeLayer(_layer, _recursive);
            }
            return true;
        }

    }
}
