using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Collections;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Mecanim
{

    public interface IAnimatorOverrideSource
    {

        int GetOverrides(Animator animator, IList<KeyValuePair<AnimationClip, AnimationClip>> lst);

    }

    public class AnimatorOverrideCollection : List<KeyValuePair<AnimationClip, AnimationClip>>, IAnimatorOverrideSource, System.IDisposable
    {

        #region CONSTRUCTOR

        public AnimatorOverrideCollection()
        {

        }

        public AnimatorOverrideCollection(int capacity) : base(capacity)
        {

        }

        public AnimatorOverrideCollection(IEnumerable<KeyValuePair<AnimationClip, AnimationClip>> clips) : base(clips)
        {

        }

        public AnimatorOverrideCollection(IAnimatorOverrideSource source, Animator animator)
        {
            source.GetOverrides(animator, this);
        }

        public AnimatorOverrideCollection(AnimatorOverrideController controller) : base(controller.overridesCount)
        {
            controller.GetOverrides(this);
        }

        public AnimatorOverrideCollection(SPAnimatorOverrideLayers layers) : base(layers.GetOverridesCount())
        {
            layers.GetOverrides(this);
        }

        #endregion

        #region Methods

        public int GetOverrides(Animator animator, IList<KeyValuePair<AnimationClip, AnimationClip>> lst)
        {
            lst.AddRange(this);
            return this.Count;
        }

        public void SetOverride(string name, AnimationClip clip)
        {
            for (int i = 0; i < this.Count; i++)
            {
                var pair = this[i];
                if (pair.Key && pair.Key.name.Equals(name))
                {
                    this[i] = new KeyValuePair<AnimationClip, AnimationClip>(pair.Key, clip);
                    return;
                }
            }
        }

        #endregion

        #region IDisposable Interface

        protected virtual void Dispose()
        {

        }

        void System.IDisposable.Dispose()
        {
            this.Dispose();
        }

        #endregion

        #region Temp Source

        private static readonly ObjectCachePool<TempAnimatorOverrideCollection> _pool = new ObjectCachePool<TempAnimatorOverrideCollection>(-1, () => new TempAnimatorOverrideCollection(), (l) => l.Clear());

        public static AnimatorOverrideCollection GetTemp()
        {
            return _pool.GetInstance();
        }

        public static AnimatorOverrideCollection GetTemp(IEnumerable<KeyValuePair<AnimationClip, AnimationClip>> clips)
        {
            var result = _pool.GetInstance();
            result.AddRange(clips);
            return result;
        }

        public static AnimatorOverrideCollection GetTemp(IAnimatorOverrideSource source, Animator animator)
        {
            var result = _pool.GetInstance();
            source.GetOverrides(animator, result);
            return result;
        }

        public static AnimatorOverrideCollection GetTemp(SPAnimatorOverrideLayers layers)
        {
            var result = _pool.GetInstance();
            layers.GetOverrides(result);
            return result;
        }

        public static AnimatorOverrideCollection GetTemp(AnimatorOverrideController overrideController)
        {
            var result = _pool.GetInstance();
            overrideController.GetOverrides(result);
            return result;
        }

        private class TempAnimatorOverrideCollection : AnimatorOverrideCollection
        {
            protected override void Dispose()
            {
                this.Clear();
                if (this.Capacity < 128) //we only pool small collections
                {
                    _pool.Release(this);
                }
            }
        }

        #endregion

    }

    [System.Serializable]
    public class AnimatorOverrideSourceRef : IAnimatorOverrideSource
    {

        #region Fields

        [SerializeField]
        private UnityEngine.Object _obj;
        [SerializeField]
        private bool _treatUnconfiguredEntriesAsValidEntries;

        [System.NonSerialized]
        private object _runtimeRef;

        #endregion

        #region Properties

        public object Value
        {
            get { return _runtimeRef ?? _obj; }
            set
            {
                if (value is IAnimatorOverrideSource || value is AnimatorOverrideController || value is IProxy)
                {
                    _runtimeRef = value;
                    _obj = value as UnityEngine.Object;
                }
                else
                {
                    _runtimeRef = null;
                    _obj = null;
                }
            }
        }

        #endregion

        #region Methods

        public void SetOverrides(AnimatorOverrideController controller, bool treatUnconfiguredEntriesAsValidEntries = false)
        {
            _obj = controller;
            _runtimeRef = controller;
            _treatUnconfiguredEntriesAsValidEntries = treatUnconfiguredEntriesAsValidEntries;
        }

        public void SetOverrides(IAnimatorOverrideSource source)
        {
            _runtimeRef = source;
            _obj = source as UnityEngine.Object;
            _treatUnconfiguredEntriesAsValidEntries = false;
        }

        #endregion

        #region IAnimatorOverrideSource Interface

        int IAnimatorOverrideSource.GetOverrides(Animator animator, IList<KeyValuePair<AnimationClip, AnimationClip>> lst)
        {
            return MecanimExtensions.GetOverrides(this.Value, animator, lst, _treatUnconfiguredEntriesAsValidEntries, true);
        }

        #endregion

    }

}
