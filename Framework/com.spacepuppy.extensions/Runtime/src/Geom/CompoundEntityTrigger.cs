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

        protected override void OnDisable()
        {
            base.OnDisable();

            _activeEntities.Clear();
        }

        public int ActiveEntityCount => _activeEntities.Count;

        public bool ContainsActive(SPEntity entity)
        {
            if (!entity) return false;

            return _activeEntities.Contains(entity);
        }

        public SPEntity[] GetActiveEntities()
        {
            if (_activeEntities.Count == 0) return ArrayUtil.Empty<SPEntity>();

            bool doclean = false;
            try
            {
                return _activeEntities.Where(o =>
                {
                    if (!ObjUtil.IsObjectAlive(o))
                    {
                        doclean = true;
                        return false;
                    }
                    return true;
                }).ToArray();
            }
            finally
            {
                if (doclean) this.CleanActive();
            }
        }

        public int GetActiveEntities<T>(ICollection<T> output) where T : SPEntity
        {
            if (_active.Count == 0) return 0;

            var e = _active.GetEnumerator();
            bool doclean = false;
            int cnt = 0;
            try
            {
                while (e.MoveNext())
                {
                    if (!ObjUtil.IsObjectAlive(e.Current))
                    {
                        doclean = true;
                        continue;
                    }
                    if (e.Current is T ent) output.Add(ent);
                }
            }
            finally
            {
                if (doclean) this.CleanActive();
            }
            return cnt;
        }


        protected override void SignalTriggerEnter(CompoundTriggerMember member, Collider other)
        {
            var entity = SPEntity.Pool.GetFromSource(other);
            if (!entity || !(this.Mask?.Intersects(other) ?? true)) return;

            if (_active.Add(other) && _activeEntities.Add(entity))
            {
                _messageSettings.Send(this.gameObject, (this, other), OnEnterFunctor);
            }
        }

        protected override void SignalTriggerExit(CompoundTriggerMember member, Collider other)
        {
            var entity = SPEntity.Pool.GetFromSource(other);
            if (!entity) return;

            if (this.AnyRelatedColliderOverlaps(other, out _)) return;

            if (!_active.Remove(other)) return;

            //test if any of our other active colliders are related to this entity
            var e = _active.GetEnumerator();
            while (e.MoveNext())
            {
                if (e.Current && e.Current != other && e.Current.transform.IsChildOf(entity.transform)) return;
            }

            if (_activeEntities.Remove(entity))
            {
                _messageSettings.Send(this.gameObject, (this, other), OnExitFunctor);
            }
        }

        protected override void HandleSignalingExitOnDisable()
        {
            foreach (var other in _active)
            {
                if (!other) continue;

                var entity = SPEntity.Pool.GetFromSource(other);
                if (!entity) continue;

                if (_activeEntities.Remove(entity))
                {
                    _messageSettings.Send(this.gameObject, (this, other), OnExitFunctor);
                    if ((this.Configuration & ConfigurationOptions.SendMessageToOtherCollider) != 0)
                    {
                        CompoundTriggerMember member;
                        Collider membercoll;
                        if (this.AnyRelatedColliderOverlaps(other, out member)) membercoll = member.Collider;
                        else membercoll = _colliders.Keys.FirstOrDefault();
                        _otherColliderMessageSettings.Send(other.gameObject, (this, membercoll), OnExitFunctor);
                    }
                }
            }
        }

        protected override void CleanActive()
        {
            base.CleanActive();

            if (_activeEntities.Count > 0)
            {
                _activeEntities.RemoveWhere(o => !ObjUtil.IsObjectAlive(o) || !o.isActiveAndEnabled);
            }
        }

    }
}
