using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy.Collections;
using com.spacepuppy.Utils;

namespace com.spacepuppy
{

    /// <summary>
    /// Place on the root of a GameObject hierarchy, or a prefab, to signify that it is a complete entity.
    /// </summary>
    [DisallowMultipleComponent()]
    [DefaultExecutionOrder(31990)]
    public class SPEntity : SPComponent, INameable
    {
        public static readonly System.Action<IEntityAwakeHandler, SPEntity> OnEntityAwakeFunctor = (o, d) => o.OnEntityAwake(d);

        #region Multiton Interface

        private static EntityPool _pool = new EntityPool();
        public static EntityPool Pool
        {
            get
            {
                return _pool;
            }
        }

        #endregion

        #region Fields

        [System.NonSerialized()]
        private bool _isAwake;

        #endregion

        #region CONSTRUCTOR

        public SPEntity()
        {
            _nameCache = new NameCache.UnityObjectNameCache(this);
        }

        protected override void Awake()
        {
            this.AddTag(SPConstants.TAG_ROOT);
            Pool.AddReference(this);

            base.Awake();

            _isAwake = true;

            var token = Messaging.CreateBroadcastTokenIfReceiversExist<IEntityAwakeHandler>(this.gameObject, true);
            if (token != null && token.Count > 0)
            {
                com.spacepuppy.Hooks.EarlyStartHook.Invoke(this.gameObject, () =>
                {
                    token.Invoke(this, OnEntityAwakeFunctor);
                });
            }
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();

            Pool.RemoveReference(this);
        }

        #endregion

        #region Properties

        public bool IsAwake { get { return _isAwake; } }
        
        #endregion

        #region INameable Interface

        private NameCache.UnityObjectNameCache _nameCache;
        public new string name
        {
            get { return _nameCache.Name; }
            set { _nameCache.Name = value; }
        }
        string INameable.Name
        {
            get { return _nameCache.Name; }
            set { _nameCache.Name = value; }
        }
        public bool CompareName(string nm)
        {
            return _nameCache.CompareName(nm);
        }
        void INameable.SetDirty()
        {
            _nameCache.SetDirty();
        }

        #endregion

        #region Special Types

        public class EntityPool : MultitonPool<SPEntity>
        {

            public EntityPool() : base()
            {

            }

            #region EntityMultiton Methods

            public bool IsSource(object obj)
            {
                if (obj is SPEntity) return true;

                return GetFromSource(obj) != null;
            }

            public virtual SPEntity GetFromSource(object obj)
            {
                if (obj == null) return null;

                SPEntity result = obj as SPEntity;
                if (!object.ReferenceEquals(result, null)) return result;

                var go = GameObjectUtil.GetGameObjectFromSource(obj, true);
                if (go == null) return null;

                result = go.GetComponent<SPEntity>();
                if (!object.ReferenceEquals(result, null)) return result;

                result = go.AddOrGetComponent<SPEntityHook>().GetEntity();
                return result;
            }

            public bool GetFromSource(object obj, out SPEntity comp)
            {
                comp = GetFromSource(obj);
                return comp != null;
            }





            public bool IsSource<TSub>(object obj) where TSub : SPEntity
            {
                if (obj is TSub) return true;

                return GetFromSource<TSub>(obj) != null;
            }

            public virtual TSub GetFromSource<TSub>(object obj) where TSub : SPEntity
            {
                if (obj == null) return null;
                if (obj is TSub) return obj as TSub;

                var go = GameObjectUtil.GetGameObjectFromSource(obj, true);
                if (go == null) return null;

                var e = go.GetComponent<SPEntity>();
                if (!object.ReferenceEquals(e, null)) return e as TSub;

                return go.AddOrGetComponent<SPEntityHook>().GetEntity() as TSub;
            }

            public virtual SPEntity GetFromSource(System.Type tp, object obj)
            {
                if (tp == null || obj == null) return null;
                if (obj is SPEntity) return tp.IsInstanceOfType(obj) ? obj as SPEntity : null;

                var go = GameObjectUtil.GetGameObjectFromSource(obj, true);
                if (go == null) return null;

                var e = go.GetComponent<SPEntity>();
                if (!object.ReferenceEquals(e, null)) return tp.IsInstanceOfType(e) ? e : null;

                e = go.AddOrGetComponent<SPEntityHook>().GetEntity();
                if (!object.ReferenceEquals(e, null)) return tp.IsInstanceOfType(e) ? e : null;

                return null;
            }

            public bool GetFromSource<TSub>(object obj, out TSub comp) where TSub : SPEntity
            {
                comp = GetFromSource<TSub>(obj);
                return comp != null;
            }

            public bool GetFromSource(System.Type tp, object obj, out SPEntity comp)
            {
                comp = GetFromSource(tp, obj);
                return comp != null;
            }

            #endregion

        }

        public class SPEntityHook : MonoBehaviour
        {
            #region Fields

            private SPEntity _entity;
            private bool _synced;

            #endregion

            #region CONSTRUCTOR

            private void OnDisable()
            {
                if (_synced)
                {
                    _synced = false;
                    if (_entity != null && !this.transform.IsChildOf(_entity.transform))
                    {
                        _entity = null;
                    }
                }
            }

            #endregion

            #region Methods

            public SPEntity GetEntity()
            {
                if (!_synced)
                {
                    _synced = true;
                    _entity = this.GetComponentInParent<SPEntity>();
                }
                return _entity;
            }

            private void OnTransformParentChanged()
            {
                _synced = false;
                _entity = null;
            }

            #endregion

        }

        #endregion

#if UNITY_EDITOR

        protected virtual void OnValidate()
        {
            if (!this.HasTag(SPConstants.TAG_ROOT)) this.AddTag(SPConstants.TAG_ROOT);
        }

#else

        protected virtual void OnValidate() { }

#endif

    }

}
