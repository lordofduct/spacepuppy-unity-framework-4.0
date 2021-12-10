using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using com.spacepuppy.Utils;

namespace com.spacepuppy
{

    public interface ISignalAwakeMessageHandler
    {
        void OnComponentAwake(Component component);
    }

    public class MSignalAwake : IAutoMixin
    {

        [System.AttributeUsage(System.AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
        public class ConfigAttribute : System.Attribute
        {

            public EntityRelativity EntityRelativity { get; set; }
            public bool IncludeInactiveObjects { get; set; }
            public bool IncludeDisabledComponents { get; set; }
        }

        [AutoMixinConfig(typeof(MSignalAwake))]
        public interface IAutoDecorator : IAutoMixinDecorator
        {

        }

        private static readonly System.Action<ISignalAwakeMessageHandler, Component> Functor = (o, d) => o.OnComponentAwake(d);

        public EntityRelativity EntityRelativity { get; set; }
        public bool IncludeInactiveObjects { get; set; }
        public bool IncludeDisabledComponents { get; set; }

        void spacepuppy.IAutoMixin.OnAutoCreated(IAutoMixinDecorator owner, System.Type autoMixinType)
        {
            var attrib = autoMixinType.GetCustomAttribute<ConfigAttribute>(false);
            this.EntityRelativity = attrib?.EntityRelativity ?? EntityRelativity.Entity;
            this.IncludeInactiveObjects = attrib?.IncludeInactiveObjects ?? false;
            this.IncludeDisabledComponents = attrib?.IncludeDisabledComponents ?? false;
        }

        bool IMixin.Awake(object owner)
        {
            var c = owner as Component;
            if (c == null) return false;

            var includeInactiveObjs = this.IncludeInactiveObjects;
            var includeDisabledComps = this.IncludeDisabledComponents;

            switch (this.EntityRelativity)
            {
                case EntityRelativity.Entity:
                    {
                        c.gameObject.FindRoot().Broadcast(c, Functor, includeInactiveObjs, includeDisabledComps);
                    }
                    break;
                case EntityRelativity.Self:
                    {
                        c.gameObject.Signal(c, Functor, includeDisabledComps);
                    }
                    break;
                case EntityRelativity.SelfAndChildren:
                    {
                        c.gameObject.Broadcast(c, Functor, includeInactiveObjs, includeDisabledComps);
                    }
                    break;
            }
            return false;
        }
    }


    public interface ISignalEnabledMessageHandler
    {
        void OnComponentEnabled(IEventfulComponent component);
        void OnComponentDisabled(IEventfulComponent component);
    }

    public class MSignalEnabled : IAutoMixin
    {

        [System.AttributeUsage(System.AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
        public class ConfigAttribute : System.Attribute
        {

            public EntityRelativity EntityRelativity { get; set; }
            public bool IncludeInactiveObjects { get; set; }
            public bool IncludeDisabledComponents { get; set; }
        }

        [AutoMixinConfig(typeof(MSignalEnabled))]
        public interface IAutoDecorator : IAutoMixinDecorator, IEventfulComponent
        {

        }

        private static readonly System.Action<ISignalEnabledMessageHandler, IEventfulComponent> EnabledFunctor = (o, d) => o.OnComponentEnabled(d);
        private static readonly System.Action<ISignalEnabledMessageHandler, IEventfulComponent> DisabledFunctor = (o, d) => o.OnComponentDisabled(d);

        public EntityRelativity EntityRelativity { get; set; }
        public bool IncludeInactiveObjects { get; set; }
        public bool IncludeDisabledComponents { get; set; }

        void spacepuppy.IAutoMixin.OnAutoCreated(IAutoMixinDecorator owner, System.Type autoMixinType)
        {
            var attrib = autoMixinType.GetCustomAttribute<ConfigAttribute>(false);
            this.EntityRelativity = attrib?.EntityRelativity ?? EntityRelativity.Entity;
            this.IncludeInactiveObjects = attrib?.IncludeInactiveObjects ?? false;
            this.IncludeDisabledComponents = attrib?.IncludeDisabledComponents ?? false;
        }

        bool IMixin.Awake(object owner)
        {
            var c = owner as IEventfulComponent;
            if (c == null) return false;

            var includeInactiveObjs = this.IncludeInactiveObjects;
            var includeDisabledComps = this.IncludeDisabledComponents;

            switch(this.EntityRelativity)
            {
                case EntityRelativity.Entity:
                    {
                        c.OnEnabled += (s, e) =>
                        {
                            c.gameObject.FindRoot().Broadcast(c, EnabledFunctor, includeInactiveObjs, includeDisabledComps);
                        };
                        c.OnDisabled += (s, e) =>
                        {
                            c.gameObject.FindRoot().Broadcast(c, DisabledFunctor, includeInactiveObjs, includeDisabledComps);
                        };
                    }
                    break;
                case EntityRelativity.Self:
                    {
                        c.OnEnabled += (s, e) =>
                        {
                            c.gameObject.Signal(c, EnabledFunctor, includeDisabledComps);
                        };
                        c.OnDisabled += (s, e) =>
                        {
                            c.gameObject.Signal(c, DisabledFunctor, includeDisabledComps);
                        };
                    }
                    break;
                case EntityRelativity.SelfAndChildren:
                    {
                        c.OnEnabled += (s, e) =>
                        {
                            c.gameObject.Broadcast(c, EnabledFunctor, includeInactiveObjs, includeDisabledComps);
                        };
                        c.OnDisabled += (s, e) =>
                        {
                            c.gameObject.Broadcast(c, DisabledFunctor, includeInactiveObjs, includeDisabledComps);
                        };
                    }
                    break;
            }
            return false;
        }
    }


    public interface ISignalDestroyedMessageHandler
    {
        void OnComponentDestroyed(IEventfulComponent component);
    }

    public class MSignalDestroyed : IAutoMixin
    {

        [System.AttributeUsage(System.AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
        public class ConfigAttribute : System.Attribute
        {

            public EntityRelativity EntityRelativity { get; set; }
            public bool IncludeInactiveObjects { get; set; }
            public bool IncludeDisabledComponents { get; set; }
        }

        [AutoMixinConfig(typeof(MSignalDestroyed))]
        public interface IAutoDecorator : IAutoMixinDecorator, IEventfulComponent
        {

        }

        private static readonly System.Action<ISignalDestroyedMessageHandler, IEventfulComponent> Functor = (o, d) => o.OnComponentDestroyed(d);

        public EntityRelativity EntityRelativity { get; set; }
        public bool IncludeInactiveObjects { get; set; }
        public bool IncludeDisabledComponents { get; set; }

        void spacepuppy.IAutoMixin.OnAutoCreated(IAutoMixinDecorator owner, System.Type autoMixinType)
        {
            var attrib = autoMixinType.GetCustomAttribute<ConfigAttribute>(false);
            this.EntityRelativity = attrib?.EntityRelativity ?? EntityRelativity.Entity;
            this.IncludeInactiveObjects = attrib?.IncludeInactiveObjects ?? false;
            this.IncludeDisabledComponents = attrib?.IncludeDisabledComponents ?? false;
        }

        bool IMixin.Awake(object owner)
        {
            var c = owner as IEventfulComponent;
            if (c == null) return false;

            var includeInactiveObjs = this.IncludeInactiveObjects;
            var includeDisabledComps = this.IncludeDisabledComponents;

            switch (this.EntityRelativity)
            {
                case EntityRelativity.Entity:
                    {
                        c.ComponentDestroyed += (s, e) =>
                        {
                            c.gameObject.FindRoot().Broadcast(c, Functor, includeInactiveObjs, includeDisabledComps);
                        };
                    }
                    break;
                case EntityRelativity.Self:
                    {
                        c.ComponentDestroyed += (s, e) =>
                        {
                            c.gameObject.Signal(c, Functor, includeDisabledComps);
                        };
                    }
                    break;
                case EntityRelativity.SelfAndChildren:
                    {
                        c.ComponentDestroyed += (s, e) =>
                        {
                            c.gameObject.Broadcast(c, Functor, includeInactiveObjs, includeDisabledComps);
                        };
                    }
                    break;
            }
            return false;
        }
    }

}
