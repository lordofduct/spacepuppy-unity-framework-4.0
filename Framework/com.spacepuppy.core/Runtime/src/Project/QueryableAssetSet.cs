#pragma warning disable 0649 // variable declared but not used.

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy.Utils;
using com.spacepuppy.Collections;

namespace com.spacepuppy.Project
{

    [CreateAssetMenu(fileName = "AssetSet", menuName = "Spacepuppy/Asset Set")]
    public class QueryableAssetSet : ScriptableObject, IAssetSet, IGuidAssetSet, IAssetGuidIdentifiable, IReadOnlyDictionary<string, UnityEngine.Object>, IReadOnlyDictionary<System.Guid, UnityEngine.Object>, IEnumerable<UnityEngine.Object>
    {

#if UNITY_EDITOR
        public const string PROP_ASSETID = nameof(_assetId);
        public const string PROP_SUPPORTNESTEDGROUPS = nameof(_supportNestedGroups);
        public const string PROP_ASSETTYPE = nameof(_assetType);
        public const string PROP_ASSETS = nameof(_assets);
#endif

        #region Fields

        [SerializeField]
        [SerializableGuid.Config(LinkedGuidMode.Asset)]
        private SerializableGuid _assetId;

        [SerializeField]
        private bool _supportNestedGroups;

        [SerializeField]
        private TypeReference _assetType = new TypeReference(typeof(UnityEngine.Object));

        [SerializeField]
        [ReorderableArray()]
        private UnityEngine.Object[] _assets = ArrayUtil.Empty<UnityEngine.Object>();

        [System.NonSerialized]
        private Dictionary<string, UnityEngine.Object> _table;
        [System.NonSerialized]
        private Dictionary<System.Guid, UnityEngine.Object> _guidTable;
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

        public QueryableAssetSet(System.Type assetType)
        {
            _nameCache = new NameCache.UnityObjectNameCache(this);
            _assetType.Type = assetType ?? typeof(UnityEngine.Object);
        }

        #endregion

        #region Properties

        public virtual System.Type AssetType
        {
            get => _assetType.Type ?? typeof(UnityEngine.Object);
            set
            {
                if (this.CanEditAssetType) _assetType.Type = value;
            }
        }

        public virtual bool CanEditAssetType => true;

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

        /// <summary>
        /// Internal access to the asset array. You should use this only for reading. 
        /// If you need to manipulate the collection use the public methods to ensure 
        /// lookup tables are synced correctly.
        /// </summary>
        protected UnityEngine.Object[] Assets
        {
            get => _assets;
        }

        #endregion

        #region Methods

        private bool SetupTable()
        {
            if (this.IsDestroyed()) return false;
            (_table ?? (_table = new Dictionary<string, Object>())).Clear();
            _guidTable?.Clear();

            _nested = false;
            for (int i = 0; i < _assets.Length; i++)
            {
                if (!_assets[i]) continue;
                _table[_assets[i].name] = _assets[i];
                if (_supportNestedGroups && _assets[i] is IAssetSet) _nested = true;

                var agid = ObjUtil.GetAsFromSource<IAssetGuidIdentifiable>(_assets[i]);
                if (agid != null && agid.AssetId != System.Guid.Empty)
                {
                    if (_guidTable == null) _guidTable = new Dictionary<System.Guid, Object>();
                    _guidTable[agid.AssetId] = _assets[i];
                }
            }
            _clean = true;
            return true;
        }

        public void ReindexAssetSet() => this.SetupTable();

        public IEnumerable<string> GetAllAssetNames(bool shallow = false)
        {
            if (!_clean && !this.SetupTable()) return Enumerable.Empty<string>();

            if (!shallow && _nested)
            {
                return _table.Keys.Union(_assets.OfType<IAssetSet>().SelectMany(o => o.GetAllAssetNames()));
            }
            else
            {
                return _table.Keys;
            }
        }

        public bool TryGetAsset(string name, out UnityEngine.Object obj)
        {
            if (!_clean && !this.SetupTable())
            {
                obj = null;
                return false;
            }

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
                    else if (_assets[i] is IAssetSet nestedassets)
                    {
                        obj = nestedassets.LoadAsset(name);
                        if (!object.ReferenceEquals(obj, null)) return true;
                    }
                }
            }

            obj = null;
            return false;
        }

        public bool TryGetAsset(string name, System.Type tp, out UnityEngine.Object obj)
        {
            if (!_clean && !this.SetupTable())
            {
                obj = null;
                return false;
            }

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
                    else if (_assets[i] is IAssetSet nestedassets)
                    {
                        obj = nestedassets.LoadAsset(name, tp);
                        if (!object.ReferenceEquals(obj, null)) return true;
                    }
                }
            }

            obj = null;
            return false;
        }

        public bool TryGetAsset<T>(string name, out T obj) where T : class
        {
            if (!_clean && !this.SetupTable())
            {
                obj = null;
                return false;
            }

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
                    else if (_assets[i] is IAssetSet nestedassets)
                    {
                        obj = nestedassets.LoadAsset(name, typeof(T)) as T;
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
            if (!_clean && !this.SetupTable()) return Enumerable.Empty<UnityEngine.Object>();

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
            if (!_clean && !this.SetupTable()) yield break;

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

        public int GetAllAssets<T>(ICollection<T> buffer, bool shallow = false) where T : class
        {
            if (!_clean && !this.SetupTable()) return 0;

            int cnt = 0;
            if (!shallow && _nested)
            {
                foreach (var obj in this.GetAllAssets().Select(o => ObjUtil.GetAsFromSource<T>(o)).Where(o => !object.ReferenceEquals(o, null)))
                {
                    cnt++;
                    buffer.Add(obj);
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
                        cnt++;
                        buffer.Add(obj);
                    }
                }
            }
            return cnt;
        }

        /// <summary>
        /// Replaces the internal collection with a new set of assets.
        /// </summary>
        /// <param name="assets"></param>
        public virtual void ResetAssets(IEnumerable<UnityEngine.Object> assets)
        {
            if (this.IsDestroyed()) return;

            _assets = assets.ToArray();
            if (_table != null)
            {
                _table.Clear();
                this.SetupTable();
            }
        }


        public bool Contains(UnityEngine.Object asset)
        {
            if (!_clean && !this.SetupTable()) return false;

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





        /// <summary>
        /// Get the available shallow asset guids. Not all assets have associated guids.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<System.Guid> GetAssetGuids(bool shallow = false)
        {
            if (!_clean && !this.SetupTable()) return Enumerable.Empty<System.Guid>();
            if (_guidTable == null) return Enumerable.Empty<System.Guid>();

            if (!shallow && _nested)
            {
                return _guidTable.Keys.Union(_assets.OfType<IGuidAssetSet>().SelectMany(o => o.GetAssetGuids()));
            }
            else
            {
                return _guidTable.Keys;
            }
        }

        public bool TryGetAsset(System.Guid guid, out UnityEngine.Object obj)
        {
            if (!_clean && !this.SetupTable())
            {
                obj = null;
                return false;
            }

            obj = null;
            if (_guidTable?.TryGetValue(guid, out obj) ?? false)
            {
                return true;
            }
            else if (_nested)
            {
                for (int i = 0; i < _assets.Length; i++)
                {
                    if (_assets[i] is QueryableAssetSet assetset)
                    {
                        if (assetset.TryGetAsset(guid, out obj)) return true;
                    }
                    else if (_assets[i] is IGuidAssetSet nestedassets)
                    {
                        obj = nestedassets.LoadAsset(guid);
                        if (!object.ReferenceEquals(obj, null)) return true;
                    }
                }
            }

            obj = null;
            return false;
        }

        public bool TryGetAsset(System.Guid guid, System.Type tp, out UnityEngine.Object obj)
        {
            if (!_clean && !this.SetupTable())
            {
                obj = null;
                return false;
            }

            obj = null;
            if (_guidTable?.TryGetValue(guid, out obj) ?? false)
            {
                obj = ObjUtil.GetAsFromSource(tp, obj) as UnityEngine.Object;
                if (!object.ReferenceEquals(obj, null)) return true;
            }
            else if (_nested)
            {
                for (int i = 0; i < _assets.Length; i++)
                {
                    if (_assets[i] is QueryableAssetSet assetset)
                    {
                        if (assetset.TryGetAsset(guid, tp, out obj)) return true;
                    }
                    else if (_assets[i] is IGuidAssetSet nestedassets)
                    {
                        obj = nestedassets.LoadAsset(guid, tp);
                        if (!object.ReferenceEquals(obj, null)) return true;
                    }
                }
            }

            obj = null;
            return false;
        }

        public bool TryGetAsset<T>(System.Guid guid, out T obj) where T : class
        {
            if (!_clean && !this.SetupTable())
            {
                obj = null;
                return false;
            }

            UnityEngine.Object o = null;
            if (_guidTable?.TryGetValue(guid, out o) ?? false)
            {
                obj = ObjUtil.GetAsFromSource<T>(o);
                if (!object.ReferenceEquals(obj, null)) return true;
            }
            else if (_nested)
            {
                for (int i = 0; i < _assets.Length; i++)
                {
                    if (_assets[i] is QueryableAssetSet assetset)
                    {
                        if (assetset.TryGetAsset(guid, out obj)) return true;
                    }
                    else if (_assets[i] is IGuidAssetSet nestedassets)
                    {
                        obj = nestedassets.LoadAsset<T>(guid);
                        if (!object.ReferenceEquals(obj, null)) return true;
                    }
                }
            }

            obj = null;
            return false;
        }

        public UnityEngine.Object GetAsset(System.Guid guid)
        {
            UnityEngine.Object obj;
            TryGetAsset(guid, out obj);
            return obj;
        }

        public UnityEngine.Object GetAsset(System.Guid guid, System.Type tp)
        {
            UnityEngine.Object obj;
            TryGetAsset(guid, tp, out obj);
            return obj;
        }

        public T GetAsset<T>(System.Guid guid) where T : class
        {
            T obj;
            TryGetAsset<T>(guid, out obj);
            return obj;
        }

        #endregion

        #region IAssetSet Interface

        public string Name { get { return this.name; } }

        public bool Contains(string name)
        {
            if (!_clean && !this.SetupTable()) return false;

            if (_table.ContainsKey(name)) return true;

            if (_nested)
            {
                for (int i = 0; i < _assets.Length; i++)
                {
                    if (_assets[i] is IAssetSet nestedassets && nestedassets.Contains(name))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        IEnumerable<string> IAssetSet.GetAllAssetNames()
        {
            return this.GetAllAssetNames();
        }

        UnityEngine.Object IAssetSet.LoadAsset(string name)
        {
            return this.GetAsset(name);
        }

        UnityEngine.Object IAssetSet.LoadAsset(string name, System.Type tp)
        {
            return this.GetAsset(name, tp);
        }

        T IAssetSet.LoadAsset<T>(string name)
        {
            return this.GetAsset<T>(name);
        }

        IEnumerable<UnityEngine.Object> IAssetSet.LoadAssets()
        {
            return this.GetAllAssets();
        }

        IEnumerable<UnityEngine.Object> IAssetSet.LoadAssets(System.Type tp)
        {
            return this.GetAllAssets(tp);
        }

        IEnumerable<T> IAssetSet.LoadAssets<T>() where T : class
        {
            return this.GetAllAssets<T>();
        }

        public void UnloadAllAssets()
        {
            if (_table != null) _table.Clear();
            for (int i = 0; i < _assets.Length; i++)
            {
                if (_assets[i] is GameObject) continue;

                if (_supportNestedGroups && _assets[i] is IAssetSet bundle)
                {
                    bundle.UnloadAllAssets();
                }

                Resources.UnloadAsset(_assets[i]);
            }
            _clean = false;
        }

        bool TryLoadAsset<T>(string name, out T asset) where T : class
        {
            return this.TryGetAsset<T>(name, out asset);
        }


        public void Dispose(bool unloadAllAssetsOnDispose = false)
        {
            if (unloadAllAssetsOnDispose)
            {
                this.UnloadAllAssets();
                Resources.UnloadAsset(this);
            }
            _table?.Clear();
            _guidTable?.Clear();
            Destroy(this);
        }
        void System.IDisposable.Dispose() => this.Dispose(false);

        #endregion

        #region IGuidAssetSet Interface

        IEnumerable<System.Guid> IGuidAssetSet.GetAssetGuids() => this.GetAssetGuids();

        public bool Contains(System.Guid guid)
        {
            if (!_clean && !this.SetupTable()) return false;

            if (_guidTable?.ContainsKey(guid) ?? false) return true;

            if (_nested)
            {
                for (int i = 0; i < _assets.Length; i++)
                {
                    if (_assets[i] is IGuidAssetSet nestedassets && nestedassets.Contains(guid))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        UnityEngine.Object IGuidAssetSet.LoadAsset(System.Guid guid) => this.GetAsset(guid);

        UnityEngine.Object IGuidAssetSet.LoadAsset(System.Guid guid, System.Type tp) => this.GetAsset(guid, tp);

        T IGuidAssetSet.LoadAsset<T>(System.Guid guid) => this.GetAsset<T>(guid);

        bool TryLoadAsset<T>(System.Guid guid, out T asset) where T : class
        {
            return this.TryGetAsset<T>(guid, out asset);
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

        #region IAssetGuidIdentifiable Interface

        public System.Guid AssetId => _assetId;

        #endregion

        #region IReadOnlyDictionary<string, UnityEngine.Object> Interface

        IEnumerable<string> IReadOnlyDictionary<string, UnityEngine.Object>.Keys => this.GetAllAssetNames();

        IEnumerable<UnityEngine.Object> IReadOnlyDictionary<string, UnityEngine.Object>.Values => this.GetAllAssets();

        int IReadOnlyCollection<KeyValuePair<string, UnityEngine.Object>>.Count
        {
            get
            {
                if (!_clean && !this.SetupTable()) return 0;

                return _nested ? this.GetAllAssetNames().Count() : _table.Count;
            }
        }

        UnityEngine.Object IReadOnlyDictionary<string, UnityEngine.Object>.this[string key] => this.GetAsset(key);

        bool IReadOnlyDictionary<string, Object>.ContainsKey(string key) => this.Contains(key);

        bool IReadOnlyDictionary<string, Object>.TryGetValue(string key, out Object value) => this.TryGetAsset(key, out value);

        IEnumerator<KeyValuePair<string, Object>> IEnumerable<KeyValuePair<string, UnityEngine.Object>>.GetEnumerator()
        {
            if (!_clean && !this.SetupTable()) return Enumerable.Empty<KeyValuePair<string, UnityEngine.Object>>().GetEnumerator();

            if (_nested)
            {
                return this.GetAllAssetNames().Select(o => new KeyValuePair<string, UnityEngine.Object>(o, this.GetAsset(o))).GetEnumerator();
            }
            else
            {
                return _table.GetEnumerator();
            }
        }

        #endregion

        #region IReadOnlyDictionary<System.Guid, UnityEngine.Object> Interface

        IEnumerable<System.Guid> IReadOnlyDictionary<System.Guid, UnityEngine.Object>.Keys => this.GetAssetGuids();

        IEnumerable<UnityEngine.Object> IReadOnlyDictionary<System.Guid, UnityEngine.Object>.Values
        {
            get
            {
                if (!_clean && !this.SetupTable()) return Enumerable.Empty<UnityEngine.Object>();

                return _nested ? this.GetAssetGuids().Select(o => this.GetAsset(o)) : (_guidTable?.Values ?? Enumerable.Empty<UnityEngine.Object>());
            }
        }

        int IReadOnlyCollection<KeyValuePair<System.Guid, UnityEngine.Object>>.Count
        {
            get
            {
                if (!_clean && !this.SetupTable()) return 0;

                return _nested ? this.GetAssetGuids().Count() : (_guidTable?.Count ?? 0);
            }
        }

        UnityEngine.Object IReadOnlyDictionary<System.Guid, UnityEngine.Object>.this[System.Guid key] => this.GetAsset(key);

        bool IReadOnlyDictionary<System.Guid, Object>.ContainsKey(System.Guid key) => this.Contains(key);

        bool IReadOnlyDictionary<System.Guid, Object>.TryGetValue(System.Guid key, out Object value) => this.TryGetAsset(key, out value);

        IEnumerator<KeyValuePair<System.Guid, Object>> IEnumerable<KeyValuePair<System.Guid, UnityEngine.Object>>.GetEnumerator()
        {
            if (!_clean && !this.SetupTable()) return Enumerable.Empty<KeyValuePair<System.Guid, UnityEngine.Object>>().GetEnumerator();
            if (_guidTable == null) return Enumerable.Empty<KeyValuePair<System.Guid, UnityEngine.Object>>().GetEnumerator();
            return _guidTable.GetEnumerator();
        }

        #endregion

        #region IEnumerable Interface

        public IEnumerator<UnityEngine.Object> GetEnumerator()
        {
            return this.GetAllAssets().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetAllAssets().GetEnumerator();
        }

        #endregion

        #region Static Factory

        public static QueryableAssetSet CreateAssetSet(System.Type assetType = null)
        {
            var inst = CreateInstance<QueryableAssetSet>();
            if (assetType != null) inst.AssetType = assetType;
            return inst;
        }

        public static QueryableAssetSet CreateAssetSet(System.Collections.IEnumerable coll, System.Type assetType = null)
        {
            var inst = CreateInstance<QueryableAssetSet>();
            if (assetType != null) inst.AssetType = assetType;
            inst.ResetAssets(coll.OfType<UnityEngine.Object>());
            return inst;
        }

        public static QueryableAssetSet CreateAssetSet<T>(IEnumerable<T> coll)
        {
            var inst = CreateInstance<QueryableAssetSet>();
            inst.AssetType = typeof(T);
            inst.ResetAssets(coll.OfType<UnityEngine.Object>());
            return inst;
        }

        #endregion

    }

}
