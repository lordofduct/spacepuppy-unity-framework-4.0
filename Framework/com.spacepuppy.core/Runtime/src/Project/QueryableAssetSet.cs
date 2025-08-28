#pragma warning disable 0649 // variable declared but not used.

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy.Collections;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Project
{

    /// <summary>
    /// An AssetSet of assets that can be queried by name or asset id. Asset id querying only supported by those 
    /// types that implement 'IAssetGuidIdentifiable'. Or if the collection was configured with asset guids at 
    /// editor time. 
    /// </summary>
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

        [SerializeField, Tooltip("EditorOnly - attempts to generate a lookup table of the asset guids if it can at editor time if the asset isn't an 'IAssetGuidIdentifiable'.")]
        private bool _tryForceAssetGuids;

        [SerializeField]
        private TypeReference _assetType = new TypeReference(typeof(UnityEngine.Object));

        [SerializeField, ReorderableArray()]
        private UnityEngine.Object[] _assets = ArrayUtil.Empty<UnityEngine.Object>();
        [SerializeField, HideInInspector]
        private string[] _forcedAssetGuids;

        [System.NonSerialized]
        private Dictionary<string, UnityEngine.Object> _table;
        [System.NonSerialized]
        private Dictionary<System.Guid, UnityEngine.Object> _guidTable;
        [System.NonSerialized]
        private bool _clean;
        [System.NonSerialized]
        private bool _nested;

        [System.NonSerialized]
        private object _lastShallowFilteredCollection;

        [System.NonSerialized]
        private int _version;

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

        protected virtual void OnDestroy()
        {
            _clean = false;
            _table?.Clear();
            _guidTable?.Clear();
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
        /// Defines if at editor time this attempts to force asset guids. 
        /// Warning - can not be set at runtime.
        /// </summary>
        public bool TryForceAssetGuids
        {
            get => _tryForceAssetGuids;
#if UNITY_EDITOR
            set => _tryForceAssetGuids = value;
#else
            set {}
#endif
        }

        /// <summary>
        /// Returns true if some assets are queryably by asset id. Not all assets are guaranteed to be queryable by asset id.
        /// </summary>
        public bool SupportsGuidLookup
        {
            get
            {
                if (!_clean && !this.SetupTable()) return false;
                return _guidTable.Count > 0;
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

        public int Version => _version;

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

                if (i < _forcedAssetGuids?.Length && !string.IsNullOrEmpty(_forcedAssetGuids[i]) && System.Guid.TryParse(_forcedAssetGuids[i], out System.Guid forcedguid) && forcedguid != System.Guid.Empty)
                {
                    if (_guidTable == null) _guidTable = new Dictionary<System.Guid, Object>();
                    _guidTable[forcedguid] = _assets[i];
                }
                else
                {
                    var agid = ObjUtil.GetAsFromSource<IAssetGuidIdentifiable>(_assets[i]);
                    if (agid != null && agid.AssetId != System.Guid.Empty)
                    {
                        if (_guidTable == null) _guidTable = new Dictionary<System.Guid, Object>();
                        _guidTable[agid.AssetId] = _assets[i];
                    }
                }
            }
            _clean = true;
            _version++;
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
            if (!_clean && !this.SetupTable()) return Enumerable.Empty<T>();

            if (!shallow && _nested)
            {
                return this.GetAllAssets().Select(o => ObjUtil.GetAsFromSource<T>(o)).Where(o => !object.ReferenceEquals(o, null));
            }
            else
            {
                return new ShallowEnumerator<T>(this);
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

        public ShallowEnumerator<T> GetShallowEnumerator<T>() where T : class => new(this);

        /// <summary>
        /// Creates a ShallowFilteredCollection of type T.
        /// </summary>
        /// <remarks>
        /// And any given time a single filteredcollection can be cached by passing in 'true' to the 'attemptToCache' parameter. 
        /// The most recently T passed with the cache flag will be what is available. 
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="attemptToCache"></param>
        /// <returns></returns>
        public ShallowFilteredCollection<T> GetShallowCollection<T>(bool attemptToCache = false) where T : class
        {
            if (_lastShallowFilteredCollection is ShallowFilteredCollection<T> cached)
            {
                return cached;
            }
            var result = new ShallowFilteredCollection<T>(this);
            if (attemptToCache) _lastShallowFilteredCollection = result;
            return result;
        }

        /// <summary>
        /// Replaces the internal collection with a new set of assets.
        /// </summary>
        /// <param name="assets"></param>
        public virtual void ResetAssets(IEnumerable<UnityEngine.Object> assets)
        {
            if (this.IsDestroyed()) return;

            this.ResetAssetsDirect(assets?.ToArray() ?? ArrayUtil.Empty<UnityEngine.Object>());
        }
        void ResetAssetsDirect(UnityEngine.Object[] assets)
        {
            _assets = assets;
#if UNITY_EDITOR
            if (_tryForceAssetGuids)
            {
                _forcedAssetGuids = _assets.Select(o =>
                {
                    //only store the guids of those assets that require it
                    if (o == null) return string.Empty;
                    var aid = ObjUtil.GetAsFromSource<IAssetGuidIdentifiable>(o);
                    if (aid != null) return string.Empty;

                    var path = UnityEditor.AssetDatabase.GetAssetPath(o);
                    return !string.IsNullOrEmpty(path) ? UnityEditor.AssetDatabase.AssetPathToGUID(path) : string.Empty;
                }).ToArray();
            }
            else
            {
                _forcedAssetGuids = ArrayUtil.Empty<string>();
            }
#else
            _forcedAssetGuids = ArrayUtil.Empty<string>();
#endif
            if (_table != null)
            {
                _table.Clear();
                this.SetupTable();
            }
        }

        /// <summary>
        /// Replaces the internal collection with a new set of assets.
        /// </summary>
        /// <param name="assets"></param>
        public virtual void ResetAssets(IEnumerable<KeyValuePair<System.Guid, UnityEngine.Object>> assets)
        {
            if (this.IsDestroyed()) return;

            if (assets != null)
            {
                using (var lst_a = TempCollection.GetList<UnityEngine.Object>())
                using (var lst_b = TempCollection.GetList<string>())
                {
                    foreach (var pair in assets)
                    {
                        lst_a.Add(pair.Value);
                        //only store the guids of those assets that require it
                        var aid = ObjUtil.GetAsFromSource<IAssetGuidIdentifiable>(pair.Value);
                        if (aid == null || (aid.AssetId != pair.Key && pair.Key != System.Guid.Empty))
                        {
                            lst_b.Add(pair.Key.ToString("N"));
                        }
                        else
                        {
                            lst_b.Add(string.Empty);
                        }
                    }
                    _assets = lst_a.ToArray();
                    _forcedAssetGuids = lst_b.ToArray();
                }
            }
            else
            {
                _assets = ArrayUtil.Empty<UnityEngine.Object>();
                _forcedAssetGuids = ArrayUtil.Empty<string>();
            }

            if (_table != null)
            {
                _table.Clear();
                this.SetupTable();
            }
        }

        /// <summary>
        /// Configures AssetType as T and then flattens all entries in the asset set. 
        /// This is useful at start of the game to ensure the collection is faster to access 
        /// for known types when combined with ShallowFilteredCollection. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void Flatten<T>() where T : class
        {
            if (this.IsDestroyed()) return;

            _assets = this.GetAllAssets<T>().OfType<UnityEngine.Object>().ToArray();
            _assetType.Type = typeof(T);
            this.SetupTable();
        }

        /// <summary>
        /// Similar to Flatten but consumes an enumerable of assets that the asset set will be reset to.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void Flatten<T>(IEnumerable<T> assets) where T : class
        {
            if (assets == null) throw new System.ArgumentNullException(nameof(assets));
            if (this.IsDestroyed()) return;

            _assets = assets.OfType<UnityEngine.Object>().ToArray();
            _assetType.Type = typeof(T);
            this.SetupTable();
        }


        public bool Contains(UnityEngine.Object asset)
        {
            if (!_clean && !this.SetupTable()) return false;

            foreach (var o in _assets)
            {
                if (object.ReferenceEquals(o, asset)) return true;
                if (_nested && o is QueryableAssetSet bundle && bundle.ContainsIndirectly(asset)) return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if it's in the asset set or attached to something in the asset set.
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        public bool ContainsIndirectly(UnityEngine.Object asset, bool testIsChildOf = false)
        {
            if (!_clean && !this.SetupTable()) return false;

            var ago = GameObjectUtil.GetGameObjectFromSource(asset);
            if (ago != null)
            {
                foreach (var o in _assets)
                {
                    if (object.ReferenceEquals(o, asset)) return true;

                    var go = GameObjectUtil.GetGameObjectFromSource(o);
                    if (go) return object.ReferenceEquals(go, ago) || (testIsChildOf && ago.transform.IsChildOf(go.transform));

                    if (_nested && o is QueryableAssetSet bundle && bundle.ContainsIndirectly(asset)) return true;
                }
            }
            else
            {
                foreach (var o in _assets)
                {
                    if (object.ReferenceEquals(o, asset)) return true;
                    if (_nested && o is QueryableAssetSet bundle && bundle.ContainsIndirectly(asset)) return true;
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

        public bool TrySlowLookupGuid(UnityEngine.Object asset, out System.Guid guid)
        {
            if (object.ReferenceEquals(asset, null) ||
                (!_clean && !this.SetupTable()))
            {
                guid = default;
                return false;
            }

            if (_guidTable != null)
            {
                foreach (var pair in _guidTable)
                {
                    if (pair.Value == asset)
                    {
                        guid = pair.Key;
                        return true;
                    }
                }
            }

            if (_nested)
            {
                foreach (var o in _assets)
                {
                    if (o is QueryableAssetSet qas && qas.TrySlowLookupGuid(asset, out guid))
                    {
                        return true;
                    }
                }
            }

            guid = default;
            return false;
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

        public static QueryableAssetSet CreateAssetSet<T>(IEnumerable<T> coll) where T : class
        {
            var inst = CreateInstance<QueryableAssetSet>();
            inst.AssetType = typeof(T);
            inst.ResetAssets(coll.OfType<UnityEngine.Object>());
            return inst;
        }

        #endregion


        #region Special Types

        public class ShallowFilteredCollection<T> : IReadOnlyCollection<T>, IEnumerable<T> where T : class
        {

            #region Fields

            protected QueryableAssetSet _assets;
            private int _count = -1;

            #endregion

            #region CONSTRUCTOR

            internal ShallowFilteredCollection(QueryableAssetSet assets)
            {
                if (assets == null) throw new System.ArgumentNullException(nameof(assets));
                _assets = assets;
            }

            #endregion

            #region Properties

            public QueryableAssetSet AssetSet => _assets;

            public int Count
            {
                get
                {
                    if (!_assets._clean || _count < 0)
                    {
                        if (!_assets._clean && !_assets.SetupTable()) return 0;
                        _count = 0;
                        var e = this.GetEnumerator();
                        while (e.MoveNext()) _count++;
                    }
                    return _count;
                }
            }

            public T this[string name] => _assets.GetAsset<T>(name);
            public T this[System.Guid guid] => _assets.GetAsset<T>(guid);

            #endregion

            #region Methods

            public bool TryGetAsset(string name, out T asset) => _assets.TryGetAsset<T>(name, out asset);
            public T GetAsset(string name) => _assets.GetAsset<T>(name);

            public bool TryGetAsset(System.Guid guid, out T asset) => _assets.TryGetAsset<T>(guid, out asset);
            public T GetAsset(System.Guid guid) => _assets.GetAsset<T>(guid);

            public bool Contains(T asset)
            {
                if (object.ReferenceEquals(asset, null)) return false;
                if (!_assets._clean && !_assets.SetupTable()) return false;

                var e = this.GetEnumerator();
                while (e.MoveNext())
                {
                    if (object.ReferenceEquals(e.Current, asset)) return true;
                }
                return false;
            }

            #endregion

            #region IEnumerable Interface

            public ShallowEnumerator<T> GetEnumerator() => new(_assets);
            IEnumerator<T> IEnumerable<T>.GetEnumerator() => this.GetEnumerator();
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this.GetEnumerator();

            #endregion

        }

        public struct ShallowEnumerator<T> : IEnumerator<T>, IEnumerable<T> where T : class
        {

            private Dictionary<string, UnityEngine.Object>.ValueCollection.Enumerator _e;
            private T _current;
            public ShallowEnumerator(QueryableAssetSet assets)
            {
                if (assets == null) throw new System.ArgumentNullException(nameof(assets));
                if (!assets._clean && !assets.SetupTable())
                {
                    _e = default;
                    _current = default;
                }
                else
                {
                    _e = assets._table.Values.GetEnumerator();
                    _current = default;
                }
            }

            public T Current => _current;

            object System.Collections.IEnumerator.Current => Current;

            public bool MoveNext()
            {
                while (_e.MoveNext())
                {
                    var obj = ObjUtil.GetAsFromSource<T>(_e.Current);
                    if (!object.ReferenceEquals(obj, null))
                    {
                        _current = obj;
                        return true;
                    }
                }
                return false;
            }

            public void Dispose()
            {
                _e.Dispose();
                _e = default;
                _current = default;
            }

            void System.Collections.IEnumerator.Reset()
            {
                (_e as System.Collections.IEnumerator).Reset();
            }

            IEnumerator<T> IEnumerable<T>.GetEnumerator() => this;
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this;
        }

        #endregion

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            this.ResetAssetsDirect(_assets); //this force resets the guids
        }
#endif

    }

}
