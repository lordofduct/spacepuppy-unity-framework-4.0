

namespace com.spacepuppy.Dynamic
{

    /// <summary>
    /// Define a component that can be used as a state modifier with its own ruleset. 
    /// 
    /// For example it may store some state information for a Camera, but the target may be a GameObject. 
    /// The IStateModifier can get the Camera from the GameObject and update it accordingly.
    /// </summary>
    public interface IStateModifier
    {

        /// <summary>
        /// Copy the current state of the IModifier to a target.
        /// </summary>
        /// <param name="obj"></param>
        void CopyTo(object targ);

        /// <summary>
        /// Lerp the state of the target to the state of the modifier.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="t"></param>
        void LerpTo(object targ, float t);

    }

}
