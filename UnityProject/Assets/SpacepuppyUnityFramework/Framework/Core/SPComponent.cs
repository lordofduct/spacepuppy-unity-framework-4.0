using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy.Utils;

namespace com.spacepuppy
{

    /// <summary>
    /// A base implementation of components used by most of Spacepuppy Framework. It expands on the functionality of MonoBehaviour as well as implements various interfaces from the Spacepuppy framework. 
    /// 
    /// All scripts that are intended to work in tandem with Spacepuppy Unity Framework should inherit from this instead of MonoBehaviour.
    /// </summary>
    public abstract class SPComponent : MonoBehaviour, IEventfulComponent, ISPDisposable
    {

        #region Events

        public event System.EventHandler OnEnabled;
        public event System.EventHandler OnStarted;
        public event System.EventHandler OnDisabled;
        public event System.EventHandler ComponentDestroyed;

        #endregion

        #region Fields
        
        [System.NonSerialized]
        private List<IMixin> _mixins;

        #endregion

        #region CONSTRUCTOR

        protected virtual void Awake()
        {
            if (this is IAutoMixinDecorator) this.RegisterMixins(MixinUtil.CreateAutoMixins(this as IAutoMixinDecorator));
        }

        protected virtual void Start()
        {
            this.started = true;
            this.OnStarted?.Invoke(this, System.EventArgs.Empty);
        }
        
        protected virtual void OnEnable()
        {
            this.OnEnabled?.Invoke(this, System.EventArgs.Empty);
        }

        protected virtual void OnDisable()
        {
            this.OnDisabled?.Invoke(this, System.EventArgs.Empty);
        }
        
        protected virtual void OnDestroy()
        {
            this.ComponentDestroyed?.Invoke(this, System.EventArgs.Empty);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Start has been called on this component.
        /// </summary>
        public bool started { get; private set; }

        #endregion

        #region Methods

        public void RegisterMixins(IEnumerable<IMixin> mixins)
        {
            if (mixins == null) throw new System.ArgumentNullException(nameof(mixins));
            foreach(var mixin in mixins)
            {
                this.RegisterMixin(mixin);
            }
        }

        public void RegisterMixin(IMixin mixin)
        {
            if (mixin == null) throw new System.ArgumentNullException(nameof(mixin));

            if(mixin.Awake(this))
            {
                (_mixins = _mixins ?? new List<IMixin>()).Add(mixin);
            }
        }

        //TODO - RadicalCoroutine
        //public new void StopAllCoroutines()
        //{
        //    RadicalCoroutineManager manager = this.GetComponent<RadicalCoroutineManager>();
        //    if (manager != null)
        //    {
        //        manager.PurgeCoroutines(this);
        //    }
        //    base.StopAllCoroutines();
        //}

        #endregion

        #region IComponent Interface

        bool IComponent.enabled
        {
            get { return this.enabled; }
            set { this.enabled = value; }
        }

        Component IComponent.component
        {
            get { return this; }
        }

        //implemented implicitly
        /*
        GameObject IComponent.gameObject { get { return this.gameObject; } }
        Transform IComponent.transform { get { return this.transform; } }
        */

        #endregion

        #region ISPDisposable Interface

        bool ISPDisposable.IsDisposed
        {
            get
            {
                return !ObjUtil.IsObjectAlive(this);
            }
        }

        void System.IDisposable.Dispose()
        {
            ObjUtil.SmartDestroy(this);
        }

        #endregion

    }

    /// <summary>
    /// Represents a component that should always exist as a member of an entity.
    /// 
    /// Such a component should not change parents frequently as it would be expensive.
    /// </summary>
    public abstract class SPEntityComponent : SPComponent
    {

        #region Fields

        [System.NonSerialized]
        private SPEntity _entity;
        [System.NonSerialized]
        private GameObject _entityRoot;
        [System.NonSerialized]
        private bool _synced;

        #endregion

        #region Properties

        public SPEntity Entity
        {
            get
            {
                if (!_synced) this.SyncRoot();
                return _entity;
            }
        }

        public GameObject entityRoot
        {
            get
            {
                if (!_synced) this.SyncRoot();
                return _entityRoot;
            }
        }

        #endregion

        #region Methods

        protected virtual void OnTransformParentChanged()
        {
            _synced = false;
            _entity = null;
            _entityRoot = null;
        }

        protected void SyncRoot()
        {
            _synced = true;
            _entity = SPEntity.Pool.GetFromSource(this);
            _entityRoot = (_entity != null) ? _entity.gameObject : this.gameObject;
        }

        #endregion

    }

}
