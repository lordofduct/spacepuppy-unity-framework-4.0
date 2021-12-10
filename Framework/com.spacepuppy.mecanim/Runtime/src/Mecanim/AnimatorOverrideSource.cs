using UnityEngine;
using System.Collections.Generic;

namespace com.spacepuppy.Mecanim
{

    [CreateAssetMenu(fileName = "AnimatorOverrideSource", menuName = "Spacepuppy/Mecanim/AnimatorOverrideSource")]
    public class AnimatorOverrideSource : ScriptableObject, IAnimatorOverrideSource
    {

        #region Fields

        [SerializeField]
        private RuntimeAnimatorController _controller;

        [SerializeField]
        private List<AnimationClipPair> _overrides;

        #endregion

        #region IAnimatorOverrideSource Interface

        public int GetOverrides(Animator animator, IList<KeyValuePair<AnimationClip, AnimationClip>> lst)
        {
            lst.Clear();
            int cnt = 0;
            for(int i = 0; i < _overrides.Count; i++)
            {
                var pair = _overrides[i];
                if(pair.Key)
                {
                    lst.Add(new KeyValuePair<AnimationClip, AnimationClip>(pair.Key, pair.Value));
                    cnt++;
                }
            }
            return cnt;
        }

        #endregion

        #region Special Types

        [System.Serializable]
        public struct AnimationClipPair
        {
            public AnimationClip Key;
            public AnimationClip Value;
        }

        #endregion

    }

}
