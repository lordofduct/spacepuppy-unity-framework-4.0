using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace com.spacepuppy.Project
{

    /// <summary>
    /// An interface that represents a bundle of resources that can be loaded on demand. This facilitates wrappers 
    /// around the global 'Resources' (see: ResourceAssetBundle), portions of 'Resources' (see: ResourcePackage), 
    /// 'AssetBundle' (see: AssetBundlePackage), as well as groups of bundles (see: AssetBundleGroup).
    /// </summary>
    public interface IAssetBundle : INameable, System.IDisposable
    {

        IEnumerable<string> GetAllAssetNames();

        bool Contains(string name);

        UnityEngine.Object LoadAsset(string name);
        UnityEngine.Object LoadAsset(string name, System.Type tp);
        T LoadAsset<T>(string name) where T : class;

        IEnumerable<UnityEngine.Object> LoadAllAssets();
        IEnumerable<UnityEngine.Object> LoadAllAssets(System.Type tp);
        IEnumerable<T> LoadAllAssets<T>() where T : class;

        void UnloadAllAssets();

    }

    [System.Serializable]
    public class AssetBundleRef : SerializableInterfaceRef<IAssetBundle>
    {

    }

    /// <summary>
    /// A wrapper around the global 'Resources' class so it can be used as an IAssetBundle.
    /// </summary>
    public sealed class ResourcesAssetBundle : IAssetBundle
    {

        #region Singleton Interface

        private static ResourcesAssetBundle _instance;
        public static ResourcesAssetBundle Instance
        {
            get
            {
                if (_instance == null) _instance = new ResourcesAssetBundle();
                return _instance;
            }
        }

        #endregion

        #region Fields

        #endregion

        #region CONSTRUCTOR

        private ResourcesAssetBundle()
        {
            //enforce as singleton
        }

        #endregion

        #region Methods

        IEnumerable<string> IAssetBundle.GetAllAssetNames()
        {
            return Enumerable.Empty<string>();
        }

        public bool Contains(string path)
        {
            //there's no way to test it, so we assume true
            return true;
        }

        public UnityEngine.Object LoadAsset(string path)
        {
            return Resources.Load(path);
        }

        public UnityEngine.Object LoadAsset(string path, System.Type tp)
        {
            return Resources.Load(path, tp);
        }

        public T LoadAsset<T>(string path) where T : class
        {
            return Resources.Load(path, typeof(T)) as T;
        }

        public IEnumerable<UnityEngine.Object> LoadAllAssets()
        {
            return Resources.LoadAll(string.Empty);
        }

        public IEnumerable<UnityEngine.Object> LoadAllAssets(System.Type tp)
        {
            return Resources.LoadAll(string.Empty, tp);
        }

        public IEnumerable<T> LoadAllAssets<T>() where T : class
        {
            return Resources.LoadAll(string.Empty, typeof(T)).Cast<T>();
        }

        public void UnloadAllAssets()
        {
            //technically this doesn't act the same as LoadedAssetBundle, it only unloads ununsed assets
            Resources.UnloadUnusedAssets();
        }

        #endregion

        #region INameable Interface

        string INameable.Name
        {
            get { return "*UnityResources*"; }
            set { }
        }

        bool INameable.CompareName(string nm)
        {
            return nm == "*UnityResources*";
        }

        void INameable.SetDirty()
        {
            //do nothing
        }

        #endregion

        #region IDisposable Interface

        public void Dispose()
        {
            this.UnloadAllAssets();
        }

        #endregion


        #region Equality Overrides

        public override int GetHashCode()
        {
            return 1;
        }

        #endregion

    }

    public sealed class AssetBundleWrapper : IAssetBundle
    {

        #region Fields

        private AssetBundle _bundle;

        #endregion

        #region CONSTRUCTOR

        public AssetBundleWrapper(AssetBundle bundle)
        {
            if (object.ReferenceEquals(bundle, null)) throw new System.ArgumentNullException(nameof(bundle));

            _bundle = bundle;
            _nameCache = new com.spacepuppy.Utils.NameCache.UnityObjectNameCache(bundle);
        }

        #endregion

        #region Properties

        public AssetBundle Bundle { get { return _bundle; } }

        #endregion

        #region IAssetBundle Interface

        public bool Contains(string name)
        {
            return _bundle.Contains(name);
        }

        public IEnumerable<string> GetAllAssetNames()
        {
            return _bundle.GetAllAssetNames();
        }

        public UnityEngine.Object LoadAsset(string name)
        {
            return _bundle.LoadAsset(name);
        }

        public UnityEngine.Object LoadAsset(string name, System.Type tp)
        {
            return _bundle.LoadAsset(name, tp);
        }

        public T LoadAsset<T>(string name) where T : class
        {
            return _bundle.LoadAsset(name, typeof(T)) as T;
        }

        public IEnumerable<UnityEngine.Object> LoadAllAssets()
        {
            return _bundle.LoadAllAssets();
        }

        public IEnumerable<UnityEngine.Object> LoadAllAssets(System.Type tp)
        {
            return _bundle.LoadAllAssets(tp);
        }

        public IEnumerable<T> LoadAllAssets<T>() where T : class
        {
            return _bundle.LoadAllAssets(typeof(T)).Cast<T>();
        }

        public void UnloadAllAssets()
        {
            _bundle.Unload(true);
        }

        public void Dispose()
        {
            if(_bundle != null)
            {
                _bundle.Unload(true);
                Resources.UnloadAsset(_bundle);
            }
        }

        #endregion

        #region INameable Interface

        private com.spacepuppy.Utils.NameCache.UnityObjectNameCache _nameCache;
        public string name
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

    }

    public static class AssetBundleUtil
    {

        public static IAssetBundle ToWrapper(this AssetBundle bundle)
        {
            return new AssetBundleWrapper(bundle);
        }

    }

}
