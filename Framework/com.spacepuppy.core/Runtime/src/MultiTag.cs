﻿#pragma warning disable 0649 // variable declared but not used.
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy.Collections;
using com.spacepuppy.Project;
using com.spacepuppy.Utils;

namespace com.spacepuppy
{

    /// <summary>
    /// Allows adding more than one tag to a given GameObject.
    /// </summary>
    [AddComponentMenu("SpacePuppy/Multi Tag")]
    [DisallowMultipleComponent()]
    public sealed class MultiTag : SPComponent, IEnumerable<string>
    {

        #region Multiton Interface

        public static readonly MultiTagPool Pool = new MultiTagPool();


        internal static bool TryGetMultiTag(GameObject go, out MultiTag c)
        {
            return Pool.TryGet(go, out c);
        }

        internal static MultiTag[] FindAll(string tag)
        {
            return (from c in MultiTag.Pool where c.HasTag(tag) select c).ToArray();
        }

        internal static void FindAll(string tag, ICollection<GameObject> coll)
        {
            var e = Pool.GetEnumerator();
            while (e.MoveNext())
            {
                if (e.Current.HasTag(tag)) coll.Add(e.Current.gameObject);
            }
        }

        internal static MultiTag Find(string tag)
        {
            var e = Pool.GetEnumerator();
            while (e.MoveNext())
            {
                if (e.Current.HasTag(tag)) return e.Current;
            }
            return null;
        }

        #endregion

        #region Fields

        [SerializeField]
        private bool _searchableIfInactive;

        [SerializeField]
        private List<string> _tags;

        #endregion

        #region CONSTRUCTOR

        // Use this for initialization
        protected override void Awake()
        {
            base.Awake();

            if (!this.gameObject.CompareTag(SPConstants.TAG_MULTITAG))
                this.gameObject.tag = SPConstants.TAG_MULTITAG;
        }

        protected override void OnEnable()
        {
            Pool.AddReference(this);

            base.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (!_searchableIfInactive)
                Pool.RemoveReference(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            Pool.RemoveReference(this);
        }

        #endregion

        #region Properties

        public int Count { get { return _tags?.Count ?? 0; } }

        public string this[int index]
        {
            get { return (_tags != null && index >= 0 && index < _tags.Count) ? _tags[index] : null; }
        }

        #endregion

        #region Methods

        public void UpdateTags(IEnumerable<string> tags)
        {
            if (tags == null)
            {
                _tags = null;
            }
            else
            {
                _tags.Clear();
                foreach (var s in tags)
                {
                    if (IsValidTag(s)) _tags.Add(s);
                }
            }
        }

        public IEnumerable<string> GetTags()
        {
            return _tags ?? Enumerable.Empty<string>();
        }

        public bool ContainsTag(string tag)
        {
            if (_tags == null || _tags.Count == 0) return MultiTag.IsEmptyTag(tag);
            return _tags.Contains(tag);
        }

        public bool ContainsTag(params string[] tags)
        {
            if (_tags == null || _tags.Count == 0) return tags.Contains(SPConstants.TAG_UNTAGGED);
            switch (tags?.Length ?? 0)
            {
                case 0:
                    return false;
                case 1:
                    return _tags.Contains(tags[0]);
                default:
                    return _tags.ContainsAny(tags);
            }
        }

        public bool ContainsTag(IEnumerable<string> tags)
        {
            if (_tags == null || _tags.Count == 0) return tags.Contains(SPConstants.TAG_UNTAGGED);
            switch ((tags as IList<string>)?.Count ?? 2)
            {
                case 0:
                    return false;
                case 1:
                    return _tags.Contains((tags as IList<string>)[0]);
                default:
                    return _tags.ContainsAny(tags);
            }
        }

        public bool AddTag(string tag)
        {
            if (!IsValidMultiTag(tag)) return false;

            if (_tags == null)
            {
                _tags = new List<string>();
                _tags.Add(tag);
            }
            else
            {
                if (!_tags.Contains(tag))
                {
                    _tags.Add(tag);
                }
            }
            return true;
        }

        public bool RemoveTag(string tag)
        {
            if (_tags == null) return false;

            return _tags.Remove(tag);
        }

        public void ClearTags()
        {
            _tags = null;
        }

        #endregion

        #region IEnumerable Interface

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<string> IEnumerable<string>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        #endregion

        #region Static Methods

        public static bool IsValidTag(string stag)
        {
            return TagData.IsValidTag(stag);
        }

        public static bool IsValidActiveTag(string stag)
        {
            return TagData.IsValidTag(stag) && stag != SPConstants.TAG_UNTAGGED;
        }

        public static bool IsValidMultiTag(string stag)
        {
            return TagData.IsValidTag(stag) && stag != SPConstants.TAG_UNTAGGED && stag != SPConstants.TAG_MULTITAG;
        }

        public static bool IsEmptyTag(string stag)
        {
            return string.IsNullOrEmpty(stag) || stag == SPConstants.TAG_UNTAGGED;
        }

        #endregion

        #region Special Types

        public struct Enumerator : IEnumerator<string>
        {

            private MultiTag _multi;
            private int _index;
            private string _current;

            public Enumerator(MultiTag multi)
            {
                _multi = multi;
                _index = 0;
                _current = null;
            }

            public string Current
            {
                get
                {
                    return _current;
                }
            }

            object System.Collections.IEnumerator.Current
            {
                get
                {
                    return _current;
                }
            }

            public bool MoveNext()
            {
                if (_multi == null || _multi._tags == null) return false;
                if (_index >= _multi._tags.Count) return false;

                _current = _multi._tags[_index];
                _index++;
                return true;
            }

            public void Dispose()
            {
                _multi = null;
                _index = 0;
                _current = null;
            }

            void System.Collections.IEnumerator.Reset()
            {
                _index = 0;
            }

        }

        public class MultiTagPool : MultitonPool<MultiTag>
        {
            private Dictionary<GameObject, MultiTag> _table = new Dictionary<GameObject, MultiTag>();

            public override void AddReference(MultiTag obj)
            {
                if (object.ReferenceEquals(obj, null)) throw new System.ArgumentNullException(nameof(obj));

                base.AddReference(obj);
                _table[obj.gameObject] = obj;
            }

            public override bool RemoveReference(MultiTag obj)
            {
                if (object.ReferenceEquals(obj, null)) throw new System.ArgumentNullException(nameof(obj));

                _table.Remove(obj.gameObject);
                return base.RemoveReference(obj);
            }

            public bool TryGet(GameObject go, out MultiTag c)
            {
                if (_table.TryGetValue(go, out c))
                {
                    return true;
                }
                else
                {
                    c = go.GetComponent<MultiTag>();
                    return c != null;
                }
            }


        }

        #endregion

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!this.gameObject.CompareTag(SPConstants.TAG_MULTITAG))
            {
                this.gameObject.tag = SPConstants.TAG_MULTITAG;
            }
        }
#endif

    }

    /// <summary>
    /// Utility class for accessing MultiTag. This implements a version of CompareTag called 'HasTag' that respects the MultiTag component. 
    /// As well as methods to add/remove tags from a GameObject while respecting MultiTag as well.
    /// 
    /// This utility will add MultiTag components automatically to GameObjects that need it if you attempt to add more than 1 tag to a GameObject.
    /// </summary>
    public static class MultiTagHelper
    {

        /// <summary>
        /// Determines if GameObject has a tag.
        /// </summary>
        /// <param name="go"></param>
        /// <returns></returns>
        public static bool HasTag(this GameObject go)
        {
            if (go == null) return false;

            if (go.CompareTag(SPConstants.TAG_UNTAGGED)) return false;

            MultiTag c;
            if (MultiTag.TryGetMultiTag(go, out c))
                return c.Count > 0;
            else
                return true;
        }

        public static bool HasTag(this Component c)
        {
            if (c == null) throw new System.ArgumentNullException("c");

            return HasTag(c.gameObject);
        }

        /**
         * HasTag
         */

        public static bool HasTag(this GameObject go, string stag)
        {
            if (go == null) return false;
            if (go.CompareTag(stag)) return true;

            MultiTag c;
            if (MultiTag.TryGetMultiTag(go, out c))
                return c.ContainsTag(stag);
            else
                return false;
        }

        public static bool HasTag(this Component c, string stag)
        {
            if (c == null) return false;
            return HasTag(c.gameObject, stag);
        }

        public static bool HasTag(this GameObject go, params string[] stags)
        {
            if (stags == null) return false;
            if (go == null) return false;

            MultiTag c;
            if (MultiTag.TryGetMultiTag(go, out c))
            {
                return c.ContainsTag(stags);
            }
            else
            {
                foreach (var stag in stags)
                {
                    if (go.CompareTag(stag)) return true;
                }
            }
            return false;
        }

        public static bool HasTag(this Component c, params string[] stags)
        {
            if (c == null) return false;
            return HasTag(c.gameObject, stags);
        }

        public static bool HasTag(this GameObject go, IEnumerable<string> stags)
        {
            if (stags == null) return false;
            if (go == null) return false;

            MultiTag c;
            if (MultiTag.TryGetMultiTag(go, out c))
            {
                return c.ContainsTag(stags);
            }
            else
            {
                return stags.Contains(go.tag);
            }
        }

        public static bool HasTag(this Component c, IEnumerable<string> stags)
        {
            if (c == null) return false;
            return HasTag(c.gameObject, stags);
        }


        /**
         * AddTag
         */

        public static bool AddTag(this GameObject go, string stag)
        {
            if (go == null) throw new System.ArgumentNullException(nameof(go));

            MultiTag multitag;
            if (MultiTag.TryGetMultiTag(go, out multitag))
            {
                return multitag.AddTag(stag);
            }
            else if (TagData.IsValidTag(stag))
            {
                //if (MultiTag.IsEmptyTag(go.tag))
                if (go.CompareTag(SPConstants.TAG_UNTAGGED))
                {
                    go.tag = stag;
                }
                else if (!go.CompareTag(stag))
                {
                    var oldtag = go.tag;
                    multitag = go.AddComponent<MultiTag>();
                    multitag.AddTag(oldtag);
                    multitag.AddTag(stag);
                }
                return true;
            }

            return false;
        }

        public static bool AddTag(this Component c, string stag)
        {
            if (c == null) throw new System.ArgumentNullException(nameof(c));

            return AddTag(c.gameObject, stag);
        }

        /**
         * RemoveTag
         */

        public static void RemoveTag(this GameObject go, string stag, bool bDestroyMultiTagComponentOnEmpty = false)
        {
            if (go == null) throw new System.ArgumentNullException("go");

            MultiTag multitag;
            if (MultiTag.TryGetMultiTag(go, out multitag))
            {
                multitag.RemoveTag(stag);
                if (bDestroyMultiTagComponentOnEmpty && multitag.Count == 0)
                {
                    ObjUtil.SmartDestroy(multitag);
                    go.tag = SPConstants.TAG_UNTAGGED;
                }
            }
            else
            {
                if (go.CompareTag(stag)) go.tag = SPConstants.TAG_UNTAGGED;
            }
        }

        public static void RemoveTag(this Component c, string stag, bool bDestroyMultiTagComponentOnEmpty = false)
        {
            if (c == null) throw new System.ArgumentNullException("c");

            RemoveTag(c.gameObject, stag, bDestroyMultiTagComponentOnEmpty);
        }

        /**
         * SetTag
         */

        public static void SetTag(this GameObject go, string stag, bool bDestroyMultiTagComponent = false)
        {
            if (go == null) throw new System.ArgumentNullException("go");

            if (bDestroyMultiTagComponent)
            {
                go.RemoveComponents<MultiTag>();
                if (MultiTag.IsValidMultiTag(stag))
                {
                    go.tag = stag;
                }
                else
                {
                    go.tag = SPConstants.TAG_UNTAGGED;
                }
            }
            else
            {
                MultiTag multitag;
                if (MultiTag.TryGetMultiTag(go, out multitag))
                {
                    multitag.ClearTags();
                    if (MultiTag.IsValidMultiTag(stag)) multitag.AddTag(stag);
                }
                else if (MultiTag.IsValidMultiTag(stag))
                {
                    go.tag = stag;
                }
                else
                {
                    go.tag = SPConstants.TAG_UNTAGGED;
                }
            }
        }

        public static void SetTag(this Component c, string stag, bool bDestroyMultiTagComponent = false)
        {
            if (c == null) throw new System.ArgumentNullException("c");

            SetTag(c.gameObject, stag, bDestroyMultiTagComponent);
        }

        /**
         * ClearTag
         */

        public static void ClearTags(this GameObject go, bool bDestroyMultiTagComponent = false)
        {
            if (go == null) throw new System.ArgumentNullException("go");

            MultiTag multitag;
            if (MultiTag.TryGetMultiTag(go, out multitag))
            {
                if (bDestroyMultiTagComponent)
                {
                    ObjUtil.SmartDestroy(multitag);
                    go.tag = SPConstants.TAG_UNTAGGED;
                }
                else
                {
                    multitag.ClearTags();
                }
            }
            else
            {
                go.tag = SPConstants.TAG_UNTAGGED;
            }
        }

        public static void ClearTags(this Component c, bool bDestroyMultiTagComponent = false)
        {
            if (c == null) throw new System.ArgumentNullException("c");

            ClearTags(c.gameObject, bDestroyMultiTagComponent);
        }

        /**
         * GetTags
         */

        public static IEnumerable<string> GetTags(this GameObject go)
        {
            if (go == null) throw new System.ArgumentNullException("go");

            MultiTag multitag;
            if (MultiTag.TryGetMultiTag(go, out multitag))
            {
                return multitag.GetTags();
            }
            else
            {
                return new string[] { go.tag };
            }
        }

        public static IEnumerable<string> GetTags(this Component c)
        {
            if (c == null) throw new System.ArgumentNullException("c");

            return GetTags(c.gameObject);
        }

        /**
         * AddTags
         */

        public static void AddTags(this GameObject go, IEnumerable<string> tags)
        {
            if (go == null) throw new System.ArgumentNullException("go");
            if (tags == null) throw new System.ArgumentNullException("tags");

            MultiTag multitag;
            if (MultiTag.TryGetMultiTag(go, out multitag))
            {
                foreach (var t in tags)
                {
                    multitag.AddTag(t);
                }
            }
            else
            {
                var oldtag = go.tag;
                multitag = go.AddComponent<MultiTag>();
                multitag.AddTag(oldtag);
                foreach (var t in tags)
                {
                    multitag.AddTag(t);
                }
            }
        }

        public static void AddTags(this GameObject go, params string[] tags)
        {
            if (go == null) throw new System.ArgumentNullException("go");
            if (tags == null || tags.Length == 0) return;

            MultiTag multitag;
            if (MultiTag.TryGetMultiTag(go, out multitag))
            {
                foreach (var t in tags)
                {
                    multitag.AddTag(t);
                }
            }
            else
            {
                if (tags.Length > 1 || !go.CompareTag(SPConstants.TAG_UNTAGGED))
                {
                    var oldtag = go.tag;
                    multitag = go.AddComponent<MultiTag>();
                    multitag.AddTag(oldtag);
                    foreach (var t in tags)
                    {
                        multitag.AddTag(t);
                    }
                }
                else
                {
                    go.tag = tags[0];
                }
            }
        }

        public static void AddTags(this GameObject go, GameObject source)
        {
            if (go == null) throw new System.ArgumentNullException("go");
            if (source == null) throw new System.ArgumentNullException("source");

            MultiTag multitag;
            if (MultiTag.TryGetMultiTag(go, out multitag))
            {
                MultiTag otherMultiTag;
                if (MultiTag.TryGetMultiTag(source, out otherMultiTag))
                {
                    var e = otherMultiTag.GetEnumerator();
                    while (e.MoveNext())
                    {
                        multitag.AddTag(e.Current);
                    }
                }
                else
                {
                    multitag.AddTag(source.tag);
                }
            }
            else
            {
                MultiTag otherMultiTag;
                if (MultiTag.TryGetMultiTag(source, out otherMultiTag))
                {
                    if (otherMultiTag.Count > 1 || !go.CompareTag(SPConstants.TAG_UNTAGGED))
                    {
                        var oldtag = go.tag;
                        multitag = go.AddComponent<MultiTag>();
                        multitag.AddTag(oldtag);
                        var e = otherMultiTag.GetEnumerator();
                        while (e.MoveNext())
                        {
                            multitag.AddTag(e.Current);
                        }
                    }
                    else
                    {
                        go.tag = otherMultiTag[0];
                    }
                }
                else if (!source.CompareTag(SPConstants.TAG_UNTAGGED))
                {
                    if (go.CompareTag(SPConstants.TAG_UNTAGGED))
                    {
                        go.tag = source.tag;
                    }
                    else
                    {
                        go.AddTag(source.tag);
                    }
                }
            }
        }

        public static void AddTags(this Component c, GameObject source)
        {
            if (c == null) throw new System.ArgumentNullException("c");

            AddTags(c.gameObject, source);
        }

    }

}