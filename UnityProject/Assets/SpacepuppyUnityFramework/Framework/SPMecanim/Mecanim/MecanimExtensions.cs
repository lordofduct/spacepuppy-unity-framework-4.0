using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Utils;

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

        #region Animator AnimatorStateInfo Tests

        public static bool GetCurrentAnimatorStateIs(this Animator animator, string name, int layerIndex)
        {
            if(layerIndex < 0)
            {
                for(int i = 0; i < layerIndex; i++)
                {
                    if(animator.GetCurrentAnimatorStateInfo(i).IsName(name))
                    {
                        return true;
                    }
                }
                return false;
            }
            else if(layerIndex < animator.layerCount)
            {
                return animator.GetCurrentAnimatorStateInfo(layerIndex).IsName(name);
            }
            else
            {
                return false;
            }
        }

        public static bool GetCurrentAnimatorStateIs(this Animator animator, string name, ref int layerIndex, out AnimatorStateInfo info)
        {
            if (layerIndex < 0)
            {
                for (int i = 0; i < layerIndex; i++)
                {
                    info = animator.GetCurrentAnimatorStateInfo(i);
                    if (info.IsName(name))
                    {
                        layerIndex = i;
                        return true;
                    }
                }
                info = default(AnimatorStateInfo);
                return false;
            }
            else if (layerIndex < animator.layerCount)
            {
                info = animator.GetCurrentAnimatorStateInfo(layerIndex);
                if (info.IsName(name)) return true;

                info = default(AnimatorStateInfo);
                return false;
            }
            else
            {
                info = default(AnimatorStateInfo);
                return false;
            }
        }

        public static bool GetNextAnimatorStateIs(this Animator animator, string name, int layerIndex)
        {
            if (layerIndex < 0)
            {
                for (int i = 0; i < layerIndex; i++)
                {
                    if (animator.GetNextAnimatorStateInfo(i).IsName(name))
                    {
                        return true;
                    }
                }
                return false;
            }
            else if (layerIndex < animator.layerCount)
            {
                return animator.GetNextAnimatorStateInfo(layerIndex).IsName(name);
            }
            else
            {
                return false;
            }
        }

        public static bool GetNextAnimatorStateIs(this Animator animator, string name, ref int layerIndex, out AnimatorStateInfo info)
        {
            if (layerIndex < 0)
            {
                for (int i = 0; i < layerIndex; i++)
                {
                    info = animator.GetNextAnimatorStateInfo(i);
                    if (info.IsName(name))
                    {
                        layerIndex = i;
                        return true;
                    }
                }
                info = default(AnimatorStateInfo);
                return false;
            }
            else if (layerIndex < animator.layerCount)
            {
                info = animator.GetNextAnimatorStateInfo(layerIndex);
                if (info.IsName(name)) return true;

                info = default(AnimatorStateInfo);
                return false;
            }
            else
            {
                info = default(AnimatorStateInfo);
                return false;
            }
        }

        #endregion

        #region SPAnimatorOverrideLayers Extensions

        internal static void StackOverrideGeneralized(Animator animator, object overrides, object token, bool targetUnconfiguredEntriesAsvalidEntriesWhenAnimatorOverrideController = false)
        {
            var src = ObjUtil.GetAsFromSource<IAnimatorOverrideSource>(overrides);
            if (!object.ReferenceEquals(src, null))
            {
                StackOverride(animator, src, token);
                return;
            }

            var controller = ObjUtil.GetAsFromSource<AnimatorOverrideController>(overrides);
            if (!object.ReferenceEquals(controller, null))
            {
                StackOverride(animator, controller, token, targetUnconfiguredEntriesAsvalidEntriesWhenAnimatorOverrideController);
                return;
            }

            var lst = overrides as IList<KeyValuePair<AnimationClip, AnimationClip>>;
            if (!object.ReferenceEquals(lst, null))
            {
                StackOverride(animator, lst, token);
                return;
            }
        }

        public static void StackOverride(this Animator animator, AnimatorOverrideController controller, object token, bool treatUnconfiguredEntriesAsValidEntries = false)
        {
            animator.AddOrGetComponent<SPAnimatorOverrideLayers>().Stack(controller, token, treatUnconfiguredEntriesAsValidEntries);
        }

        public static void StackOverride(this Animator animator, IAnimatorOverrideSource source, object token)
        {
            animator.AddOrGetComponent<SPAnimatorOverrideLayers>().Stack(source, token);
        }

        public static void StackOverride(this Animator animator, IList<KeyValuePair<AnimationClip, AnimationClip>> overrides, object token)
        {
            animator.AddOrGetComponent<SPAnimatorOverrideLayers>().Stack(overrides, token);
        }

        public static void RemoveOverride(this Animator animator, object token)
        {
            animator.AddOrGetComponent<SPAnimatorOverrideLayers>().Remove(token);
        }

        #endregion

    }

}
