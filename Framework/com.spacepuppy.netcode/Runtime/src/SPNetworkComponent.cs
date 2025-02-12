using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

using com.spacepuppy.Utils;

namespace com.spacepuppy
{

    public abstract class SPNetworkComponent : NetworkBehaviour, IEventfulComponent, ISPDisposable
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
            try
            {
                this.OnStarted?.Invoke(this, System.EventArgs.Empty);
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
            if (this.IsSpawned)
            {
                try
                {
                    this.OnStartOrNetworkSpawn();
                }
                catch (System.Exception ex)
                {
                    Debug.LogException(ex);
                }
                try
                {
                    this.OnStartOrEnableOrNetworkSpawn();
                }
                catch (System.Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        protected virtual void OnEnable()
        {
            try
            {
                this.OnEnabled?.Invoke(this, System.EventArgs.Empty);
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
            if (this.started && this.IsSpawned)
            {
                try
                {
                    this.OnStartOrEnableOrNetworkSpawn();
                }
                catch (System.Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        protected virtual void OnDisable()
        {
            try
            {
                this.OnDisabled?.Invoke(this, System.EventArgs.Empty);
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            try
            {
                this.ComponentDestroyed?.Invoke(this, System.EventArgs.Empty);
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (this.started)
            {
                try
                {
                    this.OnStartOrNetworkSpawn();
                }
                catch (System.Exception ex)
                {
                    Debug.LogException(ex);
                }
                try
                {
                    this.OnStartOrEnableOrNetworkSpawn();
                }
                catch (System.Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        protected virtual void OnStartOrNetworkSpawn()
        {

        }

        protected virtual void OnStartOrEnableOrNetworkSpawn()
        {

        }

        #endregion

        #region Properties

        /// <summary>
        /// Start has been called on this component.
        /// </summary>
        public bool started { get; private set; }

        #endregion

        #region Methods

        protected void RegisterMixins(IEnumerable<IMixin> mixins)
        {
            if (mixins == null) throw new System.ArgumentNullException(nameof(mixins));
            foreach (var mixin in mixins)
            {
                if (mixin.Awake(this))
                {
                    (_mixins = _mixins ?? new List<IMixin>()).Add(mixin);
                }
            }
        }

        protected void RegisterMixin(IMixin mixin)
        {
            if (mixin == null) throw new System.ArgumentNullException(nameof(mixin));

            if (mixin.Awake(this))
            {
                (_mixins = _mixins ?? new List<IMixin>()).Add(mixin);
            }
        }

        public T GetMixinState<T>() where T : class, IMixin
        {
            if (_mixins != null)
            {
                for (int i = 0; i < _mixins.Count; i++)
                {
                    if (_mixins[i] is T) return _mixins[i] as T;
                }
            }
            return null;
        }

        /// <summary>
        /// This should only be used if you're not using RadicalCoroutine. If you are, use StopAllRadicalCoroutines instead.
        /// </summary>
        public new void StopAllCoroutines()
        {
            //this is an attempt to capture this method, it's not guaranteed and honestly you should avoid calling StopAllCoroutines all together and instead call StopAllRadicalCoroutines.
            this.SendMessage("RadicalCoroutineManager_InternalHook_StopAllCoroutinesCalled", this, SendMessageOptions.DontRequireReceiver);
            base.StopAllCoroutines();
        }

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

    public abstract class NetworkServiceComponent<T> : SPNetworkComponent, IService where T : class, IService
    {

        #region Fields

        [SerializeField]
        private ServiceRegistrationOptions _serviceRegistrationOptions;

        #endregion

        #region CONSTRUCTOR

        public NetworkServiceComponent()
        {

        }

        public NetworkServiceComponent(Services.AutoRegisterOption autoRegister, Services.MultipleServiceResolutionOption multipleServiceResolution, UnregisterResolutionOption unregisterResolution = UnregisterResolutionOption.DoNothing)
        {
            _serviceRegistrationOptions.AutoRegisterService = autoRegister;
            _serviceRegistrationOptions.MultipleServiceResolution = multipleServiceResolution;
            _serviceRegistrationOptions.UnregisterResolution = unregisterResolution;
        }

        protected override void Awake()
        {
            base.Awake();

            if (!(this is T))
            {
                if (_serviceRegistrationOptions.MultipleServiceResolution == Services.MultipleServiceResolutionOption.UnregisterSelf)
                {
                    (this as IService).OnServiceUnregistered();
                }
                return;
            }

            if (this.ValidateService())
            {
                this.OnValidAwake();
            }
            else
            {
                this.OnFailedAwake();
            }
        }

        private bool ValidateService() => Services.ValidateService<T>(this as T, _serviceRegistrationOptions.AutoRegisterService, _serviceRegistrationOptions.MultipleServiceResolution);

        protected virtual void OnValidAwake()
        {

        }

        protected virtual void OnFailedAwake()
        {

        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            if (this is T s) Services.TryUnregister<T>(s);
        }

        #endregion

        #region Properties

        public Services.AutoRegisterOption AutoRegister
        {
            get { return _serviceRegistrationOptions.AutoRegisterService; }
            set
            {
                _serviceRegistrationOptions.AutoRegisterService = value;
                if (value > Services.AutoRegisterOption.DoNothing && this.started) this.ValidateService();
            }
        }

        public Services.MultipleServiceResolutionOption OnCreateOption
        {
            get { return _serviceRegistrationOptions.MultipleServiceResolution; }
            set
            {
                _serviceRegistrationOptions.MultipleServiceResolution = value;
                if (_serviceRegistrationOptions.MultipleServiceResolution > Services.MultipleServiceResolutionOption.DoNothing && this.started) this.ValidateService();
            }
        }

        public UnregisterResolutionOption UnregisterResolution
        {
            get { return _serviceRegistrationOptions.UnregisterResolution; }
            set { _serviceRegistrationOptions.UnregisterResolution = value; }
        }

        #endregion

        #region IService Interface

        public event System.EventHandler ServiceUnregistered;

        void IService.OnServiceRegistered(System.Type serviceTypeRegisteredAs)
        {
            this.OnServiceRegistered(serviceTypeRegisteredAs);
        }

        protected virtual void OnServiceRegistered(System.Type serviceTypeRegisteredAs)
        {

        }

        void IService.OnServiceUnregistered()
        {
            this.ServiceUnregistered?.Invoke(this, System.EventArgs.Empty);
            this.OnServiceUnregistered();
        }

        protected virtual void OnServiceUnregistered()
        {
            switch (_serviceRegistrationOptions.UnregisterResolution)
            {
                case UnregisterResolutionOption.DespawnNetworkObject:
                    if (this.NetworkObject)
                    {
                        if (this.NetworkObject.IsServer())
                        {
                            if (this.NetworkObject.IsSpawned)
                            {
                                this.NetworkObject.Despawn();
                            }
                            else
                            {
                                ObjUtil.SmartDestroy(this.NetworkObject.gameObject);
                            }
                        }
                        else if (this.NetworkObject.IsOffline())
                        {
                            ObjUtil.SmartDestroy(this.NetworkObject.gameObject);
                        }
                    }
                    break;
                case UnregisterResolutionOption.DisableSelf:
                    this.enabled = false;
                    break;
            }
        }

        #endregion

        #region Special Types

        [System.Serializable]
        public struct ServiceRegistrationOptions
        {

            [SerializeField]
            public Services.AutoRegisterOption AutoRegisterService;
            [SerializeField]
            public Services.MultipleServiceResolutionOption MultipleServiceResolution;
            [SerializeField]
            public UnregisterResolutionOption UnregisterResolution;

        }

        public enum UnregisterResolutionOption
        {
            DoNothing = 0,
            //
            DespawnNetworkObject = 2,
            DisableSelf = 3,
        }

        #endregion

    }

}
