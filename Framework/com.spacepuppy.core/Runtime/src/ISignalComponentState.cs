using UnityEngine;
using com.spacepuppy.Utils;

namespace com.spacepuppy
{

    public interface ISignalAwakeMessageHandler
    {
        void OnComponentAwake(IComponent component);
    }

    [AutoInitMixin]
    public interface IMSignalAwake : IMixin, IComponent
    {
        sealed void OnInitMixin()
        {
            switch (this.EntityRelativity)
            {
                case EntityRelativity.Entity:
                    this.gameObject.FindRoot().Broadcast<ISignalAwakeMessageHandler, IMSignalAwake>(this, (o, a) => o.OnComponentAwake(a), this.IncludeInactiveObjects, this.IncludeDisabledComponents);
                    break;
                case EntityRelativity.Self:
                    this.gameObject.Signal<ISignalAwakeMessageHandler, IMSignalAwake>(this, (o, a) => o.OnComponentAwake(a), this.IncludeInactiveObjects);
                    break;
                case EntityRelativity.SelfAndChildren:
                    this.gameObject.Broadcast<ISignalAwakeMessageHandler, IMSignalAwake>(this, (o, a) => o.OnComponentAwake(a), this.IncludeInactiveObjects, this.IncludeDisabledComponents);
                    break;
                case EntityRelativity.SelfAndParents:
                    this.gameObject.SignalUpwards<ISignalAwakeMessageHandler, IMSignalAwake>(this, (o, a) => o.OnComponentAwake(a), this.IncludeDisabledComponents);
                    break;
            }
        }

        EntityRelativity EntityRelativity => EntityRelativity.Self;
        bool IncludeInactiveObjects => false;
        bool IncludeDisabledComponents => false;

    }

    public interface ISignalEnabledMessageHandler
    {
        void OnComponentEnabled(IEventfulComponent component);
        void OnComponentDisabled(IEventfulComponent component);
    }

    [AutoInitMixin]
    public interface IMSignalEnabled : IMixin, IEventfulComponent
    {

        sealed void OnInitMixin()
        {
            this.OnEnabled += (s, e) =>
            {
                switch (this.EntityRelativity)
                {
                    case EntityRelativity.Entity:
                        this.gameObject.FindRoot().Broadcast<ISignalEnabledMessageHandler, IMSignalEnabled>(this, (o, a) => o.OnComponentEnabled(a), this.IncludeInactiveObjects, this.IncludeDisabledComponents);
                        break;
                    case EntityRelativity.Self:
                        this.gameObject.Signal<ISignalEnabledMessageHandler, IMSignalEnabled>(this, (o, a) => o.OnComponentEnabled(a), this.IncludeInactiveObjects);
                        break;
                    case EntityRelativity.SelfAndChildren:
                        this.gameObject.Broadcast<ISignalEnabledMessageHandler, IMSignalEnabled>(this, (o, a) => o.OnComponentEnabled(a), this.IncludeInactiveObjects, this.IncludeDisabledComponents);
                        break;
                    case EntityRelativity.SelfAndParents:
                        this.gameObject.SignalUpwards<ISignalEnabledMessageHandler, IMSignalEnabled>(this, (o, a) => o.OnComponentEnabled(a), this.IncludeDisabledComponents);
                        break;
                }
            };
            this.OnDisabled += (s, e) =>
            {
                switch (this.EntityRelativity)
                {
                    case EntityRelativity.Entity:
                        this.gameObject.FindRoot().Broadcast<ISignalEnabledMessageHandler, IMSignalEnabled>(this, (o, a) => o.OnComponentDisabled(a), this.IncludeInactiveObjects, this.IncludeDisabledComponents);
                        break;
                    case EntityRelativity.Self:
                        this.gameObject.Signal<ISignalEnabledMessageHandler, IMSignalEnabled>(this, (o, a) => o.OnComponentDisabled(a), this.IncludeInactiveObjects);
                        break;
                    case EntityRelativity.SelfAndChildren:
                        this.gameObject.Broadcast<ISignalEnabledMessageHandler, IMSignalEnabled>(this, (o, a) => o.OnComponentDisabled(a), this.IncludeInactiveObjects, this.IncludeDisabledComponents);
                        break;
                    case EntityRelativity.SelfAndParents:
                        this.gameObject.SignalUpwards<ISignalEnabledMessageHandler, IMSignalEnabled>(this, (o, a) => o.OnComponentDisabled(a), this.IncludeDisabledComponents);
                        break;
                }
            };
        }

        EntityRelativity EntityRelativity => EntityRelativity.Self;
        bool IncludeInactiveObjects => false;
        bool IncludeDisabledComponents => false;

    }

    public interface ISignalDestroyedMessageHandler
    {
        void OnComponentDestroyed(IEventfulComponent component);
    }

    [AutoInitMixin]
    public interface IMSignalDestroyed : IMixin, IEventfulComponent
    {
        sealed void OnInitMixin()
        {
            this.ComponentDestroyed += (s, e) =>
            {
                switch (this.EntityRelativity)
                {
                    case EntityRelativity.Entity:
                        this.gameObject.FindRoot().Broadcast<ISignalDestroyedMessageHandler, IMSignalDestroyed>(this, (o, a) => o.OnComponentDestroyed(a), this.IncludeInactiveObjects, this.IncludeDisabledComponents);
                        break;
                    case EntityRelativity.Self:
                        this.gameObject.Signal<ISignalDestroyedMessageHandler, IMSignalDestroyed>(this, (o, a) => o.OnComponentDestroyed(a), this.IncludeInactiveObjects);
                        break;
                    case EntityRelativity.SelfAndChildren:
                        this.gameObject.Broadcast<ISignalDestroyedMessageHandler, IMSignalDestroyed>(this, (o, a) => o.OnComponentDestroyed(a), this.IncludeInactiveObjects, this.IncludeDisabledComponents);
                        break;
                    case EntityRelativity.SelfAndParents:
                        this.gameObject.SignalUpwards<ISignalDestroyedMessageHandler, IMSignalDestroyed>(this, (o, a) => o.OnComponentDestroyed(a), this.IncludeDisabledComponents);
                        break;
                }
            };
        }

        EntityRelativity EntityRelativity => EntityRelativity.Self;
        bool IncludeInactiveObjects => false;
        bool IncludeDisabledComponents => false;

    }

}
