#if SP_ADDRESSABLES
using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Utils;

namespace com.spacepuppy.Addressables
{

    public class AddressableKillHandle : MonoBehaviour, IKillableEntity
    {

        public const float KILLABLEENTITYPRIORITY = 0f;

        [SerializeField]
        private float _killableEntityPriority = KILLABLEENTITYPRIORITY;

        void Awake()
        {
            this.AddTag(SPConstants.TAG_ROOT);
        }

        #region IKillableEntity Interface

        public Component component => this;

        public bool IsDead => !ObjUtil.IsObjectAlive(this);

        void IKillableEntity.OnPreKill(ref com.spacepuppy.KillableEntityToken token, UnityEngine.GameObject target)
        {
            //if this is dead, or if it's not the root of this entity being killed... exit now
            if (!ObjUtil.IsObjectAlive(this) || this.gameObject != target) return;

            token.ProposeKillCandidate(this, _killableEntityPriority);
        }

        void IOnKillHandler.OnKill(KillableEntityToken token)
        {
            //do nothing
        }

        void IKillableEntity.OnElectedKillCandidate()
        {
            if (!UnityEngine.AddressableAssets.Addressables.ReleaseInstance(this.gameObject))
            {
                Destroy(this.gameObject);
            }
        }

        #endregion

    }

}
#endif