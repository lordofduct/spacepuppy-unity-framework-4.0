#pragma warning disable 0649 // variable declared but not used.

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using com.spacepuppy.Utils;
using Type = System.Type;
using System.Runtime.CompilerServices;
using System.CodeDom;

namespace com.spacepuppy
{

    /// <summary>
    /// A special type of Singleton accessible by the Services static interface.
    /// 
    /// A service should implement some interface and is registered with Services as that interface. 
    /// This allows for a service to be accessed like a singleton, but implemented as an interface. 
    /// 
    /// See IInputManager, ICameraManager, and ISceneManager for examples.
    /// </summary>
    public interface IService
    {
        event System.EventHandler ServiceUnregistered;

        void OnServiceRegistered(System.Type serviceTypeRegisteredAs);
        void OnServiceUnregistered();
    }

    /// <summary>
    /// Access point for all registered Services.
    /// Services are registered by type, generally an interface type. When calling 'Get' you must use the type it was registered as, not the concrete type that it is. Instead use 'Find' for that.
    /// 
    /// Note - the reflected System.Type versions of these methods will not work on AOT systems.
    /// </summary>
    public static class Services
    {

        public enum AutoRegisterOption
        {
            DoNothing = 0,
            Register = 1,
            RegisterAndPersist = 2,
        }

        public enum MultipleServiceResolutionOption
        {
            DoNothing = 0,
            UnregisterSelf = 1,
            UnregisterOther = 2,
        }

        public enum UnregisterResolutionOption
        {
            DoNothing = 0,
            DestroySelf = 1,
            DestroyGameObject = 2,
        }

        #region Fields

        private static readonly HashSet<IService> _services = new HashSet<IService>();

        #endregion

        #region Methods

        public static bool Exists<T>() where T : class, IService
        {
            return !Entry<T>.Instance.IsNullOrDestroyed();
        }

        public static T Get<T>() where T : class, IService
        {
            var result = Entry<T>.Instance;
            if (!object.ReferenceEquals(result, null) && result.IsNullOrDestroyed())
            {
                Entry<T>.Instance = null;
                _services.Remove(result);
                result = null;
            }
            return result;
        }

        public static TConcrete Get<TService, TConcrete>() where TService : class, IService
                                                           where TConcrete : class, TService
        {
            return Get<TService>() as TConcrete;
        }

        public static T Find<T>() where T : class, IService
        {
            var e = _services.GetEnumerator();
            while (e.MoveNext())
            {
                if (e.Current is T && e.Current.IsAlive())
                {
                    return e.Current as T;
                }
            }
            return default(T);
        }

        public static bool TryRegister<T>(T service, bool donotSignalRegister = false) where T : class, IService
        {
            var other = Entry<T>.Instance;
            if (!other.IsNullOrDestroyed()) return false;

            Entry<T>.Instance = service;
            _services.Add(service);
            if (!donotSignalRegister)
            {
                try
                {
                    service.OnServiceRegistered(typeof(T));
                }
                catch (System.Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
            return true;
        }

        public static void Register<T>(T service, bool donotSignalRegister = false) where T : class, IService
        {
            var other = Entry<T>.Instance;
            if (!other.IsNullOrDestroyed())
            {
                if (object.ReferenceEquals(other, service)) return;
                throw new System.InvalidOperationException("You must first unregister a service before registering a new one.");
            }

            Entry<T>.Instance = service;
            _services.Add(service);
            if (!donotSignalRegister)
            {
                try
                {
                    service.OnServiceRegistered(typeof(T));
                }
                catch (System.Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        public static void Unregister<T>(bool donotSignalUnregister = false) where T : class, IService
        {
            var inst = Entry<T>.Instance;
            if (!object.ReferenceEquals(inst, null))
            {
                Entry<T>.Instance = null;
                _services.Remove(inst);
                if (!inst.IsNullOrDestroyed())
                {
                    if (!donotSignalUnregister)
                    {
                        inst.OnServiceUnregistered();
                    }
                }
            }
        }

        public static bool TryUnregister<T>(T service, bool donotSignalUnregister = false) where T : class, IService
        {
            var inst = Entry<T>.Instance;
            if (object.ReferenceEquals(inst, service) && !object.ReferenceEquals(inst, null))
            {
                Entry<T>.Instance = null;
                _services.Remove(inst);
                if (!inst.IsNullOrDestroyed())
                {
                    if (!donotSignalUnregister)
                    {
                        inst.OnServiceUnregistered();
                    }
                }
                return true;
            }

            return false;
        }

        public static object Get(Type tp)
        {
            if (tp == null) throw new System.ArgumentNullException("tp");
            if (!typeof(IService).IsAssignableFrom(tp)) throw new System.ArgumentException("Type is not a IService");

            object result = null;
            try
            {
                var klass = typeof(Entry<>);
                klass.MakeGenericType(tp);
                var field = klass.GetField("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                result = field.GetValue(null);

                if (!object.ReferenceEquals(result, null) && result.IsNullOrDestroyed())
                {
                    field.SetValue(null, null);
                    if (result is IService serv) _services.Remove(serv);
                    result = null;
                }
            }
            catch (System.Exception ex)
            {
                throw new System.InvalidOperationException("Failed to resolve type '" + tp.Name + "'.", ex);
            }

            return result;
        }

        /// <summary>
        /// Unlike 'Get', this will search all services for the one that is of type 'tp'.
        /// </summary>
        /// <param name="tp"></param>
        /// <returns></returns>
        public static object Find(Type tp)
        {
            var e = _services.GetEnumerator();
            while (e.MoveNext())
            {
                if (e.Current.IsAlive() && TypeUtil.IsType(e.Current.GetType(), tp))
                {
                    return e.Current;
                }
            }
            return null;
        }

        public static void Register(System.Type tp, IService service)
        {
            if (tp == null) throw new System.ArgumentNullException("tp");
            if (!tp.IsClass || tp.IsAbstract || !typeof(IService).IsAssignableFrom(tp)) throw new System.ArgumentException("Type must be a concrete class that implements IService.", "tp");


            System.Reflection.FieldInfo field;
            try
            {
                var klass = typeof(Entry<>);
                klass = klass.MakeGenericType(tp);
                field = klass.GetField("Instance", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            }
            catch (System.Exception ex)
            {
                throw new System.InvalidOperationException("Failed to resolve type '" + tp.Name + "'.", ex);
            }

            if (field == null) throw new System.InvalidOperationException("Failed to resolve type '" + tp.Name + "'.");

            var other = field.GetValue(null);
            if (!other.IsNullOrDestroyed() && !object.ReferenceEquals(other, service)) throw new System.InvalidOperationException("You must first unregister a service before registering a new one.");
            field.SetValue(null, service);

            try
            {
                service.OnServiceRegistered(tp);
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public static void Unregister(System.Type tp, bool donotSignalUnregister = false)
        {
            if (tp == null) throw new System.ArgumentNullException("tp");
            if (!tp.IsClass || tp.IsAbstract || !typeof(IService).IsAssignableFrom(tp)) throw new System.ArgumentException("Type must be a concrete class that implements IService.", "tp");

            IService inst;
            try
            {
                var klass = typeof(Entry<>);
                klass = klass.MakeGenericType(tp);
                var field = klass.GetField("Instance", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                inst = field.GetValue(null) as IService;
                if (inst != null)
                    field.SetValue(null, null);
            }
            catch (System.Exception ex)
            {
                throw new System.InvalidOperationException("Failed to resolve type '" + tp.Name + "'.", ex);
            }

            if (!donotSignalUnregister && inst != null)
            {
                inst.OnServiceUnregistered();
            }
        }


        public static TConcrete Create<TServiceType, TConcrete>(bool persistent = false, string name = null) where TServiceType : class, IService
                                                                                                                where TConcrete : class, TServiceType
        {
            var inst = Services.Get<TServiceType>();
            if (inst != null) throw new System.InvalidOperationException(string.Format("A service of type '{0}' already exists.", typeof(TServiceType).Name));

            var tp = typeof(TConcrete);
            if (typeof(Component).IsAssignableFrom(tp))
            {
                var obj = ServiceComponent<TServiceType>.Create(tp, persistent, name) as TConcrete;
                Services.Register<TServiceType>(obj);
                return obj;
            }
            else if (typeof(ScriptableObject).IsAssignableFrom(tp))
            {
                var obj = ServiceScriptableObject<TServiceType>.Create(tp, persistent, name) as TConcrete;
                Services.Register<TServiceType>(obj);
                return obj;
            }
            else
            {
                try
                {
                    var obj = System.Activator.CreateInstance<TConcrete>();
                    Services.Register<TServiceType>(obj);
                    return obj;
                }
                catch (System.Exception ex)
                {
                    throw new System.InvalidOperationException("Supplied concrete service type failed to construct.", ex);
                }
            }
        }

        public static T Create<T>(bool persistent = false, string name = null) where T : class, IService
        {
            var inst = Services.Get<T>();
            if (inst != null) throw new System.InvalidOperationException(string.Format("A service of type '{0}' already exists.", typeof(T).Name));

            var tp = typeof(T);
            if (typeof(Component).IsAssignableFrom(tp))
            {
                var obj = ServiceComponent<T>.Create(tp, persistent, name);
                Services.Register<T>(obj);
                return obj;
            }
            else if (typeof(ScriptableObject).IsAssignableFrom(tp))
            {
                var obj = ServiceScriptableObject<T>.Create(tp, persistent, name);
                Services.Register<T>(obj);
                return obj;
            }
            else
            {
                try
                {
                    var obj = System.Activator.CreateInstance<T>();
                    Services.Register<T>(obj);
                    return obj;
                }
                catch (System.Exception ex)
                {
                    throw new System.InvalidOperationException("Supplied concrete service type failed to construct.", ex);
                }
            }
        }

        public static TServiceType Create<TServiceType>(System.Type concreteType, bool persistent = false, string name = null) where TServiceType : class, IService
        {
            if (concreteType == null) throw new System.ArgumentNullException(nameof(concreteType));
            if (!typeof(TServiceType).IsAssignableFrom(concreteType)) throw new System.ArgumentException("Type must implement " + typeof(TServiceType).Name);

            var inst = Services.Get<TServiceType>();
            if (inst != null) throw new System.InvalidOperationException(string.Format("A service of type '{0}' already exists.", typeof(TServiceType).Name));

            if (typeof(Component).IsAssignableFrom(concreteType))
            {
                var obj = ServiceComponent<TServiceType>.Create(concreteType, persistent, name);
                Services.Register<TServiceType>(obj);
                return obj;
            }
            else if (typeof(ScriptableObject).IsAssignableFrom(concreteType))
            {
                var obj = ServiceScriptableObject<TServiceType>.Create(concreteType, persistent, name);
                Services.Register<TServiceType>(obj);
                return obj;
            }
            else
            {
                try
                {
                    var obj = System.Activator.CreateInstance(concreteType) as TServiceType;
                    Services.Register<TServiceType>(obj);
                    return obj;
                }
                catch (System.Exception ex)
                {
                    throw new System.InvalidOperationException("Supplied concrete service type failed to construct.", ex);
                }
            }
        }

        public static TServiceType GetOrCreate<TServiceType, TConcrete>(bool persistent = false, string name = null) where TServiceType : class, IService
                                                                                                                     where TConcrete : TServiceType
        {
            var inst = Services.Get<TServiceType>();
            if (inst != null) return inst;

            var tp = typeof(TConcrete);
            if (typeof(Component).IsAssignableFrom(tp))
            {
                var obj = ServiceComponent<TServiceType>.GetOrCreate(tp, persistent, name);
                Services.Register<TServiceType>(obj);
                return obj;
            }
            else if (typeof(ScriptableObject).IsAssignableFrom(tp))
            {
                var obj = ServiceScriptableObject<TServiceType>.GetOrCreate(tp, persistent, name);
                Services.Register<TServiceType>(obj);
                return obj;
            }
            else
            {
                try
                {
                    var obj = System.Activator.CreateInstance<TConcrete>();
                    Services.Register<TServiceType>(obj);
                    return obj;
                }
                catch (System.Exception ex)
                {
                    throw new System.InvalidOperationException("Supplied concrete service type failed to construct.", ex);
                }
            }
        }

        public static T GetOrCreate<T>(bool persistent = false, string name = null) where T : class, IService
        {
            var inst = Services.Get<T>();
            if (inst != null) return inst;

            var tp = typeof(T);
            if (typeof(Component).IsAssignableFrom(tp))
            {
                var obj = ServiceComponent<T>.GetOrCreate(tp, persistent, name);
                Services.Register<T>(obj);
                return obj;
            }
            else if (typeof(ScriptableObject).IsAssignableFrom(tp))
            {
                var obj = ServiceScriptableObject<T>.GetOrCreate(tp, persistent, name);
                Services.Register<T>(obj);
                return obj;
            }
            else
            {
                try
                {
                    var obj = System.Activator.CreateInstance<T>();
                    Services.Register<T>(obj);
                    return obj;
                }
                catch (System.Exception ex)
                {
                    throw new System.InvalidOperationException("Supplied concrete service type failed to construct.", ex);
                }
            }
        }

        public static TServiceType GetOrCreate<TServiceType>(System.Type concreteType, bool persistent = false, string name = null) where TServiceType : class, IService
        {
            if (concreteType == null) throw new System.ArgumentNullException("concreteType");
            if (!typeof(TServiceType).IsAssignableFrom(concreteType)) throw new System.ArgumentException("Type must implement " + typeof(TServiceType).Name);

            var inst = Services.Get<TServiceType>();
            if (inst != null) return inst;

            if (typeof(Component).IsAssignableFrom(concreteType))
            {
                var obj = ServiceComponent<TServiceType>.Create(concreteType, persistent, name);
                Services.Register<TServiceType>(obj);
                return obj;
            }
            else if (typeof(ScriptableObject).IsAssignableFrom(concreteType))
            {
                var obj = ServiceScriptableObject<TServiceType>.Create(concreteType, persistent, name);
                Services.Register<TServiceType>(obj);
                return obj;
            }
            else
            {
                try
                {
                    var obj = System.Activator.CreateInstance(concreteType) as TServiceType;
                    Services.Register<TServiceType>(obj);
                    return obj;
                }
                catch (System.Exception ex)
                {
                    throw new System.InvalidOperationException("Supplied concrete service type failed to construct.", ex);
                }
            }
        }

        public static TConcrete LoadResourceService<TServiceType, TConcrete>(string path, bool persistent = false) where TServiceType : class, IService
                                                                                                                where TConcrete : UnityEngine.Object, TServiceType
        {
            var inst = Services.Get<TServiceType>();
            if (inst != null) throw new System.InvalidOperationException(string.Format("A service of type '{0}' already exists.", typeof(TServiceType).Name));

            var service = Resources.Load<TConcrete>(path);
            if (service == null)
            {
                throw new System.InvalidOperationException("Supplied concrete service type failed to construct.");
            }
            if (service is Component)
            {
                service = UnityEngine.Object.Instantiate(service);
            }

            Services.Register<TServiceType>(service);
            if (persistent && service is Component c)
            {
                UnityEngine.Object.DontDestroyOnLoad(c.gameObject);
            }
            return service;
        }

        public static T LoadResourceService<T>(string path, bool persistent = false) where T : UnityEngine.Object, IService
        {
            var inst = Services.Get<T>();
            if (inst != null) throw new System.InvalidOperationException(string.Format("A service of type '{0}' already exists.", typeof(T).Name));

            var service = Resources.Load<T>(path);
            if (service == null)
            {
                throw new System.InvalidOperationException("Supplied concrete service type failed to construct.");
            }
            if (service is Component)
            {
                service = UnityEngine.Object.Instantiate(service);
            }

            Services.Register<T>(service);
            if (persistent && service is Component c)
            {
                UnityEngine.Object.DontDestroyOnLoad(c.gameObject);
            }
            return service;
        }

        public static TServiceType LoadResourceService<TServiceType>(System.Type concreteType, string path, bool persistent = false) where TServiceType : class, IService
        {
            if (concreteType == null) throw new System.ArgumentNullException(nameof(concreteType));
            if (!typeof(TServiceType).IsAssignableFrom(concreteType)) throw new System.ArgumentException("Type must implement " + typeof(TServiceType).Name);

            var inst = Services.Get<TServiceType>();
            if (inst != null) throw new System.InvalidOperationException(string.Format("A service of type '{0}' already exists.", typeof(TServiceType).Name));

            var resource = Resources.Load(path, concreteType);
            if (resource == null) throw new System.InvalidOperationException("Supplied concrete service type failed to construct.");

            if (!(resource is TServiceType)) throw new System.ArgumentException("Type must implement " + typeof(TServiceType).Name);

            if (resource is Component)
            {
                resource = UnityEngine.Object.Instantiate(resource);
            }

            var service = resource as TServiceType;
            Services.Register<TServiceType>(service);
            if (persistent && service is Component c)
            {
                UnityEngine.Object.DontDestroyOnLoad(c.gameObject);
            }
            return service;
        }

        public static TServiceType GetOrLoadResourceService<TServiceType, TConcrete>(string path, bool persistent = false) where TServiceType : class, IService
                                                                                                                where TConcrete : UnityEngine.Object, TServiceType
        {
            var inst = Services.Get<TServiceType>();
            if (inst != null) return inst;

            var service = Resources.Load<TConcrete>(path);
            if (service == null)
            {
                throw new System.InvalidOperationException("Supplied concrete service type failed to construct.");
            }
            if (service is Component)
            {
                service = UnityEngine.Object.Instantiate(service);
            }

            Services.Register<TServiceType>(service);
            if (persistent && service is Component c)
            {
                UnityEngine.Object.DontDestroyOnLoad(c.gameObject);
            }
            return service;
        }

        public static T GetOrLoadResourceService<T>(string path, bool persistent = false) where T : UnityEngine.Object, IService
        {
            var inst = Services.Get<T>();
            if (inst != null) return inst;

            var service = Resources.Load<T>(path);
            if (service == null)
            {
                throw new System.InvalidOperationException("Supplied concrete service type failed to construct.");
            }
            if (service is Component)
            {
                service = UnityEngine.Object.Instantiate(service);
            }

            Services.Register<T>(service);
            if (persistent && service is Component c)
            {
                UnityEngine.Object.DontDestroyOnLoad(c.gameObject);
            }
            return service;
        }

        public static TServiceType GetOrLoadResourceService<TServiceType>(System.Type concreteType, string path, bool persistent = false) where TServiceType : class, IService
        {
            if (concreteType == null) throw new System.ArgumentNullException(nameof(concreteType));
            if (!typeof(TServiceType).IsAssignableFrom(concreteType)) throw new System.ArgumentException("Type must implement " + typeof(TServiceType).Name);

            var inst = Services.Get<TServiceType>();
            if (inst != null) return inst;

            var resource = Resources.Load(path, concreteType);
            if (resource == null) throw new System.InvalidOperationException("Supplied concrete service type failed to construct.");

            if (!(resource is TServiceType)) throw new System.ArgumentException("Type must implement " + typeof(TServiceType).Name);

            if (resource is Component)
            {
                resource = UnityEngine.Object.Instantiate(resource);
            }

            var service = resource as TServiceType;
            Services.Register<TServiceType>(service);
            if (persistent && service is Component c)
            {
                UnityEngine.Object.DontDestroyOnLoad(c.gameObject);
            }
            return service;
        }



        public static bool ValidateService<T>(T service, Services.AutoRegisterOption autoRegisterService, Services.MultipleServiceResolutionOption multipleServiceResolution) where T : class, IService
        {
            var inst = Services.Get<T>();
            if (inst == null)
            {
                if (autoRegisterService > Services.AutoRegisterOption.DoNothing)
                {
                    if (Services.TryRegister<T>(service))
                    {
                        if (autoRegisterService == Services.AutoRegisterOption.RegisterAndPersist && service is Component c)
                        {
                            UnityEngine.Object.DontDestroyOnLoad(c);
                        }
                    }
                }

                return true;
            }
            else if (object.ReferenceEquals(service, inst))
            {
                return true;
            }
            else
            {
                switch (multipleServiceResolution)
                {
                    case Services.MultipleServiceResolutionOption.DoNothing:
                        return false;
                    case Services.MultipleServiceResolutionOption.UnregisterSelf:
                        service.OnServiceUnregistered();
                        return false;
                    case Services.MultipleServiceResolutionOption.UnregisterOther:
                        Services.Unregister<T>();
                        if (autoRegisterService > Services.AutoRegisterOption.DoNothing)
                        {
                            if (Services.TryRegister<T>(service))
                            {
                                if (autoRegisterService == Services.AutoRegisterOption.RegisterAndPersist && service is Component c)
                                {
                                    UnityEngine.Object.DontDestroyOnLoad(c);
                                }
                            }
                        }
                        return true;
                }
            }

            return false;
        }

        #endregion

        #region Special Types

        private static class Entry<T> where T : class, IService
        {

            public static T Instance;

        }

        #endregion

    }

    /// <summary>
    /// Abstract component for implementing a Service Component. 
    /// 
    /// When implementing pass in the interface as the generic T parameter to denote as what type this service 
    /// should be accessed when calling Service.Get&ltT&gt.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ServiceComponent<T> : SPComponent, IService where T : class, IService
    {

        #region Fields

        [SerializeField]
        private ServiceRegistrationOptions _serviceRegistrationOptions;

        #endregion

        #region CONSTRUCTOR

        public ServiceComponent()
        {

        }

        public ServiceComponent(Services.AutoRegisterOption autoRegister, Services.MultipleServiceResolutionOption multipleServiceResolution, Services.UnregisterResolutionOption unregisterResolution)
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
        }

        private bool ValidateService() => Services.ValidateService<T>(this as T, _serviceRegistrationOptions.AutoRegisterService, _serviceRegistrationOptions.MultipleServiceResolution);

        protected virtual void OnValidAwake()
        {

        }

        protected override void OnDestroy()
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

        public Services.UnregisterResolutionOption UnregisterResolution
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
                case Services.UnregisterResolutionOption.DestroySelf:
                    ObjUtil.SmartDestroy(this);
                    break;
                case Services.UnregisterResolutionOption.DestroyGameObject:
                    ObjUtil.SmartDestroy(this.gameObject);
                    break;
            }
        }

        #endregion

        #region Static Factory

        public static T Get()
        {
            return Services.Get<T>();
        }

        /// <summary>
        /// Creates a service component of type TConcrete. This only creates. It will only be registered if its configured to do so.
        /// </summary>
        /// <typeparam name="TConcrete"></typeparam>
        /// <param name="persistent"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static TConcrete Create<TConcrete>(bool persistent = false, string name = null) where TConcrete : Component, T
        {
            var inst = Services.Get<T>();
            if (inst != null) throw new System.InvalidOperationException(string.Format("A service of type '{0}' already exists.", typeof(T).Name));

            if (name == null)
                name = "Service." + typeof(T).Name;
            var go = new GameObject(name);
            if (persistent)
            {
                GameObject.DontDestroyOnLoad(go);
            }
            return go.AddComponent<TConcrete>();
        }

        /// <summary>
        /// Creates a service component of type TConcrete. This only creates. It will only be registered if its configured to do so.
        /// </summary>
        /// <typeparam name="TConcrete"></typeparam>
        /// <param name="persistent"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T Create(System.Type tp, bool persistent = false, string name = null)
        {
            if (tp == null) throw new System.ArgumentNullException("tp");
            if (!typeof(Component).IsAssignableFrom(tp) || !typeof(T).IsAssignableFrom(tp)) throw new System.ArgumentException("Type must be a Component that implements " + typeof(T).Name);

            var inst = Services.Get<T>();
            if (inst != null) throw new System.InvalidOperationException(string.Format("A service of type '{0}' already exists.", typeof(T).Name));

            if (name == null)
                name = "Service." + typeof(T).Name;
            var go = new GameObject(name);
            if (persistent)
            {
                GameObject.DontDestroyOnLoad(go);
            }
            return go.AddComponent(tp) as T;
        }

        /// <summary>
        /// Gets a service component of type TConcrete, if it doesn't exist it will be created. This only creates. It will only be registered if its configured to do so.
        /// </summary>
        /// <typeparam name="TConcrete"></typeparam>
        /// <param name="persistent"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T GetOrCreate<TConcrete>(bool persistent = false, string name = null) where TConcrete : Component, T
        {
            var inst = Services.Get<T>();
            if (inst != null) return inst;

            if (name == null)
                name = "Service." + typeof(T).Name;
            var go = new GameObject(name);
            if (persistent)
            {
                GameObject.DontDestroyOnLoad(go);
            }
            return go.AddComponent<TConcrete>();
        }

        /// <summary>
        /// Gets a service component of type TConcrete, if it doesn't exist it will be created. This only creates. It will only be registered if its configured to do so.
        /// </summary>
        /// <typeparam name="TConcrete"></typeparam>
        /// <param name="persistent"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T GetOrCreate(System.Type tp, bool persistent = false, string name = null)
        {
            if (tp == null) throw new System.ArgumentNullException("tp");
            if (!typeof(Component).IsAssignableFrom(tp) || !typeof(T).IsAssignableFrom(tp)) throw new System.ArgumentException("Type must be a Component that implements " + typeof(T).Name);

            var inst = Services.Get<T>();
            if (inst != null) return inst;

            if (name == null)
                name = "Service." + typeof(T).Name;
            var go = new GameObject(name);
            if (persistent)
            {
                GameObject.DontDestroyOnLoad(go);
            }
            return go.AddComponent(tp) as T;
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
            public Services.UnregisterResolutionOption UnregisterResolution;

        }

        #endregion

    }

    /// <summary>
    /// Abstract component for implementing a Service Component. 
    /// 
    /// When implementing pass in the interface as the generic T parameter to denote as what type this service 
    /// should be accessed when calling Service.Get&ltT&gt.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ServiceScriptableObject<T> : ScriptableObject, IService where T : class, IService
    {

        #region Fields

        [SerializeField]
        private ServiceRegistrationOptions _serviceRegistrationOptions;

        #endregion

        #region CONSTRUCTOR

        public ServiceScriptableObject()
        {

        }

        public ServiceScriptableObject(bool autoRegister, Services.MultipleServiceResolutionOption multipleServiceResolution, bool destroyOnUnregister)
        {
            _serviceRegistrationOptions.AutoRegisterService = autoRegister;
            _serviceRegistrationOptions.MultipleServiceResolution = multipleServiceResolution;
            _serviceRegistrationOptions.DestroyOnUnregister = destroyOnUnregister;
        }

        protected virtual void OnEnable() //NOTE - using OnEnable now since it appears Awake doesn't occur on SOs that are created as an asset and loaded that way.
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return;
            }
#endif

            if (!(this is T))
            {
                if (_serviceRegistrationOptions.MultipleServiceResolution == Services.MultipleServiceResolutionOption.UnregisterSelf)
                {
                    (this as IService).OnServiceUnregistered();
                }
                return;
            }

            if (Services.ValidateService<T>(this as T, _serviceRegistrationOptions.AutoRegisterService ? Services.AutoRegisterOption.Register : Services.AutoRegisterOption.DoNothing, _serviceRegistrationOptions.MultipleServiceResolution))
            {
                this.OnValidAwake();
            }
        }

        protected virtual void OnValidAwake()
        {

        }

        protected virtual void OnDestroy()
        {
            if (this is T s) Services.TryUnregister<T>(s);
        }

        #endregion

        #region Properties

        public bool DestroyOnUnregister
        {
            get { return _serviceRegistrationOptions.DestroyOnUnregister; }
            set { _serviceRegistrationOptions.DestroyOnUnregister = value; }
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
            if (this.ServiceUnregistered != null) this.ServiceUnregistered(this, System.EventArgs.Empty);
            this.OnServiceUnregistered();
        }

        protected virtual void OnServiceUnregistered()
        {
            if (_serviceRegistrationOptions.DestroyOnUnregister)
            {
                ObjUtil.SmartDestroy(this);
            }
        }

        #endregion

        #region Static Factory

        public static T Get()
        {
            return Services.Get<T>();
        }

        /// <summary>
        /// Creates a service scriptableobject. This only creates. It will only be registered if its configured to do so.
        /// </summary>
        /// <typeparam name="TConcrete"></typeparam>
        /// <param name="persistent"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static TConcrete Create<TConcrete>(bool persistent = false, string name = null) where TConcrete : ScriptableObject, T
        {
            var inst = Services.Get<T>();
            if (inst != null) throw new System.InvalidOperationException(string.Format("A service of type '{0}' already exists.", typeof(T).Name));

            if (name == null)
                name = "Service." + typeof(T).Name;
            var obj = ScriptableObject.CreateInstance<TConcrete>();
            obj.name = name;
            return obj;
        }

        /// <summary>
        /// Creates a service scriptableobject. This only creates. It will only be registered if its configured to do so.
        /// </summary>
        /// <typeparam name="TConcrete"></typeparam>
        /// <param name="persistent"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T Create(System.Type tp, bool persistent = false, string name = null)
        {
            if (tp == null) throw new System.ArgumentNullException("tp");
            if (!typeof(ScriptableObject).IsAssignableFrom(tp) || !typeof(T).IsAssignableFrom(tp)) throw new System.ArgumentException("Type must be a Component that implements " + typeof(T).Name);

            var inst = Services.Get<T>();
            if (inst != null) throw new System.InvalidOperationException(string.Format("A service of type '{0}' already exists.", typeof(T).Name));

            if (name == null)
                name = "Service." + typeof(T).Name;
            var obj = ScriptableObject.CreateInstance(tp) as ScriptableObject;
            obj.name = name;
            return obj as T;
        }

        /// <summary>
        /// Gets a service scriptableobject of type TConcrete, if it doesn't exist it will be created. This only creates. It will only be registered if its configured to do so.
        /// </summary>
        /// <typeparam name="TConcrete"></typeparam>
        /// <param name="persistent"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T GetOrCreate<TConcrete>(bool persistent = false, string name = null) where TConcrete : ScriptableObject, T
        {
            var inst = Services.Get<T>();
            if (inst != null) return inst;

            if (name == null)
                name = "Service." + typeof(T).Name;
            var obj = ScriptableObject.CreateInstance<TConcrete>();
            obj.name = name;
            return obj;
        }

        /// <summary>
        /// Gets a service scriptableobject of type TConcrete, if it doesn't exist it will be created. This only creates. It will only be registered if its configured to do so.
        /// </summary>
        /// <typeparam name="TConcrete"></typeparam>
        /// <param name="persistent"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T GetOrCreate(System.Type tp, bool persistent = false, string name = null)
        {
            if (tp == null) throw new System.ArgumentNullException("tp");
            if (!typeof(ScriptableObject).IsAssignableFrom(tp) || !typeof(T).IsAssignableFrom(tp)) throw new System.ArgumentException("Type must be a Component that implements " + typeof(T).Name);

            var inst = Services.Get<T>();
            if (inst != null) return inst;

            if (name == null)
                name = "Service." + typeof(T).Name;
            var obj = ScriptableObject.CreateInstance(tp) as ScriptableObject;
            obj.name = name;
            return obj as T;
        }

        #endregion

        #region Special Types

        [System.Serializable]
        public struct ServiceRegistrationOptions
        {

            [SerializeField]
            public bool AutoRegisterService;
            [SerializeField]
            public Services.MultipleServiceResolutionOption MultipleServiceResolution;
            [SerializeField]
            public bool DestroyOnUnregister;

        }

        #endregion

    }

}
