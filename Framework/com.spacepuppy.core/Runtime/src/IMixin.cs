using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using com.spacepuppy.Utils;

namespace com.spacepuppy
{

    public interface IMixin
    {

    }

    public class AutoInitMixinAttribute : System.Attribute
    {

        public const string INIT_METHOD_NAME = "OnInitMixin";

        public virtual void Initialize(IMixin mixin, System.Type mixinType)
        {
            mixinType.GetMethod(INIT_METHOD_NAME).Invoke(mixin, null);
        }

    }

    /// <summary>
    /// Static class for initializing mixins.
    /// </summary>
    public static class MixinUtil
    {

        private static readonly System.Type _rootMixinType = typeof(IMixin);
        private static readonly List<System.Type> _knownMixinTypes = new();
        private static readonly Dictionary<System.Type, AutoInitMixinAttribute> _autoMixinTypeAttribTable = new();
        private static bool _useAutoTableForInitialization;

        static MixinUtil()
        {
            foreach (var tp in TypeUtil.GetTypes())
            {
                if (tp != _rootMixinType && tp.IsInterface && _rootMixinType.IsAssignableFrom(tp))
                {
                    _knownMixinTypes.Add(tp);
                    var attrib = tp.GetCustomAttribute<AutoInitMixinAttribute>(false);
                    if (attrib != null) _autoMixinTypeAttribTable[tp] = attrib;
                }
            }
            _useAutoTableForInitialization = _autoMixinTypeAttribTable.Count < 128;
        }

        public static IEnumerable<System.Type> GetKnownMixinTypes() => _knownMixinTypes;

        public static void InitializeMixins(IMixin target)
        {
            if (target == null) return;

            if (_useAutoTableForInitialization)
            {
                foreach (var pair in _autoMixinTypeAttribTable)
                {
                    if (pair.Key.IsInstanceOfType(target))
                    {
                        pair.Value.Initialize(target, pair.Key);
                    }
                }
            }
            else
            {
                foreach (var tp in target.GetType().GetInterfaces())
                {
                    if (_autoMixinTypeAttribTable.TryGetValue(tp, out AutoInitMixinAttribute attrib))
                    {
                        attrib.Initialize(target, tp);
                    }
                }
            }
        }

    }

    /// <summary>
    /// On start or on enable if and only if start already occurred. This adjusts the order of 'OnEnable' so that it can be used in conjunction with 'OnDisable' to wire up handlers cleanly. 
    /// OnEnable occurs BEFORE Start sometimes, and other components aren't ready yet. This remedies that.
    /// </summary>
    /// <remarks>
    /// In earlier versions of Spacepuppy Framework this was implemented directly on SPComponent. I've since moved it to here to match our new IMixin interface, and so that only those components 
    /// that need OnStartOrEnable actually have it implemented. No need for empty method calls on ALL components.
    /// </remarks>
    [AutoInitMixin]
    public interface IMStartOrEnableReceiver : IMixin, IEventfulComponent
    {
        sealed void OnInitMixin()
        {
            this.OnStarted += (s, e) =>
            {
                this.OnStartOrEnable();
            };
            this.OnEnabled += (s, e) =>
            {
                if (this.started)
                {
                    this.OnStartOrEnable();
                }
            };
        }

        void OnStartOrEnable();

    }

    /// <summary>
    /// Sometimes you want to run Start late, to allow Start to be called on all other scripts. Basically adding a final ordering for Start similar to LateUpdate.
    /// </summary>
    [AutoInitMixin]
    public interface IMLateStartReceiver : IMixin, IEventfulComponent
    {
        sealed void OnInitMixin()
        {
            this.OnStarted += (s, e) =>
            {
                GameLoop.LateUpdateHandle.BeginInvoke(() =>
                {
                    this.OnLateStart();
                });
            };
        }

        void OnLateStart();
    }

    /// <summary>
    /// Sometimes you want to run StartOrEnable late, to allow Start to be called on all other scripts. Basically adding a final ordering point for Start similar to LateUpdate.
    /// </summary>
    [AutoInitMixin]
    public interface IMLateStartOrEnableReceiver : IMixin, IEventfulComponent
    {

        sealed void OnInitMixin()
        {
            var state = new MixinState()
            {
                target = this,
            };
            this.OnDisabled += (s, e) =>
            {
                GameLoop.LateUpdatePump.Remove(state);
            };
            this.OnEnabled += (s, e) =>
            {
                GameLoop.LateUpdatePump.Add(state);
            };
        }

        void OnLateStartOrEnable();

        class MixinState : IUpdateable
        {

            public IMLateStartOrEnableReceiver target;
            public void Update()
            {
                GameLoop.LateUpdatePump.Remove(this);
                target.OnLateStartOrEnable();
            }

        }

    }

    #region LEGACY MIXINS

    /// <summary>
    /// An implementation of mixin's for C# that are composited into SPComponent.
    /// 
    /// Since C# doesn't support multiple inheritance, we instead implement a mixin as an interface. 
    /// All mixin's should inherit from IMixin for initialization purpose. The mixin should be registered 
    /// during Awake, though could be added at other times as well (some mixins may rely on Awake and should 
    /// be documented as such).
    /// </summary>
    public interface ILegacyMixin
    {
        /// <summary>
        /// Called by the object that the mixin was composited by when its registered. Should return true if 
        /// the mixin should be cached in the owner to maintain a reference so it's not deconstructed.
        /// </summary>
        /// <param name="owner"></param>
        /// <returns></returns>
        bool Awake(object owner);
    }

    /// <summary>
    /// An IAutoMixin is an IMixin that will be auto created and registered by a SPComponent that implements the related 
    /// IAutoMixinDecorator. 
    /// 
    /// If you want the automixindecorator to have methods, you should create a static class with extension methods 
    /// that accept the automixindecorator interface as the first parameter.
    /// 
    /// IAutoMixin initializing is handled during SPComponent.Awake. If you want a non-SPComponent to handle IAutoMixin 
    /// you must call MixinUtil.Initialize during the constructor/awake of the class.
    /// </summary>
    public interface IAutoLegacyMixin : ILegacyMixin
    {
        void OnAutoCreated(IAutoLegacyMixinDecorator owner, System.Type autoMixinType);
    }

    /// <summary>
    /// An IAutoMixin should be implemented as its mixin which implements IAutoMixin as well as a related interface that 
    /// inherits from IAutoMixinDecorator. The IAutoMixinDecorator should then be attributed with AutoMixinConfigAttribute 
    /// to define which IAutoMixin concrete class should be created and registered on SPComponent.Awake. 
    /// </summary>
    public interface IAutoLegacyMixinDecorator
    {
        //T GetMixinState<T>() where T : class, ILegacyMixin;
    }

    [System.AttributeUsage(System.AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public class AutoMixinConfigAttribute : System.Attribute
    {
        public System.Type ConcreteMixinType { get; private set; }

        public AutoMixinConfigAttribute(System.Type concreteMixinType)
        {
            this.ConcreteMixinType = concreteMixinType;
        }
    }

    public abstract class StatelessAutoMixinConfigAttribute : AutoMixinConfigAttribute
    {

        public StatelessAutoMixinConfigAttribute() : base(typeof(StatelessAutoMixinConfigAttribute)) { }

        internal protected abstract void OnAutoCreated(object obj, System.Type mixinType);

    }

    #endregion

}
