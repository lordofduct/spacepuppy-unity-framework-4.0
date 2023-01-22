using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy.Collections;
using com.spacepuppy.Utils;
using com.spacepuppy.Dynamic;

namespace com.spacepuppy.Geom
{

    [Infobox("Colliders on or in this GameObject are grouped together and treated as a single collider signaling with the ICompoundTriggerXHandler message if and only if the entering Collider is associated with an Entity.")]
    public class CompoundEntityTrigger : CompoundTrigger
    {

        private HashSet<SPEntity> _activeEntities = new HashSet<SPEntity>();


        public bool ContainsActive(SPEntity entity)
        {
            if (!entity) return false;

            return _activeEntities.Contains(entity);
        }


        protected override void SignalTriggerEnter(CompoundTriggerMember member, Collider other)
        {
            var entity = SPEntity.Pool.GetFromSource(other);
            if (!entity) return;

            if (_active.Add(other) && _activeEntities.Add(entity))
            {
                _messageSettings.Send(this.gameObject, (this, other), OnEnterFunctor);
            }
        }

        protected override void SignalTriggerExit(CompoundTriggerMember member, Collider other)
        {
            var entity = SPEntity.Pool.GetFromSource(other);
            if (!entity) return;

            if (this.AnyRelatedColliderOverlaps(other)) return;

            if (!_active.Remove(other)) return;

            //test if any of our other active colliders are related to this entity
            var e = _active.GetEnumerator();
            while (e.MoveNext())
            {
                if (e.Current != other && e.Current.transform.IsChildOf(entity.transform)) return;
            }

            if (_activeEntities.Remove(entity))
            {
                _messageSettings.Send(this.gameObject, (this, other), OnExitFunctor);
            }
        }

    }
}
