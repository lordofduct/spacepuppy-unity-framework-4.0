
using com.spacepuppy.Dynamic.Accessors;

namespace com.spacepuppy.Tween
{

    /// <summary>
    /// Implement this interface along with the class attribute 'CustomTweenMemberAccessorAttribute' to define a custom provider  
    /// that will be used to create a IMemberAccessor that will be used for tweening (it can return itself if stateless). 
    /// 
    /// The CustomTweenMemberAccessorAttribute requires accepts a type to apply this accessor to, as well as the name of the property. 
    /// The property doesn't have to be an actual property on the type, and can be any custom name you define. For example the 
    /// 'GeneralMoveAccessor' is called upon for Rigidbody, Transform, and GameObject for the property "*Move".
    /// </summary>
    public interface ITweenMemberAccessorProvider
    {

        System.Type GetMemberType();

        /// <summary>
        /// Returns a member accessor for the target.
        /// tweening. 
        /// </summary>
        IMemberAccessor GetAccessor(object target, string propName, string args);

    }

}
