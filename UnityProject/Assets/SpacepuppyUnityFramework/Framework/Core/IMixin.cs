using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using com.spacepuppy.Utils;

namespace com.spacepuppy
{

    /// <summary>
    /// An implementation of mixin's for C# that are composited into SPComponent.
    /// 
    /// Since C# doesn't support multiple inheritance, we instead implement a mixin as an interface. 
    /// All mixin's should inherit from IMixin for initialization purpose. The mixin should be registered 
    /// during Awake, though could be added at other times as well (some mixins may rely on Awake and should 
    /// be documented as such).
    /// </summary>
    public interface IMixin
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
    public interface IAutoMixin : IMixin
    {
        void OnAutoCreated(IAutoMixinDecorator owner, System.Type autoMixinType);
    }

    /// <summary>
    /// An IAutoMixin should be implemented as its mixin which implements IAutoMixin as well as a related interface that 
    /// inherits from IAutoMixinDecorator. The IAutoMixinDecorator should then be attributed with AutoMixinConfigAttribute 
    /// to define which IAutoMixin concrete class should be created and registered on SPComponent.Awake. 
    /// </summary>
    public interface IAutoMixinDecorator
    {

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

    /// <summary>
    /// Static class for initializing mixins.
    /// </summary>
    public static class MixinUtil
    {

        private static System.Type _autoMixinBaseType = typeof(IAutoMixinDecorator);

        public static IEnumerable<IMixin> CreateAutoMixins(IAutoMixinDecorator obj)
        {
            if (obj == null) throw new System.ArgumentNullException(nameof(obj));

            var mixinTypes = obj.GetType().FindInterfaces((tp, c) =>
            {
                return tp != _autoMixinBaseType && tp.IsInterface && _autoMixinBaseType.IsAssignableFrom(tp);
            }, null);

            foreach (var mixinType in mixinTypes)
            {
                var configAttrib = mixinType.GetCustomAttribute<AutoMixinConfigAttribute>(false);
                if (configAttrib != null && TypeUtil.IsType(configAttrib.ConcreteMixinType, typeof(IMixin)))
                {
                    var mixin = (IMixin)System.Activator.CreateInstance(configAttrib.ConcreteMixinType);
                    (mixin as IAutoMixin)?.OnAutoCreated(obj, mixinType);
                    yield return mixin;
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
    public interface IMStartOrEnableReceiver : IAutoMixinDecorator, IEventfulComponent
    {

        void OnStartOrEnable();

    }

    internal class MStartOrEnableReceiver : IMixin
    {

        bool IMixin.Awake(object owner)
        {
            var c = owner as IMStartOrEnableReceiver;
            if (c == null) return false; 

            c.OnStarted += (s, e) =>
            {
                c.OnStartOrEnable();
            };
            c.OnEnabled += (s, e) =>
            {
                if (c.started)
                {
                    c.OnStartOrEnable();
                }
            };

            return false;
        }

    }

}
