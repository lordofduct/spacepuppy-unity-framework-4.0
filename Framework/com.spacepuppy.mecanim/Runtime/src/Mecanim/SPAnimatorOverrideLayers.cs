using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy.Collections;
using com.spacepuppy.Utils;

using IOverrideList = System.Collections.Generic.IList<System.Collections.Generic.KeyValuePair<UnityEngine.AnimationClip, UnityEngine.AnimationClip>>;

namespace com.spacepuppy.Mecanim
{

    [RequireComponent(typeof(Animator))]
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-31951)]
    public class SPAnimatorOverrideLayers : SPComponent, IIndexedEnumerable<SPAnimatorOverrideLayers.LayerInfo>
    {

        #region Fields

        [SerializeField]
        [DisableOnPlay]
        private AnimatorOverrideController _initialRuntimeAnimatorController;

        [System.NonSerialized]
        private Animator _animator;
        [System.NonSerialized]
        private RuntimeAnimatorController _baseRuntimeAnimatorController;
        [System.NonSerialized]
        private AnimatorOverrideController _overrideAnimatorController;

        [System.NonSerialized]
        private Dictionary<string, AnimationClip> _baseAnimations;
        [System.NonSerialized]
        private List<LayerInfo> _layers;
        [System.NonSerialized]
        private bool _suspended;

        [System.NonSerialized]
        private ObjectCachePool<List<KeyValuePair<AnimationClip, AnimationClip>>> _pool;

        #endregion

        #region CONSTRUCTOR

        protected override void Awake()
        {
            base.Awake();

            _animator = this.GetComponent<Animator>();
            _baseRuntimeAnimatorController = _animator.runtimeAnimatorController;
        }

        protected override void Start()
        {
            base.Start();

            if(!_suspended)
            {
                this.Apply();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            if (_animator != null && _baseRuntimeAnimatorController != null)
            {
                _animator.runtimeAnimatorController = _baseRuntimeAnimatorController;
            }

            if (_overrideAnimatorController != null)
            {
                Destroy(_overrideAnimatorController);
                _overrideAnimatorController = null;
            }
        }

        #endregion

        #region Properties

        public AnimatorOverrideController InitialRuntimeAnimatorController { get { return _initialRuntimeAnimatorController; } }

        /// <summary>
        /// True if Apply is automatically called when adding any overrides. If false, Apply must be called manually.
        /// </summary>
        public bool Suspended => _suspended;

        public int LayerCount { get { return _layers?.Count ?? 0; } }

        /// <summary>
        /// Retrieve a layer at an index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public LayerInfo this[int index]
        {
            get
            {
                if (_layers == null) throw new System.IndexOutOfRangeException();
                return _layers[index];
            }
        }

        /// <summary>
        /// Retrieve a layer by its token key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public LayerInfo this[object key]
        {
            get
            {
                if (_layers == null) throw new System.Collections.Generic.KeyNotFoundException();

                for (int i = 0; i < _layers.Count; i++)
                {
                    if (EqualityComparer<object>.Default.Equals(_layers[i].Token, key)) return _layers[i];
                }

                throw new System.Collections.Generic.KeyNotFoundException();
            }
        }

        #endregion

        #region Methods

        public void UpdateInitialRuntimeAnimatorController(AnimatorOverrideController controller, bool clearAllOverrides = false)
        {
            if (!ObjUtil.IsObjectAlive(this)) return;
            if (_initialRuntimeAnimatorController == controller) return;

            _initialRuntimeAnimatorController = controller;
            if (clearAllOverrides)
            {
                this.ResetOverrides();
            }
            else if (!_suspended)
            {
                this.Apply();
            }
        }

        public void ResetOverrides()
        {
            if (!ObjUtil.IsObjectAlive(this)) return;

            if (_layers != null && _layers.Count > 0)
            {
                for (int i = 0; i < _layers.Count; i++)
                {
                    _pool.Release(_layers[i].Overrides);
                }
                _layers.Clear();
            }

            if (!_suspended)
            {
                if (_initialRuntimeAnimatorController != null)
                {
                    _animator.runtimeAnimatorController = _initialRuntimeAnimatorController;
                }
                else
                {
                    _animator.runtimeAnimatorController = _baseRuntimeAnimatorController;
                }
            }
        }

        public void Apply()
        {
            if (_layers != null && _layers.Count > 0)
            {
                using (var lst = AnimatorOverrideCollection.GetTemp())
                {
                    this.GetOverrides(lst);
                    _overrideAnimatorController.ApplyOverrides(lst);
                    _animator.runtimeAnimatorController = _overrideAnimatorController;
                }
            }
            else
            {
                if (_initialRuntimeAnimatorController != null)
                {
                    _animator.runtimeAnimatorController = _initialRuntimeAnimatorController;
                }
                else
                {
                    _animator.runtimeAnimatorController = _baseRuntimeAnimatorController;
                }
            }
        }

        public bool TryGetLayer(object key, out LayerInfo info)
        {
            if (_layers != null)
            {
                for (int i = 0; i < _layers.Count; i++)
                {
                    if (EqualityComparer<object>.Default.Equals(_layers[i].Token, key))
                    {
                        info = _layers[i];
                        return true;
                    }
                }
            }

            info = default(LayerInfo);
            return false;
        }

        public void Stack(AnimatorOverrideController controller, object token, bool treatUnconfiguredEntriesAsValidEntries = false)
        {
            if (!ObjUtil.IsObjectAlive(this)) return;
            if (controller == null) return;
            if (object.ReferenceEquals(_overrideAnimatorController, null)) this.InitializeOverrideController();

            LayerInfo layer;
            int index = this.IndexOf(token);
            if (index >= 0)
            {
                layer = _layers[index];
                layer.Token = token;
                layer.Overrides.Clear();
                _layers.RemoveAt(index);
            }
            else
            {
                layer = new LayerInfo()
                {
                    Token = token,
                    Overrides = _pool.GetInstance(),
                };
            }

            controller.GetOverrides(layer.Overrides);
            if (!treatUnconfiguredEntriesAsValidEntries)
            {
                for (int i = 0; i < layer.Overrides.Count; i++)
                {
                    if (layer.Overrides[i].Value == null)
                    {
                        layer.Overrides.RemoveAt(i);
                        i--;
                    }
                }
            }

            _layers.Add(layer);

            if (!_suspended)
            {
                this.Apply();
            }
        }

        public void Stack(IAnimatorOverrideSource source, object token)
        {
            if (source == null) return;

            if(source is IOverrideList lst)
            {
                this.Stack(lst, token);
            }
            else
            {
                using (var tlst = AnimatorOverrideCollection.GetTemp())
                {
                    if (source.GetOverrides(_animator, tlst) > 0)
                    {
                        this.Stack((IOverrideList)tlst, token);
                    }
                }
            }
        }

        public void Stack(AnimatorOverrideCollection coll, object token)
        {
            this.Stack((IOverrideList)coll, token);
        }

        public void Stack(IList<KeyValuePair<AnimationClip, AnimationClip>> overrides, object token)
        {
            if (!ObjUtil.IsObjectAlive(this)) return;
            if (overrides == null || overrides.Count == 0) return;
            if (object.ReferenceEquals(_overrideAnimatorController, null)) this.InitializeOverrideController();

            LayerInfo layer;
            int index = this.IndexOf(token);
            if (index >= 0)
            {
                layer = _layers[index];
                layer.Token = token;
                layer.Overrides.Clear();
                _layers.RemoveAt(index);
            }
            else
            {
                layer = new LayerInfo()
                {
                    Token = token,
                    Overrides = _pool.GetInstance(),
                };
            }

            layer.Overrides.AddRange(overrides);
            _layers.Add(layer);

            if (!_suspended)
            {
                this.Apply();
            }
        }

        public void Insert(int index, AnimatorOverrideController controller, object token, bool treatUnconfiguredEntriesAsValidEntries = false)
        {
            if (!ObjUtil.IsObjectAlive(this)) return;
            if (controller == null) return;
            if (object.ReferenceEquals(_overrideAnimatorController, null)) this.InitializeOverrideController();

            LayerInfo layer;
            int oldindex = this.IndexOf(token);
            if (oldindex >= 0)
            {
                layer = _layers[oldindex];
                layer.Token = token;
                layer.Overrides.Clear();
                _layers.RemoveAt(oldindex);
            }
            else
            {
                layer = new LayerInfo()
                {
                    Token = token,
                    Overrides = _pool.GetInstance(),
                };
            }

            controller.GetOverrides(layer.Overrides);
            if (!treatUnconfiguredEntriesAsValidEntries)
            {
                for (int i = 0; i < layer.Overrides.Count; i++)
                {
                    if (layer.Overrides[i].Value == null)
                    {
                        layer.Overrides.RemoveAt(i);
                        i--;
                    }
                }
            }

            _layers.Insert(Mathf.Clamp(index, 0, _layers.Count), layer);

            if (!_suspended)
            {
                this.Apply();
            }
        }

        public void Insert(int index, IList<KeyValuePair<AnimationClip, AnimationClip>> overrides, object token)
        {
            if (!ObjUtil.IsObjectAlive(this)) return;
            if (overrides == null || overrides.Count == 0) return;
            if (object.ReferenceEquals(_overrideAnimatorController, null)) this.InitializeOverrideController();

            LayerInfo layer;
            int oldindex = this.IndexOf(token);
            if (oldindex >= 0)
            {
                layer = _layers[oldindex];
                layer.Token = token;
                layer.Overrides.Clear();
                _layers.RemoveAt(oldindex);
            }
            else
            {
                layer = new LayerInfo()
                {
                    Token = token,
                    Overrides = _pool.GetInstance(),
                };
            }

            layer.Overrides.AddRange(overrides);
            _layers.Insert(Mathf.Clamp(index, 0, _layers.Count), layer);

            if (!_suspended)
            {
                this.Apply();
            }
        }

        public void Remove(object token)
        {
            if (!ObjUtil.IsObjectAlive(this)) return;
            if (object.ReferenceEquals(_overrideAnimatorController, null)) return;

            int index = this.IndexOf(token);
            if (index >= 0)
            {
                _pool.Release(_layers[index].Overrides);
                _layers.RemoveAt(index);

                if (!_suspended)
                {
                    this.Apply();
                }
            }
        }

        public void RemoveAt(int index)
        {
            if (_layers == null) throw new System.IndexOutOfRangeException();

            _layers.RemoveAt(index);
            if (!_suspended)
            {
                this.Apply();
            }
        }

        public void Pop()
        {
            if (!ObjUtil.IsObjectAlive(this)) return;
            if (object.ReferenceEquals(_overrideAnimatorController, null)) return;

            if (_layers.Count > 0)
            {
                int index = _layers.Count - 1;
                _pool.Release(_layers[index].Overrides);
                _layers.RemoveAt(index);

                if (!_suspended)
                {
                    this.Apply();
                }
            }
        }

        public int GetInitialAnimationOverrides(IList<KeyValuePair<AnimationClip, AnimationClip>> lst)
        {
            if (!ObjUtil.IsObjectAlive(this)) return 0;

            if (lst is List<KeyValuePair<AnimationClip, AnimationClip>> rlst)
            {
                rlst.Clear();
                if (_initialRuntimeAnimatorController != null)
                {
                    _initialRuntimeAnimatorController.GetOverrides(rlst);
                }
                else
                {
                    _overrideAnimatorController.GetOverrides(rlst);
                    for (int i = 0; i < rlst.Count; i++)
                    {
                        rlst[i] = new KeyValuePair<AnimationClip, AnimationClip>(rlst[i].Key, null);
                    }
                }
                return rlst.Count;
            }
            else
            {
                using (var tlst = AnimatorOverrideCollection.GetTemp())
                {
                    if (_initialRuntimeAnimatorController != null)
                    {
                        _initialRuntimeAnimatorController.GetOverrides(tlst);
                        lst.AddRange(tlst);
                    }
                    else
                    {
                        _overrideAnimatorController.GetOverrides(tlst);
                        for (int i = 0; i < tlst.Count; i++)
                        {
                            lst.Add(new KeyValuePair<AnimationClip, AnimationClip>(lst[i].Key, null));
                        }
                    }

                    return tlst.Count;
                }
            }
        }

        public int GetInitialAnimationOverrides(IDictionary<AnimationClip, AnimationClip> dict)
        {
            if (!ObjUtil.IsObjectAlive(this)) return 0;

            dict.Clear();
            using (var lst = AnimatorOverrideCollection.GetTemp())
            {
                if (_initialRuntimeAnimatorController != null)
                {
                    _initialRuntimeAnimatorController.GetOverrides(lst);
                    for (int i = 0; i < lst.Count; i++)
                    {
                        dict[lst[i].Key] = lst[i].Value;
                    }
                }
                else
                {
                    _overrideAnimatorController.GetOverrides(lst);
                    for (int i = 0; i < lst.Count; i++)
                    {
                        dict[lst[i].Key] = null;
                    }
                }
                return lst.Count;
            }
        }

        public int GetOverrides(IList<KeyValuePair<AnimationClip, AnimationClip>> lst)
        {
            if (!ObjUtil.IsObjectAlive(this)) return 0;

            if (_layers == null || _layers.Count == 0)
            {
                return this.GetInitialAnimationOverrides(lst);
            }

            lst.Clear();
            using (var dict = TempCollection.GetDict<AnimationClip, AnimationClip>())
            {
                GetInitialAnimationOverrides(dict);
                for (int i = 0; i < _layers.Count; i++)
                {
                    var overrides = _layers[i].Overrides;
                    for (int j = 0; j < overrides.Count; j++)
                    {
                        dict[overrides[j].Key] = overrides[j].Value;
                    }
                }

                var e = dict.GetEnumerator();
                while (e.MoveNext())
                {
                    lst.Add(e.Current);
                }
            }

            return lst.Count;
        }

        public int GetOverrides(IDictionary<AnimationClip, AnimationClip> dict)
        {
            if (!ObjUtil.IsObjectAlive(this)) return 0;

            if (_layers == null || _layers.Count == 0)
            {
                return this.GetInitialAnimationOverrides(dict);
            }

            GetInitialAnimationOverrides(dict);
            for (int i = 0; i < _layers.Count; i++)
            {
                var overrides = _layers[i].Overrides;
                for (int j = 0; j < overrides.Count; j++)
                {
                    dict[overrides[j].Key] = overrides[j].Value;
                }
            }

            return dict.Count;
        }

        public int GetOverrides(IList<KeyValuePair<AnimationClip, AnimationClip>> lst, object token)
        {
            if (!ObjUtil.IsObjectAlive(this)) return 0;
            if (object.ReferenceEquals(_overrideAnimatorController, null)) return 0;

            int index = this.IndexOf(token);
            if (index >= 0)
            {
                lst.AddRange(_layers[index].Overrides);
                return _layers[index].Overrides.Count;
            }

            return 0;
        }

        public int GetOverrides(IDictionary<AnimationClip, AnimationClip> dict, object token)
        {
            if (!ObjUtil.IsObjectAlive(this)) return 0;
            if (object.ReferenceEquals(_overrideAnimatorController, null)) return 0;


            int index = this.IndexOf(token);
            if (index >= 0)
            {
                var overrides = _layers[index].Overrides;
                for (int i = 0; i < overrides.Count; i++)
                {
                    dict[overrides[i].Key] = overrides[i].Value;
                }
                return overrides.Count;
            }

            return 0;
        }

        public int IndexOf(object token)
        {
            for (int i = 0; i < _layers.Count; i++)
            {
                if (EqualityComparer<object>.Default.Equals(_layers[i].Token, token)) return i;
            }
            return -1;
        }

        public void SuspendSync()
        {
            _suspended = true;
        }

        public void ResumeSync()
        {
            _suspended = false;
        }

        /// <summary>
        /// Get all the AnimationClips that take the role of the 'Key' in all KeyValuePair<AnimatoinClip,AnimationClip> that can be applied as an override.
        /// </summary>
        /// <returns></returns>
        public AnimationClip[] GetBaseAnimations()
        {
            if (_baseAnimations == null) this.InitializeBaseAnimLookup();

            return _baseAnimations.Values.ToArray();
        }

        /// <summary>
        /// Get all the AnimationClips that take the role of the 'Key' in all KeyValuePair<AnimatoinClip,AnimationClip> that can be applied as an override.
        /// </summary>
        /// <returns></returns>
        public int GetBaseAnimations(IList<AnimationClip> lst)
        {
            if (_baseAnimations == null) this.InitializeBaseAnimLookup();

            var e = _baseAnimations.GetEnumerator();
            while (e.MoveNext())
            {
                lst.Add(e.Current.Value);
            }
            return _baseAnimations.Count;
        }

        /// <summary>
        /// Find the AnimationClip by name that takes the role of the 'Key' in all KeyValuePair<AnimatoinClip,AnimationClip> that can be applied as an override.
        /// </summary>
        /// <returns></returns>
        public AnimationClip FindBaseAnimation(string name)
        {
            AnimationClip clip;
            _baseAnimations.TryGetValue(name, out clip);
            return clip;
        }


        private void InitializeOverrideController()
        {
            if (_overrideAnimatorController != null) return;

            _overrideAnimatorController = new AnimatorOverrideController(_baseRuntimeAnimatorController);
            if (_initialRuntimeAnimatorController != null)
            {
                using (var lst = AnimatorOverrideCollection.GetTemp())
                {
                    _initialRuntimeAnimatorController.GetOverrides(lst);
                    _overrideAnimatorController.ApplyOverrides(lst);
                }
            }

            if (_layers == null) _layers = new List<LayerInfo>();
            else _layers.Clear();
            if (_pool == null) _pool = new ObjectCachePool<List<KeyValuePair<AnimationClip, AnimationClip>>>(-1, () => new List<KeyValuePair<AnimationClip, AnimationClip>>(), l => l.Clear());
        }

        private void InitializeBaseAnimLookup()
        {
            _baseAnimations = new Dictionary<string, AnimationClip>();

            var arr = _baseRuntimeAnimatorController.animationClips;
            for (int i = 0; i < arr.Length; i++)
            {
                _baseAnimations[arr[i].name] = arr[i];
            }
        }

        #endregion

        #region IIndexedEnumerable Interface

        int IReadOnlyCollection<LayerInfo>.Count => this.LayerCount;

        bool IIndexedEnumerable<LayerInfo>.Contains(LayerInfo item)
        {
            return ((IIndexedEnumerable<LayerInfo>)this).IndexOf(item) >= 0;
        }

        void IIndexedEnumerable<LayerInfo>.CopyTo(LayerInfo[] array, int startIndex)
        {
            _layers?.CopyTo(array, startIndex);
        }

        int IIndexedEnumerable<LayerInfo>.IndexOf(LayerInfo item)
        {
            if (_layers == null) return -1;

            for (int i = 0; i < _layers.Count; i++)
            {
                if (EqualityComparer<object>.Default.Equals(_layers[i].Token, item.Token)
                    && object.ReferenceEquals(_layers[i].Overrides, item.Overrides))
                {
                    return i;
                }
            }
            return -1;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<LayerInfo> IEnumerable<LayerInfo>.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        #region Special Types

        /// <summary>
        /// The Token and Overrides list for this override layer. You can modify the Overrides list (but not replace it), but you must call "Apply" on the SPAnimatorExtender for the changes to take effect.
        /// </summary>
        public struct LayerInfo
        {
            public object Token;
            public List<KeyValuePair<AnimationClip, AnimationClip>> Overrides;
        }

        public struct Enumerator : IEnumerator<LayerInfo>
        {
            private List<LayerInfo>.Enumerator _e;

            internal Enumerator(SPAnimatorOverrideLayers owner)
            {
                if (object.ReferenceEquals(owner, null)) throw new System.ArgumentNullException(nameof(owner));
                _e = owner._layers != null ? owner._layers.GetEnumerator() : default(List<LayerInfo>.Enumerator);
            }

            public LayerInfo Current => _e.Current;

            object System.Collections.IEnumerator.Current => this.Current;

            void System.IDisposable.Dispose()
            {
                _e.Dispose();
            }

            public bool MoveNext()
            {
                return _e.MoveNext();
            }

            void System.Collections.IEnumerator.Reset()
            {
                (_e as System.Collections.IEnumerator).Reset();
            }
        }

        #endregion

    }

}
