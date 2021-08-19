#pragma warning disable 0649 // variable declared but not used.

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy.Utils;

namespace com.spacepuppy.Project
{

    [CreateAssetMenu(fileName = "AssetSet", menuName = "Spacepuppy/Asset Set")]
    public class QueryableAssetSet : ScriptableObject, IAssetBundle
    {

        #region Fields

        [SerializeField]
        private bool _supportNestedGroups;

        [SerializeField]
        [ReorderableArray()]
        private UnityEngine.Object[] _assets;

        [System.NonSerialized]
        private Dictionary<string, UnityEngine.Object> _table;
        [System.NonSerialized]
        private bool _clean;
        [System.NonSerialized]
        private bool _nested;

        #endregion

        #region CONSTRUCTOR

        public QueryableAssetSet()
        {
            _nameCache = new NameCache.UnityObjectNameCache(this);
        }

        #endregion

        #region Properties

        public bool SupportNestedGroups
        {
            get { return _supportNestedGroups; }
            set
            {
                if (_supportNestedGroups == value) return;
                _supportNestedGroups = value;
                _clean = false;
            }
        }

        #endregion

        #region Methods

        private void SetupTable()
        {
            if (_table == null)
                _table = new Dictionary<string, Object>();
            else
                _table.Clear();

            _nested = false;
            for (int i = 0; i < _assets.Length; i++)
            {
                _table[_assets[i].name] = _assets[i];
                if (_supportNestedGroups && _assets[i] is IAssetBundle) _nested = true;
            }
            _clean = true;
        }

        public IEnumerable<string> GetAllAssetNames(bool shallow = false)
        {
            if (!_clean) this.SetupTable();

            if (!shallow && _nested)
            {
                return _table.Keys.Union(_assets.OfType<IAssetBundle>().SelectMany(o => o.GetAllAssetNames()));
            }
            else
            {
                return _table.Keys;
            }
        }

        public bool TryGetAsset(string name, out UnityEngine.Object obj)
        {
            if (!_clean) this.SetupTable();

            if (_table.TryGetValue(name, out obj))
            {
                return true;
            }
            else if (_nested)
            {
                for (int i = 0; i < _assets.Length; i++)
                {
                    if (_assets[i] is QueryableAssetSet assetset)
                    {
                        if (assetset.TryGetAsset(name, out obj)) return true;
                    }
                    else if (_assets[i] is IAssetBundle bundle)
                    {
                        obj = bundle.LoadAsset(name);
                        if (!object.ReferenceEquals(obj, null)) return true;
                    }
                }
            }

            obj = null;
            return false;
        }

        public bool TryGetAsset(string name, System.Type tp, out UnityEngine.Object obj)
        {
            if (!_clean) this.SetupTable();

            if (_table.TryGetValue(name, out obj))
            {
                obj = ObjUtil.GetAsFromSource(tp, obj) as UnityEngine.Object;
                if (!object.ReferenceEquals(obj, null)) return true;
            }

            if (_nested)
            {
                for (int i = 0; i < _assets.Length; i++)
                {
                    if (_assets[i] is QueryableAssetSet assetset)
                    {
                        if (assetset.TryGetAsset(name, tp, out obj)) return true;
                    }
                    else if (_assets[i] is IAssetBundle bundle)
                    {
                        obj = bundle.LoadAsset(name, tp);
                        if (!object.ReferenceEquals(obj, null)) return true;
                    }
                }
            }

            obj = null;
            return false;
        }

        public bool TryGetAsset<T>(string name, out T obj) where T : class
        {
            if (!_clean) this.SetupTable();

            UnityEngine.Object o;
            if (_table.TryGetValue(name, out o))
            {
                obj = ObjUtil.GetAsFromSource<T>(o);
                if (!object.ReferenceEquals(obj, null)) return true;
            }
            
            if (_nested)
            {
                for (int i = 0; i < _assets.Length; i++)
                {
                    if (_assets[i] is QueryableAssetSet assetset)
                    {
                        if (assetset.TryGetAsset<T>(name, out obj)) return true;
                    }
                    else if (_assets[i] is IAssetBundle bundle)
                    {
                        obj = bundle.LoadAsset(name, typeof(T)) as T;
                        if (!object.ReferenceEquals(obj, null)) return true;
                    }
                }
            }

            obj = null;
            return false;
        }

        public UnityEngine.Object GetAsset(string name)
        {
            UnityEngine.Object obj;
            TryGetAsset(name, out obj);
            return obj;
        }

        public UnityEngine.Object GetAsset(string name, System.Type tp)
        {
            UnityEngine.Object obj;
            TryGetAsset(name, tp, out obj);
            return obj;
        }

        public T GetAsset<T>(string name) where T : class
        {
            T obj;
            TryGetAsset<T>(name, out obj);
            return obj;
        }

        public IEnumerable<UnityEngine.Object> GetAllAssets(bool shallow = false)
        {
            if (!_clean) this.SetupTable();

            if (!shallow && _nested)
            {
                return this.GetAllAssetNames().Select(o => this.GetAsset(o));
            }
            else
            {
                return _table.Values;
            }
        }

        public IEnumerable<UnityEngine.Object> GetAllAssets(System.Type tp, bool shallow = false)
        {
            return this.GetAllAssets(shallow).Select(o => ObjUtil.GetAsFromSource(tp, o) as UnityEngine.Object).Where(o => !object.ReferenceEquals(o, null));
        }

        public IEnumerable<T> GetAllAssets<T>(bool shallow = false) where T : class
        {
            if (!_clean) this.SetupTable();

            if (!shallow && _nested)
            {
                foreach (var obj in this.GetAllAssets().Select(o => ObjUtil.GetAsFromSource<T>(o)).Where(o => !object.ReferenceEquals(o, null)))
                {
                    yield return obj;
                }
            }
            else
            {
                var e = _table.Values.GetEnumerator();
                while (e.MoveNext())
                {
                    var obj = ObjUtil.GetAsFromSource<T>(e.Current);
                    if (!object.ReferenceEquals(obj, null))
                    {
                        yield return obj;
                    }
                }
            }
        }

        /// <summary>
        /// Replaces the internal collection with a new set of assets.
        /// </summary>
        /// <param name="assets"></param>
        public void ResetAssets(IEnumerable<UnityEngine.Object> assets)
        {
            _assets = assets.ToArray();
            if (_table != null)
            {
                _table.Clear();
                this.SetupTable();
            }
        }


        public bool Contains(UnityEngine.Object asset)
        {
            if (!_clean) this.SetupTable();

            if (_table.Values.Contains(asset)) return true;

            if (_nested)
            {
                for (int i = 0; i < _assets.Length; i++)
                {
                    if (_assets[i] is QueryableAssetSet bundle && bundle.Contains(asset)) return true;
                }
            }

            return false;
        }

        public void UnloadAsset(UnityEngine.Object asset)
        {
            if (this.Contains(asset))
            {
                Resources.UnloadAsset(asset);
            }
        }

        #endregion

        #region IAssetBundle Interface

        public string Name { get { return this.name; } }

        public bool Contains(string name)
        {
            if (!_clean) this.SetupTable();

            if (_table.ContainsKey(name)) return true;

            if (_nested)
            {
                for (int i = 0; i < _assets.Length; i++)
                {
                    if (_assets[i] is IAssetBundle bundle && bundle.Contains(name))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        IEnumerable<string> IAssetBundle.GetAllAssetNames()
        {
            return this.GetAllAssetNames();
        }

        UnityEngine.Object IAssetBundle.LoadAsset(string name)
        {
            return this.GetAsset(name);
        }

        UnityEngine.Object IAssetBundle.LoadAsset(string name, System.Type tp)
        {
            return this.GetAsset(name, tp);
        }

        T IAssetBundle.LoadAsset<T>(string name)
        {
            return this.GetAsset<T>(name);
        }

        IEnumerable<UnityEngine.Object> IAssetBundle.LoadAllAssets()
        {
            return this.GetAllAssets();
        }

        IEnumerable<UnityEngine.Object> IAssetBundle.LoadAllAssets(System.Type tp)
        {
            return this.GetAllAssets(tp);
        }

        IEnumerable<T> IAssetBundle.LoadAllAssets<T>()
        {
            return this.GetAllAssets<T>();
        }

        public void UnloadAllAssets()
        {
            if (_table != null) _table.Clear();
            for (int i = 0; i < _assets.Length; i++)
            {
                if (_assets[i] is GameObject) continue;

                if (_supportNestedGroups && _assets[i] is IAssetBundle bundle)
                {
                    bundle.UnloadAllAssets();
                }

                Resources.UnloadAsset(_assets[i]);
            }
            _clean = false;
        }



        public void Dispose()
        {
            this.UnloadAllAssets();
            Resources.UnloadAsset(this);
        }

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

    }

}
