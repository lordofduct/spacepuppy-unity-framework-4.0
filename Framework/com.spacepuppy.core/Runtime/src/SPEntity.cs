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
    [DefaultExecutionOrder(SPEntity.DEFAULT_EXECUTION_ORDER)] //yes, this inherits
    public class SPEntity : SPComponent, INameable
    {
        public const int DEFAULT_EXECUTION_ORDER = 31990;

        public static readonly System.Action<IEntityAwakeHandler, SPEntity> OnEntityAwakeFunctor = (o, d) => o.OnEntityAwake(d);

        #region Multiton Interface

        public static readonly EntityPool Pool = new EntityPool();

        #endregion

        #region Fields

        [System.NonSerialized()]
        private bool _isAwake;

        #endregion

        #region CONSTRUCTOR

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

        #region Methods

        public SPEntity GetParentEntity()
        {
            if (this.transform.parent)
            {
                Pool.GetFromSource(this.transform.parent);
            }

            return null;
        }

        public bool TryGetParentEntity<T>(out SPEntity parent)
        {
            if (this.transform.parent)
            {
                return Pool.GetFromSource(this.transform.parent, out parent);
            }
            else
            {
                parent = null;
                return false;
            }
        }

        public T GetParentEntity<T>() where T : SPEntity
        {
            if (this.transform.parent)
            {
                Pool.GetFromSource<T>(this.transform.parent);
            }

            return null;
        }

        public bool TryGetParentEntity<T>(out T parent) where T : SPEntity
        {
            if (this.transform.parent)
            {
                return Pool.GetFromSource<T>(this.transform.parent, out parent);
            }
            else
            {
                parent = null;
                return false;
            }
        }

        #endregion

        #region Special Types

        public class EntityPool : MultitonPool<SPEntity>
        {

            public EntityPool() : base()
            {

            }

            #region EntityMultiton Methods

            public bool IsSource(object obj) => GetFromSource(obj) != null;

#if UNITY_EDITOR
            public SPEntity GetFromSource(object obj)
            {
                var go = GameObjectUtil.GetGameObjectFromSource(obj);
                return go ? go.GetComponentInParent<SPEntity>(true) : null;
            }
            public SPEntity GetFromSource(GameObject obj) => obj ? obj.GetComponentInParent<SPEntity>() : null;
            public SPEntity GetFromSource(Component obj) => obj ? obj.gameObject.GetComponentInParent<SPEntity>() : null;
#else
            public SPEntity GetFromSource(object obj) => obj is SPEntity e ? e : GameObjectUtil.GetGameObjectFromSource(obj).AddOrGetComponent<SPEntityHook>().GetEntity();

            public SPEntity GetFromSource(GameObject obj) => obj ? obj.AddOrGetComponent<SPEntityHook>().GetEntity() : null;
            public SPEntity GetFromSource(Component obj) => obj is SPEntity e ? e : (obj ? obj.gameObject.AddOrGetComponent<SPEntityHook>().GetEntity() : null);
#endif

            public bool GetFromSource(object obj, out SPEntity comp)
            {
                comp = GetFromSource(obj);
                return comp != null;
            }





            public bool IsSource<TSub>(object obj) where TSub : SPEntity => GetFromSource<TSub>(obj) != null;

#if UNITY_EDITOR
            public TSub GetFromSource<TSub>(object obj) where TSub : SPEntity
            {
                var go = GameObjectUtil.GetGameObjectFromSource(obj);
                if (go != null)
                {
                    var e = go.GetComponentInParent<TSub>(true);
                    if (e is SPEntity) return e;
                }

                return null;
            }
            public TSub GetFromSource<TSub>(GameObject obj) where TSub : SPEntity => obj ? obj.GetComponentInParent<TSub>() : null;
            public TSub GetFromSource<TSub>(Component obj) where TSub : SPEntity => obj ? obj.gameObject.GetComponentInParent<TSub>() : null;
#else
            public TSub GetFromSource<TSub>(object obj) where TSub : SPEntity => obj is SPEntity e ? e as TSub : GameObjectUtil.GetGameObjectFromSource(obj).AddOrGetComponent<SPEntityHook>().GetEntity() as TSub;

            public TSub GetFromSource<TSub>(GameObject obj) where TSub : SPEntity => obj ? GameObjectUtil.GetGameObjectFromSource(obj).AddOrGetComponent<SPEntityHook>().GetEntity() as TSub : null;

            public TSub GetFromSource<TSub>(Component obj) where TSub : SPEntity => obj is SPEntity e ? e as TSub : (obj ? obj.gameObject.AddOrGetComponent<SPEntityHook>().GetEntity() as TSub : null);
#endif

            public virtual SPEntity GetFromSource(System.Type tp, object obj)
            {
                var e = this.GetFromSource(obj);
                return !object.ReferenceEquals(e, null) && tp.IsInstanceOfType(e) ? e : null;
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

        class SPEntityHook : MonoBehaviour
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
                    _entity = null;
                }
            }

            #endregion

            #region Methods

            public SPEntity GetEntity()
            {
                if (!_synced)
                {
                    _synced = true;
                    _entity = this.GetComponentInParent<SPEntity>(true);
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
