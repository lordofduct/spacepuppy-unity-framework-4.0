using UnityEngine;
using System.Collections.Generic;
using com.spacepuppy.Collections;
using com.spacepuppy.Utils;
using System.Linq;

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

        #endregion

        #region Methods

        public int GetOverrides(Animator animator, IList<KeyValuePair<AnimationClip, AnimationClip>> lst)
        {
            lst.AddRange(this);
            return this.Count;
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

        private class TempAnimatorOverrideCollection : AnimatorOverrideCollection
        {
            protected override void Dispose()
            {
                _pool.Release(this);
            }
        }

        #endregion

    }

    [System.Serializable]
    public class AnimatorOverrideSourceRef : IAnimatorOverrideSource, ISerializationCallbackReceiver
    {

        #region Fields

        [SerializeField]
        private UnityEngine.Object _obj;
        [SerializeField]
        private bool _treatUnconfiguredEntriesAsValidEntries;

        [System.NonSerialized]
        private IAnimatorOverrideSource _value;

        #endregion

        #region Properties

        public object Value
        {
            get { return (object)_value ?? _obj; }
            set
            {
                if(value is IAnimatorOverrideSource || value is AnimatorOverrideController)
                {
                    _value = value as IAnimatorOverrideSource;
                    _obj = value as UnityEngine.Object;
                }
                else
                {
                    _value = null;
                    _obj = null;
                }
            }
        }

        #endregion

        #region Methods

        public void SetOverrides(AnimatorOverrideController controller, bool treatUnconfiguredEntriesAsValidEntries = false)
        {
            _obj = controller;
            _value = null;
            _treatUnconfiguredEntriesAsValidEntries = treatUnconfiguredEntriesAsValidEntries;
        }

        public void SetOverrides(IAnimatorOverrideSource source)
        {
            _value = source;
            _obj = source as UnityEngine.Object;
            _treatUnconfiguredEntriesAsValidEntries = false;
        }

        #endregion

        #region IAnimatorOverrideSource Interface

        int IAnimatorOverrideSource.GetOverrides(Animator animator, IList<KeyValuePair<AnimationClip, AnimationClip>> lst)
        {
            if(_value != null)
            {
                return _value.GetOverrides(animator, lst);
            }
            else if(_obj is AnimatorOverrideController ctrl)
            {
                if(lst is List<KeyValuePair<AnimationClip, AnimationClip>>)
                {
                    ctrl.GetOverrides(lst as List<KeyValuePair<AnimationClip, AnimationClip>>);
                    if(!_treatUnconfiguredEntriesAsValidEntries)
                    {
                        for(int i = 0; i < lst.Count; i++)
                        {
                            if(lst[i].Value == null)
                            {
                                lst.RemoveAt(i);
                                i--;
                            }
                        }
                    }
                    return lst.Count;
                }
                else
                {
                    lst.Clear();
                    using (var tlst = AnimatorOverrideCollection.GetTemp())
                    {
                        ctrl.GetOverrides(tlst);
                        for(int i = 0; i < tlst.Count; i++)
                        {
                            if(_treatUnconfiguredEntriesAsValidEntries || tlst[i].Value != null)
                            {
                                lst.Add(tlst[i]);
                            }
                        }
                        return lst.Count;
                    }
                }
            }

            return 0;
        }

        #endregion

        #region ISerializationCallbackReceiver Interface

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            _value = _obj as IAnimatorOverrideSource;
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            //do nothing
        }

        #endregion

    }

}
