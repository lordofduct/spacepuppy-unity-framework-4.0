using UnityEngine;
using UnityEngine.EventSystems;

using com.spacepuppy.Utils;

namespace com.spacepuppy.UI
{

    [RequireComponent(typeof(EventSystem))]
    public class SPEventSystem : ServiceComponent<IEventSystem>, IEventSystem
    {

        #region Fields

        [System.NonSerialized]
        private System.Action _registrarCallback;
        [System.NonSerialized]
        private StandardSelectedUIElementChangedProcessor _processor;

        [System.NonSerialized]
        private EventSystem _eventSystem;

        #endregion

        #region CONSTRUCTOR

        public SPEventSystem() : base(Services.AutoRegisterOption.Register, Services.MultipleServiceResolutionOption.UnregisterSelf, Services.UnregisterResolutionOption.DestroySelf)
        {

        }

        public SPEventSystem(Services.AutoRegisterOption autoRegister, Services.MultipleServiceResolutionOption multipleServiceResolution, Services.UnregisterResolutionOption unregisterResolution)
            : base(autoRegister, multipleServiceResolution, unregisterResolution)
        {

        }

        protected override void Awake()
        {
            _eventSystem = this.GetComponent<EventSystem>();
            base.Awake();
        }

        #endregion

        #region Properties

        public EventSystem EventSystem => _eventSystem;

        #endregion

        #region Methods

        EventSystem IEventSystem.GetEventSystem(object context) => _eventSystem;

        protected override void OnServiceRegistered(System.Type serviceTypeRegisteredAs)
        {
            if (_registrarCallback == null) _registrarCallback = () => _processor?.AttemptActivate();
            Messaging.RemoveHasRegisteredGlobalListenerChangedCallback<ISelectedUIElementChangedGlobalHandler>(_registrarCallback);
            Messaging.AddHasRegisteredGlobalListenerChangedCallback<ISelectedUIElementChangedGlobalHandler>(_registrarCallback);
            StandardSelectedUIElementChangedProcessor.SelectedBubblingListenerCountChanged -= _registrarCallback;
            StandardSelectedUIElementChangedProcessor.SelectedBubblingListenerCountChanged += _registrarCallback;

            (_processor ??= new StandardSelectedUIElementChangedProcessor()).AttemptActivate();
        }

        protected override void OnServiceUnregistered()
        {
            if (_registrarCallback != null)
            {
                Messaging.RemoveHasRegisteredGlobalListenerChangedCallback<ISelectedUIElementChangedGlobalHandler>(_registrarCallback);
                StandardSelectedUIElementChangedProcessor.SelectedBubblingListenerCountChanged -= _registrarCallback;
            }

            _processor?.Deactivate();
        }

        #endregion

    }

    internal class BasicSPUIEventSystemService : ServiceObject<IEventSystem>, IEventSystem
    {

        #region Static Initializer

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        static void Init()
        {
            var obj = Services.SPInternal_GetDefaultService<IEventSystem>();
            if (obj == null || obj.GetType().Name == "BasicCoreEventSystemService")
            {
                Services.SPInternal_RegisterDefaultService<IEventSystem>(new BasicSPUIEventSystemService());
            }
        }

        #endregion

        #region Fields

        private System.Action _registrarCallback;
        private StandardSelectedUIElementChangedProcessor _processor;

        #endregion

        #region Init/Deinit

        protected override void OnServiceRegistered(System.Type serviceTypeRegisteredAs)
        {
            if (_registrarCallback == null) _registrarCallback = () => _processor?.AttemptActivate();
            Messaging.RemoveHasRegisteredGlobalListenerChangedCallback<ISelectedUIElementChangedGlobalHandler>(_registrarCallback);
            Messaging.AddHasRegisteredGlobalListenerChangedCallback<ISelectedUIElementChangedGlobalHandler>(_registrarCallback);
            StandardSelectedUIElementChangedProcessor.SelectedBubblingListenerCountChanged -= _registrarCallback;
            StandardSelectedUIElementChangedProcessor.SelectedBubblingListenerCountChanged += _registrarCallback;

            (_processor ??= new StandardSelectedUIElementChangedProcessor()).AttemptActivate();
        }

        protected override void OnServiceUnregistered()
        {
            if (_registrarCallback != null)
            {
                Messaging.RemoveHasRegisteredGlobalListenerChangedCallback<ISelectedUIElementChangedGlobalHandler>(_registrarCallback);
                StandardSelectedUIElementChangedProcessor.SelectedBubblingListenerCountChanged -= _registrarCallback;
            }

            _processor?.Deactivate();
        }

        #endregion

    }

}
