#pragma warning disable 0649 // variable declared but not used.
using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Geom;
using com.spacepuppy.Utils;
using com.spacepuppy.Collections;

namespace com.spacepuppy.Events
{

    public class t_OnTriggerOccupiedForDuration : SPComponent, ICompoundTriggerEnterHandler, ICompoundTriggerExitHandler, IOccupiedTrigger, IUpdateable
    {

        #region Fields

        [SerializeField]
        private EventActivatorMaskRef _mask = new EventActivatorMaskRef();

        [SerializeField]
        private SPTimePeriod _duration = new SPTimePeriod(1f);

        [SerializeField]
        private bool _reduceOccupantsToEntityRoot = false;

        [SerializeField]
        [SPEvent.Config("occupying object (GameObject)")]
        private SPEvent _onTriggerOccupied = new SPEvent("OnTriggerOccupied");

        [SerializeField]
        [SPEvent.Config("occupying object (GameObject)")]
        private SPEvent _onTriggerOccupiedAfterDuration = new SPEvent("OnTriggerOccupied");

        [SerializeField]
        [SPEvent.Config("occupying object (GameObject)")]
        private SPEvent _onTriggerLastExited = new SPEvent("OnTriggerLastExited");

        [System.NonSerialized]
        private HashSet<GameObject> _intersectingObjects = new HashSet<GameObject>();
        [System.NonSerialized]
        private HashSet<GameObject> _activeObjects = new HashSet<GameObject>();
        [System.NonSerialized]
        private bool _usesCompoundTrigger;

        [System.NonSerialized]
        private bool _activeScannerIsRunning;
        [System.NonSerialized]
        private float _timer;

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

        protected override void OnDisable()
        {
            base.OnDisable();

            this.StopUpdate();
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
            //tracking of all objects
            if (_intersectingObjects.Add(obj) && !_activeScannerIsRunning && this.isActiveAndEnabled)
            {
                this.StartUpdate();
            }

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
            //tracking of all objects
            if (_intersectingObjects.Remove(obj) && _intersectingObjects.Count == 0)
            {
                this.StopUpdate();
            }

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

        private void StartUpdate()
        {
            _activeScannerIsRunning = true;
            if (GameLoop.UpdatePump.Contains(this)) return;

            _timer = 0f;
            GameLoop.UpdatePump.Add(this);
        }

        private void StopUpdate()
        {
            _activeScannerIsRunning = false;
            GameLoop.UpdatePump.Remove(this);
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

        #region IUpdateable Interface

        void IUpdateable.Update()
        {
            //stop if we shouldn't be running
            if (!this.isActiveAndEnabled || _activeObjects.Count == 0)
            {
                this.StopUpdate();
                return;
            }

            //perform scan of current objects
            bool containsActiveObjects = _activeObjects.Count > 0;
            using (var toRemove = TempCollection.GetSet<GameObject>())
            {
                var e = _intersectingObjects.GetEnumerator();
                while (e.MoveNext())
                {
                    if (!ObjUtil.IsObjectAlive(e.Current) || !e.Current.activeInHierarchy)
                    {
                        _activeObjects.Remove(e.Current);
                        toRemove.Add(e.Current);
                        continue;
                    }

                    if (_mask.Value != null)
                    {
                        if (_mask.Value.Intersects(e.Current))
                        {
                            _activeObjects.Add(e.Current);
                        }
                        else
                        {
                            _activeObjects.Remove(e.Current);
                        }
                    }
                }

                if (toRemove.Count > 0)
                {
                    e = toRemove.GetEnumerator();
                    while (e.MoveNext())
                    {
                        _intersectingObjects.Remove(e.Current);
                    }
                }

                //wrap up by firing of appropriate events
                if (_activeObjects.Count == 0 && _intersectingObjects.Count == 0)
                {
                    this.StopUpdate();
                }

                if (containsActiveObjects)
                {
                    if (_activeObjects.Count == 0)
                    {
                        var obj = toRemove.FirstOrDefault();
                        _onTriggerLastExited.ActivateTrigger(this, _reduceOccupantsToEntityRoot ? obj.FindRoot() : obj);
                    }
                }
                else if (_activeObjects.Count > 0)
                {
                    var obj = _activeObjects.FirstOrDefault();
                    _timer = 0f;
                    _onTriggerOccupied.ActivateTrigger(this, _reduceOccupantsToEntityRoot ? obj.FindRoot() : obj);
                }
            }

            if (_timer < _duration.Seconds && _activeObjects.Count > 0)
            {
                _timer += _duration.TimeSupplierOrDefault.Delta;
                if (_timer >= _duration.Seconds)
                {
                    _onTriggerOccupiedAfterDuration.ActivateTrigger(this, null);
                }
            }
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
