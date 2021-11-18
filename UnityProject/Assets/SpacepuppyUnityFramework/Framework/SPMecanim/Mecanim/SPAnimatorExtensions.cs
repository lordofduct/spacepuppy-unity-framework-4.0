using com.spacepuppy.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.spacepuppy
{
    public static class SPAnimatorExtensions
    {

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

    }
}
