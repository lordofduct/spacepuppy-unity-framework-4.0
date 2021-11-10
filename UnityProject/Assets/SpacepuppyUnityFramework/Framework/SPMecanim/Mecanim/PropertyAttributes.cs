using UnityEngine;
using UnityEngine.Animations;

namespace com.spacepuppy.Mecanim
{

    public class AnimatorParameterNameAttribute : PropertyAttribute
    {
        public AnimatorControllerParameterTypeMask ParameterType = AnimatorControllerParameterTypeMask.Any;

        public AnimatorParameterNameAttribute()
        {

        }

        public AnimatorParameterNameAttribute(AnimatorControllerParameterTypeMask parameterType)
        {
            ParameterType = parameterType;
        }

    }

}
