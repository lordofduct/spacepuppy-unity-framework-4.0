

namespace com.spacepuppy
{

    /// <summary>
    /// Implement this interface if you want to write a script that handles how the entity is dealt with when 'Kill' is called on it. 
    /// This overrides the default behaviour of destroying the GameObject and child GameObjects. 
    /// 
    /// This means generally a IKIllableEntity does something in place of Destroying it, or handles the destroying itself. It should return 
    /// true if it successfully did this. If all IKillableEntity scripts on an entity return 'false', then Object.Destroy will destroy the object. 
    /// If any 1 IKillableEntity script on an entity returns 'true', then Destroy will NOT be called.
    /// </summary>
    public interface IKillableEntity : IComponent
    {

        /// <summary>
        /// Returns true if the entity is in a state considered "dead".
        /// </summary>
        bool IsDead { get; }

        /// <summary>
        /// Called on the component when 'Kill' is called on the entity.
        /// </summary>
        /// <returns>Return true if Destroy is not necessary to be called.</returns>
        bool Kill();

    }

}

