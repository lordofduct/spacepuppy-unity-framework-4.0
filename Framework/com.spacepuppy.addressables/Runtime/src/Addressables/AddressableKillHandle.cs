using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Utils;

namespace com.spacepuppy.Addressables
{

    public class AddressableKillHandle : MonoBehaviour, IKillableEntity
    {

        void Awake()
        {
            this.AddTag(SPConstants.TAG_ROOT);
        }

        #region IKillableEntity Interface

        public Component component => this;

        public bool IsDead => !ObjUtil.IsObjectAlive(this);

        public bool Kill()
        {
            return UnityEngine.AddressableAssets.Addressables.ReleaseInstance(this.gameObject);
        }

        #endregion

    }

}
