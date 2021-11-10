using UnityEngine;
using UnityEngine.Animations;

namespace com.spacepuppy.Mecanim
{
    public static class MecanimExtensions
    {

        #region AnimatorControllerParameterTypeMask

        public static AnimatorControllerParameterTypeMask ToMask(this AnimatorControllerParameterType etp)
        {
            switch (etp)
            {
                case AnimatorControllerParameterType.Float:
                    return AnimatorControllerParameterTypeMask.Float;
                case AnimatorControllerParameterType.Int:
                    return AnimatorControllerParameterTypeMask.Int;
                case AnimatorControllerParameterType.Bool:
                    return AnimatorControllerParameterTypeMask.Bool;
                case AnimatorControllerParameterType.Trigger:
                    return AnimatorControllerParameterTypeMask.Trigger;
                default:
                    return AnimatorControllerParameterTypeMask.Any;
            }
        }

        public static bool FitsMask(this AnimatorControllerParameterTypeMask mask, AnimatorControllerParameterType etp)
        {
            return (mask & etp.ToMask()) != 0;
        }

        public static bool FitsMask(this AnimatorControllerParameterType etp, AnimatorControllerParameterTypeMask mask)
        {
            return (mask & etp.ToMask()) != 0;
        }

        #endregion

        #region AnimatorControllerParameter

        public static void SetParam(this AnimatorControllerParameter param, Animator animator, float value)
        {
            if (param == null) return;

            switch(param.type)
            {
                case AnimatorControllerParameterType.Float:
                    animator.SetFloat(param.nameHash, value);
                    break;
                case AnimatorControllerParameterType.Int:
                    animator.SetInteger(param.nameHash, (int)value);
                    break;
                case AnimatorControllerParameterType.Bool:
                    animator.SetBool(param.nameHash, value != 0f);
                    break;
                case AnimatorControllerParameterType.Trigger:
                    if (value != 0f) animator.SetTrigger(param.nameHash);
                    else animator.ResetTrigger(param.nameHash);
                    break;
            }
        }

        public static void SetParam(this AnimatorControllerParameter param, Animator animator, int value)
        {
            if (param == null) return;

            switch (param.type)
            {
                case AnimatorControllerParameterType.Float:
                    animator.SetFloat(param.nameHash, value);
                    break;
                case AnimatorControllerParameterType.Int:
                    animator.SetInteger(param.nameHash, value);
                    break;
                case AnimatorControllerParameterType.Bool:
                    animator.SetBool(param.nameHash, value != 0);
                    break;
                case AnimatorControllerParameterType.Trigger:
                    if (value != 0) animator.SetTrigger(param.nameHash);
                    else animator.ResetTrigger(param.nameHash);
                    break;
            }
        }

        public static void SetParam(this AnimatorControllerParameter param, Animator animator, bool value)
        {
            if (param == null) return;

            switch (param.type)
            {
                case AnimatorControllerParameterType.Float:
                    animator.SetFloat(param.nameHash, value ? 1 : 0);
                    break;
                case AnimatorControllerParameterType.Int:
                    animator.SetInteger(param.nameHash, value ? 1 : 0);
                    break;
                case AnimatorControllerParameterType.Bool:
                    animator.SetBool(param.nameHash, value);
                    break;
                case AnimatorControllerParameterType.Trigger:
                    if (value) animator.SetTrigger(param.nameHash);
                    else animator.ResetTrigger(param.nameHash);
                    break;
            }
        }

        #endregion

    }

}
