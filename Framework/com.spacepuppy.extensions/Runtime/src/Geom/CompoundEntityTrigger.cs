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

        #region Fields

        private HashSet<SPEntity> _activeEntities = new HashSet<SPEntity>();
        private ActiveEntityCollection _activeEntitiesWrapper;

        #endregion

        #region CONSTRUCTOR

        #endregion

        #region Properties

        /// <summary>
        /// This value is an estimate of the active entity count, while this number is never < the real number, it may be larger.
        /// Accessing GetActiveEntities or other methods that enumerate the overlapped colliders will reduce this to the real value 
        // and will remain accurate until the next physics event.
        /// </summary>
        public int EstimatedActiveEntityCount => _activeEntities.Count;

        public ActiveEntityCollection ActiveEntities => (_activeEntitiesWrapper ??= new(this));

        #endregion

        #region Methods

        public bool ContainsActive(SPEntity entity)
        {
            if (!ObjUtil.IsObjectAlive(entity)) return false;

            return _activeEntities.Contains(entity);
        }

        public IEnumerable<SPEntity> GetActiveEntities()
        {
            if (_isDirty) this.CleanActive();
            return _activeEntities.Where(this.ValidateEntryOrSetDirty);
        }

        public IEnumerable<T> GetActiveEntities<T>() where T : SPEntity
        {
            if (_isDirty) this.CleanActive();
            return _activeEntities.Where(this.ValidateEntryOrSetDirty).OfType<T>();
        }

        public int GetActiveEntities<T>(ICollection<T> output) where T : SPEntity
        {
            if (_active.Count == 0) return 0;

            var e = _active.GetEnumerator();
            int cnt = 0;
            try
            {
                while (e.MoveNext())
                {
                    if (!this.ValidateEntryOrSetDirty(e.Current)) continue;
                    if (e.Current is T ent) output.Add(ent);
                }
            }
            finally
            {
                if (_isDirty) this.CleanActive();
            }
            return cnt;
        }


        protected override void SignalTriggerEnter(CompoundTriggerMember member, Collider other)
        {
            var entity = SPEntity.Pool.GetFromSource(other);
            if (!entity || !(this.Mask?.Intersects(other) ?? true)) return;

            if (_active.Add(other))
            {
                _activeTargetsChanged?.Invoke(this, System.EventArgs.Empty);
                if (_activeEntities.Add(entity))
                {
                    _messageSettings.Send(this.gameObject, (this, other), OnEnterFunctor);
                    if ((this.Configuration & ConfigurationOptions.SendMessageToOtherCollider) != 0) _otherColliderMessageSettings.Send(other.gameObject, (this, member.Collider), OnEnterFunctor);
                }
            }
        }

        protected override void SignalTriggerExit(CompoundTriggerMember member, Collider other)
        {
            var entity = SPEntity.Pool.GetFromSource(other);
            if (!entity) return;

            if (this.AnyRelatedColliderOverlaps(other, out _)) return;

            if (!_active.Remove(other)) return;

            _activeTargetsChanged?.Invoke(this, System.EventArgs.Empty);

            //test if any of our other active colliders are related to this entity
            var e = _active.GetEnumerator();
            while (e.MoveNext())
            {
                if (e.Current && e.Current != other && e.Current.transform.IsChildOf(entity.transform)) return;
            }

            if (_activeEntities.Remove(entity))
            {
                _messageSettings.Send(this.gameObject, (this, other), OnExitFunctor);
                if ((this.Configuration & ConfigurationOptions.SendMessageToOtherCollider) != 0) _otherColliderMessageSettings.Send(other.gameObject, (this, member.Collider), OnExitFunctor);
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
                        else membercoll = _colliders.Keys.FirstOrDefault(c => c.enabled && c.gameObject.activeInHierarchy);
                        _otherColliderMessageSettings.Send(other.gameObject, (this, membercoll), OnExitFunctor);
                    }
                }
            }
            _active.Clear();
            _activeEntities.Clear();
            _activeTargetsChanged?.Invoke(this, System.EventArgs.Empty);
        }

        protected override void CleanActive()
        {
            base.CleanActive();

            if (_activeEntities.Count > 0)
            {
                _activeEntities.RemoveWhere(o => !ObjUtil.IsObjectAlive(o) || !o.isActiveAndEnabled);
            }
        }

        protected bool ValidateEntryOrSetDirty(SPEntity o)
        {
            if (ObjUtil.IsObjectAlive(o) && o.isActiveAndEnabled)
            {
                return true;
            }
            else
            {
                _isDirty = true;
                return false;
            }
        }

        #endregion

        #region Special Types

        public class ActiveEntityCollection : IEnumerable<SPEntity>
        {

            private CompoundEntityTrigger _owner;
            internal ActiveEntityCollection(CompoundEntityTrigger owner)
            {
                _owner = owner;
            }

            public bool Contains(SPEntity item)
            {
                if (!item) return false;
                if (_owner._isDirty) _owner.CleanActive();
                return _owner._activeEntities.Contains(item);
            }
            public void CopyTo(SPEntity[] array, int arrayIndex)
            {
                var e = new ActiveEntityEnumerator(_owner);
                while (e.MoveNext() && arrayIndex < array.Length)
                {
                    array[arrayIndex] = e.Current;
                    arrayIndex++;
                }
            }

            public ActiveEntityEnumerator GetEnumerator() => new ActiveEntityEnumerator(_owner);
            IEnumerator<SPEntity> IEnumerable<SPEntity>.GetEnumerator() => new ActiveEntityEnumerator(_owner);
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => new ActiveEntityEnumerator(_owner);

        }

        public struct ActiveEntityEnumerator : IEnumerator<SPEntity>
        {
            private CompoundEntityTrigger _owner;
            private HashSet<SPEntity>.Enumerator _e;
            internal ActiveEntityEnumerator(CompoundEntityTrigger owner)
            {
                _owner = owner;
                if (_owner._isDirty) _owner.CleanActive();
                _e = owner._activeEntities.GetEnumerator();
            }

            public SPEntity Current => _e.Current;
            object System.Collections.IEnumerator.Current => _e.Current;

            public void Dispose() => _e.Dispose();
            public bool MoveNext()
            {
                while (_e.MoveNext())
                {
                    if (_owner.ValidateEntryOrSetDirty(_e.Current)) return true;
                }
                return false;
            }
            void System.Collections.IEnumerator.Reset() => (_e as System.Collections.IEnumerator).Reset();

        }

        #endregion

    }
}
