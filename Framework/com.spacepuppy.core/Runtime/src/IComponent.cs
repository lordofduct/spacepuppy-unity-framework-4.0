using UnityEngine;

namespace com.spacepuppy
{

    /// <summary>
    /// Base contract for any interface contract that should be considered a Component
    /// </summary>
    public interface IComponent : IGameObjectSource
    {

        bool enabled { get; set; }
        bool isActiveAndEnabled { get; }
        Component component { get; }

    }

    public interface IEventfulComponent : IComponent
    {
        event System.EventHandler OnEnabled;
        event System.EventHandler OnStarted;
        event System.EventHandler OnDisabled;
        event System.EventHandler ComponentDestroyed;

        bool started { get; }
    }

    /// <summary>
    /// Implement on a component that has a 'Id' property that can be used to uniquely distinguish it from other components on the same GameObject of the same type.
    /// </summary>
    public interface IIdentifiableComponent : IComponent
    {
        string Id { get; }
    }

}
