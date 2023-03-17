#pragma warning disable 0649 // variable declared but not used.
using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Collections;
using com.spacepuppy.Geom;
using com.spacepuppy.Utils;
using System.Linq;

namespace com.spacepuppy.Events
{

    [Infobox("Similar to t_OnTriggerOccupied, only that it can perform active scanning of the region.\n" +
             "Note that when active it doesn't necessarily make sense to have 'no mask'. It would behave effectively like t_OnOccupiedTrigger but with added overhead. So either disable the scanning, or use t_OnOccupiedTrigger instead.")]
    public sealed class t_OnTriggerOccupiedActive : SPComponent, ICompoundTriggerEnterHandler, ICompoundTriggerExitHandler, IOccupiedTrigger, IUpdateable
    {

        #region Fields

        [SerializeField]
        private EventActivatorMaskRef _mask = new EventActivatorMaskRef();

        [SerializeField]
        private bool _reduceOccupantsToEntityRoot = false;

        [SerializeField]
        [Tooltip("If true, when an object is intersected we'll habitually retest the object in case its state changes for any reason.")]
        private bool _activelyScanning = true;
        [SerializeField]
        [Tooltip("Interval at which the scan occurs, leave at 0 to scan every frame. This is in 'real time' unaffected by timeScale.")]
        private float _activeScanInterval;

        [SerializeField]
        [SPEvent.Config("first occupying object (GameObject)")]
        private SPEvent _onTriggerOccupied = new SPEvent("OnTriggerOccupied");

        [SerializeField]
        [SPEvent.Config("random occupying object (GameObject) - only use if your mask filters to 1 potential target")]
        private SPEvent _onUpdate = new SPEvent("OnUpdate");

        [SerializeField]
        [SPEvent.Config("last occupying object (GameObject)")]
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
            if (_activelyScanning && _activeObjects.Count > 0) this.StartUpdate();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            this.StopUpdate();
        }

        #endregion

        #region Properties

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

        public bool ActivelyScanning
        {
            get { return _activelyScanning; }
            set
            {
                if (_activelyScanning == value) return;
                _activelyScanning = value;
                if (_activelyScanning)
                {
                    if (this.isActiveAndEnabled && _activeObjects.Count > 0) this.StartUpdate();
                }
                else
                {
                    this.StopUpdate();
                }
            }
        }

        public SPEvent OnTriggerOccupied
        {
            get { return _onTriggerOccupied; }
        }

        public SPEvent OnTriggerLastExited
        {
            get { return _onTriggerLastExited; }
        }

        public bool IsOccupied
        {
            get { return _activeObjects.Count > 0; }
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

        private void AddObject(GameObject obj)
        {
            //tracking of all objects
            if (_intersectingObjects.Add(obj) && _activelyScanning && !_activeScannerIsRunning && this.isActiveAndEnabled)
            {
                this.StartUpdate();
            }

            //tracking of active objects that signal event
            if (_mask.Value != null && !_mask.Value.Intersects(obj)) return;

            if (_activeObjects.Count == 0)
            {
                _activeObjects.Add(obj);
                _onTriggerOccupied.ActivateTrigger(this, _reduceOccupantsToEntityRoot ? obj.FindRoot() : obj);
            }
            else
            {
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

            //remove active objects
            if (_activeObjects.Remove(obj) && _activeObjects.Count == 0)
            {
                _onTriggerLastExited.ActivateTrigger(this, _reduceOccupantsToEntityRoot ? obj.FindRoot() : obj);
            }
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

        public void ResolveCompoundTrigger()
        {
            //we assume CompoundTrigger if we have one OR if we don't have anything that can signal OnTriggerEnter attached to us.
            _usesCompoundTrigger = this.HasComponent<ICompoundTrigger>() || !(this.HasComponent<Collider>() || this.HasComponent<Rigidbody>());
        }

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
            if (!_activelyScanning || !this.isActiveAndEnabled || _activeObjects.Count == 0)
            {
                this.StopUpdate();
                return;
            }

            //check timer
            if (_activeScanInterval > 0f)
            {
                _timer += Time.unscaledDeltaTime;
                if (_timer < _activeScanInterval) return;
            }
            _timer = 0f;

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
                    else if (_onUpdate.HasReceivers)
                    {
                        var obj = _activeObjects.FirstOrDefault();
                        _onUpdate.ActivateTrigger(this, _reduceOccupantsToEntityRoot ? obj.FindRoot() : obj);
                    }
                }
                else if (_activeObjects.Count > 0)
                {
                    var obj = _activeObjects.FirstOrDefault();
                    _onTriggerOccupied.ActivateTrigger(this, _reduceOccupantsToEntityRoot ? obj.FindRoot() : obj);
                }
            }
        }

        #endregion

        #region IOccupiedObservableTrigger Interface

        BaseSPEvent[] IObservableTrigger.GetEvents()
        {
            return new SPEvent[] { _onTriggerOccupied, _onTriggerLastExited };
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
