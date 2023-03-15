#if SP_ADDRESSABLES
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using com.spacepuppy.Async;
using com.spacepuppy.Utils;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.spacepuppy.Addressables
{

    /// <summary>
    /// Similar to AssetReference, but can reference either an addressable OR a direct reference.
    /// </summary>
    [System.Serializable]
    public class DynamicAssetReference : AssetReference
    {

        #region Fields

        [SerializeField]
        private UnityEngine.Object _directReference;

        #endregion

        #region CONSTRUCTOR

        public DynamicAssetReference() : base()
        {
            _directReference = null;
        }

        public DynamicAssetReference(string guid) : base(guid)
        {
            _directReference = null;
        }

        public DynamicAssetReference(UnityEngine.Object direct) : base()
        {
            _directReference = direct;
        }

        #endregion

        #region Properties

        public bool IsAddressable => !string.IsNullOrEmpty(this.AssetGUID);

        public bool IsDirectAssetReference => string.IsNullOrEmpty(this.AssetGUID);

        public UnityEngine.Object DirectAssetReference => _directReference;

        public virtual System.Type ExpectedAssetType => typeof(UnityEngine.Object);

        #endregion

        #region Methods

        public AsyncWaitHandle<T> LoadOrGetDynamicAssetAsync<T>() where T : class => this.IsDirectAssetReference ? new AsyncWaitHandle<T>(ObjUtil.GetAsFromSource<T>(_directReference, false)) : this.LoadAssetAsync<T>().AsAsyncWaitHandle();

        #endregion

        #region AssetReference Interface
#if UNITY_EDITOR
        public override bool SetEditorAsset(UnityEngine.Object value)
        {
            if (base.SetEditorAsset(value))
            {
                _directReference = null;
                return true;
            }
            else
            {
                return false;
            }
        }

        public virtual bool SetDirectReference(UnityEngine.Object value)
        {
            if (this.ValidateAsset(value))
            {
                base.SetEditorAsset(null);
                _directReference = value;
                return true;
            }
            else
            {
                return false;
            }
        }
#endif
        #endregion

    }

    /// <summary>
    /// Similar to AssetReference, but can reference either an addressable OR a direct reference.
    /// </summary>
    [System.Serializable]
    public class DynamicAssetReferenceT<TObject> : DynamicAssetReference where TObject : UnityEngine.Object
    {

        #region CONSTRUCTOR

        public DynamicAssetReferenceT() : base(string.Empty)
        {

        }

        public DynamicAssetReferenceT(string guid) : base(guid)
        {

        }

        public DynamicAssetReferenceT(TObject direct) : base(direct)
        {

        }

        #endregion

        #region Properties

        public new TObject DirectAssetReference => ObjUtil.GetAsFromSource<TObject>(base.DirectAssetReference, false);

        public override System.Type ExpectedAssetType => typeof(TObject);

        #endregion

        #region AssetReference Overrides

        public virtual AsyncWaitHandle<TObject> LoadOrGetDynamicAssetAsync() => this.IsDirectAssetReference ? new AsyncWaitHandle<TObject>(this.DirectAssetReference) : this.LoadAssetAsync<TObject>().AsAsyncWaitHandle();

        public virtual AsyncOperationHandle<TObject> LoadAssetAsync()
        {
            return LoadAssetAsync<TObject>();
        }

        public override bool ValidateAsset(Object obj)
        {
            var type = obj.GetType();
            return typeof(TObject).IsAssignableFrom(type);
        }

        public override bool ValidateAsset(string mainAssetPath)
        {
#if UNITY_EDITOR
            if (typeof(TObject).IsAssignableFrom(AssetDatabase.GetMainAssetTypeAtPath(mainAssetPath)))
                return true;

            var repr = AssetDatabase.LoadAllAssetRepresentationsAtPath(mainAssetPath);
            return repr != null && repr.Any(o => o is TObject);
#else
            return false;
#endif
        }

#if UNITY_EDITOR
        public new TObject editorAsset
        {
            get
            {
                Object baseAsset = base.editorAsset;
                TObject asset = baseAsset as TObject;
                if (asset == null && baseAsset != null)
                    Debug.Log("editorAsset cannot cast to " + typeof(TObject));
                return asset;
            }
        }
#endif

        #endregion

    }

    /// <summary>
    /// Similar to AssetReference, but can reference either an addressable OR a direct reference.
    /// </summary>
    [System.Serializable]
    public class DynamicAssetReferenceIT<TInterface> : DynamicAssetReference where TInterface : class
    {

        #region CONSTRUCTOR

        public DynamicAssetReferenceIT() : base(string.Empty)
        {
        }

        public DynamicAssetReferenceIT(string guid) : base(guid)
        {
        }

        public DynamicAssetReferenceIT(TInterface direct) : base(direct as UnityEngine.Object)
        {
        }

        #endregion

        #region Properties

        public new TInterface DirectAssetReference => ObjUtil.GetAsFromSource<TInterface>(base.DirectAssetReference, false);

        public override System.Type ExpectedAssetType => typeof(TInterface);

        #endregion

        #region AssetRefeance Overrides

        public virtual AsyncWaitHandle<TInterface> LoadOrGetDynamicAssetAsync() => this.IsDirectAssetReference ? new AsyncWaitHandle<TInterface>(this.DirectAssetReference) : this.LoadAssetAsync<TInterface>().AsAsyncWaitHandle();

        public virtual AsyncOperationHandle<TInterface> LoadAssetAsync()
        {
            return LoadAssetAsync<TInterface>();
        }

        public override bool ValidateAsset(Object obj)
        {
            var type = obj.GetType();
            return typeof(TInterface).IsAssignableFrom(type);
        }

        public override bool ValidateAsset(string mainAssetPath)
        {
#if UNITY_EDITOR
            if (typeof(TInterface).IsAssignableFrom(AssetDatabase.GetMainAssetTypeAtPath(mainAssetPath)))
                return true;

            var repr = AssetDatabase.LoadAllAssetRepresentationsAtPath(mainAssetPath);
            return repr != null && repr.Any(o => o is TInterface);
#else
            return false;
#endif
        }

#if UNITY_EDITOR
        public new TInterface editorAsset
        {
            get
            {
                Object baseAsset = base.editorAsset;
                TInterface asset = baseAsset as TInterface;
                if (asset == null && baseAsset != null)
                    Debug.Log("editorAsset cannot cast to " + typeof(TInterface));
                return asset;
            }
        }
#endif

        #endregion

    }

    [System.Serializable]
    public class DynamicAssetReferenceGameObject : DynamicAssetReferenceT<GameObject> { }

}
#endif
