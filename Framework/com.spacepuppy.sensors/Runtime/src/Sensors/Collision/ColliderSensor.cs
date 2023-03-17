#pragma warning disable 0649 // variable declared but not used.

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Geom;
using com.spacepuppy.Utils;
using com.spacepuppy.Collections;

namespace com.spacepuppy.Sensors.Collision
{
    
    public class ColliderSensor : ActiveSensor
    {

        [System.Flags()]
        public enum AllowedColliderMode
        {
            Solid = 1,
            Trigger = 2,
            Both = 3
        }

        #region Fields

        [SerializeField()]
        private bool _canDetectSelf;
        [SerializeField()]
        [EnumFlags()]
        private AllowedColliderMode _allowedColliders = AllowedColliderMode.Both;
        [SerializeField]
        [Tooltip("A mask for things we can sense, leave blank to sense all possible aspects.")]
        private EventActivatorMaskRef _mask;

        [SerializeField()]
        [Tooltip("The line of sight is naive and works as from the position of this to the center of the bounds of the target collider.")]
        private bool _requiresLineOfSight;
        [SerializeField()]
        private LayerMask _lineOfSightMask;


        [System.NonSerialized()]
        private HashSet<Collider> _intersectingColliders = new HashSet<Collider>();

        #endregion

        #region CONSTRUCTOR

        protected override void OnDisable()
        {
            base.OnDisable();

            _intersectingColliders.Clear();
        }

        #endregion

        #region Properties

        public bool CanDetectSelf
        {
            get { return _canDetectSelf; }
            set { _canDetectSelf = value; }
        }
        public AllowedColliderMode AllowedColliders
        {
            get { return _allowedColliders; }
            set { _allowedColliders = value; }
        }

        public IEventActivatorMask Mask
        {
            get { return _mask.Value; }
            set { _mask.Value = value; }
        }

        public bool RequiresLineOfSight
        {
            get { return _requiresLineOfSight; }
            set { _requiresLineOfSight = value; }
        }

        public LayerMask LineOfSightMask
        {
            get { return _lineOfSightMask; }
            set { _lineOfSightMask = value; }
        }

        #endregion

        #region Methods

        protected void OnTriggerEnter(Collider coll)
        {
            if (!this.isActiveAndEnabled) return;
            if (!this.ConcernedWith(coll)) return;

            bool none = _intersectingColliders.Count == 0;
            if (_intersectingColliders.Add(coll))
            {
                if (this.HasSensedAspectListeners)
                {
                    this.OnSensedAspect(ColliderAspect.GetAspect(coll));
                }
                if (none)
                {
                    this.OnSensorAlert();
                }
            }
        }

        protected void OnTriggerExit(Collider coll)
        {
            if (!this.isActiveAndEnabled) return;
            if (_intersectingColliders.Remove(coll) && _intersectingColliders.Count == 0)
            {
                this.OnSensorSleep();
            }
        }

        private bool ConcernedWith(Collider coll)
        {
            if (coll == null) return false;
            var mode = (coll.isTrigger) ? AllowedColliderMode.Trigger : AllowedColliderMode.Solid;
            if ((_allowedColliders & mode) == 0) return false;
            if (_mask.Value != null && !_mask.Value.Intersects(coll.gameObject)) return false;

            if (!_canDetectSelf)
            {
                var root = coll.FindRoot();
                if (root == this.entityRoot) return false;
            }

            return true;
        }

        protected bool IsLineOfSight(Collider coll)
        {
            var v = coll.GetCenter() - this.transform.position;
            //RaycastHit hit;
            //if(Physics.Raycast(this.transform.position, v, out hit, v.magnitude, _lineOfSightMask))
            //{
            //    return (hit.collider == coll);
            //}
            using (var lst = com.spacepuppy.Collections.TempCollection.GetList<RaycastHit>())
            {
                int cnt = PhysicsUtil.RaycastAll(this.transform.position, v, lst, v.magnitude, _lineOfSightMask);
                if(cnt > 0)
                {
                    var otherRoot = coll.FindRoot();
                    for (int i = 0; i < cnt; i++)
                    {
                        //we ignore ourself
                        var r = lst[i].collider.FindRoot();
                        if (r != otherRoot && r != this.entityRoot) return false;
                    }
                }
            }

            return true;
        }

        private void CleanColliders()
        {
            if (_intersectingColliders.Count == 0) return;

            using (var lst = TempCollection.GetList<Collider>())
            {
                _intersectingColliders.RemoveWhere(o => !ObjUtil.IsObjectAlive(o) || !o.IsActiveAndEnabled());
            }
        }

        #endregion

        #region Sensor Interface

        public override bool ConcernedWith(UnityEngine.Object obj)
        {
            if(obj is Collider)
            {
                return this.ConcernedWith(obj as Collider);
            }
            else
            {
                var go = GameObjectUtil.GetGameObjectFromSource(obj);
                if (go == null) return false;
                using (var set = com.spacepuppy.Collections.TempCollection.GetSet<Collider>())
                {
                    go.FindComponents<Collider>(set);
                    var e = set.GetEnumerator();
                    while(e.MoveNext())
                    {
                        if (this.ConcernedWith(e.Current))
                            return true;
                    }
                    return false;
                }
            }
        }

        public override bool SenseAny(System.Func<IAspect, bool> p = null)
        {
            if (_intersectingColliders.Count == 0) return false;

            var e = _intersectingColliders.GetEnumerator();
            bool doclean = false;
            try
            {
                while (e.MoveNext())
                {
                    if (!ObjUtil.IsObjectAlive(e.Current))
                    {
                        doclean = true;
                        continue;
                    }

                    var a = ColliderAspect.GetAspect(e.Current);
                    if ((p == null || p(a)) && (!_requiresLineOfSight || this.IsLineOfSight(e.Current))) return true;
                }
            }
            finally
            {
                if (doclean) this.CleanColliders();
            }

            return false;
        }

        public override bool Visible(IAspect aspect)
        {
            var colAspect = aspect as ColliderAspect;
            if (colAspect == null) return false;

            return _intersectingColliders.Contains(colAspect.Collider);
        }

        public override IAspect Sense(System.Func<IAspect, bool> p = null)
        {
            if (_intersectingColliders.Count == 0) return null;

            var e = _intersectingColliders.GetEnumerator();
            bool doclean = false;
            try
            {
                while (e.MoveNext())
                {
                    if (!ObjUtil.IsObjectAlive(e.Current))
                    {
                        doclean = true;
                        continue;
                    }

                    var a = ColliderAspect.GetAspect(e.Current);
                    if ((p == null || p(a)) && (!_requiresLineOfSight || this.IsLineOfSight(e.Current))) return a;
                }
            }
            finally
            {
                if (doclean) this.CleanColliders();
            }
            return null;
        }

        public override int SenseAll(ICollection<IAspect> lst, System.Func<IAspect, bool> p = null)
        {
            if (lst == null) throw new System.ArgumentNullException("lst");
            if (lst.IsReadOnly) throw new System.ArgumentException("List to fill can not be read-only.", "lst");
            if (_intersectingColliders.Count == 0) return 0;

            var e = _intersectingColliders.GetEnumerator();
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

                    var a = ColliderAspect.GetAspect(e.Current);
                    if ((p == null || p(a)) && (!_requiresLineOfSight || this.IsLineOfSight(e.Current)))
                    {
                        lst.Add(a);
                        cnt++;
                    }
                }
            }
            finally
            {
                if (doclean) this.CleanColliders();
            }
            return cnt;
        }
        
        public override IEnumerable<IAspect> SenseAll(System.Func<IAspect, bool> p = null)
        {
            bool doclean = false;
            try
            {
                if (p == null && !_requiresLineOfSight)
                {
                    return _intersectingColliders.Where(o =>
                    {
                        if (!ObjUtil.IsObjectAlive(o))
                        {
                            doclean = true;
                            return false;
                        }
                        return true;
                    }).Select(o => ColliderAspect.GetAspect(o)).ToArray();
                }
                else
                {
                    return _intersectingColliders.Where(o =>
                    {
                        if (!ObjUtil.IsObjectAlive(o))
                        {
                            doclean = true;
                            return false;
                        }
                        return true;
                    }).Select(o => ColliderAspect.GetAspect(o))
                      .Where(a => (p == null || p(a)) && (!_requiresLineOfSight || this.IsLineOfSight(a.Collider)))
                      .ToArray();
                }
            }
            finally
            {
                if (doclean) this.CleanColliders();
            }
        }

        public override int SenseAll<T>(ICollection<T> lst, System.Func<T, bool> p = null)
        {
            if (lst == null) throw new System.ArgumentNullException("lst");
            if (lst.IsReadOnly) throw new System.ArgumentException("List to fill can not be read-only.", "lst");
            if (_intersectingColliders.Count == 0) return 0;

            var e = _intersectingColliders.GetEnumerator();
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

                    var a = ColliderAspect.GetAspect(e.Current);
                    var o = ObjUtil.GetAsFromSource<T>(a);
                    if (o != null && (p == null || p(o)) && (!_requiresLineOfSight || this.IsLineOfSight(e.Current)))
                    {
                        lst.Add(e.Current as T);
                        cnt++;
                    }
                }
            }
            finally
            {
                if (doclean) this.CleanColliders();
            }
            return cnt;
        }

        #endregion

    }

}
