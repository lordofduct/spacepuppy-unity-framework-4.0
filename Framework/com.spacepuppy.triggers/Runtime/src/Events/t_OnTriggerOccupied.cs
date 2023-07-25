#pragma warning disable 0649 // variable declared but not used.
using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Geom;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Events
{

    public sealed class t_OnTriggerOccupied : SPComponent, ICompoundTriggerEnterHandler, ICompoundTriggerExitHandler, IOccupiedTrigger
    {

        #region Fields

        [SerializeField]
        private EventActivatorMaskRef _mask = new EventActivatorMaskRef();

        [SerializeField]
        private bool _reduceOccupantsToEntityRoot = false;
        
        [SerializeField]
        [SPEvent.Config("occupying object (GameObject)")]
        private SPEvent _onTriggerOccupied = new SPEvent("OnTriggerOccupied");

        [SerializeField]
        [SPEvent.Config("occupying object (GameObject)")]
        private SPEvent _onTriggerLastExited = new SPEvent("OnTriggerLastExited");
        
        [System.NonSerialized]
        private HashSet<GameObject> _activeObjects = new HashSet<GameObject>();
        [System.NonSerialized]
        private bool _usesCompoundTrigger;

        #endregion

        #region CONSTRUCTOR

        protected override void Start()
        {
            base.Start();

            this.ResolveCompoundTrigger();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            this.ResolveCompoundTrigger();
        }

        #endregion

        #region Properties

        public SPEvent OnTriggerOccupied
        {
            get { return _onTriggerOccupied; }
        }

        public SPEvent OnTriggerLastExited
        {
            get { return _onTriggerLastExited; }
        }
        
        public IEventActivatorMask Mask
        {
            get { return _mask.Value; }
            set { _mask.Value = value; }
        }

        public bool ReduceOccupantsToEntityRoot
        {
            get => _reduceOccupantsToEntityRoot;
            set => _reduceOccupantsToEntityRoot = value;
        }

        public bool IsOccupied
        {
            get
            {
                this.CleanActive();
                return _activeObjects.Count > 0;
            }
        }

        [ShowNonSerializedProperty("Uses Compound Trigger", ShowAtEditorTime = true, ShowOutsideRuntimeValuesFoldout = true)]
        public bool UsesCompoundTrigger
        {
            get
            {
#if UNITY_EDITOR
                if (!Application.isPlaying) return this.HasComponent<ICompoundTrigger>() || !(this.HasComponent<Collider>() || this.HasComponent<Rigidbody>());
#endif
                return _usesCompoundTrigger;
            }
        }

        #endregion

        #region Methods

        public void ResolveCompoundTrigger()
        {
            //we assume CompoundTrigger if we have one OR if we don't have anything that can signal OnTriggerEnter attached to us.
            _usesCompoundTrigger = this.HasComponent<ICompoundTrigger>() || !(this.HasComponent<Collider>() || this.HasComponent<Rigidbody>());
        }

        private void AddObject(GameObject obj)
        {
            if (_mask.Value != null && !_mask.Value.Intersects(obj)) return;

            if (_activeObjects.Count == 0)
            {
                _activeObjects.Add(obj);
                _onTriggerOccupied.ActivateTrigger(this, _reduceOccupantsToEntityRoot ? obj.FindRoot() : obj);
            }
            else
            {
                this.CleanActive();
                _activeObjects.Add(obj);
            }
        }

        private void RemoveObject(GameObject obj)
        {
            if ((_activeObjects.Remove(obj) || this.CleanActive() > 0)
                && _activeObjects.Count == 0)
            {
                _onTriggerLastExited.ActivateTrigger(this, _reduceOccupantsToEntityRoot ? obj.FindRoot() : obj);
            }
        }

        //clean up any potentially lost colliders since Unity doesn't signal OnTriggerExit if a collider is destroyed/disabled.
        private int CleanActive()
        {
            return _activeObjects.RemoveWhere(o => !ObjUtil.IsObjectAlive(o) || !o.activeInHierarchy);
        }

        #endregion

        #region Messages

        void OnTriggerEnter(Collider other)
        {
            if (!this.isActiveAndEnabled || _usesCompoundTrigger || other == null) return;

            this.AddObject(other.gameObject);
        }

        void OnTriggerExit(Collider other)
        {
            if (!this.isActiveAndEnabled || _usesCompoundTrigger || other == null) return;

            this.RemoveObject(other.gameObject);
        }

        void ICompoundTriggerEnterHandler.OnCompoundTriggerEnter(ICompoundTrigger trigger, Collider other)
        {
            if (!this.isActiveAndEnabled || other == null) return;
            this.AddObject(other.gameObject);
        }

        void ICompoundTriggerExitHandler.OnCompoundTriggerExit(ICompoundTrigger trigger, Collider other)
        {
            if (!this.isActiveAndEnabled || other == null) return;
            this.RemoveObject(other.gameObject);
        }

        #endregion
        
        #region IObservableTrigger Interface

        BaseSPEvent[] IObservableTrigger.GetEvents()
        {
            return new BaseSPEvent[] { _onTriggerOccupied, _onTriggerLastExited };
        }

        BaseSPEvent IOccupiedTrigger.EnterEvent
        {
            get { return _onTriggerOccupied; }
        }

        BaseSPEvent IOccupiedTrigger.ExitEvent
        {
            get { return _onTriggerLastExited; }
        }

        #endregion

    }

}
